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

        public static void AlignImages(List<string> images, string folder, ImageMergerMode mode)
        {
            Size final = new Size(1920, 1080);
            //Size final = new Size(480, 360);

            Bitmap primary = (Bitmap)Bitmap.FromFile(images[0]);
            
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
                Bitmap previousImage = (Bitmap)Bitmap.FromFile(images[i - 1]);
                Bitmap nextImage = (Bitmap)Bitmap.FromFile(images[i]);
                
                points[i - 1] = GetRelativePosition(previousImage, nextImage);

                if (OnNextPoint != null)
                {
                    OnNextPoint(next++);
                }
            } );

            Point cropUL = new Point(0, 0);
            Point cropBR = new Point(primary.Width, primary.Height);
            List<Point> pointsAdjusted = new List<Point>();
            Point accumulatedOffsets = new Point(0, 0);

            for (int i = 1; i < images.Count; i++)
            {
                Point p = points[i - 1];
                accumulatedOffsets = new Point(accumulatedOffsets.X + p.X, accumulatedOffsets.Y + p.Y);
                pointsAdjusted.Add(accumulatedOffsets);

                cropUL.X = Math.Max(cropUL.X, accumulatedOffsets.X);
                cropUL.Y = Math.Max(cropUL.Y, accumulatedOffsets.Y);
                cropBR.X = Math.Min(cropBR.X, accumulatedOffsets.X + primary.Width);
                cropBR.Y = Math.Min(cropBR.Y, accumulatedOffsets.Y + primary.Height);

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
                switch (mode)
                {
                    case ImageMergerMode.Crop:
                        b = new Bitmap(subImageRec.Width, subImageRec.Height);
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawImage(alpha2, new Rectangle(new Point(0, 0), subImageRec.Size), subImageRec, GraphicsUnit.Pixel);
                        }
                        break;
                    case ImageMergerMode.Center:
                        b = new Bitmap(alpha2.Width, alpha2.Height);
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawImage(alpha2, new Rectangle(pointsAdjusted[i - 1], alpha2.Size), new Rectangle(new Point(0, 0), alpha2.Size), GraphicsUnit.Pixel);
                        }
                        break;
                    case ImageMergerMode.Outline:
                        b = alpha2;
                        subImageRec.Width = subImageRec.Width - 3;
                        subImageRec.Height = subImageRec.Height - 3;
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawRectangle(new Pen(Brushes.Red, 3.0f), subImageRec);
                        }
                        break;
                    case ImageMergerMode.Fill:
                        b = new Bitmap(alpha2.Width, alpha2.Height);
                        Bitmap crop = new Bitmap(subImageRec.Width, subImageRec.Height);
                        using (Graphics g = Graphics.FromImage(crop))
                        {
                            g.DrawImage(alpha2, new Rectangle(new Point(0, 0), subImageRec.Size), subImageRec, GraphicsUnit.Pixel);
                        }
                        Rectangle source = new Rectangle(0, 0, crop.Width, crop.Height);
                        double ratio = (double)crop.Width / (double)crop.Height;
                        int newHeight = alpha2.Height;
                        int newWidth = (int)(newHeight * ratio);
                        if (newWidth < alpha2.Width)
                        {
                            ratio = (double)crop.Height / (double)crop.Width;
                            newWidth = alpha2.Width;
                            newHeight = (int)(newWidth * ratio);
                        }
                        Rectangle dest = new Rectangle(alpha2.Width / 2 - newWidth / 2, alpha2.Height / 2 - newHeight / 2, newWidth, newHeight);
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawImage(crop, dest, source, GraphicsUnit.Pixel);
                        }
                        break;
                    default:
                        throw new ArgumentException("ImageMergerMode not specified!");
                }

                b.Save(folder + "\\frame" + i.ToString().PadLeft(6, '0') + ".bmp", ImageFormat.Bmp);

                if (OnNextStitch != null)
                {
                    OnNextStitch(next++);
                }
            });
        }

        private static Point GetRelativePosition(Bitmap image1, Bitmap image2)
        {
            double bestDistance = double.MaxValue;
            Point best = new Point(0, 0);

            int[] refinements = new int[] { 200, 20, 5, 1 };
            
            for (int r = 0; r < refinements.Length - 1; r++)
            {
                int startx = best.X - refinements[r];
                int endx = best.X + refinements[r];
                int starty = best.Y - refinements[r];
                int endy = best.Y + refinements[r];

                for (int x = startx; x < endx; x += refinements[r + 1])
                {
                    for (int y = starty; y < endy; y += refinements[r + 1])
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

                for (int x1 = 0; x1 < a.Width; x1 += 25)
                {
                    for (int y1 = 0; y1 < a.Height; y1 += 25)
                    {
                        int x2 = x1 + x;
                        int y2 = y1 + y;

                        if (x2 >= 0 && y2 >= 0 && x2 < b.Width && y2 < b.Height)
                        {
                            dist += (ap[(x1 * 3) + y1 * aStride] - bp[(x2 * 3) + y2 * bStride]) * (ap[(x1 * 3) + y1 * aStride] - bp[(x2 * 3) + y2 * bStride]) +
                                    (ap[(x1 * 3) + y1 * aStride + 1] - bp[(x2 * 3) + y2 * bStride + 1]) * (ap[(x1 * 3) + y1 * aStride + 1] - bp[(x2 * 3) + y2 * bStride + 1]) +
                                    (ap[(x1 * 3) + y1 * aStride + 2] - bp[(x2 * 3) + y2 * bStride + 2]) * (ap[(x1 * 3) + y1 * aStride + 2] - bp[(x2 * 3) + y2 * bStride + 2]);

                            count += 3;
                        }
                    }
                }
            }

            a.UnlockBits(aData);
            b.UnlockBits(bData);

            return Math.Sqrt(dist / count);
        }
    }
}