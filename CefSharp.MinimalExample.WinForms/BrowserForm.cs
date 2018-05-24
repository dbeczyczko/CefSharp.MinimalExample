// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
using System;
using System.IO;
using System.Windows.Forms;
using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    using System.Collections.Generic;

    public partial class BrowserForm : Form
    {
        private List<string> _printings = new List<string>();
        Random random = new Random();

        public BrowserForm()
        {
            InitializeComponent();

            Text = "CefSharp";
            WindowState = FormWindowState.Maximized;

            var files = Directory.EnumerateFiles("Templates", "*.html");
            foreach (var file in files)
            {
                var allText = File.ReadAllText(file);
                _printings.Add(allText);
            }

            var browser = BuildChromiumWebBrowserWithLoadedHtml();
            this.ShowPrintPreview(browser);
        }

        private void ShowPrintPreview(ChromiumWebBrowser browser)
        {
            var printPreview = new PrintPreview();
            printPreview.Controls.Add(browser);
            printPreview.Closed += PrintPreviewOnClosed;
            printPreview.Show();
        }

        private void PrintPreviewOnClosed(object sender, EventArgs eventArgs)
        {
            var browser = this.BuildChromiumWebBrowserWithLoadedHtml();
            ShowPrintPreview(browser);
        }

        private ChromiumWebBrowser BuildChromiumWebBrowserWithLoadedHtml()
        {
            var printing = this.GetRandomPrinting();
            var browser = new ChromiumWebBrowser(string.Empty);
            browser.LoadHtml(printing, @"c:\");
            return browser;
        }

        private string GetRandomPrinting()
        {
            var index = this.random.Next(0, this._printings.Count);
            var printing = this._printings[index];
            return printing;
        }
    }
}
