using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace e2Controls.Forms
{
    public partial class e2DialogForm : e2WindowForm
    {
        public static readonly object TopLevelContainer = new object();

        public bool _disableTab = false;
        public bool DisableTab
        {
            get
            {
                return _disableTab;
            }
            set
            {
                _disableTab = value;
            }
        }

        public e2DialogForm()
        {
            InitializeComponent();
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
           BackColor = Color.White;
           MinimumSize = new Size(100, 100);

            Text = string.Empty;
            _clientPanel.Tag = e2DialogForm.TopLevelContainer;
        }

        public Panel ClientPanel
        {
            get
            {
                return _clientPanel;
            }
        }

        public void SetCaption(string text)
        {
            SetCaption(text, String.Empty);
        }

        public void SetCaption(string primary, string secondary)
        {            
        }

        public event EventHandler<CancelEventArgs> OkClick;
        public event EventHandler<CancelEventArgs> CancelClick;

        public virtual void OnOkClick()
        {
            if (this.OkClick != null)
            {
                CancelEventArgs e = new CancelEventArgs();
                this.OkClick(this, e);
                if (!e.Cancel)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public void AddControl(Control c)
        {
            c.Dock = DockStyle.Fill;
            _dialogContentPanel.Controls.Add(c);
        }

        /// <summary>
        /// Hides OK and Cancel buttons and shows Close button.
        /// </summary>
        public void ShowCloseOnly()
        {
        }

        /// <summary>
        /// Hides panel with OK and Cancel buttons and sidebar.
        /// </summary>
        public void HideDialogPanel()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;

            _clientPanel.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Shows panel with OK and Cancel buttons and sidebar.
        /// </summary>
        public void ShowDialogPanel(bool sizeable)
        {

            if (sizeable)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MaximizeBox = true;
                this.MinimizeBox = true;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }
        }

        public virtual void OnCancelClick()
        {
            if (CancelClick != null)
            {
                CancelClick(this, new CancelEventArgs());
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void _okButton_Click(object sender, EventArgs e)
        {
            e2Forms.UseWaitCursorHere();
            this.OnOkClick();
        }

        private void _cancelButton_Click(object sender, EventArgs e)
        {
            this.OnCancelClick();            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
 	        base.OnKeyDown(e);
        }

        public void AutoSizeToClient()
        {
            _clientPanel.AutoSize = true;
            _clientPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            _dialogContentPanel.Dock = DockStyle.None;
            _dialogContentPanel.AutoSize = true;
            _dialogContentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab && this.DisableTab)
            {
                return true;
            }

            var result = base.ProcessCmdKey(ref msg, keyData);
            return result;
        }
    }
}
