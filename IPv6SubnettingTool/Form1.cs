/*
 * Copyright (c) 2010-2016 Yucel Guven
 * All rights reserved.
 * 
 * This file is part of IPv6 Subnetting Tool.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted (subject to the limitations in the
 * disclaimer below) provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * NO EXPRESS OR IMPLIED LICENSES TO ANY PARTY'S PATENT RIGHTS ARE
 * GRANTED BY THIS LICENSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS 
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
 * BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Data.Odbc;

namespace IPv6SubnettingTool
{
    public partial class Form1 : Form
    {
        #region special initials/constants -yucel
        public const int ID = 0; // ID of this Form. Form1 is the main Form
        v6ST v6st = new v6ST();
        public SEaddress StartEnd = new SEaddress();
        SEaddress subnets = new SEaddress();
        SEaddress page = new SEaddress();
        public const int upto = 128;
        Graphics graph;
        BigInteger currentidx = BigInteger.Zero;
        BigInteger pix = BigInteger.Zero;
        public int delta = 0;
        public string stotaddr = "";
        public string findpfx = "";
        public string GotoForm_PrevValue = "";
        public int updatecount = 0;
        public BigInteger submax = BigInteger.Zero;
        public BigInteger totmaxval = BigInteger.Zero;
        public BigInteger submaxval = BigInteger.Zero;
        AutoCompleteStringCollection autocomp =
            new AutoCompleteStringCollection();
        int maxfontwidth = 0;
        CultureInfo culture;
        public delegate void ChangeWinFormStringsDelegate(CultureInfo culture);
        public event ChangeWinFormStringsDelegate ChangeUILanguage = delegate { };
        #endregion

        public Form1()
        {
            InitializeComponent();
            
            #region special initials -yucel
            label10.Text = label1.Text = trackBar1.Value.ToString();
            this.StartEnd.ID = ID; // ID of this Form. Form1 is the main Form.
            this.graph = this.CreateGraphics();
            this.graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            #endregion
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox2.AutoCompleteCustomSource = autocomp;
            this.EnglishToolStripMenuItem.Checked = true;
            this.TurkishToolStripMenuItem.Checked = false;
            this.culture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        }

        private void UpdateStatus()
        {
            delta = trackBar2.Value - trackBar1.Value;
            label12.Text = delta.ToString();
            submax = (BigInteger.One << delta);
            submaxval = submax - BigInteger.One;
            
            BigInteger nxt = BigInteger.One;

            if (trackBar1.Value == 128)
            {
                stotaddr = BigInteger.Pow(2, 128).ToString("R");
                totmaxval = BigInteger.Pow(2, 128) - BigInteger.One;  //MaxValue;
            }
            else
            {
                nxt = (nxt << trackBar1.Value);
                totmaxval = nxt - BigInteger.One;
                stotaddr = nxt.ToString();
            }

            toolStripStatusLabel1.Text = 
                StringsDictionary.KeyValue("Form1_UpdateStatus_delta", this.culture)
                + delta + StringsDictionary.KeyValue("Form1_UpdateStatus_subnets", this.culture)
                + submax.ToString() + StringsDictionary.KeyValue("Form1_UpdateStatus_addrs", this.culture)
                + stotaddr + "]";     
        }

        private void UpdateCount()
        {
            this.updatecount = this.listBox1.Items.Count;
            this.textBox7.Text = "[" + this.updatecount.ToString()
                + StringsDictionary.KeyValue("Form1_UpdateCount_entries", culture);

            /**/
            if (this.listBox1.Items.Count > 0)
            {
                this.currentidx =
                    BigInteger.Parse(this.listBox1.Items[0].ToString().Split('>')[0].TrimStart('p'));

                if (this.submax / 128 >= 1)
                {
                    this.pix = this.submax / 128;
                }
                else
                {
                    this.pix = 128;
                }
                this.Form1_Paint(null, null);
            }
        }

        private void UpdatePrintBin(SEaddress StartEnd, CheckState is128Checked)
        {
            richTextBox1.Text = "";
            richTextBox1.Text = this.v6st.PrintBin(StartEnd, this.trackBar1.Value, is128Checked);

            int count1 = trackBar1.Value + (trackBar1.Value / 4);
            int count2 = trackBar2.Value + (trackBar2.Value / 4) - count1;

            richTextBox1.Select(0, count1);
            richTextBox1.SelectionBackColor = Color.Red;
            richTextBox1.SelectionColor = Color.White;

            richTextBox1.Select(count1, count2);
            richTextBox1.SelectionBackColor = Color.Turquoise;
            richTextBox1.SelectionColor = Color.Black;
        }

        private void UpdateBackColor()
        {
            textBox1.BackColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
        }

        private void Calculate(string strinp)
        {
            if (v6st.IsAddressCorrect(strinp))
            {
                label5.Text = StringsDictionary.KeyValue("Form1_" + v6st.errmsg, this.culture);
                string Resv6 = v6st.FormalizeAddr(strinp);
                textBox1.Text = v6st.Kolonlar(Resv6, this.checkBox2.CheckState);

                if (this.checkBox2.CheckState == CheckState.Checked) /* 128 bits */
                {
                    if (Resv6.Length == 32)
                        Resv6 = "0" + Resv6;

                    StartEnd.Resultv6 = BigInteger.Parse(Resv6, NumberStyles.AllowHexSpecifier);
                    StartEnd.slash = this.trackBar1.Value;

                    StartEnd = v6st.StartEndAddresses(StartEnd, this.checkBox2.CheckState);
                    subnets.Start = StartEnd.Start;
                    subnets.End = BigInteger.Zero;
                    subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
                    subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;

                    textBox3.Text = textBox5.Text = "";
                    string s = String.Format("{0:x}", StartEnd.Start);
                    if (s.Length > 32)
                        s = s.Substring(1, 32);
                    s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                    textBox3.Text = s;

                    s = String.Format("{0:x}", StartEnd.End);
                    if (s.Length > 32)
                        s = s.Substring(1, 32);
                    s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                    textBox5.Text = s;
                }
                else if (this.checkBox2.CheckState == CheckState.Unchecked) /* 64 bits */
                {
                    Resv6 = Resv6.Substring(0, 16); /* From left First 64bits */
                    if (Resv6.Length == 16)
                        Resv6 = "0" + Resv6;

                    StartEnd.Resultv6 = BigInteger.Parse(Resv6, NumberStyles.AllowHexSpecifier);
                    StartEnd.slash = trackBar1.Value;

                    StartEnd = v6st.StartEndAddresses(StartEnd, this.checkBox2.CheckState);
                    subnets.Start = StartEnd.Start;
                    subnets.End = BigInteger.Zero;
                    subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
                    subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;

                    textBox3.Text = textBox5.Text = "";
                    string s = String.Format("{0:x}", StartEnd.Start);
                    if (s.Length > 16)
                        s = s.Substring(1, 16);
                    s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                    textBox3.Text = s;

                    s = String.Format("{0:x}", StartEnd.End);
                    if (s.Length > 16)
                        s = s.Substring(1, 16);
                    s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                    textBox5.Text = s;
                }

                StartEnd = v6st.NextSpace(StartEnd, this.checkBox2.CheckState);
                textBox4.Text = "#" + StartEnd.subnetidx.ToString();

                trackBar1.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                NextSpace.Enabled = true;
                PrevSpace.Enabled = true;
                ResetAll.Enabled = true;
                //
                Forwd.Enabled = false;
                Backwd.Enabled = false;
                Last.Enabled = false;
                //
                goToAddrSpaceNumberToolStripMenuItem.Enabled = true;

                UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
                autocomp.Add(strinp);
                this.Find.Focus();
            }
            else
            {
                ResetViewAll();
                ResetAll.Enabled = false;
                label5.Text = StringsDictionary.KeyValue("Form1_" + v6st.errmsg, this.culture);
            }

            UpdateStatus();
            UpdateBackColor();
        }

        private void Find_Click(object sender, EventArgs e)
        {
            textBox2.Text = textBox2.Text.Trim();
            textBox3.Text = textBox5.Text = this.textBox7.Text = "";
            this.Backwd.Enabled = this.Forwd.Enabled = this.Last.Enabled = false;

            listBox1.Items.Clear();

            Calculate(textBox2.Text);
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            label5.Text = StringsDictionary.KeyValue("Form1_textBox2_Enter", this.culture);
            this.textBox1.Text = "";
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            textBox2.Text = textBox2.Text.Trim();
            textBox2.Text = textBox2.Text.ToLower();
            textBox3.Text = textBox5.Text = richTextBox1.Text = this.textBox7.Text = "";
            this.Backwd.Enabled = this.Forwd.Enabled = this.Last.Enabled = false;

            listBox1.Items.Clear();

            Calculate(textBox2.Text);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();

            Backwd.Enabled = Forwd.Enabled = Last.Enabled = false;
            trackBar2.Value = trackBar1.Value;
            label10.Text = trackBar1.Value.ToString();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            UpdateStatus();

            listBox1.Items.Clear();
            textBox7.Text = "";

            StartEnd.slash = StartEnd.subnetslash = trackBar1.Value;

            StartEnd = v6st.StartEndAddresses(StartEnd, this.checkBox2.CheckState);
            subnets.Start = StartEnd.Start;
            subnets.End = BigInteger.Zero;
            subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
            subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;

            textBox3.Text = textBox5.Text = "";

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                string s = String.Format("{0:x}", StartEnd.Start);
                if (s.Length > 32)
                    s = s.Substring(1, 32);
                s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                textBox3.Text = s;

                s = String.Format("{0:x}", StartEnd.End);
                if (s.Length > 32)
                    s = s.Substring(1, 32);
                s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                textBox5.Text = s;
            }
            else if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                string s = String.Format("{0:x}", StartEnd.Start);
                if (s.Length > 16)
                    s = s.Substring(1, 16);
                s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                textBox3.Text = s;

                s = String.Format("{0:x}", StartEnd.End);
                if (s.Length > 16)
                    s = s.Substring(1, 16);
                s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
                textBox5.Text = s;
            }


            StartEnd = v6st.NextSpace(StartEnd, this.checkBox2.CheckState);
            textBox4.Text = "#" + StartEnd.subnetidx.ToString();

            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);

            delta = trackBar2.Value - trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            Backwd.Enabled = Forwd.Enabled = Last.Enabled = false;
            textBox7.Text = "";

            delta = trackBar2.Value - trackBar1.Value;

            if (delta < 0)
            {
                trackBar2.Value = trackBar1.Value;
                label12.Text = "0";
                delta = 0;
            }
            else
            {
                label10.Text = trackBar2.Value.ToString();
                label12.Text = delta.ToString();
            }

            StartEnd.subnetslash = trackBar2.Value;

            UpdateStatus();
            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
            graph.FillRectangle(new SolidBrush(Form1.DefaultBackColor), 250, 256, 0, 11);
            Form1_Paint(null, null);
        }

        private void Subnets_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            int delta = this.trackBar2.Value - this.trackBar1.Value;

            StartEnd.slash = this.trackBar1.Value;
            StartEnd.subnetslash = this.trackBar2.Value;
            StartEnd.upto = upto;

            subnets = v6st.ListFirstPage(StartEnd, this.checkBox2.CheckState);
            this.page.End = subnets.End;
            this.listBox1.Items.AddRange(subnets.liste.ToArray());

            BigInteger maxsub = (BigInteger.One << delta);

            if ( maxsub <= upto )
            {
                this.Backwd.Enabled = false;
                this.Forwd.Enabled = false;
                this.Last.Enabled = false;
            }
            else
            {
                this.Backwd.Enabled = false;
                this.Forwd.Enabled = true;
                this.Last.Enabled = true;
            }

            UpdateCount();
            /**/

        }

        private void Backwd_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            subnets.slash = this.trackBar1.Value;
            subnets.subnetslash = this.trackBar2.Value;
            subnets.upto = upto;
            subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
            subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;

            subnets.End = page.End = page.Start - BigInteger.One;
            subnets = v6st.ListPageBackward(subnets, this.checkBox2.CheckState);
            page.Start = subnets.Start;

            this.listBox1.Items.AddRange(subnets.liste.ToArray());

            if (subnets.Start.Equals(StartEnd.Start))
            {
                this.Backwd.Enabled = false;
                this.Forwd.Enabled = true;
                this.Last.Enabled = true;
                UpdateCount();
                return;
            }
            else
            {
                this.Forwd.Enabled = true;
                this.Last.Enabled = true;
            }

            UpdateCount();
        }

        private void Forwd_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            subnets.slash = this.trackBar1.Value;
            subnets.subnetslash = this.trackBar2.Value;
            subnets.upto = upto;
            subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
            subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;

            subnets.Start = page.Start = page.End + BigInteger.One;
            subnets = v6st.ListPageForward(subnets, this.checkBox2.CheckState);
            this.page.End = subnets.End;

            this.listBox1.Items.AddRange(subnets.liste.ToArray());

            if (subnets.End.Equals(StartEnd.End))
            {
                this.Forwd.Enabled = false;
                this.Last.Enabled = false;
                this.Backwd.Enabled = true;
                UpdateCount();
                return;
            }
            else
            {
                this.Backwd.Enabled = true;
                this.Last.Enabled = true;
            }

            UpdateCount();
        }

        private void Last_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            this.maxfontwidth = 0;
            this.listBox1.HorizontalExtent = 0;

            subnets.slash = this.trackBar1.Value;
            subnets.subnetslash = this.trackBar2.Value;
            subnets.upto = upto;
            subnets.LowerLimitAddress = StartEnd.LowerLimitAddress;
            subnets.UpperLimitAddress = StartEnd.UpperLimitAddress;


            subnets.End = page.End = StartEnd.UpperLimitAddress;
            subnets = v6st.ListLastPage(subnets, this.checkBox2.CheckState);
            page.Start = subnets.Start;

            this.listBox1.Items.AddRange(subnets.liste.ToArray());

            this.Backwd.Enabled = true;
            this.Forwd.Enabled = false;
            this.Last.Enabled = false;

            UpdateCount();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            if (checkBox1.Checked)
            {
                trackBar2.Enabled = true;
                trackBar2.Value = trackBar1.Value;
                label10.Enabled = label12.Enabled = true;
                label10.Text = trackBar1.Value.ToString();
                int delta = (trackBar2.Value - trackBar1.Value);
                label12.Text = delta.ToString();
                Subnets.Enabled = true;
                goToSubnetNumberToolStripMenuItem1.Enabled = true;
            }
            else
            {
                trackBar2.Enabled = false;
                trackBar2.Value = trackBar1.Value;
                label10.Enabled = label12.Enabled = false;
                label10.Text = label12.Text = "";
                Subnets.Enabled = Forwd.Enabled = Backwd.Enabled = Last.Enabled = false;
                goToSubnetNumberToolStripMenuItem1.Enabled = false;
            }

            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
        }

        private void ResetAll_Click(object sender, EventArgs e)
        {
            ResetViewAll();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(StringsDictionary.KeyValue("Form1_KeyDown_msg", this.culture),
                StringsDictionary.KeyValue("Form1_KeyDown_header", this.culture),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
                return;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void NextSpace_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            this.Backwd.Enabled = false;
            this.Forwd.Enabled = false;
            this.Last.Enabled = false;

            StartEnd.Start = StartEnd.End + BigInteger.One;

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                BigInteger big2 = BigInteger.One + BigInteger.One;
                if (StartEnd.End == (BigInteger.Pow(big2, 128) - BigInteger.One))
                {
                    StartEnd.Start = BigInteger.Zero;
                }
            }
            else if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                BigInteger big2 = BigInteger.One + BigInteger.One;
                if (StartEnd.End == (BigInteger.Pow(big2, 64) - BigInteger.One))
                {
                    StartEnd.Start = BigInteger.Zero;
                }
            }

            StartEnd = v6st.NextSpace(StartEnd, this.checkBox2.CheckState);

            textBox4.Text = "#" + StartEnd.subnetidx.ToString();

            subnets.Start = StartEnd.Start;
            subnets.End = BigInteger.Zero;
            StartEnd.Resultv6 = StartEnd.Start;

            textBox3.Text = textBox5.Text = textBox7.Text = "";
            string s = String.Format("{0:x}", StartEnd.Start);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            else if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            textBox1.Text = s = v6st.Kolonlar(s, this.checkBox2.CheckState);
            s += "/" + this.trackBar1.Value;
            textBox3.Text = s;
            textBox1.Text = v6st.FormalizeAddr(textBox1.Text);
            textBox1.Text = v6st.Kolonlar(textBox1.Text, this.checkBox2.CheckState);
            textBox1.BackColor = Color.FromKnownColor(KnownColor.Info);

            s = String.Format("{0:x}", StartEnd.End);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            else if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
            textBox5.Text = s;

            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
        }

        private void PrevSpace_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            this.Backwd.Enabled = false;
            this.Forwd.Enabled = false;
            this.Last.Enabled = false;

            StartEnd.End = StartEnd.Start - BigInteger.One;

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                BigInteger big2 = BigInteger.One + BigInteger.One;

                if (StartEnd.Start == BigInteger.Zero)
                {
                    StartEnd.End = BigInteger.Pow(big2, 128) - BigInteger.One;
                }
            }
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                BigInteger big2 = BigInteger.One + BigInteger.One;

                if (StartEnd.Start == BigInteger.Zero)
                {
                    StartEnd.End = BigInteger.Pow(big2, 64) - BigInteger.One;
                }
            }

            StartEnd = v6st.PrevSpace(StartEnd, this.checkBox2.CheckState);
            textBox4.Text = "#" + StartEnd.subnetidx.ToString();

            subnets.Start = StartEnd.Start;
            subnets.End = BigInteger.Zero;
            StartEnd.Resultv6 = StartEnd.Start;

            textBox3.Text = textBox5.Text = textBox7.Text = "";
            string s = String.Format("{0:x}", StartEnd.Start);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            textBox1.Text = s = v6st.Kolonlar(s, this.checkBox2.CheckState);
            s += "/" + this.trackBar1.Value;
            textBox3.Text = s;
            textBox1.Text = v6st.FormalizeAddr(textBox1.Text);
            textBox1.Text = v6st.Kolonlar(textBox1.Text, this.checkBox2.CheckState);
            textBox1.BackColor = Color.FromKnownColor(KnownColor.Info);

            s = String.Format("{0:x}", StartEnd.End);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
            textBox5.Text = s;

            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = "";
            foreach (object o in listBox1.SelectedItems)
            {
                s += o.ToString() + Environment.NewLine;
            }
            if (s != "")
                Clipboard.SetText(s);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                listBox1.SetSelected(i, true);
            }
            listBox1.Visible = true;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                listBox1.Visible = false;
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    listBox1.SetSelected(i, true);
                }
                listBox1.Visible = true;
                listBox1.Focus();
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                string s = "";
                foreach (object o in listBox1.SelectedItems)
                {
                    s += o.ToString() + Environment.NewLine;
                }
                Clipboard.SetText(s);
            }
            if (e.KeyCode == Keys.Enter)
            {
                if ((this.checkBox2.CheckState == CheckState.Unchecked && this.trackBar2.Value == 64)
                    ||
                    (this.checkBox2.CheckState == CheckState.Checked && this.trackBar2.Value == 128)
                    ||
                    listBox1.SelectedItem == null
                    )
                {
                    return;
                }
                else
                {
                    ListSubnetRange lh =
                        new ListSubnetRange(this.StartEnd, listBox1.SelectedItem.ToString(),
                            this.trackBar1.Value, this.trackBar2.Value, this.checkBox2.CheckState, this.culture);
                    lh.Show();
                    this.ChangeUILanguage += lh.SwitchLanguage;
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((this.checkBox2.CheckState == CheckState.Unchecked && this.trackBar2.Value == 64)
                ||
                (this.checkBox2.CheckState == CheckState.Checked && this.trackBar2.Value == 128)
                ||
                listBox1.SelectedItem == null
                )
            {
                return;
            }
            else
            {
                ListSubnetRange lh = new ListSubnetRange(this.StartEnd, listBox1.SelectedItem.ToString(),
                    this.trackBar1.Value, this.trackBar2.Value, this.checkBox2.CheckState, this.culture);
                lh.Show();
                this.ChangeUILanguage += lh.SwitchLanguage;
            }
        }

        private void listSubnetRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((this.checkBox2.CheckState == CheckState.Unchecked && this.trackBar2.Value == 64)
                ||
                (this.checkBox2.CheckState == CheckState.Checked && this.trackBar2.Value == 128)
                ||
                listBox1.SelectedItem == null
                )
            {
                return;
            }
            else
            {
                ListSubnetRange lh = new ListSubnetRange(this.StartEnd, listBox1.SelectedItem.ToString(),
                    this.trackBar1.Value, this.trackBar2.Value, this.checkBox2.CheckState, this.culture);
                lh.Show();
                this.ChangeUILanguage += lh.SwitchLanguage;
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            ListBox lb = (ListBox)sender;
            Graphics g = e.Graphics;
            SolidBrush sback = new SolidBrush(e.BackColor);
            SolidBrush sfore = new SolidBrush(e.ForeColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            if (e.Index % 2 == 0)
            {
                e.DrawBackground();
                DrawItemState st = DrawItemState.Selected;

                if ((e.State & st) != st)
                {
                    // Turkuaz= FF40E0D0(ARGB)
                    Color color = Color.FromArgb(30, 64, 224, 208);
                    g.FillRectangle(new SolidBrush(color), e.Bounds);
                    //g.FillRectangle(new SolidBrush(Color.WhiteSmoke), e.Bounds);
                    g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                        sfore, new PointF(e.Bounds.X, e.Bounds.Y));
                }
                else
                {
                    g.FillRectangle(sback, e.Bounds);
                    g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                        sfore, new PointF(e.Bounds.X, e.Bounds.Y));

                }
                e.DrawFocusRectangle();
            }
            else
            {
                e.DrawBackground();
                g.FillRectangle(sback, e.Bounds);
                g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                    sfore, new PointF(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();
            }


            // Last item is the widest one due to its index but may not be depending on the font-width
            // e.g., '0' and 'f' does not have he same width if the font is not fixed-width.
            // ps: Try to use Fixed-width Font if available.

            lb.HorizontalScrollbar = true;
            int horzSize = (int)g.MeasureString(lb.Items[e.Index].ToString(), lb.Font).Width;
            if (horzSize > maxfontwidth)
            {
                maxfontwidth = horzSize;
                lb.HorizontalExtent = maxfontwidth;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                this.contextMenuStrip1.Items[4].Enabled = true;
                this.contextMenuStrip1.Items[8].Enabled = true;
                this.contextMenuStrip1.Items[9].Enabled = true;
                if ( (this.trackBar2.Value - this.trackBar1.Value > 0))
                    this.contextMenuStrip1.Items[10].Enabled = true;
                else
                    this.contextMenuStrip1.Items[10].Enabled = false;
                this.contextMenuStrip1.Items[11].Enabled = true;

                if (this.listBox1.SelectedItem != null && this.listBox1.SelectedItem.ToString() != ""
                    && this.listBox1.SelectedIndex != -1)
                {
                    this.contextMenuStrip1.Items[5].Enabled = true;

                    if ((this.trackBar2.Value == 64 && this.checkBox2.CheckState == CheckState.Unchecked)
                        ||
                        (this.trackBar2.Value == 128 && this.checkBox2.CheckState == CheckState.Checked)
                        )
                    {
                        this.contextMenuStrip1.Items[2].Enabled = false;
                        this.contextMenuStrip1.Items[3].Enabled = false;      
                    }
                    else
                    {
                        if (this.checkBox2.CheckState == CheckState.Unchecked)
                            this.contextMenuStrip1.Items[2].Enabled = true;
                        else
                            this.contextMenuStrip1.Items[2].Enabled = false;

                        if (this.checkBox2.CheckState == CheckState.Checked)
                            this.contextMenuStrip1.Items[3].Enabled = true;
                        else
                            this.contextMenuStrip1.Items[3].Enabled = false;

                            this.contextMenuStrip1.Items[6].Enabled = false;
                            this.contextMenuStrip1.Items[7].Enabled = false;
                    }
                }
                else
                {
                    this.contextMenuStrip1.Items[2].Enabled = false;
                    this.contextMenuStrip1.Items[3].Enabled = false;
                    this.contextMenuStrip1.Items[5].Enabled = false;
                    this.contextMenuStrip1.Items[6].Enabled = false;
                    this.contextMenuStrip1.Items[7].Enabled = false;
                }
            }
            else
            {
                this.contextMenuStrip1.Items[2].Enabled = false;
                this.contextMenuStrip1.Items[3].Enabled = false;
                this.contextMenuStrip1.Items[4].Enabled = false;
                this.contextMenuStrip1.Items[5].Enabled = false;
                this.contextMenuStrip1.Items[6].Enabled = false;
                this.contextMenuStrip1.Items[7].Enabled = false;
                this.contextMenuStrip1.Items[8].Enabled = false;
                this.contextMenuStrip1.Items[9].Enabled = false;
                this.contextMenuStrip1.Items[10].Enabled = false;
                this.contextMenuStrip1.Items[11].Enabled = false;
                this.contextMenuStrip1.Items[13].Enabled = false;
            }
        }

        public BigInteger gotoaddrvalue
        {
            get { return BigInteger.Parse(textBox4.Text.TrimStart('#')); }
            set { textBox4.Text = "#" + value.ToString(); }
        }
        public BigInteger gotosubnetvalue
        {   /* to debug */
            //get { return Convert.ToUInt64(label8.Text); }
            get { return BigInteger.Parse(label8.Text); }
            set { label8.Text = value.ToString(); }
        }
        public string findprefix
        {
            get { return this.findpfx; }
            set { this.findpfx = value; }
        }

        private void goToAddrSpaceNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldidx = this.textBox4.Text.TrimStart('#');

            Goto gasn = new Goto(this,
                goToToolStripMenuItem.DropDownItems.IndexOf
                (goToAddrSpaceNumberToolStripMenuItem), this.totmaxval, ID, this.culture);
            
            gasn.ShowDialog();
            this.ChangeUILanguage += gasn.SwitchLanguage;

            string newidx = this.textBox4.Text.TrimStart('#');
            if (newidx == "" || newidx == oldidx)
                return;

            StartEnd.subnetidx = BigInteger.Parse(newidx);

            StartEnd = v6st.GoToAddrSpace(StartEnd, this.checkBox2.CheckState);
            StartEnd.Resultv6 = StartEnd.Start;

            listBox1.Items.Clear();

            textBox3.Text = textBox5.Text = textBox7.Text = "";

            string s = String.Format("{0:x}", StartEnd.Start);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            textBox1.Text = s = v6st.Kolonlar(s, this.checkBox2.CheckState);
            s += "/" + this.trackBar1.Value;
            textBox3.Text = s;
            textBox1.Text = v6st.FormalizeAddr(textBox1.Text);
            textBox1.Text = v6st.Kolonlar(textBox1.Text, this.checkBox2.CheckState);
            textBox1.BackColor = Color.FromKnownColor(KnownColor.Info);

            s = String.Format("{0:x}", StartEnd.End);

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                if (s.Length > 32)
                    s = s.Substring(1, 32);
            }
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                if (s.Length > 16)
                    s = s.Substring(1, 16);
            }

            s = v6st.Kolonlar(s, this.checkBox2.CheckState) + "/" + this.trackBar1.Value;
            textBox5.Text = s;

            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);

            this.Forwd.Enabled = false;
            this.Backwd.Enabled = false;
            this.Last.Enabled = false;
        }

        private void goToSubnetNumberToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Goto gsn = new Goto(this,
                goToToolStripMenuItem.DropDownItems.IndexOf
                (goToSubnetNumberToolStripMenuItem1), this.submaxval, ID, this.culture);

            this.label8.Text = "";
            gsn.ShowDialog();
            this.ChangeUILanguage += gsn.SwitchLanguage;

            String ss = "", se = "";
            int count = 0;

            string newidx = this.label8.Text;
            if (newidx == "")
                return;

            subnets.subnetidx = BigInteger.Parse(newidx, NumberStyles.Number);
            subnets.slash = this.trackBar1.Value;
            subnets.subnetslash = this.trackBar2.Value;
            subnets.Start = StartEnd.Start;
            subnets.Resultv6 = StartEnd.Resultv6;

            subnets = v6st.GoToSubnet(subnets, this.checkBox2.CheckState);
            
            // //

            page.Start = subnets.Start;
            page.End = BigInteger.Zero;

            if (subnets.End.Equals(StartEnd.End))
            {
                this.Forwd.Enabled = false;
            }

            this.listBox1.Items.Clear();

            for (count = 0; count < upto; count++)
            {
                subnets = v6st.Subnetting(subnets, this.checkBox2.CheckState);

                if (this.checkBox2.CheckState == CheckState.Checked)
                {
                    ss = String.Format("{0:x}", subnets.Start);
                    if (ss.Length > 32)
                        ss = ss.Substring(1, 32);
                    ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                    ss = v6st.CompressAddress(ss);

                    se = String.Format("{0:x}", subnets.End);
                    if (se.Length > 32)
                        se = se.Substring(1, 32);
                    se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                    //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                    ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                        this.trackBar2.Value;
                    this.listBox1.Items.Add(ss);
                    /* No need to print two times if slash=128 since start=end */
                    if (this.trackBar2.Value != 128)
                    {
                        se = "e" + subnets.subnetidx + "> " + se + "/"
                            + this.trackBar2.Value;
                        //this.listBox1.Items.Add(se);
                        //this.listBox1.Items.Add("");
                    }
                }
                else if (this.checkBox2.CheckState == CheckState.Unchecked)
                {
                    ss = String.Format("{0:x}", subnets.Start);
                    if (ss.Length > 16)
                        ss = ss.Substring(1, 16);
                    ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                    ss = v6st.CompressAddress(ss);

                    se = String.Format("{0:x}", subnets.End);
                    if (se.Length > 16)
                        se = se.Substring(1, 16);
                    se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                    //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                    ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                        this.trackBar2.Value;
                    this.listBox1.Items.Add(ss);
                    /* No need to print two times if slash=64 since start=end */
                    if (this.trackBar2.Value != 64)
                    {
                        se = "e" + subnets.subnetidx + "> " + se + "/"
                            + this.trackBar2.Value;
                        //this.listBox1.Items.Add(se);
                        //this.listBox1.Items.Add("");
                    }
                }

                if (subnets.End.Equals(StartEnd.End))
                {
                    this.Forwd.Enabled = false;
                    break;
                }
                else
                {
                    subnets.Start = subnets.End + BigInteger.One;
                }
            }
            page.End = subnets.End;

            if (BigInteger.Parse(newidx, NumberStyles.Number) == 0)
            {
                this.Backwd.Enabled = false;
            }
            else
                this.Backwd.Enabled = true;
            if (subnets.subnetidx == this.submaxval)
            {
                this.Forwd.Enabled = false;
                this.Last.Enabled = false;
            }
            else
            {
                this.Forwd.Enabled = true;
                this.Last.Enabled = true;
            }
            UpdateCount();
            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
        }

        private void listDNSReverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                StartEnd.slash = this.trackBar1.Value;
                StartEnd.subnetslash = this.trackBar2.Value;
                ListDnsReverses dnsr = new ListDnsReverses(StartEnd, this.checkBox2.CheckState, this.culture);
                dnsr.Show();
                this.ChangeUILanguage += dnsr.SwitchLanguage;
            }
        }

        private void list64SubnetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((this.checkBox2.CheckState == CheckState.Unchecked && this.trackBar2.Value == 64)
                ||
                (this.checkBox2.CheckState == CheckState.Checked && this.trackBar2.Value == 128)
                ||
                listBox1.SelectedItem == null
                )
            {
                return;
            }
            else
            {
                ListSubnetRange lh = new ListSubnetRange(this.StartEnd, listBox1.SelectedItem.ToString(),
                    this.trackBar1.Value, this.trackBar2.Value, this.checkBox2.CheckState, this.culture);
                lh.Show();
                this.ChangeUILanguage += lh.SwitchLanguage;
            }
        }

        private void listAllDNSReverseZonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                StartEnd.slash = this.trackBar1.Value;
                StartEnd.subnetslash = this.trackBar2.Value;
                ListDnsReverses dnsr = new ListDnsReverses(StartEnd, this.checkBox2.CheckState, this.culture);
                dnsr.Show();
                this.ChangeUILanguage += dnsr.SwitchLanguage;
            }
        }

        private void goToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                this.goToSubnetNumberToolStripMenuItem1.Enabled = true;
                this.findprefixtoolStripMenuItem1.Enabled = true;
            }
            else
            {
                this.goToSubnetNumberToolStripMenuItem1.Enabled = false;
                this.findprefixtoolStripMenuItem1.Enabled = false;
            }
        }

        private void toolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                this.listAllDNSReverseZonesToolStripMenuItem.Enabled = true;
            
                if (this.listBox1.SelectedItem != null && this.listBox1.SelectedItem.ToString() != ""
                    && this.listBox1.SelectedIndex != -1)
                {
                    this.workwithtoolStripMenuItem1.Enabled = true;

                    if ((this.trackBar2.Value == 64 && this.checkBox2.CheckState == CheckState.Unchecked)
                        ||
                        (this.trackBar2.Value == 128 && this.checkBox2.CheckState == CheckState.Checked)
                        )
                    {
                        this.list64SubnetsToolStripMenuItem.Enabled = false;
                        this.list128SubnetsToolStripMenuItem1.Enabled = false;
                    }
                    else
                    {
                        if (this.checkBox2.CheckState == CheckState.Unchecked)
                            this.list64SubnetsToolStripMenuItem.Enabled = true;
                        else
                            this.list64SubnetsToolStripMenuItem.Enabled = false;

                        if (this.checkBox2.CheckState == CheckState.Checked)
                            this.list128SubnetsToolStripMenuItem1.Enabled = true;
                        else
                            this.list128SubnetsToolStripMenuItem1.Enabled = false;
                    }
                }
                else
                {
                    this.list64SubnetsToolStripMenuItem.Enabled = false;
                    this.list128SubnetsToolStripMenuItem1.Enabled = false;
                    this.workwithtoolStripMenuItem1.Enabled = false;
                }
            }
            else
            {
                this.list64SubnetsToolStripMenuItem.Enabled = false;
                this.list128SubnetsToolStripMenuItem1.Enabled = false;
                this.listAllDNSReverseZonesToolStripMenuItem.Enabled = false;
                this.workwithtoolStripMenuItem1.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ResizeView();

            textBox2.Text = textBox2.Text.Trim();
            textBox2.Text = textBox2.Text.ToLower();
            textBox3.Text = textBox5.Text = richTextBox1.Text = textBox7.Text = "";
            
            listBox1.Items.Clear();
            this.Form1_Paint(null, null);

            Calculate(textBox2.Text);
        }

        private void ResizeView()
        {
            if (this.checkBox2.CheckState == CheckState.Unchecked)
            {
                this.DefaultView();
            }
            else if (this.checkBox2.CheckState == CheckState.Checked)
            {
                this.ExpandView();
            }
        }

        private void ExpandView()
        {
            this.Size = new Size(847, 537);
            this.textBox3.Size = new Size(365, 20);

            this.PrevSpace.Location = new Point(437, 92);
            this.NextSpace.Location = new Point(473, 92);

            this.PrevSpace.Size = new Size(30, 23);
            this.NextSpace.Size = new Size(30, 23);

            this.label14.Location = new Point(508, 101);

            this.textBox5.Size = new Size(365, 20);
            this.textBox4.Size = new Size(347, 20);
            this.textBox4.Location = new Point(437, 117);

            this.textBox6.Size = new Size(719, 20);
            this.richTextBox1.Size = new Size(710, 13);

            this.trackBar1.Size = new Size(730, 30);
            this.trackBar2.Size = new Size(730, 30);
            this.trackBar1.Maximum = 128;
            this.trackBar2.Maximum = 128;

            this.label2.Location = new Point(790, 176);
            this.label1.Location = new Point(800, 176);
            this.label9.Location = new Point(790, 210);
            this.label10.Location = new Point(800, 210);
            this.label17.Location = new Point(797, 215);
            this.label11.Location = new Point(780, 230);
            this.label12.Location = new Point(800, 234);
            this.textBox7.Location = new Point(734, 256);
            this.textBox7.Size = new Size(50, 13);

            this.listBox1.Size = new Size(719, 184);
            this.label20.Location = new Point(500, 465);
            this.toolStripStatusLabel1.Size = new Size(687, 19);
            //
            graph.Clear(Form1.DefaultBackColor);
        }

        private void DefaultView()
        {
            this.Size = new Size(487, 537);
            this.textBox3.Size = new Size(197, 20);

            this.PrevSpace.Location = new Point(279, 92);
            this.PrevSpace.Size = new Size(30, 23);
            this.NextSpace.Location = new Point(315, 92);
            this.NextSpace.Size = new Size(30, 23);

            this.label14.Location = new Point(349, 101);

            this.textBox5.Size = new Size(197, 20);
            this.textBox4.Size = new Size(152, 20);
            this.textBox4.Location = new Point(279, 117);

            this.textBox6.Size = new Size(366, 20);
            this.richTextBox1.Size = new Size(357, 13);

            this.trackBar1.Size = new Size(376, 30);
            this.trackBar2.Size = new Size(376, 30);
            this.trackBar1.Maximum = 64;
            this.trackBar2.Maximum = 64;

            this.label2.Location = new Point(435, 176);
            this.label1.Location = new Point(445, 176);
            this.label1.Text = this.trackBar1.Value.ToString();
            this.label9.Location = new Point(435, 210);
            this.label10.Location = new Point(445, 210);
            this.label10.Text = this.trackBar2.Value.ToString();
            this.label17.Location = new Point(436, 215);
            this.label11.Location = new Point(425, 230);
            this.label12.Location = new Point(445, 234);
            this.textBox7.Location = new Point(382, 256);
            this.textBox7.Size = new Size(50, 13);
            this.listBox1.Size = new Size(367, 184);
            this.label20.Location = new Point(145, 465);
            this.toolStripStatusLabel1.Size = new Size(370, 19);
            //
            graph.Clear(Form1.DefaultBackColor);
        }

        private void ResetViewAll()
        {
            textBox1.Text = "";
            UpdateBackColor();
            trackBar1.Value = trackBar1.Minimum;
            trackBar2.Value = trackBar2.Minimum;
            label1.Text = trackBar1.Minimum.ToString();
            label10.Text = trackBar2.Minimum.ToString();
            label12.Text = "0";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox7.Text = "";
            listBox1.Items.Clear();
            PrevSpace.Enabled = false;
            NextSpace.Enabled = false;
            trackBar1.Enabled = false;
            this.goToAddrSpaceNumberToolStripMenuItem.Enabled = false;

            if (checkBox1.Checked)
                checkBox1.Checked = false;
            checkBox1.Enabled = false;

            richTextBox1.Text = " ";
            UpdateStatus();
        }

        private void list128SubnetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((this.checkBox2.CheckState == CheckState.Unchecked && this.trackBar2.Value == 64)
                ||
                (this.checkBox2.CheckState == CheckState.Checked && this.trackBar2.Value == 128)
                ||
                listBox1.SelectedItem == null
                )
            {
                return;
            }
            else
            {
                ListSubnetRange lh = new ListSubnetRange(this.StartEnd, listBox1.SelectedItem.ToString(),
                    this.trackBar1.Value, this.trackBar2.Value, this.checkBox2.CheckState, this.culture);
                lh.Show();
                this.ChangeUILanguage += lh.SwitchLanguage;
            }
        }

        private void whoisQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count > 0)
            {
                if (this.listBox1.SelectedItem != null && this.listBox1.SelectedItem.ToString() != ""
                    && this.listBox1.SelectedIndex != -1)
                {
                    try
                    {
                        string s = (listBox1.SelectedItem.ToString().Split(' ')[1]).Split('/')[0];

                        whoisQuery whoisquery = new whoisQuery(s, this.culture);
                        whoisquery.Show();
                        this.ChangeUILanguage += whoisquery.SwitchLanguage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    whoisQuery whoisquery = new whoisQuery(this.textBox1.Text, this.culture);
                    whoisquery.Show();
                    this.ChangeUILanguage += whoisquery.SwitchLanguage;
                }
            }
            else
            {
                whoisQuery whoisquery = new whoisQuery(this.textBox1.Text, this.culture);
                whoisquery.Show();
                this.ChangeUILanguage += whoisquery.SwitchLanguage;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (MessageBox.Show(StringsDictionary.KeyValue("Form1_KeyDown_msg", this.culture),
                    StringsDictionary.KeyValue("Form1_KeyDown_header", this.culture),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    Application.Exit();
                }
                else
                    return;
            }

            if (e.Control && e.KeyCode == Keys.F)
            {
                if (this.listBox1.Items.Count > 0)
                {
                    this.findprefixtoolStripMenuItem1_Click(null, null);
                }
                else
                    return;
            }
        }

        private void whoisQueryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.whoisQueryToolStripMenuItem_Click(null, null);
        }

        private void fontsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (this.fontDialog1)
            {
                DialogResult resfont = this.fontDialog1.ShowDialog();

                if (resfont == System.Windows.Forms.DialogResult.OK)
                {
                    this.listBox1.Font = fontDialog1.Font;
                    this.listBox1.ItemHeight = this.listBox1.Font.Height;
                    this.maxfontwidth = 0;
                    this.listBox1.HorizontalExtent = 0;
                }
            }
        }

        private void goToToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Goto gsn = new Goto(this,
                goToToolStripMenuItem.DropDownItems.IndexOf(goToSubnetNumberToolStripMenuItem1),
                this.submaxval, ID, this.culture);

            this.label8.Text = "";
            gsn.ShowDialog();
            this.ChangeUILanguage += gsn.SwitchLanguage;

            String ss = "", se = "";
            int count = 0;

            string newidx = this.label8.Text;
            if (newidx == "")
                return;

            subnets.subnetidx = BigInteger.Parse(newidx, NumberStyles.Number);
            subnets.slash = this.trackBar1.Value;
            subnets.subnetslash = this.trackBar2.Value;
            subnets.Start = StartEnd.Start;
            subnets.Resultv6 = StartEnd.Resultv6;

            subnets = v6st.GoToSubnet(subnets, this.checkBox2.CheckState);

            page.Start = subnets.Start;
            page.End = BigInteger.Zero;

            if (subnets.End.Equals(StartEnd.End))
            {
                this.Forwd.Enabled = false;
            }

            this.listBox1.Items.Clear();

            for (count = 0; count < upto; count++)
            {
                subnets = v6st.Subnetting(subnets, this.checkBox2.CheckState);

                if (this.checkBox2.CheckState == CheckState.Checked)
                {
                    ss = String.Format("{0:x}", subnets.Start);
                    if (ss.Length > 32)
                        ss = ss.Substring(1, 32);
                    ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                    ss = v6st.CompressAddress(ss);

                    se = String.Format("{0:x}", subnets.End);
                    if (se.Length > 32)
                        se = se.Substring(1, 32);
                    se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                    //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                    ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                        this.trackBar2.Value;
                    this.listBox1.Items.Add(ss);
                    /* No need to print two times if slash=128 since start=end */
                    if (this.trackBar2.Value != 128)
                    {
                        se = "e" + subnets.subnetidx + "> " + se + "/"
                            + this.trackBar2.Value;
                        //this.listBox1.Items.Add(se);
                        //this.listBox1.Items.Add("");
                    }
                }
                else if (this.checkBox2.CheckState == CheckState.Unchecked)
                {
                    ss = String.Format("{0:x}", subnets.Start);
                    if (ss.Length > 16)
                        ss = ss.Substring(1, 16);
                    ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                    ss = v6st.CompressAddress(ss);

                    se = String.Format("{0:x}", subnets.End);
                    if (se.Length > 16)
                        se = se.Substring(1, 16);
                    se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                    //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                    ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                        this.trackBar2.Value;
                    this.listBox1.Items.Add(ss);
                    /* No need to print two times if slash=64 since start=end */
                    if (this.trackBar2.Value != 64)
                    {
                        se = "e" + subnets.subnetidx + "> " + se + "/"
                            + this.trackBar2.Value;
                        //this.listBox1.Items.Add(se);
                        //this.listBox1.Items.Add("");
                    }
                }

                if (subnets.End.Equals(StartEnd.End))
                {
                    this.Forwd.Enabled = false;
                    break;
                }
                else
                {
                    subnets.Start = subnets.End + BigInteger.One;
                }
            }
            page.End = subnets.End;

            if (BigInteger.Parse(newidx, NumberStyles.Number) == 0)
            {
                this.Backwd.Enabled = false;
            }
            else
                this.Backwd.Enabled = true;
            if (subnets.subnetidx == this.submaxval)
            {
                this.Forwd.Enabled = false;
                this.Last.Enabled = false;
            }
            else
            {
                this.Forwd.Enabled = true;
                this.Last.Enabled = true;
            }
            UpdateCount();
            UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
        }

        private void exportToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsText exptofile = new SaveAsText(this.StartEnd, this.checkBox2.CheckState, this.culture);
            exptofile.Show();
            this.ChangeUILanguage += exptofile.SwitchLanguage;
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (this.listBox1.Items.Count < 0 || this.listBox1.Items.Count == 0)
            {
                this.exportToFileToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.exportToFileToolStripMenuItem.Enabled = true;
            }
        }

        private void savetoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveAsText saveas = new SaveAsText(this.StartEnd, this.checkBox2.CheckState, this.culture);
            saveas.Show();
            this.ChangeUILanguage += saveas.SwitchLanguage;
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            string s = this.textBox1.Text;
            s = s.Replace(":", "");
            string addrtype = StringsDictionary.KeyValue("Form1_Address_Type", this.culture)
                + v6st.AddressType(BigInteger.Parse("0" + s, NumberStyles.AllowHexSpecifier));
            this.toolTip1.SetToolTip(this.textBox1, addrtype);
        }

        private void EnglishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UncheckAll((ToolStripMenuItem)sender);
        }

        private void SpanishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UncheckAll((ToolStripMenuItem)sender);
        }

        private void TurkishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UncheckAll((ToolStripMenuItem)sender);
        }

        private void GermanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UncheckAll((ToolStripMenuItem)sender);
        }

        private void UncheckAll(ToolStripMenuItem selected)
        {
            selected.Checked = true;

            foreach (var MenuItem in
                (from object item in selected.Owner.Items
                 let MenuItem = item as ToolStripMenuItem
                 where !item.Equals(selected) where MenuItem != null select MenuItem))
                 MenuItem.Checked = false;

            this.SwitchLanguage();
        }

        private void SwitchLanguage()
        {
            if (this.EnglishToolStripMenuItem.Checked == true)
            {
                this.culture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            }
            if (this.SpanishToolStripMenuItem.Checked == true)
            {
                this.culture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
            }
            else if (this.TurkishToolStripMenuItem.Checked == true)
            {
                this.culture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("tr-TR");
            }
            else if (this.GermanToolStripMenuItem.Checked == true)
            {
                this.culture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");
            }
            this.v6st.IsAddressCorrect(this.textBox2.Text);
            //
            this.languageToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_languageToolStripMenuItem", this.culture);
            this.TurkishToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_TurkishToolStripMenuItem", this.culture);
            this.EnglishToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_EnglishToolStripMenuItem", this.culture);
            this.SpanishToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_SpanishToolStripMenuItem", this.culture);
            this.GermanToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_GermanToolStripMenuItem", this.culture);
            //
            this.Text = StringsDictionary.KeyValue("Form1_Text", this.culture);
            this.aboutToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_aboutToolStripMenuItem.Text", this.culture);
            this.Backwd.Text = StringsDictionary.KeyValue("Form1_Backwd.Text", this.culture);
            this.checkBox1.Text = StringsDictionary.KeyValue("Form1_checkBox1.Text", this.culture);
            this.checkBox2.Text = StringsDictionary.KeyValue("Form1_checkBox2.Text", this.culture);
            this.copyToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_copyToolStripMenuItem.Text", this.culture);
            this.exitToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_exitToolStripMenuItem.Text", this.culture);
            this.exportToFileToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_exportToFileToolStripMenuItem.Text", this.culture);
            this.fileToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_fileToolStripMenuItem.Text", this.culture);
            this.Find.Text = StringsDictionary.KeyValue("Form1_Find.Text", this.culture);
            this.fontsToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_fontsToolStripMenuItem.Text", this.culture);        
            this.Forwd.Text = StringsDictionary.KeyValue("Form1_Forwd.Text", this.culture);
            this.goToAddrSpaceNumberToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_goToAddrSpaceNumberToolStripMenuItem.Text", this.culture);
            this.goToSubnetNumberToolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_goToSubnetNumberToolStripMenuItem1.Text", this.culture);
            this.goToToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_goToToolStripMenuItem.Text", this.culture);
            this.goToToolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_goToToolStripMenuItem1.Text", this.culture);
            this.helpToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_helpToolStripMenuItem.Text", this.culture);
            this.label13.Text = StringsDictionary.KeyValue("Form1_label13.Text", this.culture);
            this.label14.Text = StringsDictionary.KeyValue("Form1_label14.Text", this.culture);
            this.label20.Text = StringsDictionary.KeyValue("Form1_label20.Text", this.culture);
            this.label3.Text = StringsDictionary.KeyValue("Form1_label3.Text", this.culture);
            this.label4.Text = StringsDictionary.KeyValue("Form1_label4.Text", this.culture);
            this.label5.Text = StringsDictionary.KeyValue("Form1_label5.Text", this.culture);
            this.label6.Text = StringsDictionary.KeyValue("Form1_label6.Text", this.culture);
            this.label7.Text = StringsDictionary.KeyValue("Form1_label7.Text", this.culture);
            this.label9.Text = StringsDictionary.KeyValue("Form1_label9.Text", this.culture);
            this.Last.Text = StringsDictionary.KeyValue("Form1_Last.Text", this.culture);
            this.list128SubnetsToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_list128SubnetsToolStripMenuItem.Text", this.culture);
            this.list128SubnetsToolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_list128SubnetsToolStripMenuItem1.Text", this.culture);
            this.list64SubnetsToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_list64SubnetsToolStripMenuItem.Text", this.culture);
            this.listAllDNSReverseZonesToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_listAllDNSReverseZonesToolStripMenuItem.Text", this.culture);
            this.listDNSReverseToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_listDNSReverseToolStripMenuItem.Text", this.culture);
            this.listSubnetRangeToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_listSubnetRangeToolStripMenuItem.Text", this.culture);
            this.menuStrip1.Text = StringsDictionary.KeyValue("Form1_menuStrip1.Text", this.culture);
            this.NextSpace.Text = StringsDictionary.KeyValue("Form1_NextSpace.Text", this.culture);
            this.PrevSpace.Text = StringsDictionary.KeyValue("Form1_PrevSpace.Text", this.culture);
            this.ResetAll.Text = StringsDictionary.KeyValue("Form1_ResetAll.Text", this.culture);
            this.savetoolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_savetoolStripMenuItem1.Text", this.culture);
            this.selectAllToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_selectAllToolStripMenuItem.Text", this.culture);
            this.statusStrip1.Text = StringsDictionary.KeyValue("Form1_statusStrip1.Text", this.culture);
            this.Subnets.Text = StringsDictionary.KeyValue("Form1_Subnets.Text", this.culture);
            this.toolsToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_toolsToolStripMenuItem.Text", this.culture);
            this.whoisQueryToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_whoisQueryToolStripMenuItem.Text", this.culture);
            this.whoisQueryToolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_whoisQueryToolStripMenuItem1.Text", this.culture);
            this.workwithToolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_workwithToolStripMenuItem.Text", this.culture);
            this.workwithtoolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_workwithToolStripMenuItem.Text", this.culture);
            this.prefixsublevelstoolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_prefixsublevelstoolStripMenuItem1.Text", this.culture);
            this.compressaddrtoolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_compressaddrtoolStripMenuItem1.Text", this.culture);
            this.findprefixtoolStripMenuItem1.Text = StringsDictionary.KeyValue("Form1_findprefixtoolStripMenuItem1.Text", this.culture);
            this.findprefixtoolStripMenuItem.Text = StringsDictionary.KeyValue("Form1_findprefixtoolStripMenuItem1.Text", this.culture);

            // ToolTips
            this.toolTip1.SetToolTip(this.Backwd, StringsDictionary.KeyValue("Form1_Backwd.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.checkBox1, StringsDictionary.KeyValue("Form1_checkBox1.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.checkBox2, StringsDictionary.KeyValue("Form1_checkBox2.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.Forwd, StringsDictionary.KeyValue("Form1_Forwd.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.Last, StringsDictionary.KeyValue("Form1_Last.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.Subnets, StringsDictionary.KeyValue("Form1_Subnets.ToolTip", this.culture));

            // errmsg from checks. 
            if (v6st.errmsg != "")
                label5.Text = StringsDictionary.KeyValue("Form1_" + v6st.errmsg, this.culture);
            else
                label5.Text = StringsDictionary.KeyValue("Form1_textBox2_Enter", this.culture);

            //status bar
            toolStripStatusLabel1.Text = StringsDictionary.KeyValue("Form1_UpdateStatus_delta", this.culture)
                + delta + StringsDictionary.KeyValue("Form1_UpdateStatus_subnets", this.culture)
                + submax.ToString() + StringsDictionary.KeyValue("Form1_UpdateStatus_addrs", this.culture)
                + stotaddr + "]";

            //# of entries
            if (this.textBox7.Text != "")
                this.textBox7.Text = "[" + this.updatecount.ToString()
                    + StringsDictionary.KeyValue("Form1_UpdateCount_entries", culture);
            //
            this.ChangeUILanguage.Invoke(this.culture);
        }

        private void workwithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WorkWithPrefix();
        }

        private void workwithtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WorkWithPrefix();
        }

        private void WorkWithPrefix()
        {
            string selected = this.listBox1.SelectedItem.ToString().Split(' ')[1];
            string snet = selected.Split('/')[0];
            int plen = Convert.ToInt16(selected.Split('/')[1]);
            this.textBox2.Text = snet;

            this.trackBar1.Value = this.trackBar2.Value;
            this.label1.Text = this.trackBar1.Value.ToString();
            
            Backwd.Enabled = Forwd.Enabled = Last.Enabled = false;
            listBox1.Items.Clear();
            textBox7.Text = "";
            
            Calculate(snet);
            
            StartEnd.slash = StartEnd.subnetslash = trackBar1.Value;
        }

        private void compressaddrtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CompressAddressForm compress = new CompressAddressForm(this.culture);
            compress.Show();
            this.ChangeUILanguage += compress.SwitchLanguage;
        }

        private void findprefixtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Goto findpfx = new Goto(this,
                goToToolStripMenuItem.DropDownItems.IndexOf
                (findprefixtoolStripMenuItem1), this.totmaxval, ID, this.culture);

            findpfx.ShowDialog();
            this.ChangeUILanguage += findpfx.SwitchLanguage;

            if (this.findpfx == "")
            {
                if (findpfx is IDisposable)
                    findpfx.Dispose();
                return;
            }
            SEaddress seaddr = new SEaddress();
            seaddr.slash = this.trackBar1.Value;
            seaddr.subnetslash = this.trackBar2.Value;
            String ss = "", se = "";
            int count = 0;

            string Resv6 = v6st.FormalizeAddr(this.findpfx);

            if (this.checkBox2.CheckState == CheckState.Checked) /* 128 bits */
            {
                if (Resv6.Length == 32)
                    Resv6 = "0" + Resv6;
            }
            else if (this.checkBox2.CheckState == CheckState.Unchecked) /* 64 bits */
            {
                Resv6 = Resv6.Substring(0, 16); /* From left First 64bits */
                if (Resv6.Length == 16)
                    Resv6 = "0" + Resv6;
            }

            seaddr.Resultv6 = seaddr.Start = BigInteger.Parse(Resv6, NumberStyles.AllowHexSpecifier);

            if (seaddr.Resultv6 >= StartEnd.Start && seaddr.Resultv6 <= StartEnd.End)
            {
                // inside
                BigInteger before = seaddr.Resultv6;

                seaddr = v6st.FindPrefixIndex(seaddr, this.checkBox2.CheckState);

                subnets.subnetidx = seaddr.subnetidx;
                subnets.slash = this.trackBar1.Value;
                subnets.subnetslash = this.trackBar2.Value;
                subnets.Start = StartEnd.Start;
                subnets.Resultv6 = StartEnd.Resultv6;

                subnets = v6st.GoToSubnet(subnets, this.checkBox2.CheckState);

                if (before == subnets.Start)
                {
                    page.Start = subnets.Start;
                    page.End = BigInteger.Zero;

                    if (subnets.End.Equals(StartEnd.End))
                    {
                        this.Forwd.Enabled = false;
                    }

                    this.listBox1.Items.Clear();

                    for (count = 0; count < upto; count++)
                    {
                        subnets = v6st.Subnetting(subnets, this.checkBox2.CheckState);

                        if (this.checkBox2.CheckState == CheckState.Checked)
                        {
                            ss = String.Format("{0:x}", subnets.Start);
                            if (ss.Length > 32)
                                ss = ss.Substring(1, 32);
                            ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                            ss = v6st.CompressAddress(ss);

                            se = String.Format("{0:x}", subnets.End);
                            if (se.Length > 32)
                                se = se.Substring(1, 32);
                            se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                            //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                            ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                                this.trackBar2.Value;
                            this.listBox1.Items.Add(ss);
                            /* No need to print two times if slash=128 since start=end */
                            if (this.trackBar2.Value != 128)
                            {
                                se = "e" + subnets.subnetidx + "> " + se + "/"
                                    + this.trackBar2.Value;
                                //this.listBox1.Items.Add(se);
                                //this.listBox1.Items.Add("");
                            }
                        }
                        else if (this.checkBox2.CheckState == CheckState.Unchecked)
                        {
                            ss = String.Format("{0:x}", subnets.Start);
                            if (ss.Length > 16)
                                ss = ss.Substring(1, 16);
                            ss = v6st.Kolonlar(ss, this.checkBox2.CheckState);
                            ss = v6st.CompressAddress(ss);

                            se = String.Format("{0:x}", subnets.End);
                            if (se.Length > 16)
                                se = se.Substring(1, 16);
                            se = v6st.Kolonlar(se, this.checkBox2.CheckState);

                            //ss = "s" + subnets.subnetidx + "> " + ss + "/" +
                            ss = "p" + subnets.subnetidx + "> " + ss + "/" +
                                this.trackBar2.Value;
                            this.listBox1.Items.Add(ss);
                            /* No need to print two times if slash=64 since start=end */
                            if (this.trackBar2.Value != 64)
                            {
                                se = "e" + subnets.subnetidx + "> " + se + "/"
                                    + this.trackBar2.Value;
                                //this.listBox1.Items.Add(se);
                                //this.listBox1.Items.Add("");
                            }
                        }

                        if (subnets.End.Equals(StartEnd.End))
                        {
                            this.Forwd.Enabled = false;
                            break;
                        }
                        else
                        {
                            subnets.Start = subnets.End + BigInteger.One;
                        }
                    }
                    page.End = subnets.End;

                    if (seaddr.subnetidx == 0)
                    {
                        this.Backwd.Enabled = false;
                    }
                    else
                        this.Backwd.Enabled = true;
                    if (subnets.subnetidx == this.submaxval)
                    {
                        this.Forwd.Enabled = false;
                        this.Last.Enabled = false;
                    }
                    else
                    {
                        this.Forwd.Enabled = true;
                        this.Last.Enabled = true;
                    }
                    UpdateCount();
                    UpdatePrintBin(StartEnd, this.checkBox2.CheckState);
                }
                else
                {
                    MessageBox.Show(StringsDictionary.KeyValue("Form1_MsgBoxprefixnotfound", this.culture),
                        "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // outside
                MessageBox.Show("Out of [Start-End] interval", "Out of range",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void findprefixtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.findprefixtoolStripMenuItem1_Click(null, null);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                graph.Clear(Form1.DefaultBackColor);
                graph.DrawRectangle(new Pen(Color.Red), 250, 256, 128, 11);

                if (this.listBox1.Items.Count > 0)
                {
                    int count = 128;

                    if (this.pix > 0)
                    {
                        if (this.submax - this.currentidx <= 128)
                            graph.FillRectangle(new SolidBrush(Color.Red), 250, 256, count, 11);
                        else
                        {
                            graph.FillRectangle(new SolidBrush(Color.Red), 250, 256,
                                (float)((this.currentidx + 128) / this.pix), 11);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
