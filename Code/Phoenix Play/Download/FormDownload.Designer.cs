namespace Phoenix_Play.Download
{
    partial class FormDownload
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDownload));
            progressBarMain = new ProgressBar();
            lblTitle = new Label();
            lblPercentage = new Label();
            btnCancel = new Button();
            lblSpeed = new Label();
            lblTimeRemaining = new Label();
            panel1 = new Panel();
            pictureBoxGame = new PictureBox();
            btnClose = new Button();
            btnMinimize = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGame).BeginInit();
            SuspendLayout();
            // 
            // progressBarMain
            // 
            resources.ApplyResources(progressBarMain, "progressBarMain");
            progressBarMain.BackColor = Color.FromArgb(70, 70, 70);
            progressBarMain.ForeColor = Color.FromArgb(0, 122, 204);
            progressBarMain.MarqueeAnimationSpeed = 0;
            progressBarMain.Name = "progressBarMain";
            progressBarMain.Style = ProgressBarStyle.Continuous;
            // 
            // lblTitle
            // 
            resources.ApplyResources(lblTitle, "lblTitle");
            lblTitle.ForeColor = Color.White;
            lblTitle.Name = "lblTitle";
            // 
            // lblPercentage
            // 
            resources.ApplyResources(lblPercentage, "lblPercentage");
            lblPercentage.ForeColor = Color.LightGray;
            lblPercentage.Name = "lblPercentage";
            // 
            // btnCancel
            // 
            resources.ApplyResources(btnCancel, "btnCancel");
            btnCancel.BackColor = Color.FromArgb(70, 70, 70);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.ForeColor = Color.White;
            btnCancel.Name = "btnCancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblSpeed
            // 
            resources.ApplyResources(lblSpeed, "lblSpeed");
            lblSpeed.ForeColor = Color.LightGray;
            lblSpeed.Name = "lblSpeed";
            // 
            // lblTimeRemaining
            // 
            resources.ApplyResources(lblTimeRemaining, "lblTimeRemaining");
            lblTimeRemaining.ForeColor = Color.LightGray;
            lblTimeRemaining.Name = "lblTimeRemaining";
            // 
            // panel1
            // 
            resources.ApplyResources(panel1, "panel1");
            panel1.BackColor = Color.FromArgb(20, 20, 20);
            panel1.Controls.Add(pictureBoxGame);
            panel1.Name = "panel1";
            // 
            // pictureBoxGame
            // 
            resources.ApplyResources(pictureBoxGame, "pictureBoxGame");
            pictureBoxGame.BackColor = Color.Transparent;
            pictureBoxGame.Name = "pictureBoxGame";
            pictureBoxGame.TabStop = false;
            // 
            // btnClose
            // 
            resources.ApplyResources(btnClose, "btnClose");
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Image = Properties.Resources.pngCloseApp;
            btnClose.Name = "btnClose";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnMinimize
            // 
            resources.ApplyResources(btnMinimize, "btnMinimize");
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Image = Properties.Resources.pngMinimizzaApp;
            btnMinimize.Name = "btnMinimize";
            btnMinimize.UseVisualStyleBackColor = true;
            btnMinimize.Click += btnMinimize_Click;
            // 
            // FormDownload
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(46, 46, 46);
            Controls.Add(btnMinimize);
            Controls.Add(btnClose);
            Controls.Add(panel1);
            Controls.Add(lblTimeRemaining);
            Controls.Add(lblSpeed);
            Controls.Add(btnCancel);
            Controls.Add(lblPercentage);
            Controls.Add(lblTitle);
            Controls.Add(progressBarMain);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormDownload";
            Shown += FormDownload_Shown;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxGame).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // Aggiungi queste dichiarazioni nella classe FormDownload
        private System.Windows.Forms.ProgressBar progressBarMain;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label lblTimeRemaining;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBoxGame;

        #endregion
        private Button btnClose;
        private Button btnMinimize;
    }
}