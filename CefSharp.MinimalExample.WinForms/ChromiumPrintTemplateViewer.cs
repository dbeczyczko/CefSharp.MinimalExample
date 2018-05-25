using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using e2Controls;
using e2Controls.Forms;
using Microsoft.Win32;

using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace e2App.PrintTemplates
{
    using System.Collections;
    using System.Diagnostics;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), ComVisible(true)]
    public partial class ChromiumPrintTemplateViewer : e2DialogForm
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

        public ChromiumPrintTemplateViewer()
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

        public static ChromiumPrintTemplateViewer ShowFor(
            string html, 
            bool useHeaderFooter, 
            string title, 
            EventHandler onClosedHandler, 
            string css = null)
        {
            e2Forms.UseWaitCursorHere();
            
            ChromiumPrintTemplateViewer form = new ChromiumPrintTemplateViewer();

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
            ChromiumPrintTemplateViewer form, 
            string css)
        {
            form.UseHeaderFooter = useHeaderFooter;

            //string src = PrintTemplateUtils.RenderTemplate(html, PrintTemplateContext.Default);
            form.Title = title;

            e2Forms.AddIdleDelegate(delegate
            {
                //var sourceWithCss = css ?? HtmlPrinter.GetBasicCSS();
                //sourceWithCss += src;

                //Console.WriteLine("Viewing HTML: {0}", sourceWithCss);

                form._chromiumBrowserController.LoadDocument(html, setNoMargins: _isPrescription);
            });

            if (onClosedHandler != null)
            {
                form.Closed += onClosedHandler;
            }

            form.TopMost = true;
        }
    }

    public class PrintTemplateUtils
    {
        private const string BarcodeMarkup = "e2barcode";
        private static readonly ArrayList Placeholders = new ArrayList();
        //private static readonly BarcodePrintTemplateContentPlaceholderRenderer BarcodePrintTemplateContentPlaceholderRenderer;
        //private static readonly PrintTemplateContentPlaceholderRenderer PrintTemplateContentPlaceholderRenderer;

        //static PrintTemplateUtils()
        //{
        //    BarcodePrintTemplateContentPlaceholderRenderer = new BarcodePrintTemplateContentPlaceholderRenderer();
        //    PrintTemplateContentPlaceholderRenderer = new PrintTemplateContentPlaceholderRenderer();
        //}

        //public static string SetImageTemplate(string imageTemplateName)
        //{
        //    Image image = RenderPlaceholderImage(imageTemplateName, Resources.tag_blue_edit, e2Renderer.ColorGreenDark);
        //    string src = HtmlRenderer.GetImageSrc(image, String.Format("(e2impch){0}.png", imageTemplateName));

        //    return src;
        //}

        //public static Bitmap RenderPlaceholderImage(string text, Image image, Color color)
        //{
        //    Font font = e2Renderer.FontDefault;
        //    Size textSize = TextRenderer.MeasureText(text, font);

        //    int height = textSize.Height + 8;
        //    Bitmap bmp = new Bitmap(textSize.Width + 8 + height, height);
        //    Graphics g = Graphics.FromImage(bmp);

        //    Rectangle bounds = new Rectangle(1, 1, bmp.Width - 2, bmp.Height - 2);

        //    e2Shape shape = new e2Shape();
        //    shape.OutlineColor = color;
        //    shape.SetFill(Color.White, Color.WhiteSmoke, 90.0f);
        //    shape.CornerRadius = 4;
        //    shape.PaintTo(g, bounds);

        //    Color textColor = color;

        //    bounds = new Rectangle(bounds.X + height, bounds.Y, bounds.Width - height, height);
        //    g.DrawString(text, font, new SolidBrush(textColor), bounds, e2Renderer.StringFormatMiddleCenter);
        //    g.DrawImage(image, new Rectangle(bounds.X - height + 6, bounds.Y + 4, height - 8, height - 8));
        //    g.Dispose();

        //    return bmp;
        //}

        public static string PackTemplate(string html)
        {
            string packed = html;

            packed = Regex.Replace(packed, @"<img[^>]*(\(e2pch\)[^>]+\.png)[^>]*>", PackEval, RegexOptions.IgnoreCase);
            packed = Regex.Replace(packed, @"<img[^>]*(\(e2img\)[^>]+\.png)[^>]*>", PackEval, RegexOptions.IgnoreCase);
            packed = Regex.Replace(packed, @"<img[^>]*(\(e2dbfield\)[^>]+\.png)[^>]*>", PackEval, RegexOptions.IgnoreCase);
            packed = Regex.Replace(packed, @"<img[^>]*(\(e2dbtable\)[^>]+\.png)[^>]*>", PackEval, RegexOptions.IgnoreCase);
            return packed;
        }

        private static string PackEval(Match match)
        {
            string imgTag = match.Value;
            string pack = match.Groups[1].Value;
            string fullMatch = match.Groups[0].Value;

            string wrap1 = "<", wrap2 = ">";

            if (fullMatch.Contains(BarcodeMarkup) && pack.Contains("e2pch"))
            {
                wrap1 = ":(e2barcode):";
                wrap2 = ":.e2barcode:";
            }
            else if (pack.Contains("e2pch") || pack.Contains("e2img")) // backward compatibility with :(e2pch)...: and :(e2img)...: forms
            {
                wrap1 = wrap2 = ":";
            }

            Match m = Regex.Match(imgTag, @"width: (\d+)(px|cm)?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                pack += "?width=" + m.Groups[1].Value + m.Groups[2].Value;
            }

            m = Regex.Match(imgTag, @"height: (\d+)(px|cm)?", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                pack += "?height=" + m.Groups[1].Value + m.Groups[2].Value;
            }

            m = Regex.Match(imgTag, @"float: ((\bleft\b)|(\bright\b)|(\bmiddle\b))", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                pack += "?float=" + m.Groups[1].Value;
            }

            m = Regex.Match(imgTag, @"alt=""([^ /]+)""", RegexOptions.IgnoreCase);
            if (m.Success && !String.IsNullOrWhiteSpace(m.Groups[1].Value))
            {
                pack += "?alt=" + m.Groups[1].Value;
            }

            return wrap1 + pack + wrap2;
        }

        //public static string UnpackTemplate(string html)
        //{
        //    string unpacked = html;

        //    unpacked = Regex.Replace(unpacked, @":\(e2barcode\):([^:]+?):.e2barcode[^:]*:", UnpackBarcodeEval);
        //    unpacked = Regex.Replace(unpacked, @":(\(e2pch\)[^:]+\.png)[^:]*:", UnpackEval);
        //    unpacked = Regex.Replace(unpacked, @":(\(e2img\)[^:]+\.png)[^:]*:", UnpackEval);
        //    unpacked = Regex.Replace(unpacked, @"<(\(e2db\w+\)[^>]+\.png)[^>]*>", UnpackEval);

        //    return unpacked;
        //}

        //private static string UnpackBarcodeEval(Match match)
        //{
        //    try
        //    {
        //        string alt = GetImageAlt(match);
        //        string id = Regex.Match(match.Groups[1].Value, @"\)([a-z0-9]+)\.").Groups[1].Value;
        //        ContentPlaceholder cp = ContentPlaceholder.GetByID(id);

        //        if (cp == null)
        //        {
        //            throw new ArgumentException("Barcode content placeholder not found.");
        //        }

        //        string fileName = BarcodePrintTemplateContentPlaceholderRenderer.GetHtmlSource(cp);

        //        return String.Format(@"<img src=""{0}"" style=""{1}"" alt=""{2}"" />",
        //            fileName, PrintTemplateImageRenderer.GetImageStyle(match),
        //            alt);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceWarning(ex.ToString());
        //        return String.Empty;
        //    }
        //}

        //public static ArrayList UnpackPlaceholders(string html)
        //{
        //    Placeholders.Clear();
        //    Regex.Replace(html, @"<(\(e2db\w+\)[^>]+.png)[^>]*>", UnpackPlaceholdersEval);
        //    return Placeholders;
        //}

        //public static string UnpackPlaceholdersEval(Match match)
        //{
        //    try
        //    {
        //        if (match.Value.Contains("e2dbfield"))
        //        {
        //            e2DocumentPlaceholder pc = PrintTemplateDocumentPlaceholderRenderer.UnpackPlaceholder(match);
        //            Placeholders.Add(pc);
        //            return String.Empty;
        //        }

        //        if (match.Value.Contains("e2dbtable"))
        //        {
        //            e2DocumentTable table = PrintTemplateTableRenderer.UnpackTable(match);
        //            Placeholders.Add(table);
        //            return String.Empty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceWarning("Cannot unpack placeholder eval " + match.Value + "\r\n" + ex);
        //    }

        //    return String.Empty;
        //}

        //private static string UnpackEval(Match match)
        //{
        //    try
        //    {
        //        string fileName = HtmlRenderer.GetTempPrefix() + match.Groups[1].Value;
        //        string alt = GetImageAlt(match);

        //        if (!File.Exists(fileName))
        //        {
        //            string id = Regex.Match(match.Groups[1].Value, @"\)([a-z0-9]+)\.").Groups[1].Value;
        //            ContentPlaceholder cp = ContentPlaceholder.GetByID(id);

        //            if (cp != null)
        //            {
        //                fileName = PrintTemplateContentPlaceholderRenderer.GetHtmlSource(cp);
        //            }
        //        }

        //        return String.Format("<img src=\"{0}\" style=\"{1}\" alt=\"{2}\" />",
        //            fileName, PrintTemplateImageRenderer.GetImageStyle(match),
        //            alt);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceWarning(ex.ToString());
        //        return String.Empty;
        //    }
        //}

        //public static string RenderTemplate(string html, PrintTemplateContext context)
        //{
        //    string unpacked = html;

        //    unpacked = Regex.Replace(unpacked, @":\(e2pch\)([^:]+).png[^:]*:", match => RenderEval(match, context));
        //    unpacked = Regex.Replace(unpacked, @":\(e2img\)([^:]+).png[^:]*:", match => RenderEval(match, context));
        //    unpacked = Regex.Replace(unpacked, @"<(\(e2db\w+\)[^>]+.png)[^>]*>", match => RenderEval(match, context));
        //    unpacked = Regex.Replace(unpacked, @":\(e2barcode\)(.*?).e2barcode[^:]*:", match => RenderBarcodeEval(match, context));

        //    return unpacked;
        //}

        //private static string RenderBarcodeEval(Match match, PrintTemplateContext context)
        //{
        //    try
        //    {
        //        return BarcodePrintTemplateContentPlaceholderRenderer.RenderHtml(match.Groups[1].Value, context);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceWarning("Cannot render eval " + match.Value + "\r\n" + ex);
        //        return String.Empty;
        //    }
        //}

        //private static string RenderEval(Match match, PrintTemplateContext context)
        //{
        //    try
        //    {
        //        if (match.Value.Contains("e2dbfield"))
        //        {
        //            return PrintTemplateDocumentPlaceholderRenderer.RenderDocumentPlaceholder(match, context);
        //        }

        //        if (match.Value.Contains("e2dbtable"))
        //        {
        //            return PrintTemplateTableRenderer.RenderTable(match, context);
        //        }

        //        if (match.Value.Contains("e2img"))
        //        {
        //            return PrintTemplateImageRenderer.RenderImage(match);
        //        }

        //        ContentPlaceholder placeholder = ContentPlaceholder.GetByID(match.Groups[1].Value);
        //        var renderHtmlParams = new PrintTemplateParams(placeholder, context)
        //        {
        //            Styles = GetPlaceholderStyles(match)
        //        };
        //        return PrintTemplateContentPlaceholderRenderer.RenderHtml(renderHtmlParams);
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceWarning("Cannot render eval " + match.Value + "\r\n" + ex);
        //        return String.Empty;
        //    }
        //}

        private static string GetPlaceholderStyles(Match match)
        {
            Match width = Regex.Match(match.Value, @"\?width=(\\w+)");
            Match height = Regex.Match(match.Value, @"\?height=(\w+)");
            Match imgFloat = Regex.Match(match.Value, @"\?float=(\w+)");
            string widthStyle = "";
            string heightStyle = "";
            string floatStyle = "";

            if (width.Success)
            {
                widthStyle = $"width: {width.Groups[1].Value};";
            }
            if (height.Success)
            {
                heightStyle = $"height: {height.Groups[1].Value};";
            }
            if (imgFloat.Success)
            {
                floatStyle = $"float: {imgFloat.Groups[1].Value};";
            }

            return $"style='{widthStyle}{heightStyle}{floatStyle}'";
        }

        public static string GetImageAlt(Match match)
        {
            Match alt = Regex.Match(match.Value, @"\?alt=""{0,1}([^ >"":]+)");
            if (!alt.Success)
                return String.Empty;

            return alt.Groups[1].Value;
        }
    }

    public class PrintTemplateDocumentPlaceholderRenderer
    {
        //public static string GetInsertSrcForPlaceholder(e2DocumentPlaceholder pc, bool withData)
        //{
        //    Image image = PrintTemplateUtils.RenderPlaceholderImage(pc.Title, Resources.tag_blue_edit, e2Renderer.ColorGreenDark);
        //    string src = HtmlRenderer.GetImageSrc(image, "(e2dbfield)" + pc.ID + ".png");

        //    if (withData)
        //        src += String.Format("\" alt=\"{0}\"", HttpUtility.UrlEncode(e2DocumentPlaceholder.Serialize(pc)));

        //    return src;
        //}

        //public static string RenderDocumentPlaceholder(Match match, PrintTemplateContext context)
        //{
        //    e2DocumentPlaceholder pc = UnpackPlaceholder(match);
        //    return pc.RenderHtml(context);
        //}

        //public static e2DocumentPlaceholder UnpackPlaceholder(Match match)
        //{
        //    string alt = PrintTemplateUtils.GetImageAlt(match);
        //    return UnpackPlaceholder(alt);
        //}

        //public static e2DocumentPlaceholder UnpackPlaceholder(string s)
        //{
        //    s = s.Trim('"');
        //    return e2DocumentPlaceholder.Deserialize(HttpUtility.UrlDecode(s));
        //}
    }

    [XmlRoot]
    public class e2DocumentPlaceholder
    {
        private static XmlSerializer _serializer = new XmlSerializer(typeof(e2DocumentPlaceholder));

        public e2DocumentPlaceholder()
        {
            this.ID = Guid.NewGuid();
        }

        public static e2DocumentPlaceholder Deserialize(string s)
        {
            try
            {
                return (e2DocumentPlaceholder)_serializer.Deserialize(new StringReader(s));
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Cannot deserialize e2DocumentPlaceholder from \r\n" + s + "\r\n" + ex.ToString());
                return new e2DocumentPlaceholder();
            }
        }

        public static string Serialize(e2DocumentPlaceholder pc)
        {
            try
            {
                StringWriter sw = new StringWriter();
                _serializer.Serialize(sw, pc);
                return sw.ToString();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Cannot serialize e2DocumentPlaceholder\r\n" + ex.ToString());
                return String.Empty;
            }
        }

        public string FormatString
        {
            get;
            set;
        }

        public string DefaultValue
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string DataField
        {
            get;
            set;
        }

        public Guid ID
        {
            get;
            set;
        }

        public string GetValue(e2DocumentContext context)
        {
            if (String.IsNullOrEmpty(this.DataField))
            {
                return this.FormatValue(this.DefaultValue);
            }
            else
            {
                return this.FormatValue(context.GetData(this.DataField));
            }
        }

        //public string RenderHtml(PrintTemplateContext context)
        //{
        //    object val;

        //    if (String.IsNullOrEmpty(this.DataField))
        //    {
        //        val = this.DefaultValue;
        //    }
        //    else
        //    {
        //        val = context.QueryScalar(this.DataField);
        //    }

        //    if (String.IsNullOrEmpty(Convert.ToString(val)))
        //        val = this.DefaultValue;

        //    return this.FormatValue(val);
        //}

        public string FormatValue(object value)
        {
            try
            {
                if (this.FormatString == null)
                    return Convert.ToString(value);
                else if (this.FormatString.Contains("{0"))
                    return String.Format(this.FormatString, value);
                else
                    return String.Format("{0:" + this.FormatString + "}", value);

            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Cannot format {0}\r\n{1}", value, ex);
                return String.Empty;
            }

        }
    }

    public class e2DocumentContext
    {
        public object GetData(string dataField)
        {
            return String.Empty;
        }
    }

    //public class PrintTemplateContext
    //{
    //    public static readonly PrintTemplateContext Default = new PrintTemplateContext();

    //    private List<MySqlDataRow> _rows = new List<MySqlDataRow>();
    //    private Dictionary<string, object> _vars = new Dictionary<string, object>();

    //    private DataGridView _grid;

    //    public DataGridView Grid
    //    {
    //        get
    //        {
    //            return _grid;
    //        }
    //        set
    //        {
    //            _grid = value;
    //            if (_grid != null)
    //            {
    //                if (_grid.DataSource is MySqlBindingSource)
    //                {
    //                    MySqlBindingSource src = (_grid.DataSource as MySqlBindingSource);
    //                    this.Vector = src.DataTable;
    //                    this.Scalar = src.CurrentRow;
    //                }
    //                else if (_grid.DataSource is MySqlDataTable)
    //                {
    //                    this.Vector = _grid.DataSource as MySqlDataTable;
    //                    if (_grid.SelectedCells.Count > 0)
    //                    {
    //                        DataRowView rv = _grid.Rows[_grid.SelectedCells[0].RowIndex].DataBoundItem as DataRowView;
    //                        if (rv != null)
    //                        {
    //                            this.Scalar = rv.Row as MySqlDataRow;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public MySqlDataRow Scalar
    //    {
    //        get;
    //        set;
    //    }

    //    public MySqlDataTable Vector
    //    {
    //        get;
    //        set;
    //    }

    //    public List<MySqlDataRow> Rows
    //    {
    //        get
    //        {
    //            return _rows;
    //        }
    //    }

    //    public Dictionary<string, object> Vars
    //    {
    //        get
    //        {
    //            return _vars;
    //        }
    //    }

    //    public void Clear()
    //    {
    //        _vars.Clear();
    //        _rows.Clear();
    //        this.Grid = null;
    //        this.Vector = null;
    //        this.Scalar = null;
    //    }

    //    public object GetFieldValue(MySqlField field)
    //    {
    //        foreach (MySqlDataRow row in _rows)
    //        {
    //            try
    //            {
    //                if (row.BindingSource.DataTable.FieldMappings.ContainsKey(field))
    //                    return row[field];
    //            }
    //            catch
    //            {
    //                continue;
    //            }
    //        }

    //        return string.Empty;
    //    }

    //    private string PrepareEval(Match m)
    //    {
    //        string field = m.Groups[2].Value;
    //        return this.GetScalarSqlValue(field);
    //    }

    //    private string GetScalarSqlValue(string field)
    //    {
    //        return MySqlValueSerializer.Get(this.GetScalarValue(field));
    //    }

    //    private object GetScalarValue(string field)
    //    {
    //        field = field.Replace("`", "");
    //        if (field.Contains("."))
    //        {
    //            return this.SearchForValue(field);
    //        }
    //        else
    //        {
    //            if (_vars.ContainsKey(field))
    //            {
    //                return _vars[field];
    //            }
    //            else
    //            {
    //                return this.SearchForValue(field);
    //            }
    //        }
    //    }

    //    private object SearchForValue(string key)
    //    {
    //        if (key == null)
    //        {
    //            this.QueryErrors.Add("SearchForValue key is null");
    //            this.LastQueryScalarFailed = true;
    //            return String.Empty;
    //        }

    //        List<MySqlDataRow> rows = new List<MySqlDataRow>(_rows);

    //        string shortKey = key;

    //        if (this.Scalar != null)
    //            rows.Insert(0, this.Scalar);

    //        if (key.Contains("."))
    //            shortKey = key.Split('.')[1];

    //        foreach (MySqlDataRow row in rows)
    //        {
    //            try
    //            {
    //                foreach (MySqlField f in row.MySqlDataTable.FieldMappings.Keys)
    //                {
    //                    try
    //                    {
    //                        if (f.FieldID.Equals(key))
    //                            return row[f];
    //                    }
    //                    catch
    //                    {
    //                    }
    //                }
    //            }
    //            catch
    //            {
    //            }
    //        }

    //        if (this.Grid != null)
    //        {
    //            foreach (DataGridViewColumn gc in this.Grid.Columns)
    //            {
    //                if (gc.DataPropertyName == key)
    //                {
    //                    if (this.Grid.SelectedCells.Count > 0)
    //                    {
    //                        return this.Grid[gc.Index, this.Grid.SelectedCells[0].RowIndex].Value;
    //                    }
    //                    else if (this.Grid.Rows.Count > 0)
    //                    {
    //                        return this.Grid[gc.Index, 0].Value;
    //                    }
    //                }
    //            }
    //        }

    //        foreach (MySqlDataRow row in rows)
    //        {
    //            if (row.Table.Columns.Contains(shortKey))
    //                return row[shortKey];
    //        }

    //        this.LastQueryScalarFailed = true;
    //        this.QueryErrors.Add("Cannot resolve key: " + key);
    //        return String.Empty;
    //    }

    //    private string PrepareQuery(string sql)
    //    {
    //        sql = Regex.Replace(sql, @"(\[([a-zA-Z0-9_`.]+)\])", new MatchEvaluator(this.PrepareEval));
    //        return sql.Trim();
    //    }

    //    public bool LastQueryScalarFailed = false;
    //    public List<String> QueryErrors = new List<string>();
    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    //    public object QueryScalar(string query)
    //    {
    //        if (query == null)
    //        {
    //            return String.Empty;
    //        }

    //        try
    //        {
    //            this.LastQueryScalarFailed = false;
    //            query = this.PrepareQuery(query);
    //            if (query.StartsWith("explain select", StringComparison.InvariantCultureIgnoreCase)
    //                || query.StartsWith("select", StringComparison.InvariantCultureIgnoreCase))
    //            {
    //                lock (MySqlDb.GlobalLock)
    //                {
    //                    using (MySqlCommand cmd = new MySqlCommand(query, Estomed.Db.Connection))
    //                    {
    //                        object value = cmd.ExecuteScalar();
    //                        return value;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                return this.GetScalarValue(query);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceWarning("PrintTemplateContext.QueryScalar query error\r\n"
    //                + query + "\r\n" + ex.ToString());
    //        }

    //        this.LastQueryScalarFailed = true;
    //        this.QueryErrors.Add("Cannot resolve query: " + query);
    //        return String.Empty;
    //    }

    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    //    public object ExplainQuery(string query)
    //    {
    //        if (query.StartsWith("select", StringComparison.InvariantCultureIgnoreCase))
    //        {
    //            query = $"explain {query}";
    //        }

    //        return QueryScalar(query);
    //    }


    //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    //    public MySqlDataTable QueryVector(string query)
    //    {
    //        try
    //        {
    //            if (!String.IsNullOrEmpty(query))
    //            {
    //                query = this.PrepareQuery(query);

    //                MySqlDataAdapter adapter = new MySqlDataAdapter(query, Estomed.Db.Connection);

    //                MySqlDataTable table = new MySqlDataTable();

    //                lock (MySqlDb.GlobalLock)
    //                {
    //                    adapter.SelectCommand.CommandTimeout = 300;
    //                    adapter.Fill(table);
    //                }

    //                return table;
    //            }
    //            else if (this.Vector != null)
    //            {
    //                return this.Vector;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceWarning("PrintTemplateContext.QueryVector query error\r\n"
    //                + Convert.ToString(query) + "\r\n" + ex.ToString());
    //        }

    //        return new MySqlDataTable();
    //    }

    //    public List<e2DocumentTableColumn> GetVectorColumns(MySqlDataTable table)
    //    {
    //        return new List<e2DocumentTableColumn>();
    //    }

    //    private Dictionary<string, PrintTemplatePlaceholdersCache> _placeholdersCache = new Dictionary<string, PrintTemplatePlaceholdersCache>();
    //    public bool IsTemplateValidHere(MySqlDataRow printTemplateRow)
    //    {
    //        string id = printTemplateRow.GetString(PrintTemplate.ID);
    //        PrintTemplatePlaceholdersCache entry = null;
    //        DateTime time = printTemplateRow.GetDateTime(PrintTemplate.X_ModifyTime);

    //        this.QueryErrors.Clear();

    //        if (_placeholdersCache.ContainsKey(id))
    //        {
    //            entry = _placeholdersCache[id];
    //            if (entry.ModificationTime < time)
    //            {
    //                _placeholdersCache.Remove(id);
    //                entry = null;
    //            }
    //        }

    //        if (entry == null)
    //        {
    //            entry = new PrintTemplatePlaceholdersCache();
    //            entry.ModificationTime = time;
    //            entry.Placeholders = PrintTemplateUtils.UnpackPlaceholders(printTemplateRow.GetString(PrintTemplate.Body));
    //        }

    //        bool result = true;
    //        foreach (object pch in entry.Placeholders)
    //        {
    //            var p = pch as e2DocumentPlaceholder;
    //            var t = pch as e2DocumentTable;
    //            if (p == null && t == null)
    //            {
    //                continue;
    //            }
    //            var query = (p == null) ? t.CustomQuery : p.DataField;
    //            this.ExplainQuery(query);
    //            if (this.LastQueryScalarFailed)
    //            {
    //                result = false;
    //            }
    //        }

    //        return result;
    //    }

    //    public void SetVar(string name, object value)
    //    {
    //        if (_vars.ContainsKey(name))
    //            _vars[name] = value;
    //        else
    //            _vars.Add(name, value);
    //    }
    //}

    //public class PrintTemplatePlaceholdersCache
    //{
    //    public DateTime ModificationTime;
    //    public ArrayList Placeholders;
    //}

    //[XmlRoot]
    //public class e2DocumentTable
    //{
    //    private static XmlSerializer _serializer = new XmlSerializer(typeof(e2DocumentTable));

    //    public e2DocumentTable()
    //    {
    //        this.ID = Guid.NewGuid();
    //    }

    //    public static e2DocumentTable Deserialize(string s)
    //    {
    //        try
    //        {
    //            return (e2DocumentTable)_serializer.Deserialize(new StringReader(s));
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceWarning("Cannot deserialize e2DocumentTable from \r\n" + s + "\r\n" + ex.ToString());
    //            return new e2DocumentTable();
    //        }
    //    }

    //    public static string Serialize(e2DocumentTable table)
    //    {
    //        try
    //        {
    //            StringWriter sw = new StringWriter();
    //            _serializer.Serialize(sw, table);
    //            return sw.ToString();
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceWarning("Cannot serialize e2DocumentTable\r\n" + ex.ToString());
    //            return String.Empty;
    //        }
    //    }

    //    public List<e2DocumentTableColumn> Columns = new List<e2DocumentTableColumn>();

    //    public string Title
    //    {
    //        get;
    //        set;
    //    }

    //    public Guid ID
    //    {
    //        get;
    //        set;
    //    }

    //    public bool OrderNumberColumnVisible
    //    {
    //        get;
    //        set;
    //    }

    //    public string CustomQuery
    //    {
    //        get;
    //        set;
    //    }

    //    public string RenderHtml(PrintTemplateContext context)
    //    {
    //        MySqlDataTable table;
    //        List<e2DocumentTableColumn> columns;

    //        if (!String.IsNullOrEmpty(this.CustomQuery))
    //        {
    //            table = context.QueryVector(this.CustomQuery);
    //        }
    //        else
    //        {
    //            table = context.QueryVector(null);
    //        }

    //        if (this.Columns.Count == 0)
    //        {
    //            columns = new List<e2DocumentTableColumn>();
    //            if (context.Grid != null)
    //            {
    //                foreach (DataGridViewColumn col in context.Grid.Columns)
    //                {
    //                    if (!col.Visible)
    //                        continue;

    //                    columns.Add(new e2DocumentTableColumn()
    //                    {
    //                        DataField = col.DataPropertyName,
    //                        Title = col.HeaderText,
    //                        FormatString = col.DefaultCellStyle.Format
    //                    });
    //                }
    //            }
    //            else if (table.FieldMappings.Count > 0)
    //            {
    //                foreach (MySqlField field in table.FieldMappings.Keys)
    //                {
    //                    columns.Add(e2DocumentTableColumn.FromField(field));
    //                }
    //            }
    //            else if (table.Columns.Count > 0)
    //            {
    //                foreach (DataColumn column in table.Columns)
    //                {
    //                    columns.Add(new e2DocumentTableColumn()
    //                    {
    //                        DataField = column.ColumnName,
    //                        Title = column.Caption
    //                    });
    //                }
    //            }
    //        }
    //        else
    //        {
    //            columns = this.Columns;
    //        }

    //        return this.RenderTable(table, columns, context);
    //    }

    //    private Dictionary<e2DocumentTableColumn, Decimal> _sumCache = new Dictionary<e2DocumentTableColumn, decimal>();
    //    private string RenderTable(MySqlDataTable table, List<e2DocumentTableColumn> columns, PrintTemplateContext context)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append("<table>");

    //        sb.Append("<tr>");

    //        if (this.OrderNumberColumnVisible)
    //        {
    //            sb.Append("<th>");
    //            sb.Append("Lp");
    //            sb.Append("</th>");
    //        }

    //        foreach (e2DocumentTableColumn col in columns)
    //        {
    //            sb.Append("<th>");
    //            sb.Append(col.Title);
    //            sb.Append("</th>");
    //        }
    //        sb.Append("</tr>");

    //        if (table.Rows.Count > 0)
    //            context.Rows.Insert(0, table.GetRow(0));

    //        int lp = 0;
    //        bool summaryRowNeeded = false;
    //        _sumCache.Clear();
    //        foreach (MySqlDataRow row in table.Rows)
    //        {
    //            lp++;
    //            sb.Append("<tr>");

    //            if (this.OrderNumberColumnVisible)
    //            {
    //                sb.Append("<td>");
    //                sb.AppendFormat("{0}", lp);
    //                sb.Append("</td>");
    //            }

    //            foreach (e2DocumentTableColumn col in columns)
    //            {
    //                sb.Append("<td>");
    //                context.Rows[0] = row;
    //                sb.Append(col.RenderHtml(context));
    //                sb.Append("</td>");

    //                summaryRowNeeded |= col.SumTotal;

    //                if (col.SumTotal)
    //                {
    //                    decimal dVal = 0;
    //                    string val = Convert.ToString(context.QueryScalar(col.DataField));
    //                    if (String.IsNullOrEmpty(val))
    //                        val = col.DefaultValue;

    //                    decimal.TryParse(val, out dVal);
    //                    if (_sumCache.ContainsKey(col))
    //                    {
    //                        _sumCache[col] += dVal;
    //                    }
    //                    else
    //                    {
    //                        _sumCache.Add(col, dVal);
    //                    }
    //                }
    //            }

    //            sb.Append("</tr>");
    //        }

    //        if (summaryRowNeeded)
    //        {
    //            sb.Append("<tr>");
    //            if (this.OrderNumberColumnVisible)
    //            {
    //                sb.Append("<td class=\"e2DocumentTable_total\">");
    //                sb.Append("</td>");
    //            }

    //            foreach (e2DocumentTableColumn col in columns)
    //            {
    //                sb.Append("<td class=\"e2DocumentTable_total\">");

    //                if (_sumCache.ContainsKey(col))
    //                {
    //                    sb.Append(col.FormatValue(_sumCache[col]));
    //                }

    //                sb.Append("</td>");
    //            }
    //            sb.Append("</tr>");
    //        }

    //        if (table.Rows.Count > 0)
    //            context.Rows.RemoveAt(0);

    //        sb.Append("</table>");
    //        return sb.ToString();
    //    }


    //}

    //public class e2DocumentTableColumn : e2DocumentPlaceholder
    //{
    //    public bool Sortable
    //    {
    //        get;
    //        set;
    //    }

    //    public bool RightAlignment
    //    {
    //        get;
    //        set;
    //    }

    //    public bool SumTotal
    //    {
    //        get;
    //        set;
    //    }

    //    public override string ToString()
    //    {
    //        return this.Title;
    //    }



    //    public static e2DocumentTableColumn FromField(MySqlField field)
    //    {
    //        return new e2DocumentTableColumn()
    //                   {
    //                       Title = field.Caption,
    //                       DataField = field.FieldID,
    //                       FormatString = field.DefaultFormat
    //                   };
    //    }
    //}

    //public abstract class MySqlField : hsScaffoldField
    //{
    //    private bool _isPrimaryKey = false;
    //    private string _name;
    //    private Type _table;
    //    private string _caption;
    //    private string _category;
    //    private string _description;
    //    private object _defaultValue = null;
    //    private bool _nullable = false;
    //    private bool _gridColumnVisible = true;
    //    private Type _filteringControlType;
    //    private MySqlFieldAspect _aspect = MySqlFieldAspect.Default;

    //    public MySqlField()
    //    {
    //        _defaultValue = this.SqlDefaultValue;
    //    }

    //    public FieldInfo ModelField
    //    {
    //        get;
    //        set;
    //    }

    //    public Type PrefferedGridColumnType
    //    {
    //        get;
    //        protected set;
    //    }

    //    public static MySqlField FromDataColumn(DataColumn col)
    //    {
    //        if (col.ExtendedProperties.Contains("MySqlField"))
    //            return col.ExtendedProperties["MySqlField"] as MySqlField;

    //        else return null;
    //    }

    //    /// <summary>
    //    /// String defines this field type used in CREATE TABLE 
    //    /// and ALTER TABLE statement when updating database schema.
    //    /// </summary>
    //    public abstract string SqlType
    //    {
    //        get;
    //    }

    //    public string DefaultFormat
    //    {
    //        get;
    //        set;
    //    }

    //    public MySqlFieldAspect FormatedAs()
    //    {
    //        return _aspect;
    //    }

    //    public MySqlField FormatedAs(MySqlFieldAspect aspect)
    //    {
    //        _aspect = aspect;
    //        return this;
    //    }

    //    public virtual Type NativeType
    //    {
    //        get { return typeof(object); }
    //    }

    //    /// <summary>
    //    /// Default value at this field for new rows inserted into table.
    //    /// Typically the value is DBNull, but can be overrided and redefined
    //    /// in derived field types.
    //    /// </summary>
    //    public virtual object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return DBNull.Value;
    //        }
    //    }

    //    /// <summary>
    //    /// Name of column in database. It shouldn't contains any special 
    //    /// or white-space characters. The name of field is used in
    //    /// CREATE/ALTER TABLE while updating database schema and for
    //    /// SELECT statements.
    //    /// </summary>
    //    public virtual string Name
    //    {
    //        get
    //        {
    //            return _name;
    //        }
    //        set
    //        {
    //            _name = value;
    //        }
    //    }

    //    public string ColumnName
    //    {
    //        get
    //        {
    //            return _name;
    //        }
    //    }

    //    public static implicit operator string(MySqlField f)
    //    {
    //        return f.ColumnName;
    //    }

    //    /// <summary>
    //    /// Type of table class that contains this field.
    //    /// When field is created manually for custom queries Table is null.
    //    /// </summary>
    //    public Type Table
    //    {
    //        get
    //        {
    //            return _table;
    //        }
    //        set
    //        {
    //            _table = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Human-readable caption of field.
    //    /// Displayed on forms, grid captions and reports.
    //    /// </summary>
    //    public string Caption
    //    {
    //        get
    //        {
    //            return _caption;
    //        }
    //        set
    //        {
    //            _caption = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Human-readable category of field.
    //    /// Used to group fields in edit forms.
    //    /// </summary>
    //    public string Category
    //    {
    //        get
    //        {
    //            return _category;
    //        }
    //        set
    //        {
    //            _category = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Short text description of field.
    //    /// </summary>
    //    public string Description
    //    {
    //        get
    //        {
    //            return _description;
    //        }
    //        set
    //        {
    //            _description = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Current default value of this field. 
    //    /// Initially the MySqlDefaultValue is used, but can be redefined
    //    /// in runtime or by [FieldDefaultValue] attribute.
    //    /// </summary>
    //    public object DefaultValue
    //    {
    //        get
    //        {
    //            if (_defaultValue == null)
    //                return this.SqlDefaultValue;

    //            if (Object.Equals(_defaultValue, DefaultFieldValueAttribute.Values.CurrentDateTime))
    //            {
    //                return DateTime.Now;
    //            }
    //            else if (Object.Equals(_defaultValue, DefaultFieldValueAttribute.Values.ColorWhite))
    //            {
    //                return Color.White.ToArgb();
    //            }

    //            return _defaultValue;
    //        }
    //        set
    //        {
    //            _defaultValue = value;
    //        }
    //    }

    //    /// <summary>
    //    /// True, if field is primary key of table. False otherwise.
    //    /// </summary>
    //    public bool IsPrimaryKey
    //    {
    //        get
    //        {
    //            return _isPrimaryKey;
    //        }
    //        set
    //        {
    //            _isPrimaryKey = value;
    //        }
    //    }

    //    /// <summary>
    //    /// True, if field can contains BDNull value.
    //    /// </summary>
    //    public bool Nullable
    //    {
    //        get
    //        {
    //            return _nullable;
    //        }
    //        set
    //        {
    //            _nullable = value;
    //        }
    //    }

    //    public bool InputFocus = false;

    //    public bool InFriendlyName = false;

    //    /// <summary>
    //    /// True, if field is displayed in DataGridView.
    //    /// </summary>
    //    public bool GridColumnVisible
    //    {
    //        get
    //        {
    //            return _gridColumnVisible;
    //        }
    //        set
    //        {
    //            _gridColumnVisible = value;
    //        }
    //    }


    //    /// <summary>
    //    /// Type of filtering control.
    //    /// </summary>
    //    public Type FilteringControlType
    //    {
    //        get { return _filteringControlType; }
    //        set { _filteringControlType = value; }
    //    }


    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// equals given test value. Supports NULL value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Eq(object value)
    //    {
    //        if (value is DBNull)
    //            return this.IsNull;
    //        else
    //            return new MySqlWhere(this, "=", value);
    //    }

    //    public MySqlWhere IsNullOrEq(object value)
    //    {
    //        return new MySqlWhere(this.Eq(value).Or(this.IsNull));
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// greater than given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Gt(object value)
    //    {
    //        return new MySqlWhere(this, ">", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// greater or equal than given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere GtEq(object value)
    //    {
    //        return new MySqlWhere(this, ">=", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// less than given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Lt(object value)
    //    {
    //        return new MySqlWhere(this, "<", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// less or equals than given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere LtEq(object value)
    //    {
    //        return new MySqlWhere(this, "<=", value);
    //    }

    //    /// <summary>
    //    /// Creates sql IN expression that checks is current value 
    //    /// in given set
    //    /// </summary>
    //    /// <param name="value">Set to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere In(object value)
    //    {
    //        return new MySqlWhere(this, "IN", value);
    //    }

    //    /// <summary>
    //    /// Creates sql NOT IN expression that checks is current value 
    //    /// does not contains in given set. It realizes exclude operation
    //    /// </summary>
    //    /// <param name="value">Set of values to exclude</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere NotIn(object value)
    //    {
    //        return new MySqlWhere(this, "NOT IN", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// not equals given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere NotEq(object value)
    //    {
    //        return new MySqlWhere(this, "<>", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// text-like given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Like(object value)
    //    {
    //        return new MySqlWhere(this, "LIKE", value);
    //    }


    //    /// <summary>
    //    /// Creates sql WHERE expression that filter current field by
    //    /// given regex expression.
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public MySqlWhere Regexp(object value)
    //    {
    //        return new MySqlWhere(this, "REGEXP", value);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// start with given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere StartsWith(object value)
    //    {
    //        return Like(value.ToString() + "%");
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// ends with given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere EndsWith(object value)
    //    {
    //        return Like("%" + value.ToString());
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// contains substring of given test value.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Contains(object value)
    //    {
    //        return Like("%" + value.ToString() + "%");
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value
    //    /// betweend given values.
    //    /// </summary>
    //    /// <param name="value">Value to test.</param>
    //    /// <returns>MySqlWhere expression to filter query results.</returns>
    //    public MySqlWhere Between(object val1, object val2)
    //    {
    //        return new MySqlWhere(this, "BETWEEN", val1, "AND", val2);
    //    }

    //    public MySqlWhere InMonth(object val)
    //    {
    //        return MySqlExpression.Month(this).Eq(MySqlExpression.Month(val)).And(
    //            MySqlExpression.Year(this).Eq(MySqlExpression.Year(val)));
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value null.
    //    /// </summary>
    //    public MySqlWhere IsNull
    //    {
    //        get
    //        {
    //            return new MySqlWhere(this.ToString() + " IS NULL");
    //        }
    //    }

    //    public bool IsInternal
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return _name != null && _name.StartsWith("X_");
    //            }
    //            catch
    //            {
    //                return false;
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Returns field as given alias
    //    /// </summary>
    //    /// <param name="field"></param>
    //    /// <param name="alias"></param>
    //    /// <returns></returns>
    //    public MySqlExpression As(string alias)
    //    {
    //        var expression = new MySqlExpression(this.ToString());

    //        return expression.As(alias);
    //    }

    //    /// <summary>
    //    /// Creates sql WHERE expression that checks is current field value not null.
    //    /// </summary>
    //    public MySqlWhere IsNotNull
    //    {
    //        get
    //        {
    //            return new MySqlWhere(this.ToString() + " IS NOT NULL");
    //        }
    //    }

    //    /// <summary>
    //    /// Creates sql ORDER BY expression to sort query result 
    //    /// by this field ascending.
    //    /// </summary>
    //    public MySqlOrderBy Ascending
    //    {
    //        get
    //        {
    //            return new MySqlOrderBy(this, true);
    //        }
    //    }

    //    /// <summary>
    //    /// Creates sql ORDER BY expression to sort query result 
    //    /// by this field descending.
    //    /// </summary>
    //    public MySqlOrderBy Descending
    //    {
    //        get
    //        {
    //            return new MySqlOrderBy(this, false);
    //        }
    //    }

    //    public MySqlField In(string alias)
    //    {
    //        MySqlField field = new MySqlAliasField(this, alias);
    //        return field;
    //    }

    //    /// <summary>
    //    /// Gets string representation of fields.
    //    /// </summary>
    //    /// <returns>MySql escaped field name with table prefix.</returns>
    //    public override string ToString()
    //    {
    //        return String.Format("`{0}`.`{1}`", _table.Name, _name);
    //    }

    //    public virtual string FieldID
    //    {
    //        get
    //        {
    //            if (_table == null || _name == null)
    //                return Guid.NewGuid().ToString();
    //            else
    //                return String.Format("{0}.{1}", _table.Name, _name);
    //        }
    //    }

    //    public bool HasAttribute(Type attributeType)
    //    {
    //        if (this.ModelField == null)
    //            return false;

    //        return this.ModelField.IsDefined(attributeType, true);
    //    }

    //    public static MySqlExpression operator +(MySqlField x, object y)
    //    {
    //        return MySqlExpression.Add(x, y);
    //    }

    //    public static MySqlExpression operator -(MySqlField x, object y)
    //    {
    //        return MySqlExpression.Sub(x, y);
    //    }

    //    public static MySqlExpression operator -(object x, MySqlField y)
    //    {
    //        return MySqlExpression.Sub(x, y);
    //    }

    //    public static MySqlExpression operator *(MySqlField x, object y)
    //    {
    //        return MySqlExpression.Multiply(x, y);
    //    }

    //    public static MySqlExpression operator /(MySqlField x, object y)
    //    {
    //        return MySqlExpression.Divide(x, y);
    //    }

    //    public static MySqlExpression operator /(object x, MySqlField y)
    //    {
    //        return MySqlExpression.Divide(x, y);
    //    }

    //    public static MySqlExpression operator /(MySqlField x, MySqlField y)
    //    {
    //        return MySqlExpression.Divide(x, y);
    //    }
    //}

    //public class MySqlExpression : MySqlField
    //{
    //    public override bool Equals(object obj)
    //    {
    //        var o = obj as MySqlExpression;
    //        if (o != null && o.ToString() == this.ToString())
    //            return true;

    //        return base.Equals(obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return this.ToString().GetHashCode();
    //    }

    //    public override string Name
    //    {
    //        get
    //        {
    //            return this.ToString();
    //        }
    //        set
    //        {
    //            //base.Name = value;
    //        }
    //    }


    //    public static MySqlExpression Date(object field)
    //    {
    //        return new MySqlExpression("DATE", field);
    //    }

    //    public static MySqlExpression DateFormat(object field, string format)
    //    {
    //        return new MySqlExpression("DATE_FORMAT", field, format);
    //    }

    //    public static MySqlExpression Time(object field)
    //    {
    //        return new MySqlExpression("TIME", field);
    //    }

    //    public static MySqlExpression Substr(object field, int from)
    //    {
    //        return new MySqlExpression(
    //            String.Format("SUBSTR({0}, {1})", MySqlValueSerializer.Get(field), from));
    //    }

    //    public static MySqlExpression Substr(object field, int from, int len)
    //    {
    //        return new MySqlExpression(
    //            String.Format("SUBSTR({0}, {1}, {2})", MySqlValueSerializer.Get(field), from, len));
    //    }

    //    public static MySqlExpression Sum(object field)
    //    {
    //        return new MySqlExpression("SUM", field);
    //    }

    //    public static MySqlExpression SumZero(object field)
    //    {
    //        return MySqlExpression.IfNull(MySqlExpression.Sum(field), 0);
    //    }

    //    public static MySqlExpression Count(object field)
    //    {
    //        return new MySqlExpression("COUNT", field);
    //    }

    //    public static MySqlExpression CountDistinct(object field)
    //    {
    //        return new MySqlExpression(String.Format("COUNT(DISTINCT {0})", MySqlValueSerializer.Get(field)));
    //    }

    //    public static MySqlExpression CountAll()
    //    {
    //        return new MySqlExpression("COUNT(*)");
    //    }

    //    public static MySqlExpression CountAll(Type table)
    //    {
    //        return new MySqlExpression(string.Format("COUNT(`{0}`.*)", table.Name));
    //    }

    //    public static MySqlExpression Max(object field)
    //    {
    //        return new MySqlExpression("MAX", field);
    //    }

    //    public static MySqlExpression Min(object field)
    //    {
    //        return new MySqlExpression("MIN", field);
    //    }

    //    public static MySqlExpression Avg(object field)
    //    {
    //        return new MySqlExpression("AVG", field);
    //    }

    //    public static MySqlExpression Concat(params object[] args)
    //    {
    //        return new MySqlExpression("CONCAT", args);
    //    }

    //    public static MySqlExpression ConcatWS(params object[] args)
    //    {
    //        return new MySqlExpression("CONCAT_WS", args);
    //    }

    //    public static MySqlExpression Coalesce(object field, object defatult)
    //    {
    //        return new MySqlExpression("COALESCE", field, defatult);
    //    }

    //    public static MySqlExpression Greatest(params object[] args)
    //    {
    //        return new MySqlExpression("GREATEST", args);
    //    }

    //    public static MySqlExpression GroupConcat(object field, string separator)
    //    {
    //        return GroupConcat(field, field, separator);
    //    }

    //    public static MySqlExpression GroupConcat(object field, MySqlField orderBy)
    //    {
    //        return GroupConcat(field, orderBy, ",");
    //    }

    //    public static MySqlExpression GroupConcat(object field, object orderBy, string separator)
    //    {
    //        return new MySqlExpression(String.Format(
    //            "GROUP_CONCAT(DISTINCT {0} ORDER BY {1} DESC SEPARATOR {2})",
    //            field, orderBy, MySqlValueSerializer.Get(separator)));
    //    }

    //    public static MySqlExpression GroupConcat(object field, MySqlOrderBy orderBy, string separator)
    //    {
    //        return new MySqlExpression(String.Format(
    //            "GROUP_CONCAT(DISTINCT {0} ORDER BY {1} SEPARATOR {2})",
    //            field, orderBy, MySqlValueSerializer.Get(separator)));
    //    }

    //    public static MySqlExpression Add(object a, object b)
    //    {
    //        return new MySqlExpression("(" + Convert.ToString(a, CultureInfo.InvariantCulture) + ") + (" + Convert.ToString(b, CultureInfo.InvariantCulture) + ")");
    //    }

    //    public static MySqlExpression Sub(object a, object b)
    //    {
    //        return new MySqlExpression("(" + Convert.ToString(a, CultureInfo.InvariantCulture) + ") - (" + Convert.ToString(b, CultureInfo.InvariantCulture) + ")");
    //    }

    //    public static MySqlExpression Multiply(object a, object b)
    //    {
    //        return new MySqlExpression("(" + Convert.ToString(a, CultureInfo.InvariantCulture) + ") * (" + Convert.ToString(b, CultureInfo.InvariantCulture) + ")");
    //    }

    //    public static MySqlExpression Divide(object a, object b)
    //    {
    //        return new MySqlExpression("(" + Convert.ToString(a, CultureInfo.InvariantCulture) + ") / (" + Convert.ToString(b, CultureInfo.InvariantCulture) + ")");
    //    }

    //    public static MySqlExpression IfNull(object val, object alt)
    //    {
    //        return new MySqlExpression("IFNULL", new object[] { val, alt });
    //    }

    //    public static MySqlExpression Case(object a, object b, object c)
    //    {
    //        return new MySqlExpression(String.Format("CASE WHEN {0} THEN {1} ELSE {2} END", a, b, c));
    //    }

    //    public static MySqlExpression If(object val, object ok, object alt)
    //    {
    //        return new MySqlExpression("IF", new object[] { val, ok, alt });
    //    }

    //    public static MySqlExpression Cast(object val, string type)
    //    {
    //        return new MySqlExpression(String.Format("CAST( {0} AS {1})", val, type));
    //    }

    //    public static MySqlExpression CastAsDateTime(object val)
    //    {
    //        return Cast(val, "DateTime");
    //    }

    //    public static MySqlWhere SameDate(object field1, object field2)
    //    {
    //        try
    //        {
    //            if (field1 is MySqlField)
    //            {
    //                if (field2 is DateTime)
    //                {
    //                    return (field1 as MySqlField).Between(((DateTime)field2).Date.ToString(MySqlConfig.MySqlDateTimeFormat),
    //                        ((DateTime)field2).Date.AddDays(1).AddSeconds(-1).ToString(MySqlConfig.MySqlDateTimeFormat));
    //                }
    //                else if (field2 is String)
    //                {
    //                    DateTime dt = DateTime.Parse(field2 as String);
    //                    return (field1 as MySqlField).Between(dt.Date.ToString(MySqlConfig.MySqlDateTimeFormat),
    //                        dt.Date.AddDays(1).AddSeconds(-1).ToString(MySqlConfig.MySqlDateTimeFormat));
    //                }
    //            }
    //        }
    //        catch
    //        {
    //            Trace.WriteLine("[.SameDate] error");
    //        }

    //        return MySqlExpression.Date(field1).Eq(MySqlExpression.Date(field2));

    //        //return new MySqlExpression(field1).Eq(new MySqlExpression(field2));
    //    }

    //    public static MySqlWhere Match(object expr, params object[] fields)
    //    {
    //        List<string> fList = new List<string>();
    //        foreach (object field in fields)
    //            fList.Add(MySqlValueSerializer.Get(field));

    //        string sExpr = MySqlValueSerializer.Get(expr);

    //        return new MySqlWhere(String.Format("MATCH ({0}) AGAINST ({1} IN BOOLEAN MODE)",
    //            String.Join(",", fList.ToArray()),
    //            sExpr));
    //    }

    //    private string _expr = string.Empty;
    //    private string _as = string.Empty;

    //    public MySqlExpression(string expr)
    //    {
    //        _expr = expr;
    //    }

    //    public MySqlExpression(MySqlStatement statement)
    //    {
    //        statement.Prepare();
    //        _expr = statement.ToString();
    //    }

    //    public MySqlExpression(string func, params object[] args)
    //    {
    //        List<string> list = new List<string>();
    //        foreach (object arg in args)
    //        {
    //            list.Add(MySqlValueSerializer.Get(arg));
    //        }

    //        _expr = func + "(" + String.Join(",", list.ToArray()) + ")";
    //    }

    //    public MySqlExpression As(string name)
    //    {
    //        return this.As(name, name);
    //    }

    //    public MySqlExpression As(string name, string caption)
    //    {
    //        _as = name;
    //        this.Caption = caption;
    //        return this;
    //    }

    //    public override string ToString()
    //    {
    //        string s = "(" + _expr + ")";
    //        if (!String.IsNullOrEmpty(_as))
    //            s += " AS `" + _as + "`";

    //        return s;
    //    }


    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "MySqlExpression, do not use as column definition.";
    //        }
    //    }

    //    public static MySqlExpression Year(object x)
    //    {
    //        return new MySqlExpression("YEAR", x);
    //    }

    //    public static MySqlExpression Month(object x)
    //    {
    //        return new MySqlExpression("MONTH", x);
    //    }

    //    public static MySqlWhere Exists(MySqlStatement statement)
    //    {
    //        return new MySqlWhere(String.Format("EXISTS ({0})", statement));
    //    }

    //    public static MySqlWhere IsInString(object field, string @string)
    //    {
    //        return new MySqlWhere(String.Format("INSTR('{0}', {1})", @string, field));
    //    }

    //    public string ToSql()
    //    {
    //        return this.ToString();
    //    }

    //    public static MySqlExpression Replace(MySqlField field, string oldValue, string newValue)
    //    {
    //        return new MySqlExpression(String.Format("REPLACE({0}, '{1}', '{2}')", field, oldValue, newValue));
    //    }
    //}

    //public class MySqlWhere
    //{
    //    private string _where = String.Empty;

    //    /// <summary>
    //    /// Creates new empty expression.
    //    /// </summary>
    //    public MySqlWhere()
    //    {
    //    }

    //    public bool IsEmpty
    //    {
    //        get
    //        {
    //            return String.IsNullOrEmpty(_where) || String.Equals(_where.TrimStart('(').TrimEnd(')'), "true", StringComparison.InvariantCultureIgnoreCase);
    //        }
    //    }

    //    public static readonly MySqlWhere Empty = new MySqlWhere();

    //    /// <summary>
    //    /// Creates new expression using existing string.
    //    /// </summary>
    //    /// <param name="where">Expressiong string.</param>
    //    public MySqlWhere(string where)
    //    {
    //        if (where != null)
    //        {
    //            _where = where;
    //        }
    //    }

    //    /// <summary>
    //    /// Creates new expression as copy of existing MySqlWhere object.
    //    /// </summary>
    //    /// <param name="inner"></param>
    //    public MySqlWhere(MySqlWhere inner)
    //    {
    //        _where = Convert.ToString(inner);
    //    }

    //    //public static MySqlWhere DateBetween(object field, object d1, object d2)
    //    //{
    //    //    return 
    //    //        MySqlExpression.Date(field).Between(
    //    //            GetDateLower(d1),
    //    //            GetDateUpper(d2));

    //    //}

    //    public static MySqlWhere DateBetween(MySqlField field, object d1, object d2)
    //    {
    //        return field.Between(
    //                GetDateLower(d1),
    //                GetDateUpper(d2));
    //    }

    //    private static object GetDateLower(object d)
    //    {
    //        if (d is DateTime)
    //        {
    //            return ((DateTime)d).Date.ToString();
    //        }
    //        else
    //        {
    //            return MySqlExpression.Date(d);
    //        }
    //    }

    //    private static object GetDateUpper(object d)
    //    {
    //        if (d is DateTime)
    //        {
    //            return ((DateTime)d).Date.AddDays(1).AddSeconds(-1).ToString();
    //        }
    //        else
    //        {
    //            return MySqlExpression.Date(d);
    //        }
    //    }


    //    /// <summary>
    //    /// Creates new expression as binary operator.
    //    /// </summary>
    //    /// <param name="field">Field to test.</param>
    //    /// <param name="op">Operator.</param>
    //    /// <param name="value">Value to test.</param>
    //    public MySqlWhere(object field, string op, object value)
    //    {
    //        _where = String.Format("{0} {1} {2}",
    //            field, op, MySqlValueSerializer.Get(value));
    //    }

    //    /// <summary>
    //    /// Creates new expression with three argument operator.
    //    /// Designed for BETWEEN x AND y syntax.
    //    /// </summary>
    //    /// <param name="field">Field to test.</param>
    //    /// <param name="op1">First part of operator.</param>
    //    /// <param name="val1">First value to test.</param>
    //    /// <param name="op2">Second part of operator.</param>
    //    /// <param name="val2">Second value to test.</param>
    //    public MySqlWhere(object field, string op1, object val1, string op2, object val2)
    //    {
    //        _where = String.Format("{0} {1} {2} {3} {4}",
    //            field, op1, MySqlValueSerializer.Get(val1), op2, MySqlValueSerializer.Get(val2));
    //    }

    //    /// <summary>
    //    /// Combine current expression with given expression using AND operator.
    //    /// </summary>
    //    /// <param name="where">Expression to combine.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlWhere And(MySqlWhere where)
    //    {
    //        if (where != null)
    //        {
    //            if (String.IsNullOrEmpty(_where))
    //            {
    //                _where = String.Format(" {0}", where.ToString());
    //            }
    //            else
    //            {
    //                _where += String.Format(" AND {0}", where.ToString());
    //            }
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Combine current expression with given expression using OR operator.
    //    /// </summary>
    //    /// <param name="where">Expression to combine.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlWhere Or(MySqlWhere where)
    //    {
    //        if (where != null)
    //        {
    //            if (String.IsNullOrEmpty(_where))
    //            {
    //                _where += String.Format(" {0}", where.ToString());
    //            }
    //            else
    //            {
    //                _where += String.Format(" OR {0}", where.ToString());
    //            }
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets SQL string with defined expression.
    //    /// </summary>
    //    /// <returns>SQL WHERE clause.</returns>
    //    public override string ToString()
    //    {
    //        if (String.IsNullOrEmpty(_where))
    //            return "(TRUE)";
    //        else
    //            return "(" + _where + ")";
    //    }
    //}

    //public static class MySqlValueSerializer
    //{
    //    /// <summary>
    //    /// Gets string representation of given value.
    //    /// </summary>
    //    /// <param name="value">Value to serialize.</param>
    //    /// <returns>MySql safe value string.</returns>
    //    public static string Get(object value)
    //    {
    //        string strVal;
    //        if (value is MySqlTmpField)
    //        {
    //            strVal = value.ToString();
    //        }
    //        else if (value is MySqlField)
    //        {
    //            strVal = value.ToString();
    //        }
    //        else if (value is MySqlVar)
    //        {
    //            strVal = value.ToString();
    //        }
    //        if (value is string)
    //        {
    //            strVal = MySqlUtils.GetEscaped(value);
    //        }
    //        else if (value is bool)
    //        {
    //            strVal = ((bool)value) ? "1" : "0";
    //        }
    //        else if (value is DBNull || value == null)
    //            strVal = "NULL";
    //        else if (value is MySqlSelect)
    //            strVal = "(" + value.ToString() + ")";
    //        else if (value is DateTime)
    //            strVal = "'" + ((DateTime)value).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss") + "'";
    //        else if (value is Guid)
    //            strVal = "'" + value.ToString() + "'";
    //        else if (value is Color)
    //            strVal = ((Color)value).ToArgb().ToString();
    //        else if (value is Enum)
    //            strVal = ((int)value).ToString();
    //        else if (hsObject.CheckInterfaceImplement(value, typeof(IEnumerable<>)))
    //        {
    //            strVal = "";

    //            var type = value.GetType().GetGenericArguments()[0];

    //            var countMethod = typeof(Enumerable)
    //                .GetMethods(BindingFlags.Public | BindingFlags.Static)
    //                .Single(m => m.Name == "Count" && m.GetParameters().Length == 1)
    //                .MakeGenericMethod(type);
    //            var count = (int)countMethod.Invoke(null, new[] { value });

    //            if (count == 0)
    //                throw new ArgumentException("Empty collection passed");

    //            var lastMethod = typeof(Enumerable)
    //                .GetMethods(BindingFlags.Public | BindingFlags.Static)
    //                .Single(m => m.Name == "Last" && m.GetParameters().Length == 1)
    //                .MakeGenericMethod(type);
    //            var last = lastMethod.Invoke(null, new[] { value });

    //            strVal += "(";
    //            foreach (var item in (value as IEnumerable))
    //            {
    //                if (!item.Equals(last))
    //                {
    //                    strVal += Get(item);
    //                    strVal += ", ";
    //                }
    //            }
    //            strVal += Get(last);
    //            strVal += ")";
    //        }
    //        else
    //            strVal = Convert.ToString(value, CultureInfo.InvariantCulture);

    //        return strVal;
    //    }
    //}

    //public static class hsObject
    //{
    //    public static object GetDefault<T>(this T obj)
    //    {
    //        if (obj.GetType().IsValueType)
    //        {
    //            return Activator.CreateInstance(obj.GetType());
    //        }

    //        return null;
    //    }

    //    public static bool IsDefaultOrEmpty<T>(this T obj)
    //    {
    //        if (obj is string)
    //        {
    //            return String.IsNullOrEmpty(obj as string);
    //        }
    //        else
    //        {
    //            return obj.Equals(obj.GetDefault());
    //        }
    //    }

    //    public static void ShallowCopyFieldValues(object source, object dest)
    //    {
    //        if (source == null || dest == null)
    //            return;

    //        var sType = source.GetType();
    //        var dType = dest.GetType();

    //        while (!sType.Equals(typeof(object)))
    //        {
    //            foreach (FieldInfo sf in sType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    //            {
    //                var t = dType;
    //                FieldInfo df = null;
    //                while (df == null && !t.Equals(typeof(object)))
    //                {
    //                    df = t.GetField(sf.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    //                    t = t.BaseType;
    //                }

    //                if (df != null)
    //                {
    //                    df.SetValue(dest, sf.GetValue(source));
    //                }
    //            }

    //            sType = sType.BaseType;
    //        }
    //    }

    //    public static bool CheckInterfaceImplement(Object obj, Type interfaceType)
    //    {
    //        bool result = false;

    //        Type[] interfacesList = obj.GetType().GetInterfaces();
    //        foreach (Type t in interfacesList)
    //        {
    //            if (t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
    //            {
    //                result = true;
    //                break;
    //            }
    //        }

    //        return result;
    //    }

    //    public static T ConvertFromDbValueTo<T>(this object source)
    //    {
    //        if (source == null || source == DBNull.Value)
    //        {
    //            return default(T);
    //        }

    //        return (T)source;
    //    }
    //}

    //public class MySqlSelect : MySqlStatement, IEnumerable, ICloneable
    //{
    //    private static readonly ILog Log = LogExtensions.GetCurrentClassLogger();

    //    private List<MySqlField> _fields = new List<MySqlField>();
    //    private string _from;
    //    private List<object> _tables = new List<object>();
    //    private List<object> _ignoreDeleted = new List<object>();
    //    private MySqlWhere _where = null;
    //    private string _sql = String.Empty;
    //    private Dictionary<string, object> _variables = new Dictionary<string, object>();
    //    private bool _prepared = false;
    //    private readonly List<MySqlSelect> _unionSelects = new List<MySqlSelect>();
    //    private readonly bool _getRemoved;

    //    #region ICloneable Members

    //    public object Clone()
    //    {
    //        MySqlSelect s = new MySqlSelect();
    //        s._fields = this._fields;
    //        s._from = this._from;
    //        s._tables = new List<object>(this._tables);
    //        s._ignoreDeleted = new List<object>(this._ignoreDeleted);
    //        s._where = new MySqlWhere(_where);
    //        s._sql = this._sql;
    //        return s;
    //    }

    //    #endregion

    //    /// <summary>
    //    /// Creates new empty select statement.
    //    /// </summary>
    //    public MySqlSelect()
    //    {
    //        Distinct = false;
    //    }

    //    public MySqlSelect(bool getRemoved)
    //    {
    //        _getRemoved = getRemoved;
    //    }

    //    public bool Distinct
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Get list of used tables.
    //    /// </summary>
    //    public List<object> Tables
    //    {
    //        get
    //        {
    //            return _tables;
    //        }
    //    }


    //    /// <summary>
    //    /// Adds required fields to select.
    //    /// </summary>
    //    /// <param name="fields">List of fields for select.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect AddFields(params object[] fields)
    //    {
    //        foreach (object field in fields)
    //        {
    //            if (field is MySqlAllFields)
    //            {
    //                MySqlAllFields all = field as MySqlAllFields;
    //                foreach (MySqlField partField in all.Fields)
    //                {
    //                    _fields.Add(partField);
    //                }
    //            }
    //            else if (field is MySqlField)
    //            {
    //                _fields.Add(field as MySqlField);
    //            }
    //        }

    //        return this;
    //    }

    //    public MySqlSelect UnionAll(MySqlSelect select)
    //    {
    //        _unionSelects.Add(select);
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets list of fields for select.
    //    /// </summary>
    //    public List<MySqlField> Fields
    //    {
    //        get
    //        {
    //            return _fields;
    //        }
    //    }

    //    public void SetFrom(string s)
    //    {
    //        _from = s;
    //    }

    //    /// <summary>
    //    /// Gets SQL select statement as string.
    //    /// </summary>
    //    /// <returns>SQL SELECT clause.</returns>
    //    public override string ToString()
    //    {
    //        this.OnPrepare();

    //        StringBuilder sb = new StringBuilder();
    //        var prefix = String.Empty;
    //        if (this.Distinct)
    //            prefix += " DISTINCT ";
    //        sb.AppendLine().AppendLine();

    //        sb.AppendLine("SELECT " + prefix);

    //        bool first = true;
    //        foreach (object field in _fields)
    //        {
    //            if (field == null)
    //                continue;

    //            if (field.GetType() == typeof(MySqlDynamic))
    //                throw new InvalidOperationException("Dynamic fields cannot be used in SELECT queries.");

    //            if (first)
    //                first = false;
    //            else
    //                sb.Append(",\r\n");

    //            sb.Append("    " + field.ToString());
    //        }

    //        sb.AppendLine().AppendLine();

    //        sb.Append(_from);

    //        sb.AppendLine().AppendLine();

    //        if (_where != null)
    //        {
    //            sb.Append(" WHERE ");
    //            sb.Append(_where.ToString());
    //        }

    //        sb.AppendLine().AppendLine();

    //        sb.Append(" ");
    //        sb.AppendLine(_sql);

    //        string str = sb.ToString();

    //        foreach (string varName in _variables.Keys)
    //        {
    //            str = str.Replace("[" + varName + "]", MySqlValueSerializer.Get(_variables[varName]));
    //        }

    //        if (_unionSelects.IsEmpty())
    //        {
    //            return str;
    //        }
    //        else
    //        {
    //            var selects = _unionSelects.Select(@select => @select.ToString()).Materialize();
    //            selects.Insert(0, str);
    //            return selects.JoinStrings(" UNION ALL ");
    //        }
    //    }

    //    /// <summary>
    //    /// Gets SQL select statement as string wrapped into () expression braces.
    //    /// </summary>
    //    /// <returns>SQL SELECT clause.</returns>
    //    public string ToExpr()
    //    {
    //        return "(" + this.ToString() + ")";
    //    }

    //    /// <summary>
    //    /// Determines if select statement syntax is valid.
    //    /// Only syntax is checked, this validation doesn't prevent
    //    /// runtime errors.
    //    /// </summary>
    //    public bool IsValid => _fields.Count > 0 && _from != null && _from.Contains("FROM");

    //    /// <summary>
    //    /// Appends FROM clause to select.
    //    /// </summary>
    //    /// <param name="table">Table class or name.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect From(object table)
    //    {
    //        return From(table, null);
    //    }

    //    public MySqlSelect From(object table, string alias)
    //    {
    //        string fromStatement;

    //        if (table is MySqlStatement)
    //        {
    //            fromStatement = $"({table})";
    //        }
    //        else
    //        {
    //            fromStatement = this.Db.ObjectModel.GetTableName(table);
    //        }

    //        _from = $" FROM {fromStatement}";

    //        if (alias != null)
    //        {
    //            _from += " " + alias;
    //            _tables.Add(alias);
    //        }
    //        else
    //        {
    //            _tables.Add(this.Db.ObjectModel.GetTable(table));
    //        }

    //        return this;
    //    }

    //    public MySqlSelect From(string expr, string alias)
    //    {
    //        _from = $" FROM {expr} AS {alias}";
    //        return this;
    //    }

    //    public MySqlSelect UseIndex(string indexName)
    //    {
    //        _from += $" USE INDEX({indexName}) ";
    //        return this;
    //    }

    //    public MySqlSelect ShowDeleted(params object[] tables)
    //    {
    //        foreach (object t in tables)
    //            _ignoreDeleted.Add(t);

    //        return this;
    //    }

    //    /// <summary>
    //    /// Shows or hides deleted entries
    //    /// </summary>
    //    /// <param name="show">If true, it shows deleted entries from specified tables</param>
    //    /// <param name="tables"></param>
    //    /// <returns></returns>
    //    public MySqlSelect ToggleDeleted(bool show, params object[] tables)
    //    {
    //        if (show)
    //        {
    //            foreach (object t in tables)
    //                _ignoreDeleted.Add(t);
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends LEFT JOIN clause to select.
    //    /// </summary>
    //    /// <param name="table">Table class or name.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect LeftJoin(object table)
    //    {
    //        return LeftJoin(table, null);
    //    }

    //    public MySqlSelect LeftJoin(MySqlField foreignKeyInAlreadySelectedTable, MySqlField keyInTableToJoin)
    //    {
    //        return LeftJoin(keyInTableToJoin.Table, null).On(foreignKeyInAlreadySelectedTable.Eq(keyInTableToJoin));
    //    }

    //    public MySqlSelect LeftJoin(object table, string alias)
    //    {
    //        string t = this.Db.ObjectModel.GetTableName(table);
    //        _from += String.Format(" LEFT JOIN " + t);

    //        if (alias != null)
    //        {
    //            _from += " " + alias;
    //            _tables.Add(alias);
    //        }
    //        else
    //        {
    //            _tables.Add(this.Db.ObjectModel.GetTable(table));
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends INNER JOIN clause to select.
    //    /// </summary>
    //    /// <param name="table">Table class or name.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect InnerJoin(object table)
    //    {
    //        return InnerJoin(table, null);
    //    }

    //    public MySqlSelect InnerJoin(MySqlField foreignKeyInAlreadySelectedTable, MySqlField keyInTableToJoin)
    //    {
    //        return InnerJoin(keyInTableToJoin.Table, null).On(foreignKeyInAlreadySelectedTable.Eq(keyInTableToJoin));
    //    }

    //    public MySqlSelect InnerJoin(object table, string alias)
    //    {
    //        string t = this.Db.ObjectModel.GetTableName(table);

    //        _from += String.Format(" INNER JOIN " + t);

    //        if (alias != null)
    //        {
    //            _from += " " + alias;
    //            _tables.Add(alias);
    //        }
    //        else
    //        {
    //            _tables.Add(this.Db.ObjectModel.GetTable(table));
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends CROSS JOIN clause to select.
    //    /// </summary>
    //    /// <param name="table">Table class or name.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect CrossJoin(object table)
    //    {
    //        string t = this.Db.ObjectModel.GetTableName(table);
    //        _tables.Add(this.Db.ObjectModel.GetTable(table));
    //        _from += String.Format(" CROSS JOIN " + t);
    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends ON clause to select.
    //    /// </summary>
    //    /// <param name="rule">Join rule expression.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect On(MySqlWhere rule)
    //    {
    //        if (_ignoreDeleted.Contains(_tables[_tables.Count - 1]))
    //        {
    //            _from += String.Format(" ON {0}", rule.ToString());
    //        }
    //        else
    //        {
    //            string lastTable = this.Db.ObjectModel.GetTableName(_tables[_tables.Count - 1]);

    //            object[] jd = this.Db.ObjectModel.GetTable(lastTable).GetCustomAttributes(typeof(JoinDeletedAttribute), true);

    //            MySqlWhere fullWhere = rule;
    //            if (jd.Length == 0)
    //            {
    //                fullWhere = fullWhere.And(new MySqlWhere(lastTable + ".`X_RemoveTime` IS NULL"));
    //            }

    //            _from += String.Format(" ON {0}", fullWhere.ToString());
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends WHERE clause to select.
    //    /// </summary>
    //    /// <param name="rule">Filter rule expression.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect Where(MySqlWhere rule)
    //    {
    //        //_from += String.Format(" WHERE {0}", rule.ToString());

    //        if (_where == null)
    //        {
    //            _where = rule;
    //        }
    //        else
    //        {
    //            _where = new MySqlWhere(_where).And(rule);
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends ORDER BY clause to select.
    //    /// </summary>
    //    /// <param name="field">Field to sort.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect OrderBy(object field)
    //    {
    //        _sql += String.Format(" ORDER BY {0}", field.ToString());
    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends ORDER BY clause to select.
    //    /// </summary>
    //    /// <param name="fields">Fields to sort.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect OrderBy(params object[] fields)
    //    {
    //        _sql += String.Format(" ORDER BY");

    //        for (int i = 0; i < fields.Length; i++)
    //        {
    //            _sql += String.Format(" {0}", fields[i]);

    //            if (i != fields.Length - 1)
    //                _sql += ",";
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends GROUP BY clause to select.
    //    /// </summary>
    //    /// <param name="field">Field to group by.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect GroupBy(object field)
    //    {
    //        _sql += String.Format(" GROUP BY {0}", field.ToString());

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends GROUP BY clause to select.
    //    /// </summary>
    //    /// <param name="field">Field to group by.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect GroupBy(params object[] fields)
    //    {
    //        _sql += " GROUP BY";

    //        for (int i = 0; i < fields.Length; i++)
    //        {
    //            _sql += String.Format(" {0}", fields[i]);

    //            if (i != fields.Length - 1)
    //                _sql += ",";
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends HAVING clause to select.
    //    /// </summary>
    //    /// <param name="rule">Filer rule expression.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect Having(MySqlWhere rule)
    //    {
    //        _sql += String.Format(" HAVING {0}", rule.ToString());
    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends LIMIT clause to select.
    //    /// </summary>
    //    /// <param name="count">Maixum number of selected rows.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect Limit(int count)
    //    {
    //        _sql += String.Format(" LIMIT {0}", count);
    //        return this;
    //    }

    //    /// <summary>
    //    /// Appends LIMIT clause to select.
    //    /// </summary>
    //    /// <param name="offset">First selected row index (from top).</param>
    //    /// <param name="count">Maixum number of selected rows.</param>
    //    /// <returns>Self.</returns>
    //    public MySqlSelect Limit(int offset, int count)
    //    {
    //        _sql += String.Format(" LIMIT {0},{1}", offset, count);
    //        return this;
    //    }


    //    /// <summary>
    //    /// Gets select result as MySqlDataTable.
    //    /// </summary>
    //    /// <returns>MySqlDataTable with all rows from select result.</returns>
    //    public MySqlDataTable GetTable()
    //    {
    //        return this.GetTable(true);
    //    }

    //    public void AppendRowsToTable(MySqlDataTable table)
    //    {
    //        this.Prepare();
    //        string cmd = this.ToString();

    //        try
    //        {
    //            this.Db.OnQueryBegin(cmd);

    //            lock (MySqlDb.GlobalLock)
    //            {
    //                using (var selCmd = new MySqlCommand(cmd, this.Db.Connection))
    //                {
    //                    table.Load(selCmd.ExecuteReader());
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, $"Cannot append rows using {cmd}");
    //        }
    //    }

    //    /// <summary>
    //    /// Gets select result as MySqlDataTable.
    //    /// </summary>
    //    /// <returns>MySqlDataTable with all rows from select result.</returns>
    //    public MySqlDataTable GetTable(bool connectedWithBindingSource)
    //    {
    //        MySqlBindingSource source = new MySqlBindingSource();
    //        source.DbComponent.Db = this.Db;
    //        if (connectedWithBindingSource)
    //        {
    //            source.SelectSql = this;
    //        }
    //        else
    //        {
    //            source.FillTableOnly(this);
    //        }

    //        source.DataTable.KeyColumns = new List<DataColumn>();
    //        foreach (MySqlField field in this.Fields)
    //        {
    //            if (field.IsPrimaryKey && source.DataTable.FieldMappings.ContainsKey(field))
    //            {
    //                source.DataTable.KeyColumns.Add(source.DataTable.FieldMappings[field]);
    //            }
    //        }

    //        return source.DataTable;
    //    }

    //    /// <summary>
    //    /// Gets first row of select result or null if no data.
    //    /// </summary>
    //    /// <returns>MySqlDataRow row with connected table and binding source for updates.</returns>
    //    public MySqlDataRow GetRow()
    //    {
    //        MySqlDataTable table = this.GetTable();
    //        if (table.Rows.Count > 0)
    //        {
    //            return table.Rows[0] as MySqlDataRow;
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    public hsDataReaderWraper GetReader()
    //    {
    //        hsDataReaderWraper r = new hsDataReaderWraper(this.ExecSqlForReading());
    //        return r;
    //    }

    //    /// <summary>
    //    /// Gets value at first cell of first row or null if no data.
    //    /// </summary>
    //    /// <returns>Cell value.</returns>
    //    public object GetScalar()
    //    {
    //        MySqlDataRow row = this.GetRow();
    //        if (row != null)
    //        {
    //            return row[0];
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets value at first cell of first row or null if no data.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="def"></param>
    //    /// <returns></returns>
    //    public T GetScalar<T>()
    //    {
    //        object val = this.GetScalar();

    //        try
    //        {
    //            if (typeof(T) == typeof(Guid))
    //                return (T)((object)new Guid(Convert.ToString(val)));

    //            return (T)Convert.ChangeType(val, typeof(T));
    //        }
    //        catch
    //        {
    //            return Activator.CreateInstance<T>();
    //        }
    //    }

    //    /// <summary>
    //    /// Gets value at first cell of first row or null if no data.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    /// <param name="def"></param>
    //    /// <returns></returns>
    //    public T GetScalar<T>(T def)
    //    {
    //        object val = this.GetScalar();
    //        if (val == null || val is DBNull)
    //            return def;

    //        try
    //        {
    //            return (T)Convert.ChangeType(val, typeof(T));
    //        }
    //        catch
    //        {
    //            return def;
    //        }
    //    }



    //    protected override void OnPrepare()
    //    {
    //        if (_prepared)
    //        {
    //            return;
    //        }

    //        foreach (object tableObj in _tables)
    //        {
    //            if (_ignoreDeleted.Contains(tableObj))
    //            {
    //                continue;
    //            }

    //            if (tableObj != _tables[0] &&
    //                this.Db.ObjectModel.GetTable(tableObj)
    //                    .GetCustomAttributes(typeof(JoinDeletedAttribute), true)
    //                    .Length > 0)
    //            {
    //                continue;
    //            }

    //            string table;

    //            if (tableObj is String)
    //            {
    //                table = tableObj.ToString();
    //            }
    //            else if (tableObj is Type)
    //            {
    //                table = ((Type)tableObj).Name;
    //            }
    //            else
    //            {
    //                return;
    //            }

    //            Where(_getRemoved ? new MySqlWhere() : new MySqlWhere("`" + table + "`.`X_RemoveTime` IS NULL"));
    //        }

    //        _prepared = true;
    //    }

    //    public void ClearVariables()
    //    {
    //        _variables.Clear();
    //    }

    //    public void SetVariable(string name, object value)
    //    {
    //        if (_variables.ContainsKey(name))
    //        {
    //            _variables[name] = value;
    //        }
    //        else
    //        {
    //            _variables.Add(name, value);
    //        }

    //        foreach (var unionSelect in _unionSelects)
    //        {
    //            unionSelect.SetVariable(name, value);
    //        }
    //    }

    //    public IEnumerable GetSelectedFields()
    //    {
    //        foreach (object field in _fields)
    //        {
    //            if (field is MySqlAllFields)
    //            {
    //                foreach (object inner in (field as MySqlAllFields).Fields)
    //                    yield return inner;
    //            }
    //            else
    //            {
    //                yield return field;
    //            }
    //        }
    //    }

    //    #region IEnumerable Members

    //    public IEnumerator GetEnumerator()
    //    {
    //        return this.GetTable().Rows.GetEnumerator();
    //    }

    //    #endregion

    //    public List<T> GetList<T>(MySqlField f)
    //    {
    //        List<T> list = new List<T>();
    //        foreach (MySqlDataRow row in this)
    //        {
    //            try
    //            {
    //                if (typeof(T) == typeof(Guid))
    //                    list.Add((T)(object)new Guid(row.GetString(f)));
    //                else
    //                    list.Add((T)Convert.ChangeType(row[f], typeof(T)));
    //            }
    //            catch
    //            {
    //            }
    //        }

    //        return list;
    //    }

    //    public List<T> GetList<T>(string f)
    //    {
    //        List<T> list = new List<T>();
    //        foreach (MySqlDataRow row in this)
    //        {
    //            try
    //            {
    //                if (typeof(T) == typeof(Guid))
    //                    list.Add((T)(object)new Guid(row.GetString(f)));
    //                else
    //                    list.Add((T)Convert.ChangeType(row[f], typeof(T)));
    //            }
    //            catch
    //            {
    //            }
    //        }

    //        return list;
    //    }
    //}

    //public partial class MySqlDataTable : hsDataTable
    //{
    //    public class FieldEqualityComparer : IEqualityComparer<MySqlField>
    //    {
    //        #region IEqualityComparer<MySqlField> Members

    //        public bool Equals(MySqlField x, MySqlField y)
    //        {
    //            return String.Equals(x.ToString(), y.ToString());
    //        }

    //        public int GetHashCode(MySqlField obj)
    //        {
    //            return obj.ToString().GetHashCode();
    //        }

    //        #endregion
    //    }

    //    public class FieldMappingDictionary : Dictionary<MySqlField, DataColumn>
    //    {
    //        public FieldMappingDictionary()
    //            : base(new FieldEqualityComparer())
    //        {
    //        }
    //    }

    //    private FieldMappingDictionary _fieldMappings = new FieldMappingDictionary();
    //    private List<object> _summaryFields = new List<object>();
    //    private List<decimal> _summaryValues = new List<decimal>();
    //    private Dictionary<object, string> _summaryFormatStrings = new Dictionary<object, string>();

    //    private MySqlBindingSource _bindingSource;

    //    /// <summary>
    //    /// Creates a new instance of MySqlDataTable.
    //    /// </summary>
    //    public MySqlDataTable()
    //    {
    //        InitializeComponent();
    //        //hsObjectProfiler.Default.InstanceCreated(this);
    //    }

    //    ~MySqlDataTable()
    //    {
    //        hsObjectProfiler.Default.InstanceDeleted(this);
    //    }

    //    public override DataTable Clone()
    //    {
    //        MySqlDataTable t = (MySqlDataTable)base.Clone();
    //        foreach (MySqlField f in this.FieldMappings.Keys)
    //        {
    //            t.FieldMappings.Add(f, t.Columns[this.FieldMappings[f].ColumnName]);
    //        }


    //        return t;
    //    }


    //    public MySqlDataTable CloneTable()
    //    {
    //        MySqlDataTable t = (MySqlDataTable)this.Clone();
    //        foreach (DataRow row in this.Rows)
    //        {
    //            t.ImportRow(row);
    //        }

    //        return t;
    //    }

    //    public void SetColumnVisibility(DataColumn col, bool val)
    //    {
    //        if (!col.ExtendedProperties.ContainsKey("ColumnVisibility"))
    //            col.ExtendedProperties.Add("ColumnVisibility", val);
    //        else
    //            col.ExtendedProperties["ColumnVisibility"] = val;
    //    }

    //    public bool GetColumnVisibility(DataColumn col)
    //    {
    //        if (col.ExtendedProperties.ContainsKey("ColumnVisibility"))
    //            return (bool)col.ExtendedProperties["ColumnVisibility"];
    //        else
    //            return true;
    //    }

    //    public void DisableDefaultValues()
    //    {
    //        foreach (DataColumn col in this.Columns)
    //        {
    //            col.AllowDBNull = true;
    //            col.DefaultValue = DBNull.Value;
    //        }
    //    }


    //    /// <summary>
    //    /// Creates a new instance of MySqlDataTable connected to given binding source.
    //    /// </summary>
    //    /// <param name="bindingSource"></param>
    //    public MySqlDataTable(MySqlBindingSource bindingSource)
    //    {
    //        InitializeComponent();
    //        InitializeComponentCustom(bindingSource);
    //    }

    //    private void InitializeComponentCustom(MySqlBindingSource bindingSource)
    //    {
    //        _bindingSource = bindingSource;
    //    }

    //    /// <summary>
    //    /// Creates a new instance of MySqlDataTable (design-time support).
    //    /// </summary>
    //    /// <param name="container"></param>
    //    public MySqlDataTable(IContainer container)
    //    {
    //        //container.Add(this);

    //        InitializeComponent();
    //    }


    //    /// <summary>
    //    /// Gets or sets binding source that using this table.
    //    /// </summary>
    //    [Browsable(false)]
    //    public MySqlBindingSource BindingSource
    //    {
    //        get
    //        {
    //            return _bindingSource;
    //        }
    //        set
    //        {
    //            _bindingSource = value;
    //        }
    //    }

    //    public List<object> SummaryFields
    //    {
    //        get { return _summaryFields; }
    //    }

    //    public List<decimal> SummaryValues
    //    {
    //        get { return _summaryValues; }
    //    }

    //    public Dictionary<object, string> SummaryFormatStrings
    //    {
    //        get { return _summaryFormatStrings; }
    //    }

    //    public void AddSummaryField(object field, string formatString)
    //    {
    //        _summaryFields.Add(field);
    //        _summaryFormatStrings.Add(field, formatString);
    //    }

    //    /// <summary>
    //    /// Reads field list from select and connects each field to its data column
    //    /// in table. Invoke this method always after Fill method from DataAdapter
    //    /// and always with the same select statement, that was used for Fill operation.
    //    /// </summary>
    //    /// <param name="select">Select statement object, that was used for Fill operation.</param>
    //    public void UpdateFieldMappings(MySqlSelect select)
    //    {
    //        _fieldMappings.Clear();

    //        int columnNo = 0;
    //        foreach (MySqlField field in select.Fields)
    //        {
    //            DataColumn column = this.Columns[columnNo];

    //            try
    //            {
    //                if (!_fieldMappings.ContainsKey(field))
    //                    _fieldMappings.Add(field, column);
    //            }
    //            catch
    //            {
    //                // skip duplicates
    //                //Trace.TraceWarning("Cannot add field mapping {0}->{1}\n{2}", field, column.ColumnName, new StackTrace());
    //            }

    //            try
    //            {
    //                column.DefaultValue = field.DefaultValue;
    //            }
    //            catch
    //            {
    //            }

    //            column.Caption =
    //                String.IsNullOrEmpty(field.Caption) ? field.Name : field.Caption;


    //            column.AllowDBNull = true; // field.Nullable;
    //            column.ExtendedProperties.Add("MySqlField", field);
    //            columnNo++;
    //        }

    //    }

    //    /// <summary>
    //    /// Gets MySqlField to DataColumn mappings for this table.
    //    /// </summary>
    //    public FieldMappingDictionary FieldMappings
    //    {
    //        get
    //        {
    //            return _fieldMappings;
    //        }
    //        set
    //        {
    //            _fieldMappings = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Creates a new MySqlDataRow, that can be added to this table.
    //    /// </summary>
    //    /// <returns>New MySqlDataRow instance.</returns>
    //    public MySqlDataRow NewMySqlRow()
    //    {
    //        MySqlDataRow row = this.NewRow() as MySqlDataRow;
    //        row.Added = false;
    //        return row;
    //    }

    //    public MySqlDataRow GetRow(int index)
    //    {
    //        if (index >= this.Rows.Count)
    //            return null;

    //        return (MySqlDataRow)this.Rows[index];
    //    }

    //    public MySqlField GetColumnField(DataColumn column)
    //    {
    //        if (column.ExtendedProperties.ContainsKey("MySqlField"))
    //            return column.ExtendedProperties["MySqlField"] as MySqlField;

    //        return null;
    //    }

    //    public IEnumerable<string> GetStringsFromColumn(MySqlField field)
    //    {
    //        foreach (object val in this.GetValuesFromColumn(field))
    //            yield return Convert.ToString(val);
    //    }

    //    public IEnumerable GetValuesFromColumn(MySqlField field)
    //    {
    //        return this.GetValuesFromColumn(_fieldMappings[field]);
    //    }

    //    public IEnumerable GetValuesFromColumn(string columnName)
    //    {
    //        return this.GetValuesFromColumn(this.Columns[columnName]);
    //    }

    //    public IEnumerable GetValuesFromColumn(DataColumn column)
    //    {
    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            yield return row[column];
    //        }
    //    }

    //    public event EventHandler<MySqlDataRowEventArgs> NewRowCreated;

    //    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    //    {
    //        MySqlDataRow row = new MySqlDataRow(builder);

    //        if (this.NewRowCreated != null)
    //        {
    //            MySqlDataRowEventArgs e = new MySqlDataRowEventArgs(row);
    //            this.NewRowCreated(this, e);
    //        }

    //        return row;
    //    }

    //    protected override void OnColumnChanging(DataColumnChangeEventArgs e)
    //    {
    //        base.OnColumnChanging(e);

    //        if (!e.Column.AllowDBNull)
    //        {
    //            if (e.ProposedValue == null || e.ProposedValue is DBNull)
    //            {
    //                e.ProposedValue = e.Column.DefaultValue;
    //            }
    //        }
    //    }

    //    private void MySqlDataTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
    //    {
    //        MySqlDataRow row = e.Row as MySqlDataRow;

    //        foreach (MySqlField field in _fieldMappings.Keys)
    //        {
    //            if (field.IsPrimaryKey && field is MySqlGuid)
    //            {
    //                row[field] = Guid.NewGuid();
    //            }
    //            else if (field is MySqlText || field is MySqlString || field is MySqlLongText)
    //            {
    //                row[field] = String.Empty;
    //            }
    //        }
    //    }

    //    public void RemoveRowsWhere(MySqlField field, object value)
    //    {
    //        List<MySqlDataRow> toRemove = new List<MySqlDataRow>();
    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            if (row.RowState != DataRowState.Deleted && Object.Equals(row[field], value))
    //            {
    //                if (row.RowState == DataRowState.Added)
    //                    toRemove.Add(row);
    //                else
    //                    row.Delete();
    //            }
    //        }

    //        foreach (MySqlDataRow row in toRemove)
    //        {
    //            this.Rows.Remove(row);

    //        }
    //    }

    //    public void SetForAllRows(string field, object value)
    //    {
    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            if (row.RowState != DataRowState.Deleted)
    //                row[field] = value;
    //        }
    //    }

    //    public bool HasNullsIn(MySqlField field)
    //    {
    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            if (row.RowState != DataRowState.Deleted && row.IsNull(field))
    //                return true;

    //        }

    //        return false;
    //    }

    //    public void SetForAllRows(MySqlField field, object value)
    //    {
    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            if (row.RowState != DataRowState.Deleted)
    //                row[field] = value;
    //        }
    //    }

    //    public List<DataColumn> KeyColumns = new List<DataColumn>();

    //    public IEnumerable SortedBy(params object[] fields)
    //    {
    //        BindingSource bs = new BindingSource();
    //        bs.DataSource = this;
    //        List<string> sort = new List<string>();
    //        foreach (object field in fields)
    //        {
    //            if (field is string)
    //                sort.Add(field as string);
    //            else if (field is MySqlOrderBy)
    //                sort.Add((field as MySqlOrderBy).ToBindingSourceString());
    //            else if (field is MySqlField)
    //                sort.Add((field as MySqlField).Name);
    //        }

    //        bs.Sort = String.Join(", ", sort.ToArray());
    //        foreach (DataRowView rv in bs)
    //        {
    //            yield return rv.Row;
    //        }
    //    }

    //    public object[] GetListValues(bool withEmpty, MySqlField idField, params object[] display)
    //    {
    //        var list = new List<MySqlListEntry>();

    //        if (withEmpty)
    //            list.Add(new MySqlListEntry());

    //        foreach (MySqlDataRow row in this.Rows)
    //        {
    //            string name = row.GetString(display);
    //            if (String.IsNullOrEmpty(name))
    //                continue;

    //            list.Add(new MySqlListEntry
    //            {
    //                ID = row[idField],
    //                DisplayValue = name
    //            });
    //        }

    //        return list.OrderBy(x => x.DisplayValue).ToArray();
    //    }

    //    public decimal Sum(object field)
    //    {
    //        DataColumn col = null;

    //        if (field is int)
    //            col = this.Columns[(int)field];
    //        else if (field is string)
    //            col = this.Columns[(string)field];
    //        else if (field is MySqlField && _fieldMappings.ContainsKey(field as MySqlField))
    //            col = _fieldMappings[field as MySqlField];
    //        else
    //            return 0;


    //        decimal sum = 0;
    //        foreach (DataRow row in this.Rows)
    //        {
    //            if (row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached)
    //            {
    //                decimal val = 0;
    //                if (decimal.TryParse(row[col].ToString(), out val))
    //                    sum += val;
    //            }
    //        }

    //        return sum;
    //    }
    //}

    ////public class MorphVar
    ////{
    ////    private object _value;

    ////    public MorphVar(object value)
    ////    {
    ////        _value = value;
    ////    }

    ////    public object Value
    ////    {
    ////        get
    ////        {
    ////            return _value;
    ////        }
    ////    }
    ////}

    ////public class MorphVarConverter<T>
    ////{
    ////    public static implicit operator T(MorphVar v)
    ////    {
    ////        return (T)Convert.ChangeType(v, typeof(T));
    ////    }
    ////}

    //public class MySqlListEntry : IComparable
    //{
    //    public object ID = DBNull.Value;
    //    public string DisplayValue = String.Empty;

    //    public override string ToString()
    //    {
    //        return this.DisplayValue;
    //    }

    //    #region IComparable Members

    //    public int CompareTo(object obj)
    //    {
    //        if (this.DisplayValue == null || !(obj is string))
    //            return -1;

    //        return this.DisplayValue.CompareTo(obj);
    //    }

    //    #endregion
    //}


    ///// <summary>
    ///// Represents row in MySqlDataTable. This row class allows access to row cells
    ///// by [] passing a MySqlField object as indexer argument.
    ///// </summary>
    //public class MySqlDataRow : DataRow
    //{
    //    private bool _added = true;

    //    public MySqlDataRow(DataRowBuilder builder)
    //        : base(builder)
    //    {
    //        //hsObjectProfiler.Default.InstanceCreated(this, true);
    //    }

    //    ~MySqlDataRow()
    //    {
    //        //hsObjectProfiler.Default.InstanceDeleted(this);
    //    }

    //    /// <summary>
    //    /// Gets or sets row value at specified field.
    //    /// </summary>
    //    /// <param name="field">MySqlField instance from table definition.</param>
    //    /// <returns>Row value.</returns>
    //    public object this[MySqlField field]
    //    {
    //        get
    //        {
    //            return this[field, DataRowVersion.Default];
    //        }
    //        set
    //        {
    //            if ((Table as MySqlDataTable).FieldMappings.ContainsKey(field))
    //                base[(Table as MySqlDataTable).FieldMappings[field]] = value;
    //            else
    //                base[field.Name] = value;
    //        }
    //    }

    //    public MySqlExpression GetExpression(int fieldIndex)
    //    {
    //        return new MySqlExpression(MySqlValueSerializer.Get(this[fieldIndex]));
    //    }

    //    public MySqlExpression GetExpression(string field)
    //    {
    //        return new MySqlExpression(MySqlValueSerializer.Get(this[field]));
    //    }

    //    public MySqlExpression GetExpression(MySqlField field)
    //    {
    //        return new MySqlExpression(MySqlValueSerializer.Get(this[field]));
    //    }

    //    /// <summary>
    //    /// Serialize given object using XmlSerialized and store as text in DataRow field.
    //    /// </summary>
    //    /// <param name="field">Object to serialize.</param>
    //    /// <param name="field">MySqlField instance from table definition.</param>
    //    public void XmlSerialize(object obj, MySqlField field)
    //    {
    //        XmlSerializer xs = new XmlSerializer(obj.GetType());
    //        StringWriter sw = new StringWriter();
    //        xs.Serialize(sw, obj);

    //        this[field] = sw.ToString();
    //    }

    //    public T XmlDeserialize<T>(MySqlField field)
    //    {
    //        try
    //        {
    //            if (this.IsEmptyString(field))
    //                return Activator.CreateInstance<T>();

    //            XmlSerializer xs = new XmlSerializer(typeof(T));
    //            return (T)xs.Deserialize(this.GetTextReader(field));
    //        }
    //        catch
    //        {
    //            return Activator.CreateInstance<T>();
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets row value at specified field for given row version.
    //    /// </summary>
    //    /// <param name="field">MySqlField instance from table definition.</param>
    //    /// <param name="version">Row version.</param>
    //    /// <returns>Row value.</returns>
    //    public object this[MySqlField field, DataRowVersion version]
    //    {
    //        get
    //        {
    //            try
    //            {
    //                if (field is MySqlDynamic)
    //                {
    //                    return (field as MySqlDynamic).EvaluateValue(this);
    //                }

    //                if ((Table as MySqlDataTable).FieldMappings.ContainsKey(field))
    //                    return base[(Table as MySqlDataTable).FieldMappings[field], version];
    //                else
    //                    return base[field.Name, version];
    //            }
    //            catch
    //            {
    //                return String.Empty;
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Gets value indicating whether row has DBNull value at specified field.
    //    /// </summary>
    //    /// <param name="field">MySqlField instance from table definition.</param>
    //    /// <returns>True, if row value is DBNull.</returns>
    //    public bool IsNull(MySqlField field)
    //    {
    //        try
    //        {
    //            if ((Table as MySqlDataTable).FieldMappings.ContainsKey(field))
    //                return base.IsNull((Table as MySqlDataTable).FieldMappings[field]);
    //            else
    //                return base.IsNull(field.Name);
    //        }
    //        catch
    //        {
    //            return true;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets value indicating whether row has DBNull value at specified field.
    //    /// </summary>
    //    /// <param name="field">MySqlField instance from table definition.</param>
    //    /// <param name="version">Row version.</param>
    //    /// <returns>True, if row value is DBNull.</returns>
    //    public bool IsNull(MySqlField field, DataRowVersion version)
    //    {
    //        if ((Table as MySqlDataTable).FieldMappings.ContainsKey(field))
    //            return base.IsNull((Table as MySqlDataTable).FieldMappings[field], version);
    //        else
    //            return base.IsNull(Table.Columns[field.Name], version);
    //    }

    //    public bool IsEmptyString(MySqlField field)
    //    {
    //        return String.IsNullOrEmpty(this.GetString(field).Trim());
    //    }

    //    public bool IsAllEmpty(params MySqlField[] fields)
    //    {
    //        bool all = true;
    //        foreach (MySqlField field in fields)
    //        {
    //            all &= this.IsEmptyString(field);
    //        }

    //        return all;
    //    }

    //    /// <summary>
    //    /// Gets value indicating whether new row was added to table.
    //    /// </summary>
    //    public bool Added
    //    {
    //        get
    //        {
    //            return _added;
    //        }
    //        set
    //        {
    //            _added = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets MySqlDataTable that created this row.
    //    /// </summary>
    //    public MySqlDataTable MySqlDataTable
    //    {
    //        get
    //        {
    //            return this.Table as MySqlDataTable;
    //        }
    //    }

    //    public int IndexInTable
    //    {
    //        get
    //        {
    //            return this.Table.Rows.IndexOf(this);
    //        }
    //    }

    //    /// <summary>
    //    /// Gets binding source of table that created this row.
    //    /// </summary>
    //    public MySqlBindingSource BindingSource
    //    {
    //        get
    //        {
    //            return this.MySqlDataTable.BindingSource;
    //        }
    //    }

    //    /// <summary>
    //    /// Deletes the row.
    //    /// </summary>
    //    public new void Delete()
    //    {
    //        try
    //        {
    //            if (this.Table.Rows.IndexOf(this) == -1)
    //            {
    //                this.Table.Rows.Add(this);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            hsLog.TraceException(e);
    //        }

    //        base.Delete();
    //    }

    //    /// <summary>
    //    /// Invoke MySql query for owning table to store changes for this row.
    //    /// </summary>
    //    public void SaveChanges()
    //    {
    //        try
    //        {
    //            if (this.Table.Rows.IndexOf(this) == -1)
    //            {
    //                this.Table.Rows.Add(this);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            hsLog.TraceException(e);
    //        }

    //        if (this.Table is MySqlDataTable && this.BindingSource.DataTable != this.Table)
    //        {
    //            MySqlDataTable prevTable = this.BindingSource.DataTable;
    //            this.BindingSource.DataTable = (MySqlDataTable)this.Table;
    //            this.BindingSource.SaveChanges();
    //            this.BindingSource.DataTable = prevTable;
    //        }
    //        else
    //        {
    //            this.BindingSource.SaveChanges();
    //        }
    //    }

    //    public void CopyValuesFrom(MySqlDataRow row, object table)
    //    {
    //        foreach (MySqlField f in this.MySqlDataTable.FieldMappings.Keys)
    //        {
    //            try
    //            {
    //                if (f.Table.Equals(table) && row.MySqlDataTable.FieldMappings.ContainsKey(f))
    //                {
    //                    this[f] = (row.RowState == DataRowState.Deleted) ? row[f, DataRowVersion.Original] : row[f];
    //                }

    //            }
    //            catch
    //            {
    //                //skip
    //            }
    //        }
    //    }

    //    public MySqlStatement GetChangesAsSqlStatement()
    //    {
    //        Type table = this.MySqlDataTable.FieldMappings.First().Key.Table;

    //        if (this.RowState == DataRowState.Detached)
    //        {
    //            this.MySqlDataTable.Rows.Add(this);
    //        }

    //        if (this.RowState == DataRowState.Added)
    //        {
    //            MySqlInsert ins = GetInsertStatement(table);
    //            return ins;
    //        }
    //        else if (this.RowState == DataRowState.Deleted)
    //        {
    //            MySqlDelete del = GetDeleteStatement(table);
    //            return del;
    //        }
    //        else if (this.RowState == DataRowState.Modified)
    //        {
    //            MySqlUpdate up = GetUpdateStatement(table);
    //            return up;
    //        }

    //        return null;
    //    }

    //    private MySqlUpdate GetUpdateStatement(Type table)
    //    {
    //        MySqlUpdate up = MySqlDb.DefaultDb.Update(table);

    //        foreach (MySqlField field in this.MySqlDataTable.FieldMappings.Keys)
    //        {
    //            if (field.Table != table || field.IsInternal)
    //                continue;

    //            if (this.IsNull(field) && field.DefaultValue != null)
    //                up.Set(field, field.DefaultValue);
    //            else
    //                up.Set(field, this[field]);
    //        }

    //        MySqlWhere w = this.IdentityWhere(table);
    //        if (w != null)
    //        {
    //            up.Where(w);
    //        }
    //        else
    //        {
    //            MessageBox.Show("Could not identify row in database!");
    //            return null;
    //        }

    //        return up;
    //    }

    //    private MySqlDelete GetDeleteStatement(Type table)
    //    {
    //        MySqlDelete del = MySqlDb.DefaultDb.DeleteFrom(table);

    //        MySqlWhere w = this.IdentityWhere(table);
    //        if (w != null)
    //        {
    //            del.Where(w);
    //        }
    //        else
    //        {
    //            MessageBox.Show("Could not identify row in database!");
    //            return null;
    //        }

    //        return del;
    //    }

    //    private MySqlInsert GetInsertStatement(Type table)
    //    {
    //        MySqlInsert ins = MySqlDb.DefaultDb.InsertInto(table);
    //        bool incomplete = false;
    //        foreach (MySqlField field in this.MySqlDataTable.FieldMappings.Keys)
    //        {
    //            if (field.Table != table || field.IsInternal)
    //                continue;

    //            ins.Fields(field);

    //            if (this.IsNull(field) && field.DefaultValue != DBNull.Value && field.DefaultValue != null)
    //            {
    //                ins.Values(field.DefaultValue);
    //            }
    //            else if (this.IsNull(field) && !field.Nullable)
    //            {
    //                incomplete = true;
    //                if (Debugger.IsAttached)
    //                {
    //                    MessageBox.Show(String.Format("{0} cannot be null, insert skipped.", field));
    //                }

    //                break;
    //            }
    //            else
    //            {
    //                ins.Values(this[field]);
    //            }
    //        }

    //        if (incomplete)
    //        {
    //            return null;
    //        }

    //        return ins;
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as Guid.
    //    /// If conversion failed, zero-guid is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public Guid GetGuid(MySqlField field)
    //    {
    //        try
    //        {
    //            if (String.IsNullOrEmpty(this[field].ToString()))
    //            {
    //                return Guid.Empty;
    //            }

    //            return new Guid(this[field].ToString());
    //        }
    //        catch
    //        {
    //            return Guid.Empty;
    //        }
    //    }

    //    public Guid? GetGuidNullable(MySqlField field)
    //    {
    //        try
    //        {
    //            if (String.IsNullOrEmpty(this[field].ToString()))
    //            {
    //                return null;
    //            }

    //            return new Guid(this[field].ToString());
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as integer hash of GUID.
    //    /// If conversion failed, zero  is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public int GetGuidHash(MySqlField field)
    //    {
    //        return this.GetGuid(field).GetHashCode();
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as string.
    //    /// If conversion failed, String.Empty is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public string GetString(MySqlField field)
    //    {
    //        string val = String.Empty;

    //        if (field == null)
    //            return val;

    //        try
    //        {
    //            object rawVal = this[field];
    //            if (rawVal is byte[])
    //            {
    //                val = Encoding.Default.GetString(rawVal as byte[]);
    //            }
    //            else
    //            {
    //                val = this[field].ToString();
    //            }

    //            if (field.FormatedAs() == MySqlFieldAspect.Date)
    //            {
    //                DateTime valDT;
    //                if (DateTime.TryParse(val, out valDT))
    //                {
    //                    val = valDT.ToString(MySqlConfig.MySqlDateFormat);
    //                }
    //            }
    //            else if (field.FormatedAs() == MySqlFieldAspect.Time)
    //            {
    //                DateTime valDT;
    //                if (DateTime.TryParse(val, out valDT))
    //                {
    //                    val = valDT.ToString(MySqlConfig.MySqlTimeFormat);
    //                }
    //            }
    //            else if (field.FormatedAs() == MySqlFieldAspect.DateTime)
    //            {
    //                DateTime valDT;
    //                if (DateTime.TryParse(val, out valDT))
    //                {
    //                    val = valDT.ToString(MySqlConfig.MySqlDateTimeFormat);
    //                }
    //            }
    //            else if (field.FormatedAs() == MySqlFieldAspect.Currency)
    //            {
    //                Decimal valD;
    //                if (Decimal.TryParse(val, out valD))
    //                {
    //                    val = valD.ToString("C2");
    //                }
    //            }

    //            return val;
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceError(e.ToString());
    //            return String.Empty;
    //        }
    //    }

    //    public string GetHtml(MySqlField field)
    //    {
    //        string s = this.GetString(field);

    //        s = s.Replace("&", "&amp;");
    //        s = s.Replace("<", "&lt;");
    //        s = s.Replace(">", "&gt;");

    //        s = s.Replace(Environment.NewLine, "<br/>");
    //        return s;
    //    }

    //    public string GetStringTr(MySqlField field)
    //    {
    //        return this.GetString(field).Trim();
    //    }

    //    public string GetStringTr(string field)
    //    {
    //        return this.GetString(field).Trim();
    //    }

    //    public string GetStringTr(int fieldIndex)
    //    {
    //        return this.GetString(fieldIndex).Trim();
    //    }

    //    public string GetString(string field)
    //    {
    //        try
    //        {
    //            return this[field].ToString();
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceError(e.ToString());
    //            return String.Empty;
    //        }
    //    }

    //    public string GetString(params object[] join)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        foreach (object part in join)
    //        {
    //            if (part is string)
    //                sb.Append(part as string);
    //            else if (part is MySqlField)
    //                sb.Append(this.GetString(part as MySqlField));
    //        }

    //        string result = sb.ToString().Trim(' ', '\n', '\r', '\t', ',', ';');
    //        Regex.Replace(result, @"[ ]+", " ");
    //        return result.Trim();
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as string wrapped into TextReader.
    //    /// If conversion failed, String.Empty is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public TextReader GetTextReader(MySqlField field)
    //    {
    //        return new StringReader(this.GetString(field));
    //    }

    //    /// <summary>
    //    /// Gets rows values at as formatted string.
    //    /// If conversion failed, String.Empty is returned and exception
    //    /// logged to trace. 
    //    /// </summary>
    //    /// <param name="format">Format string</param>
    //    /// <param name="fields">List of fields</param>
    //    /// <returns></returns>
    //    public string GetFormattedString(string format, params MySqlField[] fields)
    //    {
    //        List<object> sArgs = new List<object>();
    //        foreach (MySqlField field in fields)
    //        {
    //            try
    //            {
    //                sArgs.Add(this[field]);
    //            }
    //            catch (Exception e)
    //            {
    //                Trace.TraceError(e.ToString());
    //                return string.Empty;
    //            }
    //        }

    //        try
    //        {
    //            return string.Format(format, sArgs.ToArray());
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceError(e.ToString());
    //            return string.Empty;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as Int32 object.
    //    /// If conversion failed, 0 is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public int GetInt(MySqlField field)
    //    {
    //        int val = 0;
    //        Int32.TryParse(this[field].ToString(), out val);
    //        return val;
    //    }

    //    public long GetLong(MySqlField field)
    //    {
    //        long val = 0;
    //        Int64.TryParse(this[field].ToString(), out val);
    //        return val;
    //    }

    //    public X GetEnum<X>(MySqlEnum<X> field)
    //    {
    //        return (X)(object)GetInt(field);
    //    }

    //    public void SetEnum<X>(MySqlEnum<X> field, X value)
    //    {
    //        this[field] = value;
    //    }

    //    public string GetLookupValue<T>(MySqlField field, hsReflectedDataTable<T> lookupTable) where T : DataTable, new()
    //    {
    //        return lookupTable.GetNameById(this[field]);
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as unsigned Int32 object.
    //    /// If conversion failed, 0 is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public uint GetUInt(MySqlField field)
    //    {
    //        uint val = 0;
    //        UInt32.TryParse(this[field].ToString(), out val);
    //        return val;
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as Bool object
    //    /// if conversion failed, false returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row</param>
    //    /// <returns></returns>
    //    public bool GetBool(MySqlField field)
    //    {
    //        return this.GetInt(field) == 1;
    //    }



    //    /// <summary>
    //    /// Gets row value at given index as Decimal object.
    //    /// If conversion failed, 0 is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public decimal GetDecimal(MySqlField field)
    //    {
    //        decimal val = 0;
    //        try
    //        {
    //            object cell = this[field];
    //            if (cell is Decimal)
    //                return (Decimal)cell;
    //            else
    //                Decimal.TryParse(this[field].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val);

    //        }
    //        catch
    //        {
    //            //Trace.WriteLine("[.GetDecimal] error");
    //        }

    //        return val;

    //    }

    //    public decimal GetDecimal(int fieldIndex)
    //    {
    //        decimal val = 0;
    //        try
    //        {
    //            val = Convert.ToDecimal(this[fieldIndex], CultureInfo.InvariantCulture);
    //        }
    //        catch
    //        {
    //            Trace.WriteLine("[.GetDecimal] error");
    //        }

    //        return val;

    //    }

    //    public decimal GetDecimal(string field)
    //    {
    //        decimal val = 0;
    //        try
    //        {
    //            val = Convert.ToDecimal(this[field], CultureInfo.InvariantCulture);
    //        }
    //        catch
    //        {
    //            Trace.WriteLine("[.GetDecimal] error");
    //        }

    //        return val;

    //    }

    //    /// <summary>
    //    /// Gets row value at given index as Decimal object.
    //    /// If conversion failed, 0 is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public decimal GetDecimal(MySqlField field, decimal def)
    //    {
    //        decimal val = def;
    //        try
    //        {
    //            val = Convert.ToDecimal(this[field], CultureInfo.InvariantCulture);
    //        }
    //        catch
    //        {
    //            Trace.WriteLine("[.GetDecimal] error");
    //        }

    //        return val;
    //    }

    //    public DateTime GetDateTime(string field)
    //    {
    //        return GetDateTimeInternal(this[field].ToString());
    //    }

    //    /// <summary>
    //    /// Gets row value at given index as DateTime object.
    //    /// If conversion failed, DateTime.Min is returned and exception
    //    /// logged to trace.
    //    /// </summary>
    //    /// <param name="field">Field in row.</param>
    //    /// <returns></returns>
    //    public DateTime GetDateTime(MySqlField field)
    //    {
    //        return GetDateTimeInternal(this[field].ToString());
    //    }

    //    private DateTime GetDateTimeInternal(string value)
    //    {
    //        try
    //        {
    //            DateTime d;
    //            if (!DateTime.TryParse(value, out d))
    //            {
    //                return DateTime.MinValue;
    //            }

    //            return d;
    //        }
    //        catch (Exception e)
    //        {
    //            hsLog.TraceException(e);
    //            return DateTime.MinValue;
    //        }
    //    }

    //    public TimeSpan GetTimeSpanBetween(MySqlField field1, MySqlField field2)
    //    {
    //        try
    //        {
    //            DateTime d1 = this.GetDateTime(field1), d2 = this.GetDateTime(field2);
    //            if (d1 > d2)
    //                return (d1 - d2);
    //            else
    //                return (d2 - d1);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.TraceError(e.ToString() + "\nInput string: " + this[field1].ToString() + ", " + this[field2].ToString());
    //            return TimeSpan.Zero;
    //        }
    //    }

    //    public string GetSmartDateString(MySqlField field)
    //    {
    //        DateTime date = this.GetDateTime(field);

    //        if (date.Date == DateTime.Today.Date)
    //        {
    //            return CommonCatalog.Today + ", " + date.ToShortDateString();
    //        }
    //        else if (date.Date.AddDays(1) == DateTime.Today.Date)
    //        {
    //            return CommonCatalog.Yesterday + ", " + date.ToShortDateString();
    //        }
    //        else if (date.Date.AddDays(-1) == DateTime.Today.Date)
    //        {
    //            return CommonCatalog.Tomorrow + ", " + date.ToShortDateString();
    //        }
    //        else
    //        {
    //            return date.ToShortDateString();
    //        }

    //    }

    //    public MySqlField KeyField
    //    {
    //        get
    //        {
    //            foreach (MySqlField field in this.MySqlDataTable.FieldMappings.Keys)
    //            {
    //                if (field.IsPrimaryKey)
    //                {
    //                    return field;
    //                }
    //            }

    //            return null;
    //        }
    //    }

    //    public MySqlWhere IdentityWhere()
    //    {
    //        MySqlWhere where = new MySqlWhere();
    //        bool anyKey = false;

    //        foreach (MySqlField field in this.MySqlDataTable.FieldMappings.Keys)
    //        {
    //            if (field.IsPrimaryKey)
    //            {
    //                if (this.RowState == DataRowState.Deleted)
    //                    where.And(field.Eq(this[field, DataRowVersion.Original]));
    //                else
    //                    where.And(field.Eq(this[field]));

    //                anyKey = true;
    //            }
    //        }

    //        if (anyKey)
    //            return where;

    //        return null;
    //    }

    //    public MySqlWhere IdentityWhere(object table)
    //    {
    //        MySqlWhere where = new MySqlWhere();
    //        bool anyKey = false;

    //        foreach (MySqlField field in this.MySqlDataTable.FieldMappings.Keys)
    //        {
    //            if (field.Table != table)
    //                continue;

    //            if (field.IsPrimaryKey)
    //            {
    //                if (this.RowState == DataRowState.Deleted)
    //                    where.And(field.Eq(this[field, DataRowVersion.Original]));
    //                else
    //                    where.And(field.Eq(this[field]));

    //                anyKey = true;
    //            }
    //        }

    //        if (anyKey)
    //            return where;

    //        return null;
    //    }

    //    public Color GetColor(MySqlField field)
    //    {
    //        return Color.FromArgb(255, Color.FromArgb(this.GetInt(field)));
    //    }

    //    public IEnumerable GetMySqlFields()
    //    {
    //        foreach (DataColumn col in this.Table.Columns)
    //        {
    //            MySqlField field = col.ExtendedProperties["MySqlField"] as MySqlField;
    //            if (field != null)
    //                yield return field;

    //        }
    //    }

    //    public string GetFriendlyName()
    //    {
    //        List<string> names = new List<string>();
    //        foreach (MySqlField field in this.GetMySqlFields())
    //        {
    //            if (field.InFriendlyName)
    //                names.Add(this.GetString(field));
    //        }

    //        return String.Join(" ", names.ToArray());
    //    }

    //    public void Bind(Control c, MySqlField field)
    //    {
    //        var b = new hsBindingWraper(c);
    //        b.Value = this[field];
    //        b.ValueChanged += new EventHandler<hsBindingWraper.EventArgs>(delegate (object sender, hsBindingWraper.EventArgs e)
    //        {
    //            this[field] = e.Wraper.Value;
    //        });
    //    }

    //    public object TableObject;


    //    public void Insert()
    //    {
    //        Insert(MySqlDb.DefaultDb);
    //    }

    //    public void Insert(MySqlDb db)
    //    {
    //        var ins = db.InsertInto(this.TableObject);
    //        foreach (var f in this.MySqlDataTable.FieldMappings)
    //        {
    //            if (f.Key.IsInternal)
    //                continue;

    //            ins.Set(f.Key, this[f.Value]);
    //        }

    //        ins.ExecSql();
    //    }
    //}

    //public class MySqlDb : IDisposable
    //{
    //    private const int MysqlAccessDeniedCodeException = 1045;
    //    public const string RootPassword = "sz8ThSQli2C9IOtVZoUll1kz4uTfFbZ4";

    //    public static readonly object GlobalLock = new object();
    //    private static readonly ILog Log = LogExtensions.GetCurrentClassLogger();

    //    private static List<MySqlDb> _databases = new List<MySqlDb>();
    //    private static MySqlDb _defaultDb = null;

    //    private MySqlConfig _config;
    //    private MySqlConnection _connection { get; set; }
    //    private static MySqlDbObjectModel _objectModel = new MySqlDbObjectModel();

    //    public bool HasErrors;

    //    /// <summary>
    //    /// Creates a new database instance.
    //    /// </summary>
    //    public MySqlDb()
    //    {
    //        _databases.Add(this);
    //        _config = new MySqlConfig();
    //        Schema = new MySqlSchema(this);
    //    }

    //    /// <summary>
    //    /// Creates a new database instance using given config.
    //    /// </summary>
    //    /// <param name="config"></param>
    //    public MySqlDb(MySqlConfig config)
    //    {
    //        _config = config;
    //        _databases.Add(this);
    //        Schema = new MySqlSchema(this);
    //    }

    //    ~MySqlDb()
    //    {
    //        if (_connection != null)
    //        {
    //            try
    //            {
    //                _connection.Close();
    //            }
    //            catch (Exception ex)
    //            {
    //                Log.Error(ex, "Cannot close connection.");
    //            }
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        Connection.Dispose();
    //    }

    //    public bool ThrowSqlException { get; set; } = false;

    //    /// <summary>
    //    /// Crates a new database instance.
    //    /// </summary>
    //    /// <returns>MySqlDb object.</returns>
    //    public static MySqlDb CreateDbObject()
    //    {
    //        return new MySqlDb();
    //    }

    //    /// <summary>
    //    /// Creates a new database instace with given config.
    //    /// </summary>
    //    /// <param name="config">Config to setup connection.</param>
    //    /// <returns>MySqlDb object.</returns>
    //    public static MySqlDb CreateDbObject(MySqlConfig config)
    //    {
    //        return new MySqlDb(config);
    //    }

    //    /// <summary>
    //    /// Gets list of all defined databases in application.
    //    /// </summary>
    //    public static List<MySqlDb> Databases
    //    {
    //        get
    //        {
    //            return _databases;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets the default database. In default that is first created db instance.
    //    /// </summary>
    //    public static MySqlDb DefaultDb
    //    {
    //        get
    //        {
    //            if (_defaultDb == null && _databases.Count > 0)
    //            {
    //                _defaultDb = _databases[0];
    //            }
    //            else if (_defaultDb == null)
    //            {
    //                _defaultDb = CreateDbObject();
    //            }

    //            return _defaultDb;
    //        }
    //        set
    //        {
    //            _defaultDb = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets config for MySqlConncetion.
    //    /// </summary>
    //    public MySqlConfig Config
    //    {
    //        get
    //        {
    //            return _config;
    //        }
    //        set
    //        {
    //            _config = value;
    //        }
    //    }

    //    private static bool _opening = false;
    //    private static object _firstConnectionLock = new object();

    //    /// <summary>
    //    /// Gets connection to server.
    //    /// </summary>
    //    public MySqlConnection Connection
    //    {
    //        get
    //        {
    //            lock (_firstConnectionLock)
    //            {
    //            }

    //            if (_connection == null)
    //            {
    //                lock (_firstConnectionLock)
    //                {
    //                    OpenClientConnection();
    //                }
    //            }
    //            else if (_connection.State != ConnectionState.Open && _opening == false)
    //            {
    //                _opening = true;
    //                OpenClientConnection();
    //                _opening = false;
    //            }

    //            return _connection;
    //        }
    //    }

    //    public void CloseConnection()
    //    {
    //        if (_connection != null)
    //        {
    //            Log.Info("Connection reset from state: {0}", _connection.State);
    //            _connection.Close();
    //            _connection = null;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets object model contains tables definitions from classes in project.
    //    /// </summary>
    //    public MySqlDbObjectModel ObjectModel => _objectModel;

    //    /// <summary>
    //    /// Gets schema of database on server.
    //    /// </summary>
    //    public MySqlSchema Schema { get; }

    //    /// <summary>
    //    /// Open connection to database using client permissions.
    //    /// </summary>
    //    public void OpenClientConnection()
    //    {
    //        try
    //        {
    //            Log.Info("Opening client connection {0}: {1}", _config, new StackTrace());
    //            var connectionSuccessful = InitConnection(_config.GetClientConnectionString(), true);

    //            if (!connectionSuccessful)
    //            {
    //                throw new Exception("Cannot connect to client account by credential");
    //            }

    //            Log.Info("-success");
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, "-opening FAIL");
    //            throw;
    //        }
    //    }

    //    private bool InitConnection(string cs, bool forceReconnect = false)
    //    {
    //        if (_connection != null && forceReconnect == false)
    //        {
    //            if (_connection.State == ConnectionState.Open
    //                || _connection.State == ConnectionState.Fetching
    //                || _connection.State == ConnectionState.Executing
    //                || _connection.State == ConnectionState.Connecting)
    //            {
    //                return true;
    //            }
    //        }

    //        if (!forceReconnect)
    //        {
    //            _connection?.Close();
    //        }

    //        Log.Info("Reconnecting, connection state: {0}",
    //            _connection?.State.ToString() ?? "[connection is null]");

    //        _connection = new MySqlConnection(cs);

    //        try
    //        {
    //            OpenInternal();
    //            return true;
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e, "Error while opening database.");
    //            return false;
    //        }
    //    }

    //    public bool ClientConnectionFailed
    //    {
    //        get
    //        {
    //            try
    //            {
    //                Log.Info("Probing client connection {0}", _config);
    //                OpenClientConnection();
    //                Log.Info("-success");
    //                return false;
    //            }
    //            catch (Exception ex)
    //            {
    //                Log.Error(ex, "-FAIL");
    //                return true;
    //            }
    //        }
    //    }

    //    public bool RootConnectionFailed
    //    {
    //        get
    //        {
    //            try
    //            {
    //                Log.Info("Probing root connection {0}", _config);
    //                OpenMaintenanceConnection();
    //                _connection.Close();
    //                Log.Info("-success");
    //                return false;
    //            }
    //            catch (Exception ex)
    //            {
    //                Log.Error(ex, "-FAIL");
    //                return true;
    //            }
    //        }
    //    }

    //    // SIMPLE TRANSACTIONS

    //    public Sql.MySqlTransaction NewResultTrasnaction(MySqlTransactionAction action)
    //    {
    //        return new Sql.MySqlTransaction(this, action);
    //    }

    //    public Sql.MySqlTransaction NewTrasnaction(MethodInvoker action)
    //    {
    //        return new Sql.MySqlTransaction(this, action);
    //    }

    //    // TRANSACTION WITHOUT RESULT

    //    public delegate void TD<in T>(T a);
    //    public Sql.MySqlTransaction NewTrasnaction<T>(TD<T> action, T a)
    //    {
    //        return new Sql.MySqlTransaction(this, action, a);
    //    }

    //    public delegate void TD<A, B>(A a, B b);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewTrasnaction<A, B>(TD<A, B> action, A a, B b)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b);
    //    }

    //    public delegate void TD<A, B, C>(A a, B b, C c);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewTrasnaction<A, B, C>(TD<A, B, C> action, A a, B b, C c)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c);
    //    }

    //    public delegate void TD<A, B, C, D>(A a, B b, C c, D d);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewTrasnaction<A, B, C, D>(TD<A, B, C, D> action, A a, B b, C c, D d)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d);
    //    }

    //    public delegate void TD<A, B, C, D, E>(A a, B b, C c, D d, E e);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewTrasnaction<A, B, C, D, E>(TD<A, B, C, D, E> action, A a, B b, C c, D d, E e)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d, e);
    //    }

    //    public delegate void TD<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewTrasnaction<A, B, C, D, E, F>(TD<A, B, C, D, E, F> action, A a, B b, C c, D d, E e, F f)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d, e, f);
    //    }

    //    // TRANSACTIONS WITH RESULT

    //    public delegate object TRD<A>(A a);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A>(TRD<A> action, A a)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a);
    //    }

    //    public delegate object TRD<A, B>(A a, B b);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A, B>(TRD<A, B> action, A a, B b)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b);
    //    }

    //    public delegate object TRD<A, B, C>(A a, B b, C c);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A, B, C>(TRD<A, B, C> action, A a, B b, C c)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c);
    //    }

    //    public delegate object TRD<A, B, C, D>(A a, B b, C c, D d);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A, B, C, D>(TRD<A, B, C, D> action, A a, B b, C c, D d)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d);
    //    }

    //    public delegate object TRD<A, B, C, D, E>(A a, B b, C c, D d, E e);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A, B, C, D, E>(TRD<A, B, C, D, E> action, A a, B b, C c, D d, E e)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d, e);
    //    }

    //    public delegate object TRD<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f);
    //    public MySqlObjectQuery.Sql.MySqlTransaction NewResultTrasnaction<A, B, C, D, E, F>(TRD<A, B, C, D, E, F> action, A a, B b, C c, D d, E e, F f)
    //    {
    //        return new MySqlObjectQuery.Sql.MySqlTransaction(this, action, a, b, c, d, e, f);
    //    }

    //    public int Execute(string sql)
    //    {
    //        var sqlStatement = new MySqlStatement(sql)
    //        {
    //            Db = this
    //        };
    //        return sqlStatement.ExecSql();
    //    }

    //    public T SelectScalarFromQuery<T>(string sql)
    //    {
    //        var sqlCommand = new MySqlCommand(sql, _connection);

    //        var result = sqlCommand.ExecuteScalar();

    //        return (T)Convert.ChangeType(result, typeof(T));
    //    }

    //    /// <summary>
    //    /// Creates new sql select statement object using given list of fields.
    //    /// </summary>
    //    /// <param name="fields">List of fields to select.</param>
    //    /// <returns>Sql select object.</returns>
    //    public MySqlSelect Select(params object[] fields)
    //    {
    //        return GetSelectStatement(false, fields);
    //    }

    //    private MySqlSelect GetSelectStatement(bool getRemoved, params object[] fields)
    //    {
    //        MySqlSelect select = new MySqlSelect(getRemoved);
    //        select.Db = this;
    //        select.AddFields(fields);
    //        return select;
    //    }

    //    public MySqlSelect SelectDistinct(params object[] fields)
    //    {
    //        MySqlSelect select = new MySqlSelect();
    //        select.Distinct = true;
    //        select.Db = this;
    //        select.AddFields(fields);
    //        return select;
    //    }

    //    public MySqlSelect SmartSelect(params object[] fields)
    //    {
    //        MySqlSelect select = new MySqlSelect();
    //        select.Db = this;
    //        select.AddFields(fields);

    //        var primaryTables = new List<Type>();
    //        primaryTables.Add(select.Fields[0].Table);
    //        select.From(primaryTables[0]);

    //        for (int i = 1; i < select.Fields.Count; i++)
    //        {
    //            var f = select.Fields[i];
    //            if (f == null)
    //            {
    //                continue;
    //            }

    //            //added rule f.Table != null, check if that is correct !!!
    //            // without this, when you want to add new MySqlExpression to SmartSelect, it doesn't work
    //            if (!primaryTables.Contains(f.Table) && f.Table != null)
    //            {
    //                foreach (Type table in primaryTables)
    //                {
    //                    if (f.RelatedSource == table)
    //                    {
    //                        select.InnerJoin(this.ObjectModel.GetPrimaryKey(f.RelatedSource), f);
    //                        primaryTables.Add(f.Table);
    //                        goto table_connected;
    //                    }

    //                    foreach (MySqlField pf in this.ObjectModel.GetFields(table))
    //                    {
    //                        if (pf.RelatedSource == f.Table)
    //                        {
    //                            select.InnerJoin(pf, this.ObjectModel.GetPrimaryKey(pf.RelatedSource));
    //                            primaryTables.Add(f.Table);
    //                            goto table_connected;
    //                        }
    //                    }
    //                }

    //                table_connected:
    //                continue;
    //            }
    //        }

    //        return select;
    //    }

    //    /// <summary>
    //    /// Creates new sql select statement object as select * from given table.
    //    /// </summary>
    //    /// <param name="table">Table class or name.</param>
    //    /// <returns>Sql select statement object.</returns>
    //    public MySqlSelect SelectAllFrom(object table)
    //    {
    //        MySqlSelect select = new MySqlSelect();
    //        select.Db = this;
    //        select.AddFields(new object[] { _objectModel.GetAllFields(table) });
    //        select.From(_objectModel.GetTable(table));
    //        return select;
    //    }

    //    /// <summary>
    //    /// Gets count of all rows in given table.
    //    /// </summary>
    //    /// <param name="table"></param>
    //    /// <returns></returns>
    //    public int CountOf(object table)
    //    {
    //        return Convert.ToInt32(
    //            this.Select(MySqlExpression.CountAll()).From(table).GetScalar());
    //    }

    //    /// <summary>
    //    /// Gets count of all rows in given table.
    //    /// </summary>
    //    /// <param name="table"></param>
    //    /// <returns></returns>
    //    public int CountOf(object table, MySqlWhere where)
    //    {
    //        return Convert.ToInt32(
    //            this.Select(MySqlExpression.CountAll()).From(table).Where(where).GetScalar());
    //    }

    //    /// <summary>
    //    /// Creates a new sql insert statetment object to given table.
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql insert statement object.</returns>
    //    public MySqlInsert InsertInto(object table)
    //    {
    //        MySqlInsert insert = new MySqlInsert();
    //        insert.Db = this;
    //        insert.Table = table;
    //        return insert;
    //    }

    //    /// <summary>
    //    /// Creates a new sql insert ignore statetment object to given table.
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql insert statement object.</returns>
    //    public MySqlInsert InsertIgnoreInto(object table)
    //    {
    //        MySqlInsert insert = new MySqlInsert();
    //        insert.Db = this;
    //        insert.Table = table;
    //        insert.Ignore(true);
    //        return insert;
    //    }

    //    /// <summary>
    //    /// Creates a new sql update statetment object to given table.
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql update statement object.</returns>
    //    public MySqlUpdate Update(object table)
    //    {
    //        MySqlUpdate update = new MySqlUpdate();
    //        update.Db = this;
    //        update.Table = table;
    //        return update;
    //    }

    //    /// <summary>
    //    /// Set object as deleted for given table. (<b>not remove from database!!!</b>)
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql delete statement object.</returns>
    //    public MySqlDelete DeleteFrom(object table)
    //    {
    //        MySqlDelete delete = new MySqlDelete { Db = this, Table = table };
    //        return delete;
    //    }

    //    /// <summary>
    //    /// Creates a new sql delete statement object to given table. (<b>remove from database!!!</b>)
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql delete statement object.</returns>
    //    public MySqlRemove RemoveFrom(object table)
    //    {
    //        MySqlRemove remove = new MySqlRemove { Db = this, Table = table };
    //        return remove;
    //    }

    //    /// <summary>
    //    /// Creates a new sql undelete statement object to given table.
    //    /// </summary>
    //    /// <param name="table">Table name or class.</param>
    //    /// <returns>Sql undelete statement object.</returns>
    //    public MySqlUndelete UndeleteFrom(object table)
    //    {
    //        MySqlUndelete undelete = new MySqlUndelete();
    //        undelete.Db = this;
    //        undelete.Table = table;
    //        return undelete;
    //    }

    //    /// <summary>
    //    /// Checks if in designated table exists one or more rows that satisfies designated conditions.
    //    /// </summary>
    //    /// <param name="inTable">Table to look for rows in.</param>
    //    /// <param name="conditions">Conditions that rows have to satisfy.</param>
    //    /// <returns>Returns true if one or more rows exist.</returns>
    //    public bool CheckRowExist(object inTable, MySqlWhere conditions)
    //    {
    //        int count = this.Select(MySqlExpression.CountAll())
    //            .From(inTable)
    //            .Where(conditions)
    //            .GetScalar<int>();

    //        return count > 0;
    //    }

    //    /// <summary>
    //    /// Creates a new MySqlDataRow from given table.
    //    /// The row can be stored by SaveChanges() method.
    //    /// </summary>
    //    /// <param name="table"></param>
    //    /// <returns></returns>
    //    public MySqlDataRow CreateNewRow(object table)
    //    {
    //        MySqlBindingSource bs = new MySqlBindingSource();
    //        bs.SelectSql = this.SelectAllFrom(table).Limit(0);
    //        MySqlDataRow row = bs.DataTable.NewMySqlRow();
    //        row.Added = false;
    //        row.TableObject = table;
    //        return row;
    //    }

    //    /// <summary>
    //    /// Creates a new MySqlDataRow from given table.
    //    /// The row can be stored by SaveChanges() method.
    //    /// </summary>
    //    /// <param name="table"></param>
    //    /// <returns></returns>
    //    public MySqlDataRow CreateNewRowWithCursor(object table)
    //    {
    //        MySqlBindingSource bs = new MySqlBindingSource();
    //        bs.SelectSql = this.SelectAllFrom(table).Limit(0);
    //        MySqlDataRow row = bs.DataTable.NewMySqlRow();
    //        bs.DataTable.Rows.Add(row);
    //        return row;
    //    }

    //    /// <summary>
    //    /// Sends INSERT, UPDATE or DELETE statement to apply changes
    //    /// from data table into database.
    //    /// </summary>
    //    /// <param name="table">MySqlTable instance.</param>
    //    /// /// <param name="dataTable">DataTable with modified rows.</param>
    //    public void ApplyChanges(object table, MySqlDataTable dataTable)
    //    {
    //        MySqlBindingSource dest = new MySqlBindingSource();
    //        dest.SelectSql = SelectAllFrom(table).Limit(0);

    //        foreach (MySqlDataRow row in dataTable.Rows)
    //        {
    //            if (row.RowState != DataRowState.Unchanged)
    //            {
    //                var newRow = dest.DataTable.NewMySqlRow();
    //                newRow.CopyValuesFrom(row, table);

    //                dest.DataTable.Rows.Add(newRow);

    //                if (row.RowState == DataRowState.Deleted)
    //                {
    //                    newRow.AcceptChanges();
    //                    newRow.Delete();
    //                }
    //                else if (row.RowState == DataRowState.Modified)
    //                {
    //                    newRow.AcceptChanges();
    //                    newRow.SetModified();
    //                }
    //            }
    //        }

    //        dest.SaveChanges();
    //    }

    //    public bool IsEmpty(Type table)
    //    {
    //        try
    //        {
    //            return Convert.ToInt32(Select(MySqlExpression.CountAll()).From(table).GetScalar()) == 0;
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private static bool OpenFlag;

    //    private void OpenInternal()
    //    {
    //        if (OpenFlag)
    //        {
    //            return;
    //        }

    //        OpenFlag = true;
    //        try
    //        {
    //            var repeatCount = 0;
    //            do
    //            {
    //                Thread.Sleep(200);
    //                OpenThread();
    //                repeatCount++;
    //            } while (repeatCount < 10 && _connection.State != ConnectionState.Open);

    //            if (_connection.State != ConnectionState.Open)
    //            {
    //                throw new Exception("Cannot connect (details above).");
    //            }

    //            lock (GlobalLock)
    //            {
    //                MySqlCommand tout = new MySqlCommand("set net_read_timeout = 3600", _connection);
    //                tout.ExecuteNonQuery();

    //                tout = new MySqlCommand("set net_write_timeout = 3600", _connection);
    //                tout.ExecuteNonQuery();

    //                tout = new MySqlCommand("set wait_timeout = 3600", _connection);
    //                tout.ExecuteNonQuery();
    //            }
    //        }
    //        finally
    //        {
    //            OpenFlag = false;
    //        }
    //    }

    //    private void OpenThread()
    //    {
    //        try
    //        {
    //            _connection.Open();
    //        }
    //        catch (MySqlException ex)
    //        {
    //            Log.Error(ex, "Cannot open connection.");
    //            if (ex.Number == MysqlAccessDeniedCodeException)
    //            {
    //                throw new Exception();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, "Cannot open connection. Unknown Error.");
    //            throw;
    //        }
    //    }

    //    public event MySqlQueryHandler QueryBegin;

    //    public void OnQueryBegin(string sql)
    //    {
    //        QueryBegin?.Invoke(new MySqlQueryEventArgs(this, sql));
    //    }

    //    public event MySqlQueryHandler QueryEnd;
    //    public void OnQueryEnd(string sql, long time)
    //    {
    //        QueryEnd?.Invoke(new MySqlQueryEventArgs(this, sql, null, time));
    //    }

    //    public event MySqlQueryHandler QueryError;
    //    public void OnQueryError(string sql, Exception e)
    //    {
    //        this.HasErrors = true;
    //        Log.Error(e, "Unexpected SQL error");
    //        if (QueryError != null)
    //        {
    //            QueryError(new MySqlQueryEventArgs(this, sql, e));

    //            if (Debugger.IsAttached)
    //                throw new Exception("SQL Error: + " + sql, e);
    //        }
    //    }

    //    private static string _userGuid = Guid.Empty.ToString();
    //    public static string UserGuid
    //    {
    //        get
    //        {
    //            return _userGuid;
    //        }
    //        set
    //        {
    //            _userGuid = value;
    //        }
    //    }

    //    public static string MachineName
    //    {
    //        get
    //        {
    //            return Environment.MachineName + "/" + Environment.UserName;
    //        }
    //    }

    //    public void OpenMaintenanceConnection()
    //    {
    //        try
    //        {
    //            var rootUsername = MySqlConfig.RootUserName;
    //            var emptyRootPassword = string.Empty;
    //            var emptyDatabase = string.Empty;

    //            var connectionStringByClientCredentials = _config.GetMySqlConnectionString(
    //                _config.ClientUserLogin,
    //                _config.ClientUserPassword,
    //                emptyDatabase,
    //                MySqlConfig.LongTimeout);

    //            var connectionStringByDefaultCredentials = _config.GetMySqlConnectionString(
    //                rootUsername,
    //                RootPassword,
    //                emptyDatabase,
    //                MySqlConfig.LongTimeout);

    //            var connectionStringByEmptyCredentials = _config.GetMySqlConnectionString(
    //                rootUsername,
    //                emptyRootPassword,
    //                emptyDatabase,
    //                MySqlConfig.LongTimeout);

    //            Log.Info("Opening maintenance connection {0}: {1}", _config, new StackTrace());

    //            var connectionSuccessful =
    //                InitConnection(connectionStringByClientCredentials) |
    //                InitConnection(connectionStringByDefaultCredentials) |
    //                InitConnection(connectionStringByEmptyCredentials);

    //            if (!connectionSuccessful)
    //            {
    //                throw new Exception("Cannot connect to root account by any credentials");
    //            }

    //            Log.Info("-success");
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, "-opening FAIL");
    //            throw;
    //        }
    //    }
    //}

    //public class MySqlQueryEventArgs
    //{
    //    public MySqlDb Db;
    //    public string Sql;
    //    public Exception Exception;
    //    public long Ticks;

    //    public MySqlQueryEventArgs()
    //    {
    //    }

    //    public MySqlQueryEventArgs(MySqlDb db, string sql)
    //        : this(db, sql, null, -1)
    //    {
    //    }

    //    public MySqlQueryEventArgs(MySqlDb db, string sql, Exception ex)
    //        : this(db, sql, ex, -1)
    //    {
    //    }

    //    public MySqlQueryEventArgs(MySqlDb db, string sql, Exception ex, long ticks)
    //    {
    //        this.Db = db;
    //        this.Sql = sql;
    //        this.Exception = ex;
    //        this.Ticks = ticks;
    //    }
    //}

    //public delegate void MySqlQueryHandler(MySqlQueryEventArgs e);

    //public class MySqlConfig
    //{
    //    public static string DefaultServer = "localhost";
    //    public static string DefaultDbName = "e2db";
    //    public static string DefaultCharset = "cp1250";
    //    public static string DefaultUserLogin = String.Empty;
    //    public static string DefaultUserPassword = String.Empty;
    //    public static uint DefaultPort = 10020;
    //    public static readonly string DefaultDemoUserLogin = "Demo";
    //    public static readonly string DefaultDemoUserPassword = "ufD5PznZGTP4egRTxcPcyH";

    //    public const string MySqlTimeFormat = "HH:mm";
    //    public const string MySqlDateFormat = "yyyy-MM-dd";
    //    public const string MySqlDateTimeFormat = MySqlDateFormat + " " + MySqlTimeFormat;
    //    public const int LongTimeout = 180;
    //    public const int ClientTimeout = 5;
    //    public const string RootUserName = "root";
    //    public const string E2clientUserName = "e2client";

    //    public string Server { get; set; } = DefaultServer;

    //    public string ClientUserLogin { get; set; } = DefaultUserLogin;
    //    public string ClientUserMySqlLogin => MySqlHelper.EscapeString(ClientUserLogin);

    //    public string ClientUserPassword { get; set; } = DefaultUserPassword;

    //    public string DbName { get; set; } = DefaultDbName;

    //    public string Charset { get; set; } = DefaultCharset;

    //    public uint Port { get; set; } = DefaultPort;

    //    public string GetMySqlConnectionString(string username, string password, string database, uint timeout)
    //    {
    //        try
    //        {
    //            return GetConnectionString(HashMySqlUsername(username), password, database, timeout);
    //        }

    //        catch (Exception ex)
    //        {
    //            return String.Empty;
    //        }
    //    }

    //    public string GetMaintenanceConnectionString()
    //    {
    //        try
    //        {
    //            // root do not connect to any database because there is a possibility that it does not exist yet
    //            string database = "";
    //            string user = ClientUserLogin;
    //            string password = ClientUserPassword;
    //            if (ClientUserLogin.IsNullOrWhiteSpace()) // when no db credentials provided (e.g. no user created yet) try default root credentials
    //            {
    //                user = RootUserName;
    //                password = String.Empty;
    //            }
    //            return GetConnectionString(HashMySqlUsername(user), password, database, LongTimeout);
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceInformation("Cannot build root connection string\r\n" + ex);
    //            return String.Empty;
    //        }
    //    }

    //    public string GetClientConnectionString()
    //    {
    //        try
    //        {
    //            var user = ClientUserLogin;
    //            var password = ClientUserPassword;
    //            if (ClientUserLogin.IsNullOrWhiteSpace()) // when no db credentials provided (e.g. no user created yet) try default root credentials
    //            {
    //                user = RootUserName;
    //                password = String.Empty;
    //            }
    //            var hashMySqlUsername = HashMySqlUsername(user);

    //            return GetConnectionString(hashMySqlUsername, password, DbName, ClientTimeout);
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.TraceInformation("Cannot build client connection string\r\n" + ex);
    //            return String.Empty;
    //        }
    //    }

    //    public static string HashMySqlUsername(string username)
    //    {
    //        if (username == RootUserName || username == E2clientUserName)
    //        {
    //            return username;
    //        }

    //        const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //        byte[] bytes = Encoding.UTF8.GetBytes(username);

    //        SHA256Managed hashString = new SHA256Managed();
    //        var hash = hashString.ComputeHash(bytes);

    //        var hash2 = new char[16];

    //        // Note that here we are wasting bits of hash! 
    //        // But it isn't really important, because hash.Length == 32
    //        for (int i = 0; i < hash2.Length; i++)
    //        {
    //            hash2[i] = Chars[hash[i] % Chars.Length];
    //        }

    //        return new string(hash2);
    //    }

    //    private string GetConnectionString(string userName, string password, string database, uint timeout)
    //    {
    //        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    //        {
    //            Server = Server,
    //            Port = Port,
    //            UserID = userName,
    //            Password = password,
    //            CharacterSet = Charset,
    //            ConnectionTimeout = timeout,
    //            Pooling = true,
    //            ConvertZeroDateTime = true,
    //            AllowUserVariables = true,
    //        };

    //        if (database != null)
    //        {
    //            builder.Database = database;
    //        }

    //        return builder.ConnectionString;
    //    }

    //    public override string ToString()
    //    {
    //        return String.Format("{0}@{1}:{2}/{3}", ClientUserLogin, Server, Port, DbName);
    //    }
    //}


    //public static class StringExtensions
    //{
    //    public static string RightSubstring(this string source, int howMany)
    //    {
    //        return howMany >= source.Length
    //                   ? source
    //                   : source.Substring(source.Length - howMany);
    //    }

    //    public static IList<string> SplitBy(this string source, params string[] separators)
    //    {
    //        return source.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
    //    }

    //    public static bool IsNullOrWhiteSpace(this string value)
    //    {
    //        if (value == null) return true;
    //        return string.IsNullOrEmpty(value.Trim());
    //    }

    //    public static bool IsEmptyGuid(this string str)
    //    {
    //        return str == Guid.Empty.ToString();
    //    }

    //    public static bool Contains(this string source, string toCheck, StringComparison comp)
    //    {
    //        return source.IndexOf(toCheck, comp) >= 0;
    //    }

    //    public static bool ContainsIgnoreCase(this string source, string toCheck)
    //    {
    //        if (source == null || toCheck == null)
    //        {
    //            return false;
    //        }

    //        return source.Contains(toCheck, StringComparison.InvariantCultureIgnoreCase);
    //    }
    //}

    //[Designer(typeof(MySqlBindingSourceDesigner), typeof(IDesigner))]
    //public partial class MySqlBindingSource : BindingSource
    //{
    //    private static readonly ILog Log = LogExtensions.GetCurrentClassLogger();

    //    private MySqlDbComponent _dbComponent = MySqlDbComponent.Default;

    //    private MySqlSelect _selectSql;
    //    private Type _table = null;

    //    private MySqlDataTable _dataTable;

    //    private bool _connectToDataSource = true;

    //    /// <summary>
    //    /// Creates a new instance of MySqlBindingSource component.
    //    /// </summary>
    //    public MySqlBindingSource()
    //    {
    //        InitializeComponent();
    //        InitializeComponentCustom();
    //    }

    //    private void InitializeComponentCustom()
    //    {
    //        this.Refresh();
    //    }

    //    /// <summary>
    //    /// Creates a new instance of MySqlBindingSource component.
    //    /// </summary>
    //    public MySqlBindingSource(IContainer container)
    //    {
    //        //container.Add(this);
    //        InitializeComponent();
    //        InitializeComponentCustom();
    //    }

    //    /// <summary>
    //    /// Gets current binding item as DataRowView
    //    /// </summary>
    //    public DataRowView CurrentRowView
    //    {
    //        get
    //        {
    //            DataRowView view = base.Current as DataRowView;
    //            return view;
    //        }
    //    }

    //    public void CleanUp()
    //    {
    //        DataSource = null;
    //        _dataTable = null;
    //        SelectSql = null;
    //    }

    //    public void Reset()
    //    {
    //        DataTable?.Clear();
    //        SelectSql = null;
    //    }

    //    /// <summary>
    //    /// Gets current binding item as MySqlDataRow
    //    /// </summary>
    //    public MySqlDataRow CurrentRow
    //    {
    //        get
    //        {
    //            DataRowView view = base.Current as DataRowView;

    //            return view?.Row as MySqlDataRow;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets DbComponent that provides connection for queries.
    //    /// If null, the default MySqlDb in project is used.
    //    /// </summary>
    //    [TypeConverter(typeof(ExpandableObjectConverter))]
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //    public MySqlDbComponent DbComponent
    //    {
    //        get
    //        {
    //            return _dbComponent;
    //        }
    //        set
    //        {
    //            _dbComponent = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets select stemenet object to fill internal data table.
    //    /// </summary>
    //    [Browsable(false)]
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //    public virtual MySqlSelect SelectSql
    //    {
    //        get
    //        {
    //            return _selectSql;
    //        }
    //        set
    //        {
    //            _selectSql = value;
    //            this.Refresh();
    //        }
    //    }

    //    public void FillTableOnly(MySqlSelect select)
    //    {
    //        _connectToDataSource = false;
    //        this.SelectSql = select;
    //    }

    //    /// <summary>
    //    /// Gets or sets name of MySql table in database
    //    /// to retreive all data from it.
    //    /// </summary>
    //    [Category("MySql")]
    //    [Editor(typeof(MySqlTableNamePicker), typeof(UITypeEditor))]
    //    public Type TableName
    //    {
    //        get
    //        {
    //            return _table;
    //        }
    //        set
    //        {
    //            if (_table == value)
    //                return;

    //            _table = value;

    //            try
    //            {
    //                if (_table != null)
    //                {
    //                    _dbComponent.Db.ObjectModel.LoadTableClass(_table);
    //                    this.SelectSql = _dbComponent.Db.SelectAllFrom(_table).Limit(0);
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                Log.Error(e, "Cannot fill DataTable for BindingSource");
    //            }

    //        }
    //    }

    //    /// <summary>
    //    /// Gets MySqlDataTable filled by last executed query.
    //    /// </summary>
    //    public MySqlDataTable DataTable
    //    {
    //        get
    //        {
    //            if (this.DataSource is MySqlDataTable)
    //            {
    //                return this.DataSource as MySqlDataTable;
    //            }
    //            else
    //            {
    //                return _dataTable;
    //            }
    //        }
    //        set
    //        {
    //            if (_connectToDataSource)
    //                this.DataSource = value;
    //            _dataTable = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Clears internal data table and fill it from select results.
    //    /// </summary>
    //    public void Refresh()
    //    {
    //        if (DesignMode)
    //            return;

    //        if (_selectSql == null)
    //            return;

    //        this.DataSource = null;
    //        _dataTable = null;

    //        if (_dataTable == null)
    //        {
    //            _dataTable = new MySqlDataTable(this);
    //            _dataTable.TableNewRow += new DataTableNewRowEventHandler(_dataTable_TableNewRow);
    //        }
    //        else
    //        {
    //            _dataTable.Clear();
    //        }

    //        try
    //        {
    //            this.FillDataTable();
    //        }
    //        catch (Exception e)
    //        {
    //            if (DesignMode)
    //            {
    //                MessageBox.Show(e.ToString());
    //            }
    //            else
    //            {
    //                Log.Error(e, "Cannot refresh data table");
    //                throw e;
    //            }
    //        }

    //        if (_connectToDataSource)
    //        {
    //            this.DataSource = _dataTable;
    //            if (this.ResetBindings)
    //                this.ResetBindings(true);
    //        }
    //    }

    //    public bool ResetBindings = true;

    //    public event EventHandler<MySqlDataRowEventArgs> NewRow;

    //    private void _dataTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
    //    {
    //        if (this.NewRow != null)
    //        {
    //            MySqlDataRowEventArgs args = new MySqlDataRowEventArgs(e.Row as MySqlDataRow);
    //            this.NewRow(this, args);
    //        }
    //    }

    //    /// <summary>
    //    /// Calls sql insert, update or delete statements for each modified row
    //    /// in internal data table to store changes.
    //    /// </summary>
    //    public virtual void SaveChanges()
    //    {
    //        try
    //        {
    //            this.EndEdit();
    //            this.UpdateDataTable();
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e, "Cannot save changes");
    //        }
    //    }

    //    /// <summary>
    //    /// Returns DataColumn for given MySql field in received data table.
    //    /// </summary>
    //    /// <param name="field"></param>
    //    /// <returns></returns>
    //    public DataColumn GetColumn(MySqlField field)
    //    {
    //        return this.DataTable.FieldMappings[field];
    //    }

    //    public DataColumn GetColumn(string columnName)
    //    {
    //        return this.DataTable.Columns[columnName];
    //    }

    //    protected virtual MySqlSelect CreateSelectStatement()
    //    {
    //        return _selectSql;
    //    }

    //    public void FillDataTable(MySqlSelect select)
    //    {
    //        Exception outerEx = null;

    //        try
    //        {
    //            if (select != null && select.IsValid)
    //            {
    //                select.Prepare();
    //                string cmd = select.ToString();

    //                try
    //                {
    //                    _dbComponent.Db.OnQueryBegin(cmd);
    //                    long time = 0;

    //                    lock (MySqlDb.GlobalLock)
    //                    {
    //                        using (var adapter = new MySqlDataAdapter(cmd, _dbComponent.Db.Connection))
    //                        {
    //                            adapter.SelectCommand.CommandTimeout = -1;

    //                            time = DateTime.Now.Ticks;
    //                            adapter.Fill(_dataTable);
    //                        }
    //                    }

    //                    _dbComponent.Db.OnQueryEnd(cmd, DateTime.Now.Ticks - time);
    //                    _dataTable.UpdateFieldMappings(select);
    //                    _dbComponent.Db.HasErrors = false;
    //                }
    //                catch (Exception firstEx)
    //                {
    //                    if (this.DesignMode)
    //                        MessageBox.Show(cmd + "\n" + firstEx.ToString());

    //                    _dbComponent.Db.OnQueryError(cmd, firstEx);
    //                }
    //            }

    //            _selectSql = select;
    //        }
    //        catch (Exception ex)
    //        {
    //            outerEx = ex;
    //        }

    //        if (outerEx != null)
    //            throw outerEx;
    //    }

    //    private void FillDataTable()
    //    {
    //        this.FillDataTable(this.CreateSelectStatement());
    //    }

    //    public event MySqlRowEventHandler RowInserting;

    //    protected virtual void UpdateDataTable()
    //    {
    //        int inserted = 0, updated = 0, removed = 0;
    //        if (_dataTable == null || _selectSql == null)
    //            return;

    //        for (int i = 0; i < _dataTable.Rows.Count; i++)
    //        {
    //            if (i < 0)
    //                continue;

    //            MySqlDataRow row = _dataTable.Rows[i] as MySqlDataRow;

    //            if (_selectSql.Tables.Count > 0)
    //            {
    //                var table = _selectSql.Tables[0];
    //                if (row.RowState == DataRowState.Added)
    //                {
    //                    if (this.RowInserting != null)
    //                    {
    //                        MySqlRowEventArgs ie = new MySqlRowEventArgs();
    //                        ie.Cancel = false;
    //                        ie.Row = row;

    //                        this.RowInserting(this, ie);
    //                        if (ie.Cancel)
    //                        {
    //                            row.RejectChanges();
    //                            continue;
    //                        }
    //                    }

    //                    MySqlInsert ins = _dbComponent.Db.InsertInto(table);
    //                    bool incomplete = false;
    //                    foreach (MySqlField field in _dataTable.FieldMappings.Keys)
    //                    {
    //                        if (field.Table != table || field.IsInternal)
    //                            continue;

    //                        ins.Fields(field);

    //                        if (row.IsNull(field) && field.DefaultValue != DBNull.Value && field.DefaultValue != null)
    //                        {
    //                            ins.Values(field.DefaultValue);
    //                        }
    //                        else if (row.IsNull(field) && !field.Nullable)
    //                        {
    //                            incomplete = true;
    //                            if (Debugger.IsAttached)
    //                            {
    //                                MessageBox.Show(String.Format("{0} cannot be null, insert skipped.", field));
    //                            }
    //                            break;
    //                        }
    //                        else
    //                        {
    //                            ins.Values(row[field]);
    //                        }
    //                    }

    //                    if (!incomplete)
    //                    {
    //                        inserted++;
    //                        ins.ExecSql();
    //                    }
    //                }
    //                else if (row.RowState == DataRowState.Detached)
    //                {
    //                    i--;
    //                }
    //                else if (row.RowState == DataRowState.Deleted)
    //                {
    //                    MySqlDelete del = _dbComponent.Db.DeleteFrom(table);

    //                    MySqlWhere w = row.IdentityWhere(table);
    //                    if (w != null)
    //                    {
    //                        del.Where(w);
    //                        del.ExecSql();
    //                        removed++;
    //                    }
    //                    else
    //                    {
    //                        Trace.TraceWarning("Null delete identity where for {0} row\r\n{1}",
    //                            row.GetFriendlyName(), new StackTrace());
    //                    }

    //                    i--;
    //                }
    //                else if (row.RowState == DataRowState.Modified)
    //                {
    //                    MySqlUpdate up = _dbComponent.Db.Update(table);

    //                    foreach (MySqlField field in _dataTable.FieldMappings.Keys)
    //                    {
    //                        if (field.Table != table || field.IsInternal)
    //                            continue;

    //                        if (row.IsNull(field) && field.DefaultValue != null)
    //                            up.Set(field, field.DefaultValue);
    //                        else
    //                            up.Set(field, row[field]);
    //                    }

    //                    MySqlWhere w = row.IdentityWhere(table);
    //                    if (w != null)
    //                    {
    //                        up.Where(w);
    //                        up.ExecSql();
    //                        updated++;
    //                    }
    //                    else
    //                    {
    //                        Trace.TraceWarning("Null update identity where for {0} row\r\n{1}",
    //                            row.GetFriendlyName(), new StackTrace());
    //                    }
    //                }
    //            }

    //            if (row.RowState != DataRowState.Unchanged)
    //            {
    //                if (row.Table.Rows.IndexOf(row) != -1)
    //                {
    //                    try
    //                    {
    //                        row.AcceptChanges();
    //                    }
    //                    catch
    //                    {
    //                        Trace.WriteLine("[MySqlBindingSource.UpdateDataTable] error");
    //                    }
    //                }
    //            }
    //        }

    //        //Trace.TraceInformation("Update table: {0} inserted, {1} updated, {2} removed.",
    //        //    inserted, updated, removed);
    //    }

    //    public class MySqlRowEventArgs : CancelEventArgs
    //    {
    //        public MySqlDataRow Row;
    //    }

    //    public delegate void MySqlRowEventHandler(object sender, MySqlRowEventArgs e);

    //    public MySqlDataRow GetRow(int index)
    //    {
    //        if (this.Count > index)
    //        {
    //            return (this[index] as DataRowView).Row as MySqlDataRow;
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }
    //}

    //public class MySqlTableNamePicker : UITypeEditor
    //{
    //    class Editor : Panel
    //    {
    //        private IWindowsFormsEditorService _service;
    //        private Type _value;
    //        private ListBox _listBox;

    //        public Editor(IWindowsFormsEditorService service, IServiceProvider provider)
    //        {
    //            _service = service;
    //            _listBox = new ListBox();
    //            _listBox.Dock = DockStyle.Fill;
    //            _listBox.IntegralHeight = false;
    //            _listBox.Parent = this;
    //            _listBox.MouseClick += new MouseEventHandler(_listBox_MouseClick);
    //            _listBox.FormattingEnabled = true;
    //            _listBox.Format += new ListControlConvertEventHandler(_listBox_Format);

    //            try
    //            {
    //                ITypeDiscoveryService tds = (ITypeDiscoveryService)provider.GetService(typeof(ITypeDiscoveryService));
    //                foreach (Type type in tds.GetTypes(typeof(MySqlTableBase), false))
    //                {
    //                    if (MySqlTableBase.IsTable(type))
    //                    {
    //                        MySqlDb.DefaultDb.ObjectModel.LoadTableClass(type);
    //                        _listBox.Items.Add(type);
    //                    }
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                _listBox.Items.Add("Failed to get list of tables in project:");
    //                _listBox.Items.Add(e.Message);
    //            }

    //            _listBox.Sorted = true;
    //        }

    //        void _listBox_Format(object sender, ListControlConvertEventArgs e)
    //        {
    //            if (e.ListItem is Type)
    //            {
    //                e.Value = (e.ListItem as Type).Name;
    //            }
    //        }

    //        void _listBox_MouseClick(object sender, MouseEventArgs e)
    //        {
    //            _value = _listBox.SelectedItem as Type;
    //            _service.CloseDropDown();
    //        }

    //        public Type Value
    //        {
    //            get
    //            {
    //                return _value;
    //            }
    //            set
    //            {
    //                _value = value;
    //                if (value != null)
    //                {
    //                    _listBox.SelectedIndex = _listBox.Items.IndexOf(_value);
    //                }
    //            }
    //        }
    //    }

    //    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    //    {
    //        return UITypeEditorEditStyle.DropDown;
    //    }

    //    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    //    {
    //        if ((context != null) && (provider != null))
    //        {
    //            IWindowsFormsEditorService editorService =
    //              (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

    //            if (editorService != null)
    //            {
    //                Editor dropDownEditor = new Editor(editorService, provider);
    //                dropDownEditor.Value = value as Type;
    //                editorService.DropDownControl(dropDownEditor);
    //                return dropDownEditor.Value;
    //            }
    //        }
    //        return base.EditValue(context, provider, value);
    //    }
    //}

    //public class MySqlBindingSourceDesigner : ComponentDesigner
    //{
    //    class ActionList : DesignerActionList
    //    {
    //        public ActionList(IComponent component)
    //            : base(component)
    //        {
    //        }

    //        public override DesignerActionItemCollection GetSortedActionItems()
    //        {
    //            DesignerActionItemCollection items = new DesignerActionItemCollection();
    //            items.Add(new DesignerActionMethodItem(this, "Refresh", "Refresh", true));
    //            return items;
    //        }

    //        public void Refresh()
    //        {
    //            (this.Component as MySqlBindingSource).Refresh();
    //        }
    //    }

    //    public override DesignerActionListCollection ActionLists
    //    {
    //        get
    //        {
    //            DesignerActionListCollection lists = new DesignerActionListCollection();
    //            lists.Add(new ActionList(this.Component));
    //            return lists;
    //        }
    //    }
    //}

    //public class MySqlDataRowEventArgs : EventArgs
    //{
    //    private MySqlDataRow _row;

    //    public MySqlDataRow Row
    //    {
    //        get
    //        {
    //            return _row;
    //        }
    //    }

    //    public MySqlDataRowEventArgs(MySqlDataRow row)
    //    {
    //        _row = row;
    //    }
    //}

    //public class MySqlTableBase
    //{
    //    /// <summary>
    //    /// Checks if given type is MySqlTable class.
    //    /// </summary>
    //    /// <param name="type">Type to check.</param>
    //    /// <returns>True if type is MySqlTable, false otherwise.</returns>
    //    public static bool IsTable(Type type)
    //    {
    //        return type.IsSubclassOf(typeof(MySqlTableBase)) && !type.IsGenericTypeDefinition;
    //    }
    //}

    ///// <summary>
    ///// Base class form custom mysql tables used in application.
    ///// To define a table, create a NewClass inherited from 
    ///// MySqlTable(NewClass) and add public static fields for each
    ///// column in table.
    ///// </summary>
    ///// <typeparam name="T">
    ///// Type of derivered class. The type must be passed to base class
    ///// to generate Table and All fields that describes table type
    ///// instance and special field All that continas all fields from table.
    ///// </typeparam>
    //public class MySqlTable<T> : MySqlTableBase
    //{
    //    private static Type _table;
    //    private static MySqlAllFields _all;

    //    static MySqlTable()
    //    {
    //        MySqlDb.DefaultDb.ObjectModel.LoadTableClass(typeof(T));
    //    }

    //    protected static void Initialize()
    //    {
    //        MySqlDb.DefaultDb.ObjectModel.LoadTableClass(typeof(T));
    //    }

    //    public static void DeleteWhere(MySqlWhere where)
    //    {
    //        MySqlDb.DefaultDb.DeleteFrom(typeof(T))
    //            .Where(where)
    //            .ExecSql();
    //    }

    //    public static int Count()
    //    {
    //        return MySqlDb.DefaultDb.Select(MySqlExpression.CountAll())
    //            .From(typeof(T))
    //            .GetScalar<int>(0);
    //    }

    //    public static MySqlDataTable GetAll()
    //    {
    //        return MySqlDb.DefaultDb.SelectAllFrom(typeof(T)).GetTable();
    //    }

    //    public static MySqlDataTable GetAll(MySqlOrderBy order)
    //    {
    //        return MySqlDb.DefaultDb.SelectAllFrom(typeof(T)).OrderBy(order).GetTable();
    //    }

    //    public static int CountWhere(MySqlWhere where)
    //    {
    //        return MySqlDb.DefaultDb.Select(MySqlExpression.CountAll())
    //            .From(typeof(T))
    //            .Where(where)
    //            .GetScalar<int>(0);
    //    }

    //    public static int CountWhere(MySqlDb db, MySqlWhere where)
    //    {
    //        return db.Select(MySqlExpression.CountAll())
    //            .From(typeof(T))
    //            .Where(where)
    //            .GetScalar<int>(0);
    //    }

    //    public static Type Table
    //    {
    //        get
    //        {
    //            if (_table == null)
    //                _table = typeof(T);

    //            return _table;
    //        }
    //    }

    //    public static MySqlAllFields All
    //    {
    //        get
    //        {
    //            if (_all == null)
    //                _all = new MySqlAllFields(typeof(T));

    //            return _all;
    //        }
    //    }


    //    [InternalField]
    //    public static MySqlDateTime X_CreateTime = new MySqlDateTime();

    //    [InternalField]
    //    public static MySqlDateTime X_ModifyTime = new MySqlDateTime();

    //    [Nullable]
    //    [InternalField]
    //    [Indexed]
    //    public static MySqlDateTime X_RemoveTime = new MySqlDateTime();

    //    [InternalField]
    //    public static MySqlGuid X_CreateUser = new MySqlGuid();

    //    [InternalField]
    //    public static MySqlGuid X_ModifyUser = new MySqlGuid();

    //    [Nullable]
    //    [InternalField]
    //    public static MySqlGuid X_RemoveUser = new MySqlGuid();


    //    [InternalField]
    //    public static MySqlString X_CreateMachine = new MySqlString();

    //    [InternalField]
    //    public static MySqlString X_ModifyMachine = new MySqlString();

    //    [Nullable]
    //    [InternalField]
    //    public static MySqlString X_RemoveMachine = new MySqlString();
    //}
    //public class MySqlOrderBy
    //{
    //    private string _orderby;
    //    private string _direction = String.Empty;
    //    private string _shorField = String.Empty;

    //    /// <summary>
    //    /// Creates a new order-by clause
    //    /// that sorts select by given field ascending.
    //    /// </summary>
    //    /// <param name="field">Field to sort.</param>
    //    public MySqlOrderBy(object field)
    //    {
    //        if (field is MySqlField)
    //            _shorField = (field as MySqlField).Name;

    //        _orderby = field.ToString();
    //    }

    //    /// <summary>
    //    /// Creates a new order-by clause
    //    /// that sorts select by given field ascending or descending.
    //    /// </summary>
    //    /// <param name="field">Field to sort.</param>
    //    /// <param name="ascending">True, to sort ascendign, false for descending sorting.</param>
    //    public MySqlOrderBy(object field, bool ascending)
    //    {
    //        if (field is MySqlField)
    //            _shorField = (field as MySqlField).Name;

    //        _direction = (ascending ? " ASC" : " DESC");

    //        _orderby = field.ToString() + _direction;
    //    }

    //    /// <summary>
    //    /// Combine current order-by clause with another to sort by multiple columns.
    //    /// </summary>
    //    /// <param name="next">MySqlOrderBy object to combine with.</param>
    //    /// <returns></returns>
    //    public MySqlOrderBy And(MySqlOrderBy next)
    //    {
    //        _orderby += ", " + next.ToString();
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets sql order-by clause as string.
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        return _orderby;
    //    }

    //    public string ToBindingSourceString()
    //    {
    //        return _shorField + " " + _direction;
    //    }
    //}

    //public class InternalFieldAttribute : Attribute
    //{
    //}
    //public class IndexedAttribute : Attribute
    //{
    //}
    //public class MySqlAllFields
    //{
    //    private Type _table;
    //    private List<MySqlField> _fields = null;

    //    /// <summary>
    //    /// Creates a new instance of all-fields selector object.
    //    /// </summary>
    //    public MySqlAllFields()
    //    {
    //    }

    //    /// <summary>
    //    /// Creates a new instance of all-fields selector object from given table.
    //    /// </summary>
    //    public MySqlAllFields(Type table)
    //    {
    //        _table = table;
    //    }

    //    /// <summary>
    //    /// Gets or sets table, which fields are selected from.
    //    /// </summary>
    //    public Type Table
    //    {
    //        get
    //        {
    //            return _table;
    //        }
    //        set
    //        {
    //            _table = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets list of all fields from table.
    //    /// </summary>
    //    public List<MySqlField> Fields
    //    {
    //        get
    //        {
    //            if (_fields == null)
    //            {
    //                this.GetFields();
    //            }

    //            return _fields;
    //        }
    //    }

    //    private void GetFields()
    //    {
    //        _fields = new List<MySqlField>();
    //        foreach (FieldInfo field in MySqlDbObjectModel.GetStaticFields(_table))
    //        {
    //            if (field.FieldType.IsSubclassOf(typeof(MySqlField)) && !field.FieldType.IsSubclassOf(typeof(MySqlVirtual)))
    //            {
    //                var fieldToAdd = field.GetValue(null);
    //                _fields.Add(fieldToAdd as MySqlField);
    //            }
    //        }
    //    }
    //}

    //public class MySqlVar
    //{
    //    public string Name
    //    {
    //        get;
    //        set;
    //    }

    //    public MySqlVar(string name)
    //    {
    //        this.Name = name;
    //    }

    //    public override string ToString()
    //    {
    //        return "[" + Name + "]";
    //    }
    //}

    ///// <summary>
    ///// Integer field type.
    ///// </summary>
    //public class MySqlInt : MySqlField
    //{
    //    public MySqlInt()
    //    {
    //        this.DefaultFormat = "0";
    //    }

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "INT(11)";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(int);
    //        }
    //    }
    //}

    //public class MySqlBigInt : MySqlField
    //{
    //    public MySqlBigInt()
    //    {
    //        this.DefaultFormat = "0";
    //    }

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "BIGINT(20)";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(long);
    //        }
    //    }
    //}

    //public class MySqlEnum<T> : MySqlInt
    //{
    //    public MySqlWhere EqName(T val)
    //    {
    //        return base.Eq(val);
    //    }
    //}

    //public class MySqlColor : MySqlInt
    //{
    //}

    ///// <summary>
    ///// Boolean field type.
    ///// </summary>
    //public class MySqlBool : MySqlField
    //{
    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "INT(11)";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(bool);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Varchar string field type. Default length is 50.
    ///// </summary>
    //public class MySqlString : MySqlField
    //{
    //    public const int FieldLength = 50;

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return String.Format("VARCHAR({0})", MySqlString.FieldLength);
    //        }
    //    }

    //    public override object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return String.Empty;
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(string);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Varchar string field type. Default length is 255.
    ///// </summary>
    //public class MySqlLongString : MySqlField
    //{
    //    public const int FieldLength = 255;

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return String.Format("VARCHAR({0})", MySqlLongString.FieldLength);
    //        }
    //    }

    //    public override object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return String.Empty;
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(string);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Text string field type.
    ///// </summary>
    //public class MySqlText : MySqlField
    //{

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "TEXT";
    //        }
    //    }

    //    public override object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return string.Empty;
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(string);
    //        }
    //    }
    //}




    ///// <summary>
    ///// Text string field type (long).
    ///// </summary>
    //public class MySqlLongText : MySqlField
    //{

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "LONGTEXT";
    //        }
    //    }

    //    public override object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return string.Empty;
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(string);
    //        }
    //    }
    //}

    //public class MySqlLongBlob : MySqlField
    //{

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "LONGBLOB";
    //        }
    //    }

    //    public override object SqlDefaultValue
    //    {
    //        get
    //        {
    //            return new byte[0];
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(byte[]);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Date/time field type.
    ///// </summary>
    //public class MySqlDateTime : MySqlField
    //{
    //    public MySqlDateTime()
    //    {
    //        this.DefaultFormat = "yyyy-MM-dd";
    //    }

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "DATETIME";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(DateTime);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Decimal field type.
    ///// </summary>
    //public class MySqlDecimal : MySqlField
    //{
    //    public const string SqlFieldType = "DECIMAL(10,2)";

    //    public MySqlDecimal()
    //    {
    //        this.DefaultFormat = "0";
    //    }

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return SqlFieldType;
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(decimal);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Decimal field type.
    ///// </summary>
    //public class MySqlDecimalPrecise : MySqlField
    //{
    //    public MySqlDecimalPrecise()
    //    {
    //        this.DefaultFormat = "0";
    //    }

    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "DECIMAL(12,4)";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(decimal);
    //        }
    //    }
    //}

    ///// <summary>
    ///// Guid field type.
    ///// </summary>
    //public class MySqlGuid : MySqlField
    //{
    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "VARCHAR(36)";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(Guid);
    //        }
    //    }
    //}

    ///// <summary>
    ///// TimeSpan field type.
    ///// </summary>
    //public class MySqlTime : MySqlField
    //{
    //    public override string SqlType
    //    {
    //        get
    //        {
    //            return "TIME";
    //        }
    //    }

    //    public override Type NativeType
    //    {
    //        get
    //        {
    //            return typeof(TimeSpan);
    //        }
    //    }
    //}

    //public class MySqlDbObjectModel
    //{
    //    private Dictionary<string, Type> _tables
    //         = new Dictionary<string, Type>();

    //    //private MySqlDb _db;

    //    /// <summary>
    //    /// Creates a new database object model for given db instance.
    //    /// </summary>
    //    /// <param name="db"></param>
    //    public MySqlDbObjectModel()
    //    {
    //        //_db = db;
    //    }

    //    private bool _loadedAll = false;

    //    /// <summary>
    //    /// Loads each MySqlTable class from specified assembly.
    //    /// </summary>
    //    /// <param name="assembly">Assembly to scan.</param>
    //    public void LoadAllTables(Assembly assembly)
    //    {
    //        foreach (Type type in assembly.GetTypes())
    //        {
    //            if (MySqlTableBase.IsTable(type))
    //            {
    //                try
    //                {
    //                    LoadTableClass(type);
    //                }
    //                catch (Exception e)
    //                {
    //                    Trace.TraceWarning("Unable to load table {0} from assemlby {1} for LoadAllTables.\n{2}",
    //                                            type.Name, assembly.FullName, e);
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Gets list of all loaded tables.
    //    /// </summary>
    //    public IEnumerable<Type> Tables
    //    {
    //        get
    //        {
    //            return _tables.Values;
    //        }
    //    }

    //    public class UnknownTable : MySqlTableBase
    //    {
    //    }

    //    /// <summary>
    //    /// Gets table by name or class type. If table not found, an exception is throwed.
    //    /// The table must be previously loaded usin InitTableClass or LoadAllTables methods.
    //    /// </summary>
    //    /// <param name="table">String with table name or table type.</param>
    //    /// <returns>Table.</returns>
    //    public Type GetTable(object table)
    //    {
    //        if (table is Type && MySqlTableBase.IsTable((Type)table))
    //        {
    //            return (Type)table;
    //        }
    //        else if (table is string)
    //        {
    //            string t = table.ToString().Replace("`", "");
    //            try
    //            {
    //                return _tables[t];
    //            }
    //            catch
    //            {
    //                return typeof(UnknownTable);
    //            }
    //        }
    //        else
    //        {
    //            return typeof(UnknownTable);
    //        }
    //    }

    //    /// <summary>
    //    /// Gets MySql safe table name. If table not found an exception is throwed.
    //    /// </summary>
    //    /// <param name="table">String with table name or table type.</param>
    //    /// <returns>Safe table name as string.</returns>
    //    public string GetTableName(object table)
    //    {
    //        if (table is String)
    //            return String.Format("`{0}`", table);

    //        Type tableType = this.GetTable(table);
    //        return String.Format("`{0}`", tableType.Name);
    //    }

    //    /// <summary>
    //    /// Creates special field describes all fields from given table.
    //    /// </summary>
    //    /// <param name="tableName">Source table.</param>
    //    /// <returns>MySqlAllFields object contains all fields from given table.</returns>
    //    public MySqlAllFields GetAllFields(object tableName)
    //    {
    //        Type table = this.GetTable(tableName);
    //        return new MySqlAllFields(table);
    //    }

    //    /// <summary>
    //    /// Gets list of fieds from given table.
    //    /// </summary>
    //    /// <param name="tableName">Source table.</param>
    //    /// <returns>List of MySqlField objects.</returns>
    //    public List<MySqlField> GetFields(object tableName)
    //    {
    //        Type table = this.GetTable(tableName);

    //        List<MySqlField> list = new List<MySqlField>();
    //        foreach (FieldInfo field in MySqlDbObjectModel.GetStaticFields(table))
    //        {
    //            if (field.FieldType.IsSubclassOf(typeof(MySqlField)) && !field.FieldType.IsSubclassOf(typeof(MySqlVirtual)))
    //            {
    //                list.Add(field.GetValue(null) as MySqlField);
    //            }
    //        }

    //        return list;
    //    }

    //    public MySqlField GetPrimaryKey(object tableName)
    //    {
    //        foreach (MySqlField f in this.GetFields(tableName))
    //        {
    //            if (f.IsPrimaryKey)
    //                return f;
    //        }

    //        return null;
    //    }

    //    /// <summary>
    //    /// Loads and initializes MySqlTable derivered table type.
    //    /// This function creates instances of each MySqlField object in table
    //    /// and applies additional atrributes for they.
    //    /// Also the static properties Table and All are set.
    //    /// </summary>
    //    /// <param name="tableType"></param>
    //    public void LoadTableClass(Type tableType)
    //    {
    //        if (!_tables.ContainsKey(tableType.Name))
    //        {
    //            _tables.Add(tableType.Name, tableType);
    //        }
    //        else
    //        {
    //            return;
    //        }

    //        foreach (FieldInfo field in GetStaticFields(tableType))
    //        {
    //            if (field.FieldType.IsSubclassOf(typeof(MySqlField)))
    //            {
    //                if (field.FieldType.IsSubclassOf(typeof(MySqlVirtual)))
    //                    continue;

    //                MySqlField sqlField;
    //                if (field.GetValue(null) != null)
    //                {
    //                    sqlField = (MySqlField)field.GetValue(null);
    //                }
    //                else
    //                {
    //                    sqlField = Activator.CreateInstance(field.FieldType) as MySqlField;
    //                    sqlField.Caption = field.Name;
    //                }

    //                sqlField.ModelField = field;
    //                sqlField.Name = field.Name;
    //                sqlField.Table = tableType;
    //                // Caption can be set direct in C# code, so it shouldn't be reseted this way.

    //                foreach (object attr in field.GetCustomAttributes(true))
    //                {
    //                    if (attr is PrimaryKeyAttribute)
    //                    {
    //                        sqlField.IsPrimaryKey = true;
    //                    }
    //                    else if (attr is CaptionAttribute)
    //                    {
    //                        Trace.WriteLine("WARNING! Please remove [Caption] from " + tableType.Name + "." + sqlField.Name);
    //                    }
    //                    else if (attr is DescriptionAttribute)
    //                    {
    //                        Trace.WriteLine("WARNING! Please remove [Description] from " + tableType.Name + "." + sqlField.Name);
    //                    }
    //                    else if (attr is CaptionLocalizationEntryAttribute)
    //                    {
    //                        string localizationEntryId = (attr as CaptionLocalizationEntryAttribute).LocalizationEntryId;
    //                        sqlField.Caption = localizationEntryId;
    //                    }
    //                    else if (attr is DefaultFieldValueAttribute)
    //                    {
    //                        sqlField.DefaultValue = (attr as DefaultFieldValueAttribute).Value;
    //                    }
    //                    else if (attr is NullableAttribute)
    //                    {
    //                        sqlField.Nullable = true;
    //                    }
    //                    else if (attr is GridColumnVisibleAttribute)
    //                    {
    //                        sqlField.GridColumnVisible = (attr as GridColumnVisibleAttribute).Visible;
    //                    }
    //                    else if (attr is FilteringControlAttribute)
    //                    {
    //                        sqlField.FilteringControlType = (attr as FilteringControlAttribute).ControlType;
    //                    }
    //                    else if (attr is CategoryAttribute)
    //                    {
    //                        sqlField.Category = (attr as CategoryAttribute).Category;
    //                    }
    //                    else if (attr is DescriptionLocalizationEntryAttribute)
    //                    {
    //                        string localizationEntryId = (attr as DescriptionLocalizationEntryAttribute).LocalizationEntryId;
    //                        sqlField.Description =localizationEntryId;
    //                    }
    //                    else if (attr is RelatedToTableAttribute)
    //                    {
    //                        sqlField.RelatedSource = (attr as RelatedToTableAttribute).Table;
    //                    }
    //                }

    //                field.SetValue(null, sqlField);
    //            }

    //        }

    //        //Trace.TraceInformation("Type {0} loaded as MySqlTable.", tableType.FullName);
    //    }

    //    public static IEnumerable<FieldInfo> GetStaticFields(Type type)
    //    {
    //        while (type.IsSubclassOf(typeof(MySqlTableBase)))
    //        {
    //            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
    //            {
    //                yield return field;
    //            }

    //            type = type.BaseType;
    //        }
    //    }

    //    public MySqlField FindFieldByID(string id)
    //    {
    //        if (id == null)
    //            return null;

    //        id = System.Text.RegularExpressions.Regex.Replace(id, "\r|\n|\t|`| ", "");
    //        string[] parts = id.Split('.');
    //        if (parts.Length != 2)
    //            return null;

    //        if (!_tables.ContainsKey(parts[0]))
    //            return null;

    //        Type table = _tables[parts[0]];
    //        foreach (MySqlField field in this.GetFields(table))
    //        {
    //            if (field.Name == parts[1])
    //                return field;
    //        }

    //        return null;
    //    }
    //}

    //public abstract class MySqlVirtual : MySqlField
    //{
    //}

    //public class RelatedToTableAttribute : Attribute
    //{
    //    public object Table
    //    {
    //        get;
    //        set;
    //    }

    //    public RelatedToTableAttribute(object table)
    //    {
    //        this.Table = table;
    //    }
    //}

    //public class hsScaffoldField
    //{
    //    public hsScaffoldField()
    //    {
    //        IsRequired = false;
    //    }

    //    public object LocalizationEntry
    //    {
    //        get;
    //        set;
    //    }

    //    public ScaffoldVisibility ShowInScaffold
    //    {
    //        get;
    //        set;
    //    }

    //    public object RelatedSource
    //    {
    //        get;
    //        set;
    //    }

    //    public Boolean IsRequired
    //    {
    //        get;
    //        set;
    //    }
    //}

    //public class FilteringControlAttribute : Attribute
    //{
    //    private Type _controlType;

    //    public FilteringControlAttribute()
    //        : this(typeof(TextBox))
    //    {
    //    }

    //    public FilteringControlAttribute(Type controlType)
    //    {
    //        _controlType = controlType;
    //    }

    //    public Type ControlType
    //    {
    //        get { return _controlType; }
    //        set { _controlType = value; }
    //    }

    //}
    //public class DescriptionLocalizationEntryAttribute : LocalizationEntryAttribute
    //{
    //    public DescriptionLocalizationEntryAttribute(string localizationEntryId)
    //        : base(localizationEntryId)
    //    {
    //    }

    //    public DescriptionLocalizationEntryAttribute(LocalizationEntry localizationEntry)
    //        : base(localizationEntry)
    //    {
    //    }
    //}
    //public class GridColumnVisibleAttribute : Attribute
    //{
    //    private bool _visible;

    //    public GridColumnVisibleAttribute(bool visible)
    //    {
    //        _visible = visible;
    //    }

    //    public bool Visible
    //    {
    //        get
    //        {
    //            return _visible;
    //        }
    //        set
    //        {
    //            _visible = value;
    //        }
    //    }
    //}
    //public class DefaultFieldValueAttribute : Attribute
    //{
    //    public enum Values
    //    {
    //        CurrentDateTime,
    //        ColorWhite
    //    }

    //    private object _value;
    //    public DefaultFieldValueAttribute(object value)
    //    {
    //        _value = value;
    //    }

    //    /// <summary>
    //    /// Value for new rows.
    //    /// </summary>
    //    public object Value
    //    {
    //        get
    //        {
    //            return _value;
    //        }
    //    }
    //}

    //public enum ScaffoldVisibility
    //{
    //    None = 0,
    //    ScGrid = 1,
    //    ScEntity = 2
    //}
    //public class PrimaryKeyAttribute : Attribute
    //{
    //}

    //[XmlInclude(typeof(Translation))]
    //[Serializable]
    //public class LocalizationEntry
    //{
    //    public LocalizationEntry()
    //    {
    //        this.CatalogName = string.Empty;
    //        this.Identifier = string.Empty;
    //        this.Translation = new Dictionary<string, string>();
    //        this.TranslationsList = new List<Translation>();
    //    }

    //    public static readonly LocalizationEntry Empty = new LocalizationEntry();

    //    public string CatalogName
    //    {
    //        get;
    //        set;
    //    }

    //    public string Identifier
    //    {
    //        get;
    //        set;
    //    }

    //    [Obsolete("Use GetTranslation(\"PL\")")]
    //    [XmlIgnore]
    //    public string TranslationPL
    //    {
    //        get
    //        {
    //            return this.GetTranslation("PL");
    //        }
    //        set
    //        {
    //            this.SetTranslation(value, "PL");
    //        }
    //    }

    //    [Obsolete("Use GetTranslation(\"GB\")")]
    //    [XmlIgnore]
    //    public string TranslationGB
    //    {
    //        get
    //        {
    //            return this.GetTranslation("GB");
    //        }
    //        set
    //        {
    //            this.SetTranslation(value, "GB");
    //        }
    //    }

    //    [Obsolete("Use GetTranslation(\"US\")")]
    //    [XmlIgnore]
    //    public string TranslationUS
    //    {
    //        get
    //        {
    //            return this.GetTranslation("US");
    //        }
    //        set
    //        {
    //            this.SetTranslation(value, "US");
    //        }
    //    }

    //    [XmlIgnore]
    //    public Dictionary<string, string> Translation
    //    {
    //        get;
    //        set;
    //    }

    //    [XmlIgnore]
    //    public List<Translation> TranslationsList
    //    {
    //        get
    //        {
    //            var list = new List<Translation>();

    //            foreach (var item in this.Translation)
    //            {
    //                list.Add(new Translation(item.Key, item.Value));
    //            }

    //            return list;
    //        }
    //        set
    //        {
    //            foreach (var item in value)
    //            {
    //                this.Translation.Add(item.Language, item.Text);
    //            }
    //        }
    //    }

    //    [XmlArray(ElementName = "Translations")]
    //    public Translation[] TranslationArray
    //    {
    //        get
    //        {
    //            return TranslationsList.ToArray();
    //        }

    //        set
    //        {
    //            TranslationsList = new List<Translation>(value);
    //        }
    //    }



    //    public string GetTranslation(string language)
    //    {
    //        language = language.ToUpper();
    //        //string trans = "";

    //        if (this.Translation.ContainsKey(language))
    //        {
    //            var t = this.Translation[language];
    //            if (t == null)
    //                t = String.Empty;

    //            return t;
    //        }
    //        else
    //            return String.Empty;
    //    }

    //    public void SetTranslation(string translation, string language)
    //    {
    //        language = language.ToUpper();

    //        if (this.Translation.ContainsKey(language))
    //        {
    //            this.Translation[language] = translation;
    //        }
    //        else
    //        {
    //            this.Translation.Add(language, translation);
    //        }
    //    }

    //    [XmlIgnore]
    //    public string Text
    //    {
    //        get
    //        {
    //            return "LocalizationText";
    //        }
    //    }

    //    public string FormatUsing(params object[] args)
    //    {
    //        try
    //        {
    //            return string.Format(this.Text, args);
    //        }
    //        catch
    //        {
    //            return this.Text;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns identifier like type-name_entry-name.
    //    /// </summary>
    //    /// <returns></returns>
    //    public string GetFullIdentifier()
    //    {
    //        return String.Format("DID_{0}_{1}", this.CatalogName, this.Identifier);
    //    }

    //    public static implicit operator string(LocalizationEntry e)
    //    {
    //        if (e == null)
    //            return String.Empty;

    //        return e.Text;
    //    }

    //    public override string ToString()
    //    {
    //        return this.Text;
    //    }
    //}

    //[Serializable]
    //public class Translation
    //{
    //    [XmlAttribute(AttributeName = "lang")]
    //    public string Language
    //    {
    //        get;
    //        set;
    //    }

    //    [XmlText]
    //    public string Text
    //    {
    //        get;
    //        set;
    //    }

    //    public Translation()
    //    {

    //    }

    //    public Translation(string language, string translation)
    //    {
    //        this.Language = language;
    //        this.Text = translation;
    //    }
    //}


    ///// <summary>
    ///// Allows DBNull value in field.
    ///// </summary>
    //public class NullableAttribute : Attribute
    //{
    //}
    //public class CaptionLocalizationEntryAttribute : LocalizationEntryAttribute
    //{
    //    public CaptionLocalizationEntryAttribute(string localizationEntryId)
    //        : base(localizationEntryId)
    //    {
    //    }

    //    public CaptionLocalizationEntryAttribute(LocalizationEntry localizationEntry)
    //        : base(localizationEntry)
    //    {
    //    }
    //}

    //public class LocalizationEntryAttribute : Attribute
    //{
    //    public string LocalizationEntryId
    //    {
    //        get;
    //        set;
    //    }

    //    private LocalizationEntry LocalizationEntry
    //    {
    //        get;
    //        set;
    //    }

    //    protected LocalizationEntryAttribute(string localizationEntryId)
    //    {
    //        this.LocalizationEntryId = localizationEntryId;
    //    }

    //    protected LocalizationEntryAttribute(LocalizationEntry localizationEntry)
    //    {
    //        this.LocalizationEntry = localizationEntry;
    //    }
    //}

    //[Obsolete("Should be replaced with CaptionLocalizationEntryAttribute")]
    //public class CaptionAttribute : Attribute
    //{
    //    private string _text = String.Empty;
    //    private bool _sortable = false;

    //    /// <summary>
    //    /// Defines field caption text.
    //    /// </summary>
    //    /// <param name="text">Text to display as field caption.</param>
    //    public CaptionAttribute(string text)
    //        : this(text, false)
    //    {
    //    }

    //    /// <summary>
    //    /// Defines field caption text and sortable behaviour.
    //    /// </summary>
    //    /// <param name="text">Text to display as field caption.</param>
    //    /// <param name="sortable">True if grid column of this field can be sortable.</param>
    //    public CaptionAttribute(string text, bool sortable)
    //    {
    //        _text = text;
    //        _sortable = sortable;
    //    }

    //    /// <summary>
    //    /// Gets or set caption text.
    //    /// </summary>
    //    public string Text
    //    {
    //        get
    //        {
    //            return _text;
    //        }
    //        set
    //        {
    //            _text = value;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets value indicating whether grid column
    //    /// for this field is sortable.
    //    /// </summary>
    //    public bool Sortable
    //    {
    //        get
    //        {
    //            return _sortable;
    //        }
    //        set
    //        {
    //            _sortable = value;
    //        }
    //    }
    //}

    //public class MySqlStatement
    //{
    //    protected int _affected = 0;
    //    private string _sql = String.Empty;

    //    public MySqlStatement()
    //    {
    //    }

    //    public MySqlStatement(string sql)
    //    {
    //        _sql = sql;
    //    }

    //    public MySqlStatement(string format, params object[] args)
    //    {
    //        _sql = String.Format(format, args);
    //    }

    //    private MySqlDb _db;

    //    public MySqlDb Db
    //    {
    //        get
    //        {
    //            if (_db == null)
    //                _db = MySqlDb.DefaultDb;

    //            return _db;
    //        }
    //        set
    //        {
    //            _db = value;
    //        }
    //    }

    //    protected virtual void OnExecuted()
    //    {

    //    }

    //    protected void InvalidateCache(string table)
    //    {
    //        try
    //        {
    //            if (_affected > 0)
    //            {
    //                Type tableType = this.Db.ObjectModel.GetTable(table);
    //                MethodInfo method = tableType.GetMethod("InvalidateCache",
    //                    BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);

    //                if (method != null)
    //                {
    //                    method.Invoke(null, null);
    //                }
    //            }
    //        }
    //        catch { }
    //    }

    //    public int ExecSql()
    //    {
    //        this.Prepare();

    //        MySqlCommand cmd = new MySqlCommand(
    //            this.ToString(),
    //            this.Db.Connection);

    //        cmd.CommandTimeout = -1;

    //        try
    //        {
    //            Db.OnQueryBegin(cmd.CommandText);
    //            long time = DateTime.Now.Ticks;

    //            _affected = 0;
    //            Exception outerEx = null;

    //            try
    //            {
    //                lock (MySqlDb.GlobalLock)
    //                {
    //                    _affected = cmd.ExecuteNonQuery();
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                outerEx = ex;
    //            }

    //            if (outerEx != null)
    //                throw outerEx;

    //            Db.OnQueryEnd(cmd.CommandText, DateTime.Now.Ticks - time);
    //            this.OnExecuted();
    //            Db.HasErrors = false;

    //            return _affected;
    //        }
    //        catch (Exception firstEx)
    //        {
    //            Db.OnQueryError(cmd.CommandText, firstEx);

    //            if (Db.ThrowSqlException)
    //            {
    //                throw new Exception();
    //            }

    //            if (Debugger.IsAttached)
    //            {
    //                throw;
    //            }
    //        }

    //        return 0;
    //    }

    //    public IDataReader ExecSqlForReading()
    //    {
    //        this.Prepare();

    //        MySqlCommand cmd = new MySqlCommand(
    //            this.ToString(),
    //            this.Db.Connection);

    //        cmd.CommandTimeout = -1;

    //        try
    //        {
    //            Db.OnQueryBegin(cmd.CommandText);
    //            long time = DateTime.Now.Ticks;
    //            IDataReader r = cmd.ExecuteReader();
    //            Db.OnQueryEnd(cmd.CommandText, DateTime.Now.Ticks - time);
    //            this.OnExecuted();
    //            Db.HasErrors = false;

    //            return r;
    //        }
    //        catch (Exception firstEx)
    //        {
    //            Db.OnQueryError(cmd.CommandText, firstEx);
    //        }

    //        return null;
    //    }

    //    private bool _prepared = false;
    //    public void Prepare()
    //    {
    //        if (_prepared)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            this.OnPrepare();
    //        }
    //        catch (Exception e)
    //        {
    //        }

    //        _prepared = true;
    //    }

    //    protected virtual void OnPrepare()
    //    {
    //    }

    //    public override string ToString()
    //    {
    //        return _sql;
    //    }
    //}


    //public class MySqlUpdate : MySqlStatement
    //{
    //    private string _table = String.Empty;
    //    private string _from = String.Empty;

    //    private List<string> _sets = new List<string>();
    //    private MySqlWhere _where;
    //    private MySqlOrderBy _orderby;

    //    public MySqlUpdate()
    //    {
    //    }

    //    /// <summary>
    //    /// Gets or sets table name for insert.
    //    /// The value can be table name or MySqlTable type.
    //    /// </summary>
    //    public object Table
    //    {
    //        get
    //        {
    //            return _from;
    //        }
    //        set
    //        {
    //            if (value is string)
    //            {
    //                _from = (string)value;
    //            }
    //            else
    //            {
    //                _from = this.Db.ObjectModel.GetTableName(value);
    //            }

    //            _table = _from;
    //        }
    //    }

    //    public void SetTables(params object[] tables)
    //    {
    //        List<string> names = new List<string>();
    //        foreach (object table in tables)
    //            names.Add(this.Db.ObjectModel.GetTableName(table));

    //        _from = String.Join(",", names.ToArray());

    //        if (names.Count > 0)
    //            _table = names[0];
    //    }

    //    public MySqlUpdate InnerJoin(object table)
    //    {
    //        return InnerJoin(table, null);
    //    }

    //    public MySqlUpdate InnerJoin(object table, string alias)
    //    {
    //        string t = this.Db.ObjectModel.GetTableName(table);

    //        _from += String.Format(" INNER JOIN " + t);

    //        if (alias != null)
    //        {
    //            _from += " " + alias;
    //        }


    //        return this;
    //    }

    //    public MySqlUpdate On(MySqlWhere rule)
    //    {
    //        _from += String.Format(" ON {0}", rule.ToString());

    //        return this;
    //    }

    //    /// <summary>
    //    /// Adds set caluse to update statement.
    //    /// </summary>
    //    /// <param name="field">Field to set.</param>
    //    /// <param name="value">Field value.</param>
    //    public MySqlUpdate Set(object field, object value)
    //    {
    //        if (field is MySqlField)
    //        {
    //            MySqlField mf = field as MySqlField;
    //            if (value == null || value is DBNull)
    //                value = mf.DefaultValue;
    //        }

    //        _sets.Add(field.ToString() + "=" + MySqlValueSerializer.Get(value));
    //        return this;
    //    }

    //    public MySqlUpdate SetName<T>(MySqlEnum<T> field, T value)
    //    {
    //        return this.Set(field, value);
    //    }

    //    /// <summary>
    //    /// Appends where clasuse to update statement.
    //    /// </summary>
    //    /// <param name="where">Where rule to control update statement.</param>
    //    public MySqlUpdate Where(MySqlWhere where)
    //    {
    //        _where = where;
    //        return this;
    //    }

    //    public MySqlUpdate OrderBy(MySqlOrderBy orderby)
    //    {
    //        _orderby = orderby;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets SQL update statement as string.
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        StringBuilder s = new StringBuilder();
    //        s.AppendFormat("UPDATE " + _from);

    //        string sets = String.Join(",", _sets.ToArray());

    //        s.Append(" SET " + sets);

    //        if (_where != null)
    //        {
    //            s.Append(" WHERE " + _where.ToString());
    //        }

    //        if (_orderby != null)
    //        {
    //            s.Append(" ORDER BY " + _orderby.ToString());
    //        }

    //        return s.ToString();
    //    }

    //    protected override void OnPrepare()
    //    {
    //        DateTime stamp = DateTime.Now;
    //        string user = MySqlDb.UserGuid;
    //        string machine = MySqlDb.MachineName;

    //        this.Set(
    //            _table + ".`X_ModifyTime`",
    //            stamp);

    //        this.Set(
    //            _table + ".`X_ModifyUser`",
    //            user);

    //        this.Set(
    //            _table + ".`X_ModifyMachine`",
    //            machine);
    //    }

    //    protected override void OnExecuted()
    //    {
    //        base.OnExecuted();
    //        InvalidateCache(_table);
    //    }
    //}

    //public class MySqlDelete : MySqlStatement
    //{
    //    private string _table = String.Empty;

    //    private MySqlWhere _where;

    //    public MySqlDelete()
    //    {
    //    }

    //    /// <summary>
    //    /// Gets or sets table name for insert.
    //    /// The value can be table name or MySqlTable type.
    //    /// </summary>
    //    public object Table
    //    {
    //        get
    //        {
    //            return _table;
    //        }
    //        set
    //        {
    //            _table = this.Db.ObjectModel.GetTableName(value);
    //        }
    //    }

    //    /// <summary>
    //    /// Appends where clasuse to delete statement.
    //    /// </summary>
    //    /// <param name="where">Where rule to control delete statement.</param>
    //    public MySqlDelete Where(MySqlWhere where)
    //    {
    //        _where = where;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets SQL delete statement as string.
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        StringBuilder s = new StringBuilder();
    //        s.AppendFormat("UPDATE {0} ", _table);

    //        s.AppendFormat("SET {0}='{1}', {2}='{3}', {4}='{5}'",
    //            _table + ".`X_RemoveTime`",
    //            DateTime.Now.ToString(MySqlConfig.MySqlDateTimeFormat),
    //            _table + ".`X_RemoveUser`",
    //            MySqlDb.UserGuid,
    //            _table + ".`X_RemoveMachine`",
    //            MySqlDb.MachineName);

    //        if (_where != null)
    //        {
    //            s.Append(" WHERE " + _where.ToString());
    //        }

    //        return s.ToString();
    //    }

    //    protected override void OnExecuted()
    //    {
    //        base.OnExecuted();
    //        InvalidateCache(_table);
    //    }
    //}

    //public class MySqlInsert : MySqlStatement
    //{
    //    private object _tableObject;
    //    private string _table = String.Empty;
    //    private bool _ignore = false;

    //    private List<string> _fields = new List<string>();
    //    private List<List<string>> _values = new List<List<string>>();

    //    public MySqlInsert()
    //    {
    //        _values.Add(new List<string>());
    //    }

    //    /// <summary>
    //    /// Gets or sets table name for insert.
    //    /// The value can be table name or MySqlTable type.
    //    /// </summary>
    //    public object Table
    //    {
    //        get
    //        {
    //            return _table;
    //        }
    //        set
    //        {
    //            _tableObject = value;
    //            _table = this.Db.ObjectModel.GetTableName(value);
    //        }
    //    }

    //    /// <summary>
    //    /// Adds given list of fields to insert clause.
    //    /// </summary>
    //    /// <param name="fields"></param>
    //    public MySqlInsert Fields(params object[] fields)
    //    {
    //        foreach (object f in fields)
    //        {
    //            _fields.Add(f.ToString());
    //            _fields = _fields.Distinct().ToList();
    //        }

    //        return this;
    //    }

    //    /// <summary>
    //    /// Adds given list of values to insert clause.
    //    /// </summary>
    //    /// <param name="values"></param>
    //    public MySqlInsert Values(params object[] values)
    //    {
    //        foreach (object v in values)
    //        {
    //            _values.Last().Add(MySqlValueSerializer.Get(v));
    //        }

    //        return this;
    //    }

    //    public MySqlInsert Set(object field, object value)
    //    {
    //        this.Fields(field);

    //        var f = field as MySqlField;
    //        if (f != null && !f.Nullable && (value == null || value is DBNull))
    //        {
    //            value = f.DefaultValue;
    //        }

    //        this.Values(value);
    //        return this;
    //    }

    //    public MySqlInsert RemoveLastRow()
    //    {
    //        _values.Remove(_values.Last());
    //        RemovePreparedValuesFromLastRow();
    //        return this;
    //    }

    //    public MySqlInsert NewRow()
    //    {
    //        OnPrepare();
    //        _values.Add(new List<string>());
    //        return this;
    //    }

    //    public MySqlInsert Ignore(bool ignore)
    //    {
    //        _ignore = ignore;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets SQL insert statement as string.
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        StringBuilder s = new StringBuilder();

    //        if (_ignore)
    //        {
    //            s.AppendFormat("INSERT IGNORE " + _table);
    //        }
    //        else
    //        {
    //            s.AppendFormat("REPLACE INTO " + _table);
    //        }

    //        List<string> fList = new List<string>(_fields);

    //        foreach (MySqlField field in this.Db.ObjectModel.GetFields(_tableObject))
    //        {
    //            if (fList.Contains(field.ToString()) || field.IsInternal)
    //            {
    //                continue;
    //            }

    //            fList.Add(field.ToString());
    //        }

    //        var valuesVectors = GetDataVectors().ToArray();
    //        var fields = String.Join(",", fList.ToArray());

    //        s.Append(String.Format("( {0} )", fields));
    //        s.Append(" VALUES " + String.Join(", ", valuesVectors));

    //        return s.ToString();
    //    }

    //    private void RemovePreparedValuesFromLastRow()
    //    {
    //        DateTime stamp = DateTime.Now;
    //        var user = CreateUser ?? MySqlDb.UserGuid;
    //        var machine = MySqlUtils.GetEscaped(MySqlDb.MachineName);

    //        RemoveValues(
    //            CreateTime,
    //            stamp,
    //            user,
    //            user,
    //            machine,
    //            machine);
    //    }

    //    private void RemoveValues(params object[] values)
    //    {
    //        foreach (object v in values)
    //        {
    //            _values.Last().Remove(MySqlValueSerializer.Get(v));
    //        }
    //    }

    //    private IEnumerable<string> GetDataVectors()
    //    {
    //        foreach (var valuesVector in _values.Where(list => list.Any()))
    //        {
    //            List<string> vList = new List<string>(valuesVector);
    //            foreach (MySqlField field in this.Db.ObjectModel.GetFields(_tableObject))
    //            {
    //                if (_fields.Contains(field.ToString()) || field.IsInternal)
    //                {
    //                    continue;
    //                }

    //                vList.Add(MySqlValueSerializer.Get(field.DefaultValue));
    //            }

    //            var valuesRows = String.Format("( {0} )", String.Join(",", vList.ToArray()));
    //            yield return valuesRows;
    //        }
    //    }

    //    public DateTime CreateTime = DateTime.Now;
    //    public object CreateUser = null;

    //    protected override void OnPrepare()
    //    {
    //        DateTime stamp = DateTime.Now;
    //        var user = this.CreateUser ?? MySqlDb.UserGuid;
    //        var machine = MySqlUtils.GetEscaped(MySqlDb.MachineName);

    //        this.Fields(
    //            _table + ".`X_CreateTime`",
    //            _table + ".`X_ModifyTime`",
    //            _table + ".`X_CreateUser`",
    //            _table + ".`X_ModifyUser`",
    //            _table + ".`X_CreateMachine`",
    //            _table + ".`X_ModifyMachine`");

    //        this.Values(
    //            this.CreateTime,
    //            stamp,
    //            user,
    //            user,
    //            machine,
    //            machine);
    //    }

    //    protected override void OnExecuted()
    //    {
    //        base.OnExecuted();
    //        InvalidateCache(_table);
    //    }
    //}
    //public static class MySqlUtils
    //{
    //    public static string GetEscaped(object element)
    //    {
    //        var content = element.ToString();

    //        content = content.Replace("\\", "\\\\");
    //        content = "'" + content.Replace("'", @"\'") + "'";
    //        content = content.Trim();

    //        return content;
    //    }
    //}
}