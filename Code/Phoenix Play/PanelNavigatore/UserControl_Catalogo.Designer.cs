namespace Phoenix_Play.PanelNavigatore
{
    partial class UserControl_Catalogo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControl_Catalogo));
            tableLayoutPanelHome = new TableLayoutPanel();
            SuspendLayout();
            // 
            // tableLayoutPanelHome
            // 
            resources.ApplyResources(tableLayoutPanelHome, "tableLayoutPanelHome");
            tableLayoutPanelHome.Name = "tableLayoutPanelHome";
            // 
            // UserControl_Catalogo
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(40, 40, 45);
            Controls.Add(tableLayoutPanelHome);
            Name = "UserControl_Catalogo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanelHome;
    }
}
