using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Icons
{
    public class Icon : IDisposable
    {
        public readonly static Icon RawGrid = new Icon("grid.png");
        public readonly static Icon EmptyGrid = new Icon("empty.png");
        public readonly static Icon Number1Grid = new Icon("number1.png");
        public readonly static Icon Number2Grid = new Icon("number2.png");
        public readonly static Icon Number3Grid = new Icon("number3.png");
        public readonly static Icon Number4Grid = new Icon("number4.png");
        public readonly static Icon Number5Grid = new Icon("number5.png");
        public readonly static Icon Number6Grid = new Icon("number6.png");
        public readonly static Icon Number7Grid = new Icon("number7.png");
        public readonly static Icon Number8Grid = new Icon("number8.png");
        public readonly static Icon FlagGrid = new Icon("flag.png");
        public readonly static Icon QuestionGrid = new Icon("question.png");
        public readonly static Icon BombGrid = new Icon("bomb.png");
        public readonly static Icon RedBombGrid = new Icon("redBomb.png");

        public readonly static Icon NormalFace = new Icon("normal.png");
        public readonly static Icon WinFace = new Icon("win.png");
        public readonly static Icon LoseFace = new Icon("lose.png");

        public readonly static Icon Count0 = new Icon("count0.png");
        public readonly static Icon Count1 = new Icon("count1.png");
        public readonly static Icon Count2 = new Icon("count2.png");
        public readonly static Icon Count3 = new Icon("count3.png");
        public readonly static Icon Count4 = new Icon("count4.png");
        public readonly static Icon Count5 = new Icon("count5.png");
        public readonly static Icon Count6 = new Icon("count6.png");
        public readonly static Icon Count7 = new Icon("count7.png");
        public readonly static Icon Count8 = new Icon("count8.png");
        public readonly static Icon Count9 = new Icon("count9.png");

        public readonly string FileName;
        public readonly IntegerMap IntegerMap;
        public Icon(string fileName)
        {
            FileName = fileName;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + "." + fileName;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var bitmap = new Bitmap(stream);
                IntegerMap = new IntegerMap(bitmap);
            }
        }
        public void Dispose()
        {
            IntegerMap.Dispose();
        }
    }
}
