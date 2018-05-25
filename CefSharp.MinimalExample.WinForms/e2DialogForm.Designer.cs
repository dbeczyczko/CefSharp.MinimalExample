namespace e2Controls.Forms
{
    partial class e2DialogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(e2DialogForm));
            this._dialogContentPanel = new System.Windows.Forms.Panel();
            this._clientPanel = new System.Windows.Forms.Panel();
            this._dialogContentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _dialogContentPanel
            // 
            this._dialogContentPanel.Controls.Add(this._clientPanel);
            this._dialogContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dialogContentPanel.Location = new System.Drawing.Point(104, 4);
            this._dialogContentPanel.Name = "_dialogContentPanel";
            this._dialogContentPanel.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this._dialogContentPanel.Size = new System.Drawing.Size(484, 258);
            this._dialogContentPanel.TabIndex = 25;
            // 
            // _clientPanel
            // 
            this._clientPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._clientPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._clientPanel.Location = new System.Drawing.Point(4, 50);
            this._clientPanel.Margin = new System.Windows.Forms.Padding(104, 50, 0, 0);
            this._clientPanel.MinimumSize = new System.Drawing.Size(250, 50);
            this._clientPanel.Name = "_clientPanel";
            this._clientPanel.Size = new System.Drawing.Size(480, 208);
            this._clientPanel.TabIndex = 2;
            // 
            // e2DialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(592, 266);
            this.ControlBox = false;
            this.Controls.Add(this._dialogContentPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "e2DialogForm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "e2DialogForm";
            this._dialogContentPanel.ResumeLayout(false);
            this._dialogContentPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Panel _dialogContentPanel;
        protected System.Windows.Forms.Panel _clientPanel;
    }
}