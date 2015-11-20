namespace Steady_Image_Merger
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cbCrop = new System.Windows.Forms.CheckBox();
            this.cbCenter = new System.Windows.Forms.CheckBox();
            this.cbFit = new System.Windows.Forms.CheckBox();
            this.cbOutline = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Go";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "video.mp4";
            this.openFileDialog1.Filter = "All Files|*.*";
            this.openFileDialog1.Title = "Select a Video";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(93, 12);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(547, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(90, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Ready";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileName = "finished.avi";
            this.saveFileDialog1.Filter = "AVI Movies|*.avi";
            this.saveFileDialog1.Title = "Save Finished Movie As";
            // 
            // cbCrop
            // 
            this.cbCrop.AutoSize = true;
            this.cbCrop.Checked = true;
            this.cbCrop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCrop.Location = new System.Drawing.Point(12, 57);
            this.cbCrop.Name = "cbCrop";
            this.cbCrop.Size = new System.Drawing.Size(48, 17);
            this.cbCrop.TabIndex = 3;
            this.cbCrop.Text = "Crop";
            this.cbCrop.UseVisualStyleBackColor = true;
            // 
            // cbCenter
            // 
            this.cbCenter.AutoSize = true;
            this.cbCenter.Checked = true;
            this.cbCenter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCenter.Location = new System.Drawing.Point(110, 57);
            this.cbCenter.Name = "cbCenter";
            this.cbCenter.Size = new System.Drawing.Size(57, 17);
            this.cbCenter.TabIndex = 4;
            this.cbCenter.Text = "Center";
            this.cbCenter.UseVisualStyleBackColor = true;
            this.cbCenter.CheckedChanged += new System.EventHandler(this.cbCenter_CheckedChanged);
            // 
            // cbFit
            // 
            this.cbFit.AutoSize = true;
            this.cbFit.Location = new System.Drawing.Point(66, 57);
            this.cbFit.Name = "cbFit";
            this.cbFit.Size = new System.Drawing.Size(38, 17);
            this.cbFit.TabIndex = 5;
            this.cbFit.Text = "Fill";
            this.cbFit.UseVisualStyleBackColor = true;
            // 
            // cbOutline
            // 
            this.cbOutline.AutoSize = true;
            this.cbOutline.Location = new System.Drawing.Point(173, 57);
            this.cbOutline.Name = "cbOutline";
            this.cbOutline.Size = new System.Drawing.Size(59, 17);
            this.cbOutline.TabIndex = 6;
            this.cbOutline.Text = "Outline";
            this.cbOutline.UseVisualStyleBackColor = true;
            this.cbOutline.CheckedChanged += new System.EventHandler(this.cbOutline_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 86);
            this.Controls.Add(this.cbOutline);
            this.Controls.Add(this.cbFit);
            this.Controls.Add(this.cbCenter);
            this.Controls.Add(this.cbCrop);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Steady Image Merger";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox cbCrop;
        private System.Windows.Forms.CheckBox cbCenter;
        private System.Windows.Forms.CheckBox cbFit;
        private System.Windows.Forms.CheckBox cbOutline;
    }
}

