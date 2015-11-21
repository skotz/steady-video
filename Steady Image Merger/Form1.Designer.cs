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
            this.rbCrop = new System.Windows.Forms.RadioButton();
            this.rbFill = new System.Windows.Forms.RadioButton();
            this.rbOutline = new System.Windows.Forms.RadioButton();
            this.rbCenter = new System.Windows.Forms.RadioButton();
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
            // rbCrop
            // 
            this.rbCrop.AutoSize = true;
            this.rbCrop.Location = new System.Drawing.Point(12, 57);
            this.rbCrop.Name = "rbCrop";
            this.rbCrop.Size = new System.Drawing.Size(47, 17);
            this.rbCrop.TabIndex = 7;
            this.rbCrop.Text = "Crop";
            this.rbCrop.UseVisualStyleBackColor = true;
            // 
            // rbFill
            // 
            this.rbFill.AutoSize = true;
            this.rbFill.Checked = true;
            this.rbFill.Location = new System.Drawing.Point(65, 57);
            this.rbFill.Name = "rbFill";
            this.rbFill.Size = new System.Drawing.Size(37, 17);
            this.rbFill.TabIndex = 8;
            this.rbFill.TabStop = true;
            this.rbFill.Text = "Fill";
            this.rbFill.UseVisualStyleBackColor = true;
            // 
            // rbOutline
            // 
            this.rbOutline.AutoSize = true;
            this.rbOutline.Location = new System.Drawing.Point(108, 57);
            this.rbOutline.Name = "rbOutline";
            this.rbOutline.Size = new System.Drawing.Size(58, 17);
            this.rbOutline.TabIndex = 9;
            this.rbOutline.Text = "Outline";
            this.rbOutline.UseVisualStyleBackColor = true;
            // 
            // rbCenter
            // 
            this.rbCenter.AutoSize = true;
            this.rbCenter.Location = new System.Drawing.Point(172, 57);
            this.rbCenter.Name = "rbCenter";
            this.rbCenter.Size = new System.Drawing.Size(56, 17);
            this.rbCenter.TabIndex = 10;
            this.rbCenter.Text = "Center";
            this.rbCenter.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 86);
            this.Controls.Add(this.rbCenter);
            this.Controls.Add(this.rbOutline);
            this.Controls.Add(this.rbFill);
            this.Controls.Add(this.rbCrop);
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
        private System.Windows.Forms.RadioButton rbCrop;
        private System.Windows.Forms.RadioButton rbFill;
        private System.Windows.Forms.RadioButton rbOutline;
        private System.Windows.Forms.RadioButton rbCenter;
    }
}

