using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class StrategyBoard
    {
        public Grid[,] Grids { get; private set; }
        public int TotalBombCount { get; private set; }
        public int GridCount { get; private set; }

        public int RawCount { get; private set; }
        public int EmptyCount { get; private set; }

        public int MarkBombCount { get; private set; }
        public int UnmarkedBombCount { get; private set; }

        public StrategyBoard(Grid[,] grids, int totalBombCount)
        {
            Grids = (Grid[,])grids.Clone();
            TotalBombCount = totalBombCount;
            GridCount = Grids.GetLength(0) * Grids.GetLength(1);

            var points = EnumerableUtil.Rectangle(Grids.GetSize());
            RawCount = Count(Grid.Raw);
            EmptyCount = Count(Grid.Empty);

            MarkBombCount = Count(Grid.Bomb) + Count(Grid.Flag);
            UnmarkedBombCount = Math.Max(TotalBombCount - MarkBombCount, 0);
        }
        private int Count(Grid grid)
        {
            var points = EnumerableUtil.Rectangle(Grids.GetSize());
            return points.Count(point => Grids[point.X, point.Y] == grid);
        }
    }
}
