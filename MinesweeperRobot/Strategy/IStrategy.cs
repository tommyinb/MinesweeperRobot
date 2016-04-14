using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Strategy
{
    public interface IStrategy
    {
        IEnumerable<GuessGrid> Guess(StrategyBoard board);
    }
}
