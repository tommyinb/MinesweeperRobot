using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class RatioStrategy : IStrategy
    {
        public IEnumerable<GuessGrid> Deduce(StrategyBoard board)
        {
            var size = board.Grids.GetSize();
            var gridPoints = EnumerableUtil.Rectangle(size);
            var rawPoints = gridPoints.Where(point => board.Grids[point.X, point.Y] == Grid.Raw);

            var emptyGuess = rawPoints.Select(t => new GuessGrid
            {
                Point = t,
                Grid = Grid.Empty,
                Confidence = 1 - (float)board.UnmarkedBombCount / board.RawCount
            });
            var bombGuess = rawPoints.Select(t => new GuessGrid
            {
                Point = t,
                Grid = Grid.Bomb,
                Confidence = (float)board.UnmarkedBombCount / board.RawCount
            });
            return emptyGuess.Concat(bombGuess);
        }
    }
}
