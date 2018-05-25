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

    using e2App.PrintTemplates;

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

            this.ShowPrintPreview();
        }

        private void ShowPrintPreview()
        {
            var printing = this.GetRandomPrinting();

            var chromiumPrintTemplateViewer = kurwa.ShowFor(printing, false, string.Empty, null);
            chromiumPrintTemplateViewer.Closed += (sender, args) => ShowPrintPreview();
        }

        //private ChromiumWebBrowser BuildChromiumWebBrowserWithLoadedHtml()
        //{
        //    var printing = this.GetRandomPrinting();
        //    var browser = new ChromiumWebBrowser(string.Empty);
        //    browser.LoadHtml(printing, @"c:\");
        //    return browser;
        //}

        private string GetRandomPrinting()
        {
            var index = this.random.Next(0, this._printings.Count);
            var printing = this._printings[index];
            return printing;
        }
    }
}
