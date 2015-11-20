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

            List<BitmapPseudo> pseudo = new List<BitmapPseudo>();
            for (int i = 0; i < images.Count; i++)
            {
                pseudo.Add(new BitmapPseudo());
            }

            //Parallel.For(0, images.Count, i =>
            //{
            //    pseudo[i] = new BitmapPseudo((Bitmap)Bitmap.FromFile(images[i]));
            //});

            //Bitmap alpha = (Bitmap)Bitmap.FromFile(images[0]);
            ////Bitmap beta;
            //BitmapPseudo alphaPseudo = new BitmapPseudo(alpha);
            //BitmapPseudo betaPseudo;

            List<Point> points = new List<Point>();
            for (int i = 1; i < images.Count; i++)
            {
                points.Add(new Point(0, 0));
            }

            int next = 1;
            Parallel.For(1, images.Count, i =>
            {
                bool firstPresent = true;
                bool secondPresent = true;
                if (pseudo[i].Width == 0)
                {
                    firstPresent = false;
                    pseudo[i] = new BitmapPseudo((Bitmap)Bitmap.FromFile(images[i]));
                }
                if (pseudo[i - 1].Width == 0)
                {
                    secondPresent = false;
                    pseudo[i - 1] = new BitmapPseudo((Bitmap)Bitmap.FromFile(images[i - 1]));
                }

                //BitmapPseudo betaPseudo = new BitmapPseudo((Bitmap)Bitmap.FromFile(images[i - 1]));
                //BitmapPseudo alphaPseudo = new BitmapPseudo((Bitmap)Bitmap.FromFile(images[i]));

                BitmapPseudo betaPseudo = pseudo[i - 1];
                BitmapPseudo alphaPseudo = pseudo[i];

                points[i - 1] = GetRelativePosition(betaPseudo, alphaPseudo);

                if (OnNextPoint != null)
                {
                    OnNextPoint(next++);
                }

                // We only need each index twice, so if it existed upon starting this iteration we can delete it
                if (firstPresent)
                {
                    pseudo[i] = new BitmapPseudo();
                }
                if (secondPresent)
                {
                    pseudo[i - 1] = new BitmapPseudo();
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

            //if (crop)
            //{
            //    CreateNewBitmap(primary, new Rectangle(cropUL.X, cropUL.Y, cropBR.X - cropUL.X, cropBR.Y - cropUL.Y)).Save("0000.png");
            //}
            //else
            //{
            //    CreateNewBitmap(primary, new Point(0, 0), final).Save("0000.png");
            //}

            // Not sure (yet) why this was a problem
            int attempts = 10;
            while (!Directory.Exists(folder) && attempts-- > 0)
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

        private static Point GetRelativePosition(BitmapPseudo image1, BitmapPseudo image2)
        {
            double bestDistance = double.MaxValue;
            Point best = new Point(0, 0);
            //Semaphore s = new Semaphore(1, 1);

            int refine = 40;

            //List<int> exes = new List<int>();
            //for (int x = -500; x < 500; x += refine)
            //{
            //    exes.Add(x);
            //}

            //Parallel.ForEach(exes, x =>
            for (int x = -200; x < 200; x += refine)
            {
                for (int y = -200; y < 200; y += refine)
                {
                    double distance = CalculateDistance(image1, image2, x, y);

                    //s.WaitOne();
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best.X = x;
                        best.Y = y; // = new Point(-x, -y);
                    }
                    //s.Release();
                }
            }//);

            //exes = new List<int>();
            //for (int x = -best.X - refine; x < -best.X + refine; x += 6)
            //{
            //    exes.Add(x);
            //}

            //Parallel.ForEach(exes, x =>
            for (int x = best.X - refine; x < best.X + refine; x += 6)
            {
                for (int y = best.Y - refine; y < best.Y + refine; y += 6)
                {
                    double distance = CalculateDistance(image1, image2, x, y);

                    //s.WaitOne();
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best.X = x;
                        best.Y = y; // = new Point(-x, -y);
                    }
                    //s.Release();
                }
            }//);

            refine = 6;
            for (int x = best.X - refine; x < best.X + refine; x += 1)
            {
                for (int y = best.Y - refine; y < best.Y + refine; y += 1)
                {
                    double distance = CalculateDistance(image1, image2, x, y);

                    //s.WaitOne();
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best.X = -x;
                        best.Y = -y; // = new Point(-x, -y);
                    }
                    //s.Release();
                }
            }

            return best;
        }

        private static Point GetRelativePosition(Bitmap image1, Bitmap image2)
        {
            double bestDistance = double.MaxValue;
            Point best = new Point(0, 0);

            Parallel.For(-1000, 1000, x =>
            {
                for (int y = -1000; y < 1000; y++)
                {
                    double distance = CalculateDistance(image1, image2, x, y);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = new Point(x, y);
                    }
                }
            });

            return best;
        }

        private static double CalculateDistance(BitmapPseudo a, BitmapPseudo b, int x, int y)
        {
            double dist = 0.0;
            double count = 0.0;
            for (int x1 = 0; x1 < a.Width && x1 + x >= 0 && x1 + x < b.Width; x1 += 25)
            {
                for (int y1 = 0; y1 < a.Height && y1 + y >= 0 && y1 + y < b.Height; y1 += 25)
                {
                    int x2 = x1 + x;
                    int y2 = y1 + y;

                    dist += (a.Image[x1, y1][0] - b.Image[x2, y2][0]) * (a.Image[x1, y1][0] - b.Image[x2, y2][0]) +
                            (a.Image[x1, y1][1] - b.Image[x2, y2][1]) * (a.Image[x1, y1][1] - b.Image[x2, y2][1]) +
                            (a.Image[x1, y1][2] - b.Image[x2, y2][2]) * (a.Image[x1, y1][2] - b.Image[x2, y2][2]);

                    count += 3;
                }
            }

            return Math.Sqrt(dist / count);
        }

        private static double CalculateDistance(Bitmap a, Bitmap b, int x, int y)
        {
            double dist = 0.0;
            double count = 0.0;
            for (int i = 0; i < b.Width + a.Width; i++)
            {
                for (int j = 0; j < b.Height + a.Height; j++)
                {
                    int x1 = i;
                    int y1 = j;
                    if (x1 >= 0 && y1 >= 0 && x1 < a.Width && y1 < a.Height)
                    {
                        int x2 = i + x;
                        int y2 = j + y;
                        if (x2 >= 0 && y2 >= 0 && x2 < b.Width && y2 < b.Height)
                        {
                            dist += CalculateDistance(a.GetPixel(x1, y1), b.GetPixel(x2, y2));
                            count++;
                        }
                    }
                }
            }
            return Math.Sqrt(dist / count);
        }

        private static double CalculateDistance(Color a, Color b)
        {
            return Math.Sqrt((((double)a.R - b.R) * ((double)a.R - b.R) + ((double)a.G - b.G) * ((double)a.G - b.G) + ((double)a.B - b.B) * ((double)a.B - b.B)) / 3.0);
        }

        private static double CalculateDistance(byte[] a, byte[] b)
        {
            return (a[0] - b[0]) * (a[0] - b[0]) + 
                   (a[1] - b[1]) * (a[1] - b[1]) + 
                   (a[2] - b[2]) * (a[2] - b[2]);
        }
    }
}

public struct BitmapPseudo
{
    public byte[,][] Image { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public BitmapPseudo(Bitmap image)
        : this()
    {
        Width = image.Width;
        Height = image.Height;
        Image = new byte[Width, Height][];

        //for (int i = 0; i < Width; i++)
        //{
        //    for (int j = 0; j < Height; j++)
        //    {
        //        Color c = image.GetPixel(i, j);
        //        Image[i, j] = new byte[] { c.R, c.G, c.B };
        //    }
        //}

        BitmapData bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        int stride = bmData.Stride;
        IntPtr Scan0 = bmData.Scan0;

        //int[] hist = new int[image.Width];
        //for (int x = 0; x < image.Width; x++)
        //{
        //    hist[x] = 0;
        //}

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;
            int nOffset = stride - image.Width * 3;

            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    Image[x, y] = new byte[] { p[2], p[1], p[0] };

                    p += 3;
                }
                p += nOffset;
            }
        }

        image.UnlockBits(bmData);
    }
}
