using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class BruteForceStrategy : IStrategy
    {
        public BruteForceStrategy(StrategyBoard board, int chainLength)
        {
            this.board = board;
            this.chainLength = chainLength;
        }
        private readonly StrategyBoard board;
        private readonly int chainLength;

        public IEnumerable<GuessGrid> Guess()
        {
            var chains = GetChains(board).ToArray();
            foreach (var chain in chains)
            {
                var combinations = GetCombinations(chain, board);
                var guesses = GetGuesses(chain, combinations);

                foreach (var guess in guesses)
                {
                    yield return guess;
                }
            }
        }

        private IEnumerable<Chain<Point>> GetChains(StrategyBoard board)
        {
            var numberPoints = EnumerableUtil.Rectangle(board.Size).Where(t => board.Grids[t.X, t.Y].IsNumber());
            var surroundingRawPoints = numberPoints.Select(numberPoint =>
            {
                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
                return surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToChain();
            });

            return MergeChains(surroundingRawPoints).Distinct();
        }
        private IEnumerable<Chain<Point>> MergeChains(IEnumerable<Chain<Point>> chains)
        {
            var currChains = new List<Chain<Point>>(chains);
            for (int i = 0; i < currChains.Count; i++)
            {
                var currChain = currChains[i];

                for (int j = i + 1; j < currChains.Count; j++)
                {
                    var nextRawChain = currChains[j];

                    var connectedLength = currChain.Length + nextRawChain.Length;
                    if (connectedLength > chainLength) continue;

                    var nextChainSurroundingPoints = nextRawChain.SelectMany(t => t.Surrounding());
                    var chainsConnected = nextChainSurroundingPoints.Intersect(currChain).Any();
                    if (chainsConnected)
                    {
                        currChain = currChain.Concat(nextRawChain).Distinct().ToChain();
                        currChains.RemoveAt(j);
                        j -= 1;
                    }
                }

                yield return currChain;
            }
        }

        private IEnumerable<GuessValue[]> GetCombinations(Chain<Point> chain, StrategyBoard board)
        {
            var possibleValues = chain.Select(t => new[] { GuessValue.Empty, GuessValue.Bomb }).ToArray();
            var combinations = EnumerableUtil.Combinations(possibleValues);
            return combinations.Where(combination =>
            {
                var pointValues = chain.ToDictionary((t, i) => t, (t, i) => combination[i]);

                var surroundingPoints = chain.SelectMany(point => point.Surrounding()).Where(t => board.Size.Contains(t)).Distinct();
                var surroundingNumberPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y].IsWithin(Grid.Empty, Grid.Number8));

                return surroundingNumberPoints.All(t => MatchValue(t, pointValues));
            });
        }
        private bool MatchValue(Point numberPoint, Dictionary<Point, GuessValue> guessValues)
        {
            var value = (int)board.Grids[numberPoint.X, numberPoint.Y];

            var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
            var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();

            var surroundingGuessValues = surroundingPoints.Intersect(guessValues.Keys).Select(t => guessValues[t]).ToArray();
            var surroundingGuessCount = surroundingGuessValues.Count();

            var minSurroundingBombCount = surroundingGuessValues.Count(t => t == GuessValue.Bomb);
            if (minSurroundingBombCount > value) return false;

            var maxSurroundingBombCount = surroundingRawPoints.Count() - surroundingGuessCount + minSurroundingBombCount;
            if (maxSurroundingBombCount < value) return false;

            return true;
        }

        private IEnumerable<GuessGrid> GetGuesses(Chain<Point> chain, IEnumerable<GuessValue[]> combinations)
        {
            for (int i = 0; i < chain.Length; i++)
            {
                var point = chain[i];
                var values = combinations.Select(t => t[i]).ToArray();

                var valueGroups = values.GroupBy(t => t);
                var probableValueGroup = valueGroups.OrderByDescending(t => t.Count()).First();

                var confidence = (double)probableValueGroup.Count() / values.Count();
                if (confidence >= 1)
                {
                    yield return new GuessGrid
                    {
                        Point = point,
                        Value = probableValueGroup.Key,
                        Confidence = 1
                    };
                }
                else
                {
                    if (probableValueGroup.Key == GuessValue.Empty)
                    {
                        yield return new GuessGrid
                        {
                            Point = point,
                            Value = probableValueGroup.Key,
                            Confidence = confidence
                        };
                    }
                }
            }
        }
    }
}
