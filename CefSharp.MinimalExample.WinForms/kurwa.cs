using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefSharp.MinimalExample.WinForms
{
    using e2App.PrintTemplates;

    using e2Controls;
    using e2Controls.Forms;

    using Microsoft.Win32;

    public partial class kurwa : e2DialogForm
    {
        private const string PAGE_SETUP_REGISTRY_KEY = "SOFTWARE\\MICROSOFT\\Internet Explorer\\PageSetup";

        private readonly ChromiumBrowserController _chromiumBrowserController;

        private object[] _oldMargins = new object[4];
        private object _oldHeader;
        private object _oldFooter;
        private bool _modified = false;
        private bool _isInEditingMode = false;
        private static bool _isPrescription = false;

        public string Title
        {
            get;
            set;
        }

        public Guid? DocumentID
        {
            get;
            set;
        }

        public bool UseHeaderFooter
        {
            get;
            set;
        }

        public kurwa()
        {
            InitializeComponent();
            InitializeComponentCustom();

            _chromiumBrowserController = new ChromiumBrowserController(_browser);
        }

        private void InitializeComponentCustom()
        {
            UseHeaderFooter = true;

            SetBrowserRegistrySettings();

            HideDialogPanel();
        }

        void SetBrowserRegistrySettings()
        {
            RegistryKey psKey = Registry.CurrentUser.CreateSubKey(PAGE_SETUP_REGISTRY_KEY);

            if (_isPrescription)
            {
                psKey.SetValue("margin_left", 0);
                psKey.SetValue("margin_right", 0);
                psKey.SetValue("margin_top", 0);
                psKey.SetValue("margin_bottom", 0);
                psKey.Flush();
                return;
            }

            _oldMargins[0] = psKey.GetValue("margin_left");
            _oldMargins[1] = psKey.GetValue("margin_right");
            _oldMargins[2] = psKey.GetValue("margin_top");
            _oldMargins[3] = psKey.GetValue("margin_bottom");
            _oldHeader = psKey.GetValue("header");
            _oldFooter = psKey.GetValue("footer");

            //psKey.SetValue("header", Estomed.Settings.GetString(EstomedSettings.PrintHeader));
            //psKey.SetValue("footer", Estomed.Settings.GetString(EstomedSettings.PrintFooter));

            //psKey.SetValue("margin_left", PriningHelper.StringMilimetersToStringInches(EstomedSettings.PrintMarginLeft));
            //psKey.SetValue("margin_right", PriningHelper.StringMilimetersToStringInches(EstomedSettings.PrintMarginRight));
            //psKey.SetValue("margin_top", PriningHelper.StringMilimetersToStringInches(EstomedSettings.PrintMarginTop));
            //psKey.SetValue("margin_bottom", PriningHelper.StringMilimetersToStringInches(EstomedSettings.PrintMarginBottom));

            psKey.Flush();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                RegistryKey psKey = Registry.CurrentUser.CreateSubKey(PAGE_SETUP_REGISTRY_KEY);

                psKey.SetValue("margin_left", _oldMargins[0]);
                psKey.SetValue("margin_right", _oldMargins[1]);
                psKey.SetValue("margin_top", _oldMargins[2]);
                psKey.SetValue("margin_bottom", _oldMargins[3]);

                psKey.SetValue("header", _oldHeader);
                psKey.SetValue("footer", _oldFooter);

                psKey.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //if (DocumentID != null && _modified &&
            //    _dialogVisualizer.AskQuestion(ManagementCatalog.TextAlteredSaveChangesMsg))
            //{
            //    string html = _chromiumBrowserController.GetSource();
            //    MySqlDataRow docRow = PatientDocument.GetRowByID(DocumentID);
            //    string path = Estomed.Storage.GetFilePath(docRow.GetString(PatientDocument.Path));

            //    File.WriteAllText(path, html, Encoding.Unicode);
            //}

            base.OnClosed(e);
        }

        public static kurwa ShowFor(
            string html,
            bool useHeaderFooter,
            string title,
            EventHandler onClosedHandler,
            string css = null)
        {
            e2Forms.UseWaitCursorHere();

            kurwa form = new kurwa();

            PrepareFormForShow(html, useHeaderFooter, title, onClosedHandler, form, css);

            form.Show();
            form.TopMost = false;

            return form;
        }

        private static void PrepareFormForShow(
            string html,
            bool useHeaderFooter,
            string title,
            EventHandler onClosedHandler,
            kurwa form,
            string css)
        {
            form.UseHeaderFooter = useHeaderFooter;

            //string src = PrintTemplateUtils.RenderTemplate(html, PrintTemplateContext.Default);
            form.Title = title;

            //e2Forms.AddIdleDelegate(delegate
            //{
                //var sourceWithCss = css ?? HtmlPrinter.GetBasicCSS();
                //sourceWithCss += src;

                //Console.WriteLine("Viewing HTML: {0}", sourceWithCss);

                form._chromiumBrowserController.LoadDocument(html, setNoMargins: _isPrescription);
            //});

            if (onClosedHandler != null)
            {
                form.Closed += onClosedHandler;
            }

            form.TopMost = true;
        }
    }
}
