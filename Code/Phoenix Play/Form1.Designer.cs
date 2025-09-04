namespace Phoenix_Play
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new RoundedTableLayoutPanel();
            roundedPanel1 = new RoundedPanel();
            label_libreria = new Label();
            textcercalibreria = new TextBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panelBottoni = new Panel();
            label1 = new Label();
            btn_SteamTools = new Button();
            pictureBox1 = new PictureBox();
            btn_Catalogo = new Button();
            btn_Home = new Button();
            panel3 = new Panel();
            labeldownload = new AnimatedButtonControl();
            btn_Impostazioni = new Button();
            panel2 = new Panel();
            tableLayoutPanel3 = new RoundedTableLayoutPanel();
            panelnavigatore = new Panel();
            paneltitolo = new Panel();
            btnMinimize = new Button();
            btnMaxi = new Button();
            btnClose = new Button();
            labelNavigatore = new Label();
            textBox_CercaCat = new TextBox();
            panel1 = new Panel();
            label2 = new Label();
            pictureBox2 = new PictureBox();
            labelversion = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            roundedPanel1.SuspendLayout();
            panelBottoni.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            paneltitolo.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(panel2, 1, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(tableLayoutPanel2, "tableLayoutPanel2");
            tableLayoutPanel2.Controls.Add(panel3, 0, 2);
            tableLayoutPanel2.Controls.Add(roundedPanel1, 0, 1);
            tableLayoutPanel2.Controls.Add(panelBottoni, 0, 0);
            tableLayoutPanel2.CornerRadius = 20;
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowBorderColor = Color.Gray;
            tableLayoutPanel2.RowBorderWidth = 1;
            // 
            // roundedPanel1
            // 
            resources.ApplyResources(roundedPanel1, "roundedPanel1");
            roundedPanel1.BackColor = Color.FromArgb(71, 71, 69);
            roundedPanel1.Controls.Add(label_libreria);
            roundedPanel1.Controls.Add(textcercalibreria);
            roundedPanel1.Controls.Add(flowLayoutPanel1);
            roundedPanel1.CornerRadius = 20;
            roundedPanel1.HeaderText = "";
            roundedPanel1.Name = "roundedPanel1";
            // 
            // label_libreria
            // 
            resources.ApplyResources(label_libreria, "label_libreria");
            label_libreria.Cursor = Cursors.Hand;
            label_libreria.ForeColor = Color.White;
            label_libreria.Image = Properties.Resources.pngLibreria;
            label_libreria.Name = "label_libreria";
            label_libreria.Click += label_libreria_Click;
            // 
            // textcercalibreria
            // 
            resources.ApplyResources(textcercalibreria, "textcercalibreria");
            textcercalibreria.BackColor = Color.FromArgb(71, 71, 69);
            textcercalibreria.BorderStyle = BorderStyle.FixedSingle;
            textcercalibreria.ForeColor = Color.White;
            textcercalibreria.Name = "textcercalibreria";
            textcercalibreria.TextChanged += textcercalibreria_TextChanged;
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(flowLayoutPanel1, "flowLayoutPanel1");
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // panelBottoni
            // 
            resources.ApplyResources(panelBottoni, "panelBottoni");
            panelBottoni.Controls.Add(label1);
            panelBottoni.Controls.Add(btn_SteamTools);
            panelBottoni.Controls.Add(pictureBox1);
            panelBottoni.Controls.Add(btn_Catalogo);
            panelBottoni.Controls.Add(btn_Home);
            panelBottoni.Name = "panelBottoni";
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.ForeColor = Color.White;
            label1.Name = "label1";
            // 
            // btn_SteamTools
            // 
            resources.ApplyResources(btn_SteamTools, "btn_SteamTools");
            btn_SteamTools.BackColor = Color.FromArgb(71, 71, 69);
            btn_SteamTools.Cursor = Cursors.Hand;
            btn_SteamTools.FlatAppearance.BorderSize = 0;
            btn_SteamTools.ForeColor = Color.White;
            btn_SteamTools.Image = Properties.Resources.pngSteamToolApp;
            btn_SteamTools.Name = "btn_SteamTools";
            btn_SteamTools.UseVisualStyleBackColor = false;
            btn_SteamTools.Click += btn_SteamTools_Click;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(pictureBox1, "pictureBox1");
            pictureBox1.Image = Properties.Resources.pngLogoApp;
            pictureBox1.Name = "pictureBox1";
            pictureBox1.TabStop = false;
            // 
            // btn_Catalogo
            // 
            resources.ApplyResources(btn_Catalogo, "btn_Catalogo");
            btn_Catalogo.BackColor = Color.FromArgb(71, 71, 69);
            btn_Catalogo.Cursor = Cursors.Hand;
            btn_Catalogo.FlatAppearance.BorderSize = 0;
            btn_Catalogo.ForeColor = Color.White;
            btn_Catalogo.Image = Properties.Resources.pngCategoryApp;
            btn_Catalogo.Name = "btn_Catalogo";
            btn_Catalogo.UseVisualStyleBackColor = false;
            btn_Catalogo.Click += btn_Catalogo_Click;
            // 
            // btn_Home
            // 
            resources.ApplyResources(btn_Home, "btn_Home");
            btn_Home.BackColor = Color.FromArgb(71, 71, 69);
            btn_Home.Cursor = Cursors.Hand;
            btn_Home.FlatAppearance.BorderSize = 0;
            btn_Home.ForeColor = Color.White;
            btn_Home.Image = Properties.Resources.pngNewApp;
            btn_Home.Name = "btn_Home";
            btn_Home.UseVisualStyleBackColor = false;
            btn_Home.Click += btn_Home_Click;
            // 
            // panel3
            // 
            panel3.Controls.Add(labeldownload);
            panel3.Controls.Add(btn_Impostazioni);
            resources.ApplyResources(panel3, "panel3");
            panel3.Name = "panel3";
            // 
            // labeldownload
            // 
            resources.ApplyResources(labeldownload, "labeldownload");
            labeldownload.BackColor = Color.FromArgb(71, 71, 69);
            labeldownload.ButtonText = "";
            labeldownload.ForeColor = Color.White;
            labeldownload.Name = "labeldownload";
            // 
            // btn_Impostazioni
            // 
            resources.ApplyResources(btn_Impostazioni, "btn_Impostazioni");
            btn_Impostazioni.Cursor = Cursors.Hand;
            btn_Impostazioni.FlatAppearance.BorderSize = 0;
            btn_Impostazioni.ForeColor = Color.White;
            btn_Impostazioni.Image = Properties.Resources.pngImpoApp;
            btn_Impostazioni.Name = "btn_Impostazioni";
            btn_Impostazioni.UseVisualStyleBackColor = true;
            btn_Impostazioni.Click += btn_Impostazioni_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(tableLayoutPanel3);
            resources.ApplyResources(panel2, "panel2");
            panel2.Name = "panel2";
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(tableLayoutPanel3, "tableLayoutPanel3");
            tableLayoutPanel3.BackColor = Color.FromArgb(40, 40, 45);
            tableLayoutPanel3.Controls.Add(panelnavigatore, 0, 1);
            tableLayoutPanel3.Controls.Add(paneltitolo, 0, 0);
            tableLayoutPanel3.Controls.Add(panel1, 0, 2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // panelnavigatore
            // 
            resources.ApplyResources(panelnavigatore, "panelnavigatore");
            panelnavigatore.BackColor = Color.FromArgb(40, 40, 45);
            panelnavigatore.Name = "panelnavigatore";
            // 
            // paneltitolo
            // 
            resources.ApplyResources(paneltitolo, "paneltitolo");
            paneltitolo.BackColor = Color.FromArgb(40, 40, 45);
            paneltitolo.Controls.Add(btnMinimize);
            paneltitolo.Controls.Add(btnMaxi);
            paneltitolo.Controls.Add(btnClose);
            paneltitolo.Controls.Add(labelNavigatore);
            paneltitolo.Controls.Add(textBox_CercaCat);
            paneltitolo.Name = "paneltitolo";
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
            // btnMaxi
            // 
            resources.ApplyResources(btnMaxi, "btnMaxi");
            btnMaxi.Cursor = Cursors.Hand;
            btnMaxi.FlatAppearance.BorderSize = 0;
            btnMaxi.Image = Properties.Resources.pngMassimizzaApp;
            btnMaxi.Name = "btnMaxi";
            btnMaxi.UseVisualStyleBackColor = true;
            btnMaxi.Click += btnMaxi_Click;
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
            // labelNavigatore
            // 
            resources.ApplyResources(labelNavigatore, "labelNavigatore");
            labelNavigatore.ForeColor = Color.White;
            labelNavigatore.Name = "labelNavigatore";
            // 
            // textBox_CercaCat
            // 
            resources.ApplyResources(textBox_CercaCat, "textBox_CercaCat");
            textBox_CercaCat.BackColor = Color.FromArgb(40, 40, 45);
            textBox_CercaCat.BorderStyle = BorderStyle.FixedSingle;
            textBox_CercaCat.ForeColor = Color.White;
            textBox_CercaCat.Name = "textBox_CercaCat";
            textBox_CercaCat.KeyDown += textBox_CercaCat_KeyDown;
            // 
            // panel1
            // 
            resources.ApplyResources(panel1, "panel1");
            panel1.BackColor = Color.FromArgb(40, 40, 45);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(labelversion);
            panel1.Name = "panel1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.ForeColor = Color.White;
            label2.Name = "label2";
            // 
            // pictureBox2
            // 
            resources.ApplyResources(pictureBox2, "pictureBox2");
            pictureBox2.Cursor = Cursors.Hand;
            pictureBox2.Image = Properties.Resources.support_me_on_kofi_red;
            pictureBox2.Name = "pictureBox2";
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // labelversion
            // 
            resources.ApplyResources(labelversion, "labelversion");
            labelversion.ForeColor = Color.White;
            labelversion.Name = "labelversion";
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(46, 46, 46);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            Load += Form1_Load;
            Shown += Form1_Shown;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            roundedPanel1.ResumeLayout(false);
            roundedPanel1.PerformLayout();
            panelBottoni.ResumeLayout(false);
            panelBottoni.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            paneltitolo.ResumeLayout(false);
            paneltitolo.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private RoundedTableLayoutPanel tableLayoutPanel2;
        private Panel panelBottoni;
        private Button btn_Catalogo;
        private PictureBox pictureBox1;
        private Button btn_Impostazioni;
        private TextBox textcercalibreria;
        private Panel panel2;
        private RoundedTableLayoutPanel tableLayoutPanel3;
        private Panel paneltitolo;
        private TextBox textBox_CercaCat;
        public Button btn_Home;
        public FlowLayoutPanel flowLayoutPanel1;
        private Label labelNavigatore;
        private Label label_libreria;
        private Panel panelnavigatore;
        private Panel panel1;
        private Button btnClose;
        private Button btnMaxi;
        private Button btnMinimize;
        private Label labelversion;
        private Button btn_SteamTools;
        private PictureBox pictureBox2;
        private Panel panel3;
        private AnimatedButtonControl labeldownload;
        private RoundedPanel roundedPanel1;
        private Label label1;
        private Label label2;
    }
}
