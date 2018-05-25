using System.Windows.Forms;
using CefSharp.WinForms;

namespace e2App.PrintTemplates
{
    partial class ChromiumPrintTemplateViewer
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
            this._browser = new ChromiumWebBrowser("about:blank");
            this.SuspendLayout();
            // 
            // _dialogContentPanel
            // 
            this._clientPanel.Controls.Add(_browser);
            // 
            // _clientPanel
            // 
            this._clientPanel.Location = new System.Drawing.Point(4, 22);
            this._clientPanel.Size = new System.Drawing.Size(580, 231);
            // 
            // ChromiumPrintTemplateViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(592, 289);
            this.Name = "ChromiumPrintTemplateViewer";
            this.ResumeLayout(false);

        }

        #endregion

        private ChromiumWebBrowser _browser;
    }
}