using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public class IntegerMap : IDisposable
    {
        private readonly BitmapData bitmapData;
        public readonly Size Size;

        private readonly Bitmap bitmap;
        private readonly bool bitmapDisposable;

        public IntegerMap(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                this.bitmap = bitmap;
                bitmapDisposable = false;
            }
            else
            {
                this.bitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(this.bitmap))
                {
                    graphics.DrawImage(bitmap, 0, 0);
                }

                bitmapDisposable = true;
            }

            Size = this.bitmap.Size;
            var rectangle = new Rectangle(Point.Empty, Size);
            bitmapData = this.bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        }
        public void Dispose()
        {
            bitmap.UnlockBits(bitmapData);

            if (bitmapDisposable)
            {
                bitmap.Dispose();
            }
        }

        public unsafe uint this[int x, int y]
        {
            get
            {
                if (x < 0) throw new ArgumentException();
                if (x >= Size.Width) throw new ArgumentException();
                if (y < 0) throw new ArgumentException();
                if (y >= Size.Height) throw new ArgumentException();

                var pointer = (uint*)bitmapData.Scan0 + bitmapData.Width * y + x;
                return *pointer;
            }
        }
    }
}
