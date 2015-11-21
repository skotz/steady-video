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
            List<Rectangle> runningCrop = new List<Rectangle>();
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

                runningCrop.Add(new Rectangle(-pointsAdjusted[i - 1].X + cropUL.X, -pointsAdjusted[i - 1].Y + cropUL.Y, cropBR.X - cropUL.X, cropBR.Y - cropUL.Y));

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
                Bitmap originalFrame = (Bitmap)Bitmap.FromFile(images[i]);

                Rectangle subImageRec = new Rectangle(-pointsAdjusted[i - 1].X + cropUL.X, -pointsAdjusted[i - 1].Y + cropUL.Y, cropBR.X - cropUL.X, cropBR.Y - cropUL.Y);

                Bitmap finalFrame;
                switch (mode)
                {
                    case ImageMergerMode.Crop:
                        finalFrame = CropFrame(originalFrame, subImageRec);
                        break;
                    case ImageMergerMode.Center:
                        finalFrame = CenterFrame(originalFrame, pointsAdjusted[i - 1]);
                        break;
                    case ImageMergerMode.Outline:
                        finalFrame = OutlineFrame(originalFrame, subImageRec, runningCrop[i - 1]);
                        break;
                    case ImageMergerMode.Fill:
                        finalFrame = FillFrame(originalFrame, subImageRec);
                        break;
                    default:
                        throw new ArgumentException("ImageMergerMode not specified!");
                }

                finalFrame.Save(folder + "\\frame" + i.ToString().PadLeft(6, '0') + ".bmp", ImageFormat.Bmp);

                if (OnNextStitch != null)
                {
                    OnNextStitch(next++);
                }
            });
        }

        private static Bitmap CropFrame(Bitmap originalFrame, Rectangle subImageRec)
        {
            Bitmap finalFrame = new Bitmap(subImageRec.Width, subImageRec.Height);
            using (Graphics g = Graphics.FromImage(finalFrame))
            {
                g.DrawImage(originalFrame, new Rectangle(new Point(0, 0), subImageRec.Size), subImageRec, GraphicsUnit.Pixel);
            }
            return finalFrame;
        }

        private static Bitmap CenterFrame( Bitmap originalFrame, Point adjustedPoint)
        {
            Bitmap finalFrame = new Bitmap(originalFrame.Width, originalFrame.Height);
            using (Graphics g = Graphics.FromImage(finalFrame))
            {
                g.DrawImage(originalFrame, new Rectangle(adjustedPoint, originalFrame.Size), new Rectangle(new Point(0, 0), originalFrame.Size), GraphicsUnit.Pixel);
            }
            return finalFrame;
        }

        private static Bitmap OutlineFrame(Bitmap originalFrame, Rectangle crop, Rectangle runningCrop, bool showRunningCrop = true, bool showAspectCrop = true)
        {
            Bitmap finalFrame = originalFrame;
            int lineThickness = 3;
            Rectangle cropOutline = new Rectangle(crop.X, crop.Y, crop.Width - lineThickness, crop.Height - lineThickness);
            Rectangle runningCropOutline = new Rectangle(runningCrop.X, runningCrop.Y, runningCrop.Width - lineThickness, runningCrop.Height - lineThickness);

            double desiredRatio = 1920.0 / 1080.0;
            double actualRatio = (double)crop.Width / (double)crop.Height;
            Rectangle ratioOutline = new Rectangle();

            if (actualRatio > desiredRatio)
            {
                // Wider than desired, scale to height
                ratioOutline.Height = crop.Height;
                ratioOutline.Width = (int)(ratioOutline.Height * desiredRatio);
                ratioOutline.X = crop.X + crop.Width / 2 - ratioOutline.Width / 2;
                ratioOutline.Y = crop.Y;
                ratioOutline.Height -= lineThickness;
                ratioOutline.Width -= lineThickness;
            }
            else
            {
                // Taller than desired, scale to width
                ratioOutline.Width = crop.Width;
                ratioOutline.Height = (int)(ratioOutline.Width / desiredRatio);
                ratioOutline.X = crop.X;
                ratioOutline.Y = crop.Y + crop.Height / 2 - ratioOutline.Height / 2;
                ratioOutline.Height -= lineThickness;
                ratioOutline.Width -= lineThickness;
            }

            using (Graphics g = Graphics.FromImage(finalFrame))
            {
                g.DrawRectangle(new Pen(Brushes.Yellow, 3.0f), runningCropOutline);
                g.DrawRectangle(new Pen(Brushes.Red, 3.0f), cropOutline);
                g.DrawRectangle(new Pen(Brushes.Lime, 3.0f), ratioOutline);
            }

            return finalFrame;
        }

        private static Bitmap FillFrame(Bitmap originalFrame, Rectangle subImageRec)
        {
            Bitmap finalFrame = new Bitmap(originalFrame.Width, originalFrame.Height);
            Bitmap crop = new Bitmap(subImageRec.Width, subImageRec.Height);
            using (Graphics g = Graphics.FromImage(crop))
            {
                g.DrawImage(originalFrame, new Rectangle(new Point(0, 0), subImageRec.Size), subImageRec, GraphicsUnit.Pixel);
            }
            Rectangle source = new Rectangle(0, 0, crop.Width, crop.Height);
            double ratio = (double)crop.Width / (double)crop.Height;
            int newHeight = originalFrame.Height;
            int newWidth = (int)(newHeight * ratio);
            if (newWidth < originalFrame.Width)
            {
                ratio = (double)crop.Height / (double)crop.Width;
                newWidth = originalFrame.Width;
                newHeight = (int)(newWidth * ratio);
            }
            Rectangle dest = new Rectangle(originalFrame.Width / 2 - newWidth / 2, originalFrame.Height / 2 - newHeight / 2, newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(finalFrame))
            {
                g.DrawImage(crop, dest, source, GraphicsUnit.Pixel);
            }
            return finalFrame;
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