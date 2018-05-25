using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    partial class kurwa
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
            this._clientPanel.SuspendLayout();

            this.SuspendLayout();
            // 
            // panel1
            // 

            this._clientPanel.Controls.Add(this._browser);
            this._clientPanel.Location = new System.Drawing.Point(0, 50);
            this._clientPanel.Size = new System.Drawing.Size(1031, 659);
            // 
            // kurwa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1031, 659);
            this.ControlBox = true;

            //            this.Controls.Add(this._browser);
            this._clientPanel.ResumeLayout(false);

            this.Name = "kurwa";
            this.Text = "kurwa";
            this.ResumeLayout(false);

        }

        #endregion

        private ChromiumWebBrowser _browser;
    }
}