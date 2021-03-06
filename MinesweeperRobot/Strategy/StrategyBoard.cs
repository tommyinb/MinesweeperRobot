﻿using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class StrategyBoard
    {
        public Grid[,] Grids { get; private set; }
        public Size Size { get; private set; }

        public int BombCount { get; private set; }
        public int RawCount { get; private set; }

        public StrategyBoard(Grid[,] grids, int bombCount)
        {
            Grids = (Grid[,])grids.Clone();
            Size = Grids.GetSize();

            BombCount = bombCount;

            Reduce();
            RawCount = Count(Grid.Raw);
        }

        private void Reduce()
        {
            var gridSize = Grids.GetSize();
            var points = EnumerableUtil.Rectangle(gridSize);

            var flagPoints = points.Where(t => Grids[t.X, t.Y] == Grid.Flag).ToArray();
            foreach (var flagPoint in flagPoints)
            {
                var surroundingPoints = flagPoint.Surrounding().Where(t => gridSize.Contains(t));
                foreach (var surroundingPoint in surroundingPoints)
                {
                    var surroundingValue = Grids[surroundingPoint.X, surroundingPoint.Y];
                    if (surroundingValue.IsNumber())
                    {
                        var reducedValue = surroundingValue - 1;
                        Grids[surroundingPoint.X, surroundingPoint.Y] = reducedValue;
                    }
                }

                Grids[flagPoint.X, flagPoint.Y] = Grid.None;
            }

            var questionPoints = points.Where(t => Grids[t.X, t.Y] == Grid.Question).ToArray();
            foreach (var questionPoint in questionPoints)
            {
                Grids[questionPoint.X, questionPoint.Y] = Grid.Raw;
            }
        }
        private int Count(Grid grid)
        {
            var points = EnumerableUtil.Rectangle(Grids.GetSize());
            return points.Count(point => Grids[point.X, point.Y] == grid);
        }
    }
}
