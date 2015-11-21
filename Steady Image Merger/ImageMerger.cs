using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Drawing.Imaging;

namespace Steady_Image_Merger
{
    class ImageMerger
    {
        public delegate void GetRelativeHandler(int num);
        public static event GetRelativeHandler OnNextRelative;

        public delegate void GetPointHandler(int num);
        public static event GetPointHandler OnNextPoint;

        public delegate void GetStitchHandler(int num);
        public static event GetStitchHandler OnNextStitch;

        public static void AlignImages(List<string> images, string folder, bool crop = true, bool centered = true, bool outline = false)
        {
            Size final = new Size(1920, 1080);
            //Size final = new Size(480, 360);

            Bitmap primary = (Bitmap)Bitmap.FromFile(images[0]);
            Point prev = new Point(0, 0);

            if (Directory.Exists(folder))
            {
                Helper.DeleteDirectory(folder);
            }
            
            List<Point> points = new List<Point>();
            for (int i = 1; i < images.Count; i++)
            {
                points.Add(new Point(0, 0));
            }

            int next = 1;
            Parallel.For(1, images.Count, i =>
            {
                Bitmap betaPseudo = (Bitmap)Bitmap.FromFile(images[i - 1]);
                Bitmap alphaPseudo = (Bitmap)Bitmap.FromFile(images[i]);
                
                points[i - 1] = GetRelativePosition(betaPseudo, alphaPseudo);

                if (OnNextPoint != null)
                {
                    OnNextPoint(next++);
                }
            } );

            Point cropUL = new Point(0, 0);
            Point cropBR = new Point(primary.Width, primary.Height);
            List<Point> pointsAdjusted = new List<Point>();

            for (int i = 1; i < images.Count; i++)
            {
                Point p = points[i - 1];
                prev = new Point(prev.X + p.X, prev.Y + p.Y);
                pointsAdjusted.Add(prev);

                cropUL.X = Math.Max(cropUL.X, prev.X);
                cropUL.Y = Math.Max(cropUL.Y, prev.Y);
                cropBR.X = Math.Min(cropBR.X, prev.X + primary.Width);
                cropBR.Y = Math.Min(cropBR.Y, prev.Y + primary.Height);

                if (OnNextRelative != null)
                {
                    OnNextRelative(i);
                }
            }
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            next = 1;
            Parallel.For(1, images.Count, i =>
            {
                // Store previous
                Bitmap alpha2 = (Bitmap)Bitmap.FromFile(images[i]);

                Rectangle subImageRec = new Rectangle(-pointsAdjusted[i - 1].X + cropUL.X, -pointsAdjusted[i - 1].Y + cropUL.Y, cropBR.X - cropUL.X, cropBR.Y - cropUL.Y);

                Bitmap b;
                if (centered)
                {
                    if (crop)
                    {
                        b = CreateNewBitmap(alpha2, subImageRec);
                    }
                    else
                    {
                        b = CreateNewBitmap(alpha2, pointsAdjusted[i - 1]);
                    }
                }
                else
                {
                    b = alpha2;

                    if (outline)
                    {
                        subImageRec.Width = subImageRec.Width - 1;
                        subImageRec.Height = subImageRec.Height - 1;
                        
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawRectangle(Pens.Red, subImageRec);
                        }
                    }
                }
                b.Save(folder + "\\frame" + i.ToString().PadLeft(6, '0') + ".bmp", ImageFormat.Bmp);

                if (OnNextStitch != null)
                {
                    OnNextStitch(next++);
                }
            });
        }

        private static Bitmap CreateNewBitmap(Bitmap original, Point relative, Size start)
        {
            Bitmap b = new Bitmap(start.Width, start.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                //g.Clear(Color.White);
                g.DrawImage(original, new Rectangle(relative, original.Size));
            }
            return b;
        }

        private static Bitmap CreateNewBitmap(Bitmap original, Rectangle area)
        {
            Bitmap b = new Bitmap(area.Width, area.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(original, new Rectangle(new Point(0, 0), area.Size), area, GraphicsUnit.Pixel);
            }
            return b;
        }

        private static Bitmap CreateNewBitmap(Bitmap original, Point adjusted)
        {
            Bitmap b = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(original, new Rectangle(adjusted, original.Size), new Rectangle(new Point(0, 0), original.Size), GraphicsUnit.Pixel);
            }
            return b;
        }

        private static Point GetRelativePosition(Bitmap image1, Bitmap image2)
        {
            double bestDistance = double.MaxValue;
            Point best = new Point(0, 0);

            int[] refinements = new int[] { 200, 40, 6, 1 };
            
            for (int r = 0; r < refinements.Length - 1; r++)
            {
                for (int x = best.X - refinements[r]; x < best.X + refinements[r]; x += refinements[r + 1])
                {
                    for (int y = best.Y - refinements[r]; y < best.Y + refinements[r]; y += refinements[r + 1])
                    {
                        double distance = CalculateDistance(image1, image2, x, y);

                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            best.X = x;
                            best.Y = y;
                        }
                    }
                }
            }

            best = new Point(-best.X, -best.Y);

            return best;
        }

        private static double CalculateDistance(Bitmap a, Bitmap b, int x, int y)
        {
            BitmapData aData = a.LockBits(new Rectangle(0, 0, a.Width, a.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int aStride = aData.Stride;
            IntPtr aScan = aData.Scan0;
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bStride = bData.Stride;
            IntPtr bScan = bData.Scan0;

            double dist = 0.0;
            double count = 0.0;

            unsafe
            {
                byte* ap = (byte*)(void*)aScan;
                byte* bp = (byte*)(void*)bScan;
                int aOffset = aStride - a.Width * 3;
                int bOffset = bStride - b.Width * 3;
                
                for (int x1 = 0; x1 < a.Width && x1 + x >= 0 && x1 + x < b.Width; x1 += 25)
                {
                    for (int y1 = 0; y1 < a.Height && y1 + y >= 0 && y1 + y < b.Height; y1 += 25)
                    {
                        int x2 = x1 + x;
                        int y2 = y1 + y;

                        dist += (ap[(x1 * 3) + y1 * aStride] - bp[(x2 * 3) + y2 * bStride]) * (ap[(x1 * 3) + y1 * aStride] - bp[(x2 * 3) + y2 * bStride]) +
                                (ap[(x1 * 3) + y1 * aStride + 1] - bp[(x2 * 3) + y2 * bStride + 1]) * (ap[(x1 * 3) + y1 * aStride + 1] - bp[(x2 * 3) + y2 * bStride + 1]) +
                                (ap[(x1 * 3) + y1 * aStride + 2] - bp[(x2 * 3) + y2 * bStride + 2]) * (ap[(x1 * 3) + y1 * aStride + 2] - bp[(x2 * 3) + y2 * bStride + 2]);

                        count += 3;
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);

            return Math.Sqrt(dist / count);
        }
    }
}