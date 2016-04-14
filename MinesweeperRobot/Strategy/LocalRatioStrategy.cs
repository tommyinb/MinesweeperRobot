using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public class LocalRatioStrategy : IStrategy
    {
        public IEnumerable<GuessGrid> Guess(StrategyBoard board)
        {
            var emptyPoints = EnumerableUtil.Rectangle(board.Size).Where(t => board.Grids[t.X, t.Y] == Grid.Empty);
            foreach (var emptyPoint in emptyPoints)
            {
                var surroundingPoints = emptyPoint.Surrounding().Where(t => board.Size.Contains(t)).ToArray();
                var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();
                foreach (var surroundingRawPoint in surroundingRawPoints)
                {
                    yield return new GuessGrid { Value = GuessValue.Empty, Point = surroundingRawPoint, Confidence = 1 };
                }
            }

            var numberPoints = EnumerableUtil.Rectangle(board.Size).Where(t => board.Grids[t.X, t.Y].IsNumber());
            foreach (var numberPoint in numberPoints)
            {
                var numberValue = board.Grids[numberPoint.X, numberPoint.Y];

                var surroundingPoints = numberPoint.Surrounding().Where(t => board.Size.Contains(t)).ToArray();
                var surroundingRawPoints = surroundingPoints.Where(t => board.Grids[t.X, t.Y] == Grid.Raw).ToArray();
                if (surroundingRawPoints.Any() == false) continue;

                if (surroundingRawPoints.Count() == (int)numberValue)
                {
                    foreach (var surroundingRawPoint in surroundingRawPoints)
                    {
                        yield return new GuessGrid { Value = GuessValue.Bomb, Point = surroundingRawPoint, Confidence = 1 };
                    }
                }
            }
        }
    }
}
