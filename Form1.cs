﻿using HtmlAgilityPack;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace Manga_Downloader
{
    public partial class MainWindow : Form
    {
        string toConsole = "";
        bool broken_link = false;
        public delegate void InvokeDelegate();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            txtConsole.Clear();
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            string path;

            fbdBrowse.ShowDialog();
            path = fbdBrowse.SelectedPath;

            txtPath.Text = path;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnClearConsole.Enabled = false;
            btnPath.Enabled = false;
            btnStart.Enabled = false;

            txtLink.ReadOnly = true;
            txtName.ReadOnly = true;
            txtPath.ReadOnly = true;

            backgroundWorker1.RunWorkerAsync();
        }

        private List<string> FindAllImgSrc(HtmlAgilityPack.HtmlDocument htmlSnippet)
        {
            List<string> hrefTags = new List<string>();

            try
            {
                foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//img[@src]"))
                {
                    HtmlAttribute att = link.Attributes["src"];
                    hrefTags.Add(att.Value);
                }
            }
            catch (System.NullReferenceException)
            {
                Write(0, "Link not valid", 1);
                broken_link = true;
            }

            return hrefTags;
        }

        private List<string> CheckPage(HtmlAgilityPack.HtmlDocument htmlSnippet)
        {
            List<string> hrefTags = new List<string>();

            try
            {
                foreach (HtmlNode link in htmlSnippet.DocumentNode.SelectNodes("//div[@id]"))
                {
                    HtmlAttribute att = link.Attributes["id"];
                    hrefTags.Add(att.Value);
                }
            }
            catch (System.NullReferenceException)
            {
                Write(0, "Link not valid", 1);
                broken_link = true;
            }

            return hrefTags;
        }

        private void Write(int lines_before, string text, int lines_after)
        {
            int i = 0, o = 0;
            string output = "";

            while (i < lines_before)
            {
                output = output + "\n";
                i++;
            }

            output = output + text;

            while (o < lines_after)
            {
                output = output + "\n";
                o++;
            }

            toConsole = output;

            txtConsole.BeginInvoke(new InvokeDelegate(WriteToConsole));
        }

        void WriteToConsole()
        {
            txtConsole.AppendText(toConsole);
        }

        private int DownloadPage(int chapter, int page, string link, string name, string path, HtmlAgilityPack.HtmlDocument doc)
        {
            int returnCode = 0;
            List<string> img;

            Write(0, "Downloading page " + page.ToString() + " in chapter " + chapter.ToString() + "...", 1);
            CheckFolderStructure(path, name, chapter);
            img = FindAllImgSrc(doc);

            using (WebClient clientPicture = new WebClient())
            {
                clientPicture.DownloadFile(img[0], path + "\\" + name + "\\" + chapter.ToString() + "\\" + page.ToString() + ".jpg");
            }

            return returnCode;
        }

        private int CheckFolderStructure(string path, string name, int chapter)
        {
            int returnCode = 0;

            try
            {
                if (!Directory.Exists(path + "\\" + name + "\\" + chapter.ToString()))
                    Directory.CreateDirectory(path + "\\" + name + "\\" + chapter.ToString());

                if (!Directory.Exists(path + "\\CBZ"))
                    Directory.CreateDirectory(path + "\\CBZ");
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is ArgumentException || ex is ArgumentNullException || ex is PathTooLongException)
                {
                    Write(1, "Path is too long, there is no path set or you do not have permission to write to the specified path.", 1);
                }
            }
            

            return returnCode;
        }

        private void ZipManga(string path, string name, int chapter)
        {
            Write(1, "Creating .cbz file...", 1);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(path + "\\" + name);
                zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                zip.Save(path + "\\CBZ\\" + name + " 1-" + chapter.ToString() + ".cbz");
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int chapter = 1, page = 1, chapter_incs = 0;
            string link = txtLink.Text, name = txtName.Text, path = txtPath.Text;
            var regexItem = new Regex("^[a-zA-Z0-9_ ]+$");
            bool more_chapters = true, cont = true, download = true;
            List<string> divId;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            WebClient client = new WebClient();

            btnStart.Enabled = false;

            while (cont == true)
            {
                try
                {
                    string html = client.DownloadString(link + "/" + chapter.ToString() + "/" + page.ToString());
                    client.Dispose();
                    doc.LoadHtml(html);
                    download = true;
                }
                catch (System.Net.WebException)
                {
                    download = false;

                    if (chapter_incs >= 50)
                    {
                        Write(1, "Link invalid after 50 chapter increases.", 1);
                        chapter = chapter - 50;
                        cont = false;
                    }
                    else
                    {
                        Write(1, "Going to next chapter... (Web Exception)", 1);
                        chapter++;
                        page = 1;
                        chapter_incs++;
                    }
                }

                divId = CheckPage(doc);
                foreach (string s in divId)
                {
                    if (s == "recom_info")
                    {
                        download = false;

                        if (chapter_incs >= 50)
                        {
                            Write(1, "Link invalid after 50 chapter increases.", 1);
                            chapter = chapter - 50;
                            cont = false;
                        }
                        else
                        {
                            chapter++;
                            page = 1;
                            chapter_incs++;
                        }
                    }
                }

                if (regexItem.IsMatch(name) && more_chapters == true && broken_link == false)
                {
                    if (download == true)
                    {
                        DownloadPage(chapter, page, link, name, path, doc);
                        chapter_incs = 0;
                        page++;
                    }
                }
                else
                    MessageBox.Show("Name can only have letters, numbers, space and underscores. Check your name and try again.", "Name error");
            }

            if (chkCBZ.Checked == true)
            {
                ZipManga(path, name, chapter);
            }

            Write(1, "Done! :)", 1);
            btnStart.Enabled = true;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnClearConsole.Enabled = true;
            btnPath.Enabled = true;
            btnStart.Enabled = true;

            txtLink.ReadOnly = false;
            txtName.ReadOnly = false;
            txtPath.ReadOnly = false;
        }
    }
}
