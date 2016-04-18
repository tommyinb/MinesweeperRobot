using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinesweeperRobot.Utility;
using System.Threading;
using MinesweeperRobot.Icons;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MinesweeperRobot
{
    public class GameBoard
    {
        public GameBoard(IntPtr windowHandle)
        {
            WindowHandle = windowHandle;

            gridScanner = new IconScanner(Icons.Icon.RawGrid, Icons.Icon.EmptyGrid,
                Icons.Icon.Number1Grid, Icons.Icon.Number2Grid, Icons.Icon.Number3Grid, Icons.Icon.Number4Grid,
                Icons.Icon.Number5Grid, Icons.Icon.Number6Grid, Icons.Icon.Number7Grid, Icons.Icon.Number8Grid,
                Icons.Icon.FlagGrid, Icons.Icon.QuestionGrid, Icons.Icon.BombGrid, Icons.Icon.RedBombGrid);

            faceScanner = new IconScanner(Icons.Icon.NormalFace, Icons.Icon.WinFace, Icons.Icon.LoseFace);

            bombCountScanner = new IconScanner(Icons.Icon.Count0, Icons.Icon.Count1,
                Icons.Icon.Count2, Icons.Icon.Count3, Icons.Icon.Count4, Icons.Icon.Count5,
                Icons.Icon.Count6, Icons.Icon.Count7, Icons.Icon.Count8, Icons.Icon.Count9);
        }
        public readonly IntPtr WindowHandle;

        public Rectangle WindowRectangle { get; private set; }
        private Bitmap windowBitmap;
        public void CaptureWindow()
        {
            var captureLog = TimeLog.Stopwatch("Capture Window");

            WindowRectangle = Window.GetRectangle(WindowHandle);
            if (WindowRectangle == default(Rectangle)) throw new GameScanException("Capture window failed");

            windowBitmap = new Bitmap(WindowRectangle.Width, WindowRectangle.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(windowBitmap))
            {
                graphics.CopyFromScreen(WindowRectangle.Location, Point.Empty, windowBitmap.Size);
            }

            windowBitmap.Save("capture.png", ImageFormat.Png);

            captureLog.Dispose();
        }
        private Bitmap GetCapture(Rectangle screenRectangle)
        {
            var relativeLocation = screenRectangle.Location.Minus(WindowRectangle.Location);
            var relativeRectangle = new Rectangle(relativeLocation, screenRectangle.Size);

            var withinWindowBitmap = new Rectangle(Point.Empty, windowBitmap.Size).Contains(relativeRectangle);
            if (withinWindowBitmap == false) throw new GameScanException("Capture window part failed");

            return windowBitmap.Clone(relativeRectangle, windowBitmap.PixelFormat);
        }
        public void FullScanWindow()
        {
            using (var integerMap = new IntegerMap(windowBitmap))
            {
                var gridIcons = gridScanner.FullScan(integerMap).ToArray();
                var gridLeft = gridIcons.Min(t => t.Item1.X);
                var gridRight = gridIcons.Max(t => t.Item1.X + t.Item2.IntegerMap.Size.Width - 1);
                var gridTop = gridIcons.Min(t => t.Item1.Y);
                var gridBottom = gridIcons.Max(t => t.Item1.Y + t.Item2.IntegerMap.Size.Height - 1);
                var gridRectangle = new Rectangle(gridLeft, gridTop, gridRight - gridLeft + 1, gridBottom - gridTop + 1);
                GridRectangle = gridRectangle.Add(WindowRectangle.Location);

                var gridCountX = GridRectangle.Width / Icons.Icon.RawGrid.IntegerMap.Size.Width;
                var gridCountY = GridRectangle.Height / Icons.Icon.RawGrid.IntegerMap.Size.Height;
                Grids = new Grid[gridCountX, gridCountY];
                QuickScanGrids();

                var faceIcons = faceScanner.FullScan(integerMap).ToArray();
                var faceIcon = faceIcons.Single();
                var faceRectangle = new Rectangle(faceIcon.Item1, faceIcon.Item2.IntegerMap.Size);
                FaceRectangle = faceRectangle.Add(WindowRectangle.Location);
                QuickScanFace();

                var scoreIcons = bombCountScanner.FullScan(integerMap).ToArray();
                var bombCountIcons = scoreIcons.Where(t => t.Item1.X < integerMap.Size.Width / 2).OrderBy(t => t.Item1.X);
                var bombCountRectangles = bombCountIcons.Select(t => new Rectangle(t.Item1, t.Item2.IntegerMap.Size));
                BombCountRectangles = bombCountRectangles.Select(t => t.Add(WindowRectangle.Location)).ToArray();
                QuickScanBombCount();
            }
        }
        private void Click(Point point, MouseButton mouseButton)
        {
            Cursor.Position = point;
            Thread.Sleep(TimeSpan.FromMilliseconds(.1));

            Mouse.MouseDown(mouseButton);
            Thread.Sleep(TimeSpan.FromMilliseconds(.1));

            Mouse.MouseUp(mouseButton);
        }

        public Rectangle GridRectangle { get; private set; }
        public Grid[,] Grids { get; private set; }
        private IconScanner gridScanner;
        public void QuickScanGrids()
        {
            if (GridRectangle == Rectangle.Empty) throw new InvalidOperationException();
            if (Grids == null) throw new InvalidOperationException();

            var captureLog = TimeLog.Stopwatch("Capture Grid");
            var bitmap = GetCapture(GridRectangle);
            captureLog.Dispose();

            var readLog = TimeLog.Stopwatch("Read Grid");
            using (var integerMap = new IntegerMap(bitmap))
            {
                var gridCountX = Grids.GetLength(0);
                var gridCountY = Grids.GetLength(1);
                var gridIndexes = EnumerableUtil.Rectangle(gridCountX, gridCountY);
                foreach (var gridIndex in gridIndexes)
                {
                    var gridLocation = gridIndex.Multiply(Icons.Icon.RawGrid.IntegerMap.Size);
                    var gridIcon = gridScanner.QuickRead(integerMap, gridLocation);
                    if (gridIcon == null) throw new GameScanException("Scan grid failed");

                    var gridValue = MapGrid(gridIcon);
                    Grids[gridIndex.X, gridIndex.Y] = gridValue;
                }
            }
            readLog.Dispose();
        }
        private Grid MapGrid(Icons.Icon icon)
        {
            if (icon == Icons.Icon.RawGrid)
            {
                return Grid.Raw;
            }
            else if (icon == Icons.Icon.EmptyGrid)
            {
                return Grid.Empty;
            }
            else if (icon == Icons.Icon.Number1Grid)
            {
                return Grid.Number1;
            }
            else if (icon == Icons.Icon.Number2Grid)
            {
                return Grid.Number2;
            }
            else if (icon == Icons.Icon.Number3Grid)
            {
                return Grid.Number3;
            }
            else if (icon == Icons.Icon.Number4Grid)
            {
                return Grid.Number4;
            }
            else if (icon == Icons.Icon.Number5Grid)
            {
                return Grid.Number5;
            }
            else if (icon == Icons.Icon.Number6Grid)
            {
                return Grid.Number6;
            }
            else if (icon == Icons.Icon.Number7Grid)
            {
                return Grid.Number7;
            }
            else if (icon == Icons.Icon.Number8Grid)
            {
                return Grid.Number8;
            }
            else if (icon == Icons.Icon.FlagGrid)
            {
                return Grid.Flag;
            }
            else if (icon == Icons.Icon.FlagGrid)
            {
                return Grid.Flag;
            }
            else if (icon == Icons.Icon.QuestionGrid)
            {
                return Grid.Question;
            }
            else if (icon == Icons.Icon.BombGrid)
            {
                return Grid.Bomb;
            }
            else if (icon == Icons.Icon.RedBombGrid)
            {
                return Grid.Bomb;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public Point CheckGrid(Point gridIndex)
        {
            if (GridRectangle == Rectangle.Empty) throw new InvalidOperationException();
            if (Grids == null) throw new InvalidOperationException();
            if (Grids.GetSize().Contains(gridIndex) == false) throw new ArgumentException();

            return ClickGrid(gridIndex, MouseButton.Left);
        }
        public Point FlagGrid(Point gridIndex)
        {
            if (GridRectangle == Rectangle.Empty) throw new InvalidOperationException();
            if (Grids == null) throw new InvalidOperationException();
            if (Grids.GetSize().Contains(gridIndex) == false) throw new ArgumentException();

            return ClickGrid(gridIndex, MouseButton.Right);
        }
        private Point ClickGrid(Point gridIndex, MouseButton mouseButton)
        {
            var gridLocation = GridRectangle.Location.Add(gridIndex.Multiply(Icons.Icon.RawGrid.IntegerMap.Size));
            var gridCenter = new Rectangle(gridLocation, Icons.Icon.RawGrid.IntegerMap.Size).Center();

            Click(gridCenter, mouseButton);

            return gridCenter;
        }

        public Rectangle FaceRectangle { get; private set; }
        public Face Face { get; private set; }
        private IconScanner faceScanner;
        public void QuickScanFace()
        {
            if (FaceRectangle == Rectangle.Empty) throw new InvalidOperationException();

            var captureLog = TimeLog.Stopwatch("Capture Face");
            var bitmap = GetCapture(FaceRectangle);
            captureLog.Dispose();

            var readLog = TimeLog.Stopwatch("Read Face");
            using (var integerMap = new IntegerMap(bitmap))
            {
                var faceIcon = faceScanner.QuickRead(integerMap, Point.Empty);
                if (faceIcon == null) throw new GameScanException("Scan face failed");

                Face = MapFace(faceIcon);
            }
            readLog.Dispose();
        }
        private Face MapFace(Icons.Icon faceIcon)
        {
            if (faceIcon == Icons.Icon.NormalFace)
            {
                return Face.Normal;
            }
            else if (faceIcon == Icons.Icon.WinFace)
            {
                return Face.Win;
            }
            else if (faceIcon == Icons.Icon.LoseFace)
            {
                return Face.Lose;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public Point ClickFace()
        {
            if (FaceRectangle == Rectangle.Empty) throw new InvalidOperationException();

            var point = FaceRectangle.Center();

            Click(point, MouseButton.Left);

            return point;
        }

        public Rectangle[] BombCountRectangles { get; private set; }
        public int BombCount { get; private set; }
        private IconScanner bombCountScanner;
        public void QuickScanBombCount()
        {
            if (BombCountRectangles == null) throw new InvalidOperationException();

            var captureLog = TimeLog.Stopwatch("Capture Bomb Count");
            var bitmaps = BombCountRectangles.Select(GetCapture).ToArray();
            captureLog.Dispose();

            var readLog = TimeLog.Stopwatch("Read Face");
            var bombCountDigits = bitmaps.Select(bitmap =>
            {
                using (var integerMap = new IntegerMap(bitmap))
                {
                    var bombCountIcon = bombCountScanner.QuickRead(integerMap, Point.Empty);
                    if (bombCountIcon == null) throw new GameScanException("Scan bomb count failed");

                    return MapBombCount(bombCountIcon);
                }
            });
            BombCount = bombCountDigits.Aggregate((total, curr) => total * 10 + curr);
            readLog.Dispose();
        }
        private int MapBombCount(Icons.Icon bombCountIcon)
        {
            if (bombCountIcon == Icons.Icon.Count0)
            {
                return 0;
            }
            else if (bombCountIcon == Icons.Icon.Count1)
            {
                return 1;
            }
            else if (bombCountIcon == Icons.Icon.Count2)
            {
                return 2;
            }
            else if (bombCountIcon == Icons.Icon.Count3)
            {
                return 3;
            }
            else if (bombCountIcon == Icons.Icon.Count4)
            {
                return 4;
            }
            else if (bombCountIcon == Icons.Icon.Count5)
            {
                return 5;
            }
            else if (bombCountIcon == Icons.Icon.Count6)
            {
                return 6;
            }
            else if (bombCountIcon == Icons.Icon.Count7)
            {
                return 7;
            }
            else if (bombCountIcon == Icons.Icon.Count8)
            {
                return 8;
            }
            else if (bombCountIcon == Icons.Icon.Count9)
            {
                return 9;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
