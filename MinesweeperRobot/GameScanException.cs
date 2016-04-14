using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot
{

    [Serializable]
    public class GameScanException : Exception
    {
        public GameScanException() { }
        public GameScanException(string message) : base(message) { }
        public GameScanException(string message, Exception inner) : base(message, inner) { }
        protected GameScanException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
