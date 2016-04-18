using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot
{

    [Serializable]
    public class GamePauseException : Exception
    {
        public GamePauseException() { }
        public GamePauseException(string message) : base(message) { }
        public GamePauseException(string message, Exception inner) : base(message, inner) { }
        protected GamePauseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
