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
        public IEnumerable<GuessGrid> Guess(StrategyBoard board)
        {
            var numberPoints = EnumerableUtil.Rectangle(board.Size).Where(t => board.Grids[t.X, t.Y].IsNumber());
            foreach (var numberPoint in numberPoints)
            {
                var numberValue = board.Grids[numberPoint.X, numberPoint.Y];

                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
                var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();
                if (surroundingRawPoints.Any() == false) continue;

                var surroundingPossibleValues = surroundingRawPoints.Select(t => new[] { GuessValue.Empty, GuessValue.Bomb }).ToArray();
                var surroundingCombinations = EnumerableUtil.Combinations(surroundingPossibleValues);
                var validSurroundingCombinations = surroundingCombinations.Where(combination =>
                {
                    var bombCount = combination.Count(t => t == GuessValue.Bomb);
                    return bombCount == (int)numberValue;
                }).Where(combination =>
                {
                    var surroundingBombPoints = surroundingRawPoints.Where((t, i) => combination[i] == GuessValue.Bomb).ToList();

                    return surroundingRawPoints.All(surroundingPoint =>
                    {
                        var secondarySurroundingPoints = surroundingPoint.Surrounding().Where(t => board.Size.Contains(t));
                        var secondarySurroundingNumberPoints = secondarySurroundingPoints.Where(t => board.Grids[t.X, t.Y].IsNumber());

                        return secondarySurroundingNumberPoints.All(secondarySurroundingPoint =>
                        {
                            var tertiarySurroundingPoints = secondarySurroundingPoint.Surrounding().Where(t => board.Size.Contains(t));
                            var tertiarySurroundingBombCount = tertiarySurroundingPoints.Count(surroundingBombPoints.Contains);

                            var secondarySurroundingValue = board.Grids[secondarySurroundingPoint.X, secondarySurroundingPoint.Y];

                            return tertiarySurroundingBombCount <= (int)secondarySurroundingValue;
                        });
                    });
                }).ToArray();

                var validCombinationCount = validSurroundingCombinations.GetLength(0);
                for (int i = 0; i < surroundingRawPoints.Length; i++)
                {
                    var surroundingPoint = surroundingRawPoints[i];

                    var validValues = Enumerable.Range(0, validCombinationCount).Select(j => validSurroundingCombinations[j][i]).ToArray();
                    var probableValues = validValues.GroupBy(t => t).OrderByDescending(t => t.Count()).First();

                    var confidence = (double)probableValues.Count() / validValues.Count();
                    if (confidence >= 1)
                    {
                        yield return new GuessGrid
                        {
                            Point = surroundingPoint,
                            Value = probableValues.Key,
                            Confidence = 1
                        };
                    }
                    else
                    {
                        if (probableValues.Key == GuessValue.Empty)
                        {
                            yield return new GuessGrid
                            {
                                Point = surroundingPoint,
                                Value = probableValues.Key,
                                Confidence = confidence
                            };
                        }
                    }
                }
            }
        }
    }
}
