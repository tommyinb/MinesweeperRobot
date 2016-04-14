using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot
{
    public static class TimeLog
    {
        public static void Log(string item)
        {
            var timeTag = DateTime.Now.ToString(@"mm:ss.fff");
            var line = "[" + timeTag + "] " + item;
            File.AppendAllLines("time.log", new[] { line });
        }
        public static TimeLogStopwatch Stopwatch(string item)
        {
            return new TimeLogStopwatch(item);
        }
    }

    public class TimeLogStopwatch : IDisposable
    {
        private string item;
        private Stopwatch stopwatch;

        public TimeLogStopwatch(string item)
        {
            this.item = item;
            stopwatch = Stopwatch.StartNew();

            TimeLog.Log(item + " (Start)");
        }
        public void Dispose()
        {
            stopwatch.Stop();

            TimeLog.Log(item + " (Stop with length " + stopwatch.Elapsed.ToString(@"ss\.fff") + ")");
        }
    }
}
