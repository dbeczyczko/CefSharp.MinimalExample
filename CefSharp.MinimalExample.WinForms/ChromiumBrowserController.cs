using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;

using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e2App.PrintTemplates
{
    using CefSharp.MinimalExample.WinForms;

    public class ChromiumBrowserController
    { 
        private const string PageSetupRegistryKey = "SOFTWARE\\MICROSOFT\\Internet Explorer\\PageSetup";
        private const string EsScriptNodePath = "//body/es-script";
        private const string LabelWithDottedUnderline = "<label style=\"border-bottom: 1px #000 dotted\">{0}</label>";
        private const string LabelWithoutDottedUnderline = "<label>{0}</label>";

        private readonly ChromiumWebBrowser _browser;

        private string _documentId;
        private bool _setNoMargins;

        public ChromiumBrowserController(ChromiumWebBrowser browser)
        {
            _browser = browser;
            _browser.FrameLoadEnd += WebBrowserOnDocumentCompleted;

            RemoveJsOnExit = true;
        }
        
        public bool RemoveJsOnExit { get; set; }

        public void LoadDocument(string source, string documentId = null, bool setNoMargins = false)
        {
            _documentId = documentId;

            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(source);

            var htmlNode = document.DocumentNode.SelectSingleNode("//html");
            if (htmlNode == null)
            {
                source = WrapContentInHtmlTemplate(source);
            }

            source = AddScriptToSource(source);
            LoadHtml(source, _browser);

            _setNoMargins = setNoMargins;
        }

        public void LoadFromPath(string url)
        {
            _browser.Load(url);
        }

        public Optional<DocumentDto> GetLastDocument()
        {
            if (_documentId == null)
            {
                return Optional<DocumentDto>.Absent();
            }

            var documentDto = new DocumentDto
            {
                Id = _documentId,
                Source = GetSource()
            };

            return Optional<DocumentDto>.Of(documentDto);
        }

        public string GetSource()
        {
            var document = GetCurrentRenderedDOM();

            if (RemoveJsOnExit)
            {
                RemoveEsScriptFromSource(document);
            }
            RemoveMetaNodeFromSource(document);

            return document.DocumentNode.InnerHtml;
        }

        public string GetSourceForPrinting()
        {
            var document = GetCurrentRenderedDOM();

            RemoveEsScriptFromSource(document);

            return document.DocumentNode.InnerHtml;
        }

        private HtmlDocument GetCurrentRenderedDOM()
        {
            var source = GetDocumentText(_browser);
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(source);

            var htmlNode = document.DocumentNode.SelectSingleNode("//html");
            var htmlNodeStringWithJavaScriptRendered = GetDocumentText(_browser);
            var htmlNodeWithJavaScriptRendered = HtmlNode.CreateNode(htmlNodeStringWithJavaScriptRendered);

            if (RemoveJsOnExit)
            {
                RemoveJavaScriptNodesFrom(htmlNodeWithJavaScriptRendered);
            }

            if (htmlNode == null)
            {
                document.DocumentNode.RemoveAll();
                document.DocumentNode.AppendChild(htmlNodeWithJavaScriptRendered);
            }
            else
            {
                document.DocumentNode.ReplaceChild(htmlNodeWithJavaScriptRendered, htmlNode);
            }

            return document;
        }

        private static void RemoveJavaScriptNodesFrom(HtmlNode htmlNodeWithJavaScriptRendered)
        {
            IEnumerable<HtmlNode> scriptNodes = htmlNodeWithJavaScriptRendered.SelectNodes("//script");
            if (scriptNodes == null)
            {
                return;
            }

            scriptNodes = scriptNodes.ToList();
            foreach (var scriptNode in scriptNodes)
            {
                scriptNode.Remove();
            }
        }

        public string GetConvertedSource()
        {
            var source = GetSource();
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(source);
            ConvertSelects(document);
            ConvertTextAreas(document);
            ConvertTextBoxes(document);

            return document.DocumentNode.InnerHtml;
        }

        public void Unload()
        {
            _browser.Load("about:blank");
            _documentId = null;
        }

        public void ShowPrintPreview()
        {
            Print();
        }

        public void Print()
        {
            _browser.Print();
        }

        public void PrintToPdf()
        {
            var tempFilename = Guid.NewGuid() + ".pdf";

            var pathToTempPdfFile = Path.Combine(Path.GetTempPath(), tempFilename);

            var result = _browser.PrintToPdfAsync(pathToTempPdfFile).Result;

            if (result)
            {
                Process.Start(pathToTempPdfFile);
            }
        }

        public void QuickPrint()
        {
            Print();
        }

        public void SaveAs()
        {
            var source = GetDocumentText(_browser);

            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Plik HTML|*.html|Wszystko|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (dialog)
                {
                    File.WriteAllText(dialog.FileName, source);
                }
            }
        }

        public void Show()
        {
            _browser.Show();
        }

        private static void RemoveEsScriptFromSource(HtmlDocument document)
        {
            var esScriptNode = document.DocumentNode.SelectSingleNode(EsScriptNodePath);
            esScriptNode?.RemoveAllChildren();
        }

        private void WebBrowserOnDocumentCompleted(object sender, FrameLoadEndEventArgs e)
        {
        }

        private string GetDocumentText(ChromiumWebBrowser browser)
        {
            if (browser.InvokeRequired)
            {
                return browser.Invoke(new Action(() => GetDocumentText(browser))) as string;
            }

            var scriptAsync = _browser.EvaluateScriptAsync("document.getElementsByTagName('html')[0].innerHTML;");
            var domAsString = scriptAsync.Result.Result as string;
            return $"<HTML>{domAsString}</HTML>";
        }

        private void RemoveMetaNodeFromSource(HtmlDocument document)
        {
            var metaNodes =
                document.DocumentNode.SelectNodes("//meta[@content='IE=9']");
            if (metaNodes != null)
            {
                foreach (var metaNode in metaNodes)
                {
                    metaNode.Remove();
                }
            }
        }

        private void ConvertTextAreas(HtmlDocument document)
        {
            var textAreas = document.DocumentNode.SelectNodes("//textarea");

            if (textAreas != null)
            {
                foreach (var node in textAreas)
                {
                    var textAreaValue = node.InnerHtml;
                    HtmlNode labelNode;

                    if (textAreaValue.Contains(Environment.NewLine))
                    {
                        var textAreaValueAfterFormat = textAreaValue.Replace(Environment.NewLine, "<br/>");
                        labelNode = HtmlNode.CreateNode(String.Format(LabelWithoutDottedUnderline, textAreaValueAfterFormat));
                    }
                    else
                    {
                        labelNode = HtmlNode.CreateNode(String.Format(LabelWithDottedUnderline, textAreaValue));
                    }

                    node.ParentNode.ReplaceChild(labelNode, node);
                }
            }
        }

        private void ConvertTextBoxes(HtmlDocument document)
        {
            var inputs = document.DocumentNode.SelectNodes("//input");
            if (inputs != null)
            {
                var textBoxes = inputs.Where(node =>
                {
                    var inputType = node.GetAttributeValue("type", String.Empty);
                    return inputType == "text" || String.IsNullOrWhiteSpace(inputType);
                });

                foreach (var node in textBoxes)
                {
                    var value = node.GetAttributeValue("Value", String.Empty);
                    var labelNode = HtmlNode.CreateNode(String.Format(LabelWithDottedUnderline, value));
                    node.ParentNode.ReplaceChild(labelNode, node);
                }
            }
        }

        private void ConvertSelects(HtmlDocument document)
        {
            var selectNodes = document.DocumentNode.SelectNodes("//select");

            if (selectNodes != null)
            {
                foreach (var node in selectNodes)
                {
                    var options = node.SelectNodes("option");
                    if (options == null)
                    {
                        continue;
                    }

                    var selectedOptions = options
                        .Where(htmlNode => htmlNode.Attributes.Contains("selected"))
                        .ToList();

                    if (selectedOptions.Count > 0)
                    {
                        var values = selectedOptions.Select(selectedOption => selectedOption.GetAttributeValue("value", String.Empty)).ToList();
                        var value = String.Join(", ", values);

                        var style = options
                            .FirstOrDefault(htmlNode => htmlNode.Attributes.Contains("selected"))
                            .GetAttributeValue("style", String.Empty);

                        var labelNode = HtmlNode.CreateNode(String.Format("<label>{0}</label>", value));
                        labelNode.Attributes.Add("style", style);
                        node.ParentNode.ReplaceChild(labelNode, node);
                    }
                    else
                    {
                        var labelNode = HtmlNode.CreateNode(String.Format("<label>{0}</label>", "---"));
                        node.ParentNode.ReplaceChild(labelNode, node);
                    }
                }
            }
        }

        private string AddScriptToSource(string source)
        {
            var document = new HtmlDocument { OptionOutputOriginalCase = true };
            document.LoadHtml(source);
            SetBrowserCompatibilityMode(document);
            var esScriptNode = GetEsScriptNode(document);
            if (esScriptNode != null)
            {
                AppendJquery(esScriptNode);

                var newNode = HtmlNode.CreateNode(Resources.EsScript);
                esScriptNode.AppendChild(newNode);
            }

            return document.DocumentNode.InnerHtml;
        }

        private void AppendJquery(HtmlNode esScriptNode)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<script type=\"text/javascript\">");
            stringBuilder.AppendLine(Resources.jquery);
            stringBuilder.AppendLine("</script>");
            var jqueryNode = HtmlNode.CreateNode(stringBuilder.ToString());
            esScriptNode.AppendChild(jqueryNode);
        }

        private void SetBrowserCompatibilityMode(HtmlDocument document)
        {
            var metaNode = HtmlNode.CreateNode("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\" >");
            if (document.DocumentNode.SelectSingleNode("//head") == null)
            {
                var headNode = HtmlNode.CreateNode("<head></head>");
                var selectHtmlNode = document.DocumentNode.SelectSingleNode("//html");
                selectHtmlNode.AppendChild(headNode);
            }
            document.DocumentNode.SelectSingleNode("//head").AppendChild(metaNode);
        }

        private HtmlNode GetEsScriptNode(HtmlDocument document)
        {
            var node = document.DocumentNode.SelectSingleNode(EsScriptNodePath);
            if (node == null)
            {
                node = HtmlNode.CreateNode("<es-script/>");
                document.DocumentNode.SelectSingleNode("//body").AppendChild(node);
            }

            return node;
        }

        private void LoadHtml(string html, ChromiumWebBrowser browser)
        {
            browser.LoadHtml(html, @"c:\");
        }

        private string WrapContentInHtmlTemplate(string source)
        {
            return String.Format(Resources.HtmlTemplate, source);
        }
    }

    public struct Optional<T>
    {
        private readonly T _value;
        private bool _hasValue;

        private Optional(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public T Value
        {
            get
            {
                if (HasValue)
                {
                    return _value;
                }
                else
                {
                    throw new InvalidOperationException("value is null");
                }
            }
        }

        public bool HasValue
        {
            get { return _hasValue; }
        }

        public bool IsAbsent => !HasValue;

        public override string ToString()
        {
            string value = HasValue ? string.Format("Of('{0}')", Value) : "Absent";
            return string.Format("Optional<{0}>{1}", typeof(T), value);
        }

        private bool Equals(Optional<T> other)
        {
            return HasValue.Equals(other.HasValue) && EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Optional<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(_value) * 397) ^ HasValue.GetHashCode();
            }
        }

        public static Optional<T> Absent()
        {
            return new Optional<T>();
        }

        public static Optional<T> Of(T value)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            //if value is value type it will never be null and always valid
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return new Optional<T>(value);
        }

        public void UseValue(Action<T> action)
        {
            if (HasValue)
            {
                action(Value);
            }
        }

        public Optional<TDest> UseValue<TDest>(Func<T, TDest> func)
        {
            if (HasValue)
            {
                var dest = func(Value);
                return Optional<TDest>.Of(dest);
            }
            return Optional<TDest>.Absent();
        }

    }

    public class DocumentDto
    {
        public string Id { get; set; }
        public string Source { get; set; }
    }

}