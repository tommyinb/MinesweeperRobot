using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class ObjectUtil
    {
        public static bool EqualsDefault<T>(this T @object) where T : struct
        {
            return @object.Equals(default(T));
        }
    }
}
