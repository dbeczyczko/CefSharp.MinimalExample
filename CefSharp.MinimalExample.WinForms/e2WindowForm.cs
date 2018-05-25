using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace e2Controls.Forms
{
    public interface Ie2Control
    {

    }

    public partial class e2WindowForm : Form, Ie2Control
    {       	
        public e2WindowForm()
        {
            this.KeepAlive = false;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.Selectable, true);             
            InitializeComponent();
            InitializeComponentCustom();
        }

        private void InitializeComponentCustom()
        {
            this.BackColor = Color.White;
            this.MinimumSize = new Size(100, 100);
        }

        /// <summary>
        /// If is false, the form will be disposed after close. This is default behaviour. If you
        /// want to access some components after the form is closed, you need to set it to true, before
        /// Show or ShowDialog call, then you must Dispose the form manually, when it's no longer needed.
        /// </summary>
        public bool KeepAlive
        {
            get;
            set;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (!this.KeepAlive)
            {
                this.Dispose(true);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Focus();
            base.OnClosing(e);
        }

        /// <summary>
        /// Show window after all content is painted. Use in form constructor to prevent flickering.
        /// </summary>
        public void DelayDisplay()
        {
            e2Forms.RegisterDelayedDisplayForm(this);
        }

        private string _localizationEntryID;

        [RefreshProperties(RefreshProperties.All)]
        public string LocalizationEntryId
        {
            get
            {
                return _localizationEntryID;
            }
            set
            {
                    _localizationEntryID = value;
                }
            }

        public override sealed string Text
        {
            get
            {
                return string.Empty;
            }
            set
            {
                base.Text = value;
            }
        }

        protected virtual bool AllowEscClose()
        {
            return true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && this.AllowEscClose())
            {
                this.Close();
            }
            
            base.OnKeyDown(e);
        }
    }
}
