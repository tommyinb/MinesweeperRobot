using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class JoinRatioStrategy : IStrategy
    {
        public IEnumerable<GuessGrid> Guess(StrategyBoard board)
        {
            var numberPoints = EnumerableUtil.Rectangle(board.Size).Where(t => board.Grids[t.X, t.Y].IsNumber());
            var orderedNumberPoints = numberPoints.OrderBy(numberPoint =>
            {
                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
                return surroundingPoints.Count(t => board.Grids[t.X, t.Y] == Grid.Raw);
            }).ThenBy(numberPoint =>
            {
                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
                var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw);
                return surroundingRawPoints.Sum(surroundingRawPoint =>
                {
                    var secondarySurroundingPoints = surroundingRawPoint.Surrounding().Where(t => board.Size.Contains(t));
                    return secondarySurroundingPoints.Count(t => board.Grids[t.X, t.Y].IsNumber());
                });
            });

            foreach (var numberPoint in numberPoints)
            {
                var numberValue = board.Grids[numberPoint.X, numberPoint.Y];

                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t));
                var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();

                var surroundingCombinations = GetCombinations(surroundingRawPoints.Count());
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

                    var validValues = Enumerable.Range(0, validCombinationCount).Select(j => validSurroundingCombinations[j][i]);
                    if (validValues.Distinct().Count() == 1)
                    {
                        yield return new GuessGrid
                        {
                            Point = surroundingPoint,
                            Value = validValues.First(),
                            Confidence = 1
                        };
                    }
                }
            }
        }

        private static IEnumerable<GuessValue[]> GetCombinations(int rawCount)
        {
            var counts = Enumerable.Range(0, rawCount);
            return counts.Aggregate((IEnumerable<GuessValue[]>)new[] { new GuessValue[0] }, (combinations, i) =>
            {
                return combinations.SelectMany(combination =>
                {
                    var emptyAdded = combination.Concat(new[] { GuessValue.Empty }).ToArray();
                    var bombAdded = combination.Concat(new[] { GuessValue.Bomb }).ToArray();
                    return new[] { emptyAdded, bombAdded };
                });
            });
        }
    }
}
