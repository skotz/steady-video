using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Steady_Image_Merger
{
    public partial class Form1 : Form
    {
        private BackgroundWorker worker;
        private Stopwatch sw;
        private string lastStatus;
        List<string> frames;

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

        private void ImageMerger_OnNextStitch(int num)
        {
            ReportProgress(num, "Stitching Images...");
        }

        private void ImageMerger_OnNextRelative(int num)
        {
            ReportProgress(num, "Finding Bounds...");
        }

        private void ImageMerger_OnNextPoint(int num)
        {
            ReportProgress(num, "Calculating Overlays...");
        }

        private void ReportProgress(int num, string message)
        {
            int percentage = 0;
            if (frames != null) 
            {
                percentage = (int)(((double)num / (double)frames.Count) * 100.0);
            }
            worker.ReportProgress(percentage, new Progress() { Number = (double)num, Status = message });
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
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

                if (!double.IsNaN(remaining))
                {
                    TimeSpan left = TimeSpan.FromMilliseconds(remaining);

                    double fps = p.Number / ((double)sw.ElapsedMilliseconds / 1000.0);

                    label1.Text = p.Status + " Remaining: " + left.ToFriendlyString() + " @ " + fps.ToString("0.00") + " fps";
                }
                else
                {
                    label1.Text = p.Status;
                }

                progressBar1.Value = e.ProgressPercentage;
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private void ImageMerger_OnNextSet(int num)
        {
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            sw.Start();

            ReportProgress(1, "Extracting Frames...");

            ExtractImages(openFileDialog1.FileName);

            frames = Directory.GetFiles("images", "*.bmp").ToList();

            ImageMerger.AlignImages(frames, "processed", cbCrop.Checked, cbCenter.Checked, cbOutline.Checked);

            ReportProgress(99, "Stitching Frames...");

            StitchImages(openFileDialog1.FileName);

            ReportProgress(100, "Done");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult d = openFileDialog1.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.OK)
            {
                DialogResult d2 = saveFileDialog1.ShowDialog();
                if (d2 == System.Windows.Forms.DialogResult.OK)
                {
                    button1.Enabled = false;
                    cbCenter.Enabled = false;
                    cbFit.Enabled = false;
                    cbCrop.Enabled = false;
                    cbOutline.Enabled = false;
                    sw = new Stopwatch();
                    worker.RunWorkerAsync();
                }
            }
        }

        private void StitchImages(string originalvideo)
        {
            Process process = new Process();
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            process.StartInfo.FileName = "ffmpeg.exe";
            process.StartInfo.Arguments = @"-i .\processed\frame%06d.bmp -y -vcodec mpeg4 """ + saveFileDialog1.FileName + @"""";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            File.WriteAllText("ffmpeg.log", process.StandardError.ReadToEnd());

            process.WaitForExit();
        }

        private void ExtractImages(string video)
        {
            if (File.Exists("images\\done"))
            {
                string[] parts = File.ReadAllText("images\\done").Split('|');
                if (parts.Length == 2 && parts[0] == video && parts[1] == new FileInfo(video).Length.ToString())
                {
                    return;
                }
            }

            if (Directory.Exists("images"))
            {
                Helper.DeleteDirectory("images");
            }
            Directory.CreateDirectory("images");

            Process process = new Process();
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            process.StartInfo.FileName = "ffmpeg.exe";
            process.StartInfo.Arguments = @"-i """ + video + @""" -r 30 .\images\frame%06d.bmp";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            File.WriteAllText("ffmpeg.log", process.StandardError.ReadToEnd());

            process.WaitForExit();

            File.WriteAllText("images\\done", video + "|" + new FileInfo(video).Length);
        }

        private void cbCenter_CheckedChanged(object sender, EventArgs e)
        {
            cbOutline.Checked = !cbCenter.Checked;
        }

        private void cbOutline_CheckedChanged(object sender, EventArgs e)
        {
            cbCenter.Checked = !cbOutline.Checked;
        }
    }

    public class Progress
    {
        public double Number { get; set; }

        public string Status { get; set; }
    }
}