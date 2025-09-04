namespace Phoenix_Play.PanelNavigatore
{
    partial class UserControl_SteamTools
    {
        /// <summary> 
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary> 
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare 
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControl_SteamTools));
            panel1 = new Panel();
            groupBox2 = new GroupBox();
            listboxcompleta = new ListBox();
            pictureBox1 = new PictureBox();
            btnCambia = new Button();
            textBox_percorsoinstallazionegiochi = new TextBox();
            label1 = new Label();
            btnInstalla = new Button();
            progressBarDownload = new ProgressBar();
            textBox_CercaSteam = new TextBox();
            groupBox1 = new GroupBox();
            richTextBox1 = new RichTextBox();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(groupBox2);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(btnCambia);
            panel1.Controls.Add(textBox_percorsoinstallazionegiochi);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnInstalla);
            panel1.Controls.Add(progressBarDownload);
            panel1.Controls.Add(textBox_CercaSteam);
            panel1.Controls.Add(groupBox1);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            // 
            // groupBox2
            // 
            resources.ApplyResources(groupBox2, "groupBox2");
            groupBox2.Controls.Add(listboxcompleta);
            groupBox2.ForeColor = Color.White;
            groupBox2.Name = "groupBox2";
            groupBox2.TabStop = false;
            // 
            // listboxcompleta
            // 
            resources.ApplyResources(listboxcompleta, "listboxcompleta");
            listboxcompleta.BackColor = Color.FromArgb(45, 45, 55);
            listboxcompleta.ForeColor = Color.White;
            listboxcompleta.FormattingEnabled = true;
            listboxcompleta.Name = "listboxcompleta";
            listboxcompleta.SelectedIndexChanged += listboxcompleta_SelectedIndexChanged;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(pictureBox1, "pictureBox1");
            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Image = Properties.Resources.pngHelpSteamTools;
            pictureBox1.Name = "pictureBox1";
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // btnCambia
            // 
            resources.ApplyResources(btnCambia, "btnCambia");
            btnCambia.BackColor = Color.FromArgb(70, 130, 180);
            btnCambia.Cursor = Cursors.Hand;
            btnCambia.FlatAppearance.BorderSize = 0;
            btnCambia.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 100, 150);
            btnCambia.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 150, 200);
            btnCambia.ForeColor = Color.White;
            btnCambia.Name = "btnCambia";
            btnCambia.UseVisualStyleBackColor = false;
            btnCambia.Click += btnCambia_Click;
            // 
            // textBox_percorsoinstallazionegiochi
            // 
            resources.ApplyResources(textBox_percorsoinstallazionegiochi, "textBox_percorsoinstallazionegiochi");
            textBox_percorsoinstallazionegiochi.BackColor = Color.FromArgb(70, 70, 80);
            textBox_percorsoinstallazionegiochi.BorderStyle = BorderStyle.FixedSingle;
            textBox_percorsoinstallazionegiochi.ForeColor = Color.White;
            textBox_percorsoinstallazionegiochi.Name = "textBox_percorsoinstallazionegiochi";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.ForeColor = Color.White;
            label1.Name = "label1";
            // 
            // btnInstalla
            // 
            resources.ApplyResources(btnInstalla, "btnInstalla");
            btnInstalla.BackColor = Color.FromArgb(70, 130, 180);
            btnInstalla.Cursor = Cursors.Hand;
            btnInstalla.FlatAppearance.BorderSize = 0;
            btnInstalla.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 100, 150);
            btnInstalla.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 150, 200);
            btnInstalla.ForeColor = Color.White;
            btnInstalla.Image = Properties.Resources.pngInstallaRedistributableApp;
            btnInstalla.Name = "btnInstalla";
            btnInstalla.UseVisualStyleBackColor = false;
            btnInstalla.Click += BtnInstalla_Click;
            // 
            // progressBarDownload
            // 
            resources.ApplyResources(progressBarDownload, "progressBarDownload");
            progressBarDownload.Name = "progressBarDownload";
            // 
            // textBox_CercaSteam
            // 
            resources.ApplyResources(textBox_CercaSteam, "textBox_CercaSteam");
            textBox_CercaSteam.BackColor = Color.FromArgb(46, 46, 46);
            textBox_CercaSteam.BorderStyle = BorderStyle.FixedSingle;
            textBox_CercaSteam.ForeColor = Color.White;
            textBox_CercaSteam.Name = "textBox_CercaSteam";
            textBox_CercaSteam.KeyDown += textBox_CercaSteam_KeyDown;
            // 
            // groupBox1
            // 
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Controls.Add(richTextBox1);
            groupBox1.ForeColor = Color.White;
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.FromArgb(45, 45, 55);
            resources.ApplyResources(richTextBox1, "richTextBox1");
            richTextBox1.ForeColor = Color.White;
            richTextBox1.Name = "richTextBox1";
            // 
            // UserControl_SteamTools
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(40, 40, 45);
            Controls.Add(panel1);
            Name = "UserControl_SteamTools";
            Load += UserControl_SteamTools_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private GroupBox groupBox1;
        private ProgressBar progressBarDownload;
        private TextBox textBox_CercaSteam;
        private Button btnInstalla;
        private RichTextBox richTextBox1;
        private Button btnCambia;
        private TextBox textBox_percorsoinstallazionegiochi;
        private Label label1;
        private ListBox listboxcompleta;
        private PictureBox pictureBox1;
        private GroupBox groupBox2;
    }
}
