namespace Phoenix_Play.PanelNavigatore
{
    partial class UserControl_Impostazioni
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControl_Impostazioni));
            panel2 = new Panel();
            richTextBox1 = new RichTextBox();
            btnUpdate = new Button();
            btnInstallaRequisiti = new Button();
            label4 = new Label();
            label3 = new Label();
            comboBox1 = new ComboBox();
            label2 = new Label();
            btnCambia = new Button();
            textBox_percorsoinstallazionegiochi = new TextBox();
            label1 = new Label();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            resources.ApplyResources(panel2, "panel2");
            panel2.BackColor = Color.FromArgb(40, 40, 45);
            panel2.Controls.Add(richTextBox1);
            panel2.Controls.Add(btnUpdate);
            panel2.Controls.Add(btnInstallaRequisiti);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(comboBox1);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(btnCambia);
            panel2.Controls.Add(textBox_percorsoinstallazionegiochi);
            panel2.Controls.Add(label1);
            panel2.Name = "panel2";
            // 
            // richTextBox1
            // 
            resources.ApplyResources(richTextBox1, "richTextBox1");
            richTextBox1.BackColor = Color.FromArgb(45, 45, 55);
            richTextBox1.ForeColor = Color.White;
            richTextBox1.Name = "richTextBox1";
            // 
            // btnUpdate
            // 
            resources.ApplyResources(btnUpdate, "btnUpdate");
            btnUpdate.BackColor = Color.FromArgb(70, 130, 180);
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 100, 150);
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 150, 200);
            btnUpdate.ForeColor = Color.White;
            btnUpdate.Image = Properties.Resources.pngUpdateApp;
            btnUpdate.Name = "btnUpdate";
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnInstallaRequisiti
            // 
            resources.ApplyResources(btnInstallaRequisiti, "btnInstallaRequisiti");
            btnInstallaRequisiti.BackColor = Color.FromArgb(70, 130, 180);
            btnInstallaRequisiti.Cursor = Cursors.Hand;
            btnInstallaRequisiti.FlatAppearance.BorderSize = 0;
            btnInstallaRequisiti.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 100, 150);
            btnInstallaRequisiti.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 150, 200);
            btnInstallaRequisiti.ForeColor = Color.White;
            btnInstallaRequisiti.Image = Properties.Resources.pngInstallaRedistributableApp;
            btnInstallaRequisiti.Name = "btnInstallaRequisiti";
            btnInstallaRequisiti.UseVisualStyleBackColor = false;
            btnInstallaRequisiti.Click += btnInstallaRequisiti_Click;
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.ForeColor = Color.LightGray;
            label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.ForeColor = Color.White;
            label3.Name = "label3";
            // 
            // comboBox1
            // 
            resources.ApplyResources(comboBox1, "comboBox1");
            comboBox1.BackColor = Color.FromArgb(70, 70, 80);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.ForeColor = Color.White;
            comboBox1.FormattingEnabled = true;
            comboBox1.Name = "comboBox1";
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.ForeColor = Color.White;
            label2.Name = "label2";
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
            // UserControl_Impostazioni
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(40, 40, 45);
            Controls.Add(panel2);
            Name = "UserControl_Impostazioni";
            Load += UserControl_Impostazioni_Load;
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }
        #endregion

        private Panel panel2;
        private Button btnInstallaRequisiti;
        private Label label4;
        private Label label3;
        private ComboBox comboBox1;
        private Label label2;
        private Button btnCambia;
        private TextBox textBox_percorsoinstallazionegiochi;
        private Label label1;
        private Button btnUpdate;
        private RichTextBox richTextBox1;
    }
}
