using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public class LogWriter : TextWriter
    {
        public readonly TextWriter TextWriter;
        public LogWriter(TextWriter textWriter, string logPath)
        {
            TextWriter = textWriter;
            LogPath = logPath;
        }

        public readonly string LogPath;
        public override Encoding Encoding { get { return Encoding.UTF8; } }
        private readonly StringBuilder cache = new StringBuilder();

        public override void Write(char value)
        {
            TextWriter.Write(value);

            cache.Append(value);

            if (cache.Length > 1000)
            {
                Flush();
            }
        }
        public override void Flush()
        {
            TextWriter.Flush();

            File.AppendAllText(LogPath, cache.ToString(), Encoding);

            cache.Clear();
        }
    }
}
