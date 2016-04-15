using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class RandomStrategy : IStrategy
    {
        public IEnumerable<GuessGrid> Guess(StrategyBoard board)
        {
            var gridPoints = EnumerableUtil.Rectangle(board.Size);
            var rawPoints = gridPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();
            if (rawPoints.Length <= 0) return new GuessGrid[0];

            var bombConfidence = (double)board.BombCount / board.RawCount;
            if (bombConfidence >= 1)
            {
                return rawPoints.Select(t => new GuessGrid
                {
                    Value = GuessValue.Bomb,
                    Point = t,
                    Confidence = 1
                });
            }
            else if (bombConfidence <= 0)
            {
                return rawPoints.Select(t => new GuessGrid
                {
                    Value = GuessValue.Empty,
                    Point = t,
                    Confidence = 1
                });
            }
            else
            {
                var firstRawPoint = RandomUtil.Randomize(rawPoints).First();
                var guessGrid = new GuessGrid
                {
                    Value = GuessValue.Empty,
                    Point = firstRawPoint,
                    Confidence = 1 - bombConfidence
                };
                return new[] { guessGrid };
            }
        }
    }
}
