using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Steady_Image_Merger
{
    public partial class Form1 : Form
    {
        BackgroundWorker worker;
        Stopwatch sw;
        string lastStatus;

        public Form1()
        {
            InitializeComponent();

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            ImageMerger.OnNextPoint += new ImageMerger.GetPointHandler(ImageMerger_OnNextPoint);
            ImageMerger.OnNextRelative += new ImageMerger.GetRelativeHandler(ImageMerger_OnNextRelative);
            ImageMerger.OnNextStitch += new ImageMerger.GetStitchHandler(ImageMerger_OnNextStitch);

            lastStatus = "";
        }

        void ImageMerger_OnNextStitch(int num)
        {
            worker.ReportProgress((int)(((double)num / (double)openFileDialog1.FileNames.Length) * 100.0), new Progress() { Number = (double)num, Status = "Stitching Images..." });
        }

        void ImageMerger_OnNextRelative(int num)
        {
            worker.ReportProgress((int)(((double)num / (double)openFileDialog1.FileNames.Length) * 100.0), new Progress() { Number = (double)num, Status = "Finding Bounds..." });
        }

        void ImageMerger_OnNextPoint(int num)
        {
            worker.ReportProgress((int)(((double)num / (double)openFileDialog1.FileNames.Length) * 100.0), new Progress() { Number = (double)num, Status = "Calculating Overlays..."});
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                Progress p = (Progress)e.UserState;

                if (p.Status != lastStatus)
                {
                    lastStatus = p.Status;
                    sw.Restart();
                }

                long elapsed = sw.ElapsedMilliseconds;
                double remaining = (elapsed / (double)e.ProgressPercentage) * (100.0 - (double)e.ProgressPercentage);
                TimeSpan left = TimeSpan.FromMilliseconds(remaining);

                double fps = p.Number / ((double)sw.ElapsedMilliseconds / 1000.0);

                label1.Text = p.Status + " Remaining: " + left.ToString() + " @ " + fps.ToString("0.00") + " fps";
                progressBar1.Value = e.ProgressPercentage;
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        void ImageMerger_OnNextSet(int num)
        {
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            sw.Start();
            ImageMerger.AlignImages(openFileDialog1.FileNames.ToList(), true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //List<Bitmap> images = new List<Bitmap>();
            //images.Add((Bitmap)Bitmap.FromFile("image1.jpg"));
            //images.Add((Bitmap)Bitmap.FromFile("image2.jpg"));
            //images.Add((Bitmap)Bitmap.FromFile("image3.jpg"));

            DialogResult d = openFileDialog1.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.OK)
            {
                richTextBox1.Lines = openFileDialog1.FileNames;
                button1.Enabled = false;
                sw = new Stopwatch();
                worker.RunWorkerAsync();
            }
        }
    }

    public class Progress
    {
        public double Number { get; set; }
        public string Status { get; set; }
    }
}
