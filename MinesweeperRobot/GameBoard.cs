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
                Icons.Icon.FlagGrid, Icons.Icon.BombGrid, Icons.Icon.RedBombGrid);

            faceScanner = new IconScanner(Icons.Icon.NormalFace, Icons.Icon.WinFace, Icons.Icon.LoseFace);
        }
        public readonly IntPtr WindowHandle;
        public Rectangle WindowRectangle { get; private set; }

        public Rectangle GridRectangle { get; private set; }
        public Grid[,] Grids { get; private set; }
        private IconScanner gridScanner;

        public Rectangle FaceRectangle { get; private set; }
        public Face Face { get; private set; }
        private IconScanner faceScanner;

        public void FullScanWindow()
        {
            if (Window.GetForegroundWindow() != WindowHandle)
            {
                Window.SetForegroundWindow(WindowHandle);
                Thread.Sleep(100);
            }

            WindowRectangle = Window.GetRectangle(WindowHandle);

            var bitmap = CaptureScreen(WindowRectangle);
            bitmap.Save("window.png", ImageFormat.Png);
            using (var integerMap = new IntegerMap(bitmap))
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
            }
        }
        private Bitmap CaptureScreen(Rectangle rectangle)
        {
            var bitmap = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(rectangle.Location, Point.Empty, bitmap.Size);
            }

            return bitmap;
        }

        public void QuickScanGrids()
        {
            if (GridRectangle == Rectangle.Empty) throw new InvalidOperationException();
            if (Grids == null) throw new InvalidOperationException();

            var bitmap = CaptureScreen(GridRectangle);
            bitmap.Save("grids.png", ImageFormat.Png);
            using (var integerMap = new IntegerMap(bitmap))
            {
                var gridCountX = Grids.GetLength(0);
                var gridCountY = Grids.GetLength(1);
                var gridIndexes = EnumerableUtil.Rectangle(gridCountX, gridCountY);
                foreach (var gridIndex in gridIndexes)
                {
                    var gridLocation = gridIndex.Multiply(Icons.Icon.RawGrid.IntegerMap.Size);
                    var gridIcon = gridScanner.QuickRead(integerMap, gridLocation);

                    var gridValue = MapGrid(gridIcon);
                    Grids[gridIndex.X, gridIndex.Y] = gridValue;
                }
            }
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
        public void ClickGrid(Point gridIndex)
        {
            if (GridRectangle == Rectangle.Empty) throw new InvalidOperationException();
            if (Grids == null) throw new InvalidOperationException();

            var point = GridRectangle.Location.Add(new Point(
                (int)((gridIndex.X + .5) * Icons.Icon.RawGrid.IntegerMap.Size.Width),
                (int)((gridIndex.Y + .5) * Icons.Icon.RawGrid.IntegerMap.Size.Height)));

            Click(point);
        }

        public void QuickScanFace()
        {
            if (FaceRectangle == Rectangle.Empty) throw new InvalidOperationException();

            var bitmap = CaptureScreen(FaceRectangle);
            bitmap.Save("face.png", ImageFormat.Png);
            using (var integerMap = new IntegerMap(bitmap))
            {
                var faceIcon = faceScanner.QuickRead(integerMap, Point.Empty);
                Face = MapFace(faceIcon);
            }
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
        public void ClickFace()
        {
            if (FaceRectangle == Rectangle.Empty) throw new InvalidOperationException();

            var point = new Point(
                FaceRectangle.X + FaceRectangle.Width / 2,
                FaceRectangle.Y + FaceRectangle.Height / 2);

            Click(point);
        }

        private void Click(Point point)
        {
            Mouse.MoveTo(point);
            Thread.Sleep(1);

            Mouse.MouseDown(MouseButton.Left);
            Thread.Sleep(1);

            Mouse.MouseUp(MouseButton.Left);
        }
    }
}
