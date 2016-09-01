﻿/*
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
using System.Windows.Forms;
using System.Globalization;
using System.Resources;
using System.Numerics;
using System.IO;
using System.Threading;

namespace IPv6SubnettingTool
{
    public partial class SaveAsText : Form
    {
        #region special initials/constants -yucel
        public const int ID = 3; // ID of this Form.
        public int incomingID;
        public BigInteger maxsubnet = BigInteger.Zero;
        SEaddress StartEnd = new SEaddress();
        v6ST v6st = new v6ST();
        BigInteger FromIndex = BigInteger.Zero;
        BigInteger ToIndex = BigInteger.Zero;
        BigInteger TotalBytes = BigInteger.Zero;

        CheckState is128Checked;
        FileDialog saveDialog = new SaveFileDialog();
        DiskSpace diskspace = new DiskSpace();
        BigInteger count = BigInteger.Zero;
        BigInteger howmany = BigInteger.Zero;
        CultureInfo culture;

        public class CurrentState
        {
            public BigInteger SavedLines = BigInteger.Zero;
            public int percentage = 0;
        }
        CurrentState saveState = new CurrentState();
        #endregion

        public SaveAsText(SEaddress input, CheckState is128Checked, CultureInfo culture)
        {
            InitializeComponent();

            #region special initials -yucel
            this.StartEnd.ID = ID;
            this.incomingID = input.ID;
            this.culture = culture;
            #endregion

            this.SwitchLanguage(this.culture);

            this.is128Checked = is128Checked;
            this.StartEnd.LowerLimitAddress = input.LowerLimitAddress;
            this.StartEnd.Resultv6 = input.Resultv6;
            this.StartEnd.slash = input.slash;
            this.StartEnd.Start = input.Start;
            this.StartEnd.End = input.End;
            this.StartEnd.subnetidx = input.subnetidx;
            this.StartEnd.subnetslash = input.subnetslash;
            this.StartEnd.UpperLimitAddress = input.UpperLimitAddress;
            this.StartEnd.upto = input.upto;

            string ss = String.Format("{0:x}", input.Start);
            string se = String.Format("{0:x}", input.End);
            ss = ss.TrimStart('0'); se = se.TrimStart('0');

            if (this.is128Checked == CheckState.Unchecked)
            {
                ss = ss.PadLeft(16, '0');
                ss = v6st.Kolonlar(ss, this.is128Checked) + "/" + input.subnetslash.ToString();
                se = se.PadLeft(16, '0');
                se = v6st.Kolonlar(se, this.is128Checked) + "/" + input.subnetslash.ToString();
            }
            else if (this.is128Checked == CheckState.Checked)
            {
                ss = ss.PadLeft(32, '0');
                ss = v6st.Kolonlar(ss, this.is128Checked) + "/" + input.subnetslash.ToString();
                se = se.PadLeft(32, '0');
                se = v6st.Kolonlar(se, this.is128Checked) + "/" + input.subnetslash.ToString();
            }

            this.label8.Text = ss;
            this.label9.Text = se;

            if (input.ID == 0 || input.ID == 2)
                this.maxsubnet = (BigInteger.One << (this.StartEnd.subnetslash - this.StartEnd.slash));
            else if (input.ID == 1)
            {
                if (this.is128Checked == CheckState.Unchecked)
                {
                    this.maxsubnet = (BigInteger.One << (64 - this.StartEnd.subnetslash));
                }
                else if (this.is128Checked == CheckState.Checked)
                {
                    this.maxsubnet = (BigInteger.One << (128 - this.StartEnd.subnetslash));
                }
            }
            else
            {
                return;
            }

            this.textBox4.Text = (maxsubnet - 1).ToString();

            ShowDiskInfo();
        }

        private void SaveAs_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
            this.backgroundWorker2.CancelAsync();
            this.progressBar1.Value = 0;

            if (this.textBox1.Text.Trim() == "" || this.textBox2.Text.Trim() == "")
            {
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                return;
            }

            try
            {
                this.FromIndex = BigInteger.Parse(this.textBox1.Text, NumberStyles.Number);
            }
            catch
            {
                this.textBox1.Text = "";
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                this.textBox1.Focus();
                return;
            }
            try
            {
                this.ToIndex = BigInteger.Parse(this.textBox2.Text, NumberStyles.Number);
            }
            catch
            {
                this.textBox2.Text = "";
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                this.textBox2.Focus();
                return;
            }

            if (this.ToIndex > (maxsubnet - 1))
            {
                this.textBox2.BackColor = Color.FromKnownColor(KnownColor.Info);
                this.textBox2.SelectAll();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e1", this.culture);
                return;
            }
            else if (this.ToIndex < this.FromIndex)
            {
                this.textBox2.BackColor = Color.FromKnownColor(KnownColor.Info);
                this.textBox2.SelectAll();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e2", this.culture);
                return;
            }
            else
            {
                this.SaveAs.Enabled = false;
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
                this.label5.Text = "";

                StartEnd.subnetidx = this.FromIndex;
                this.TotalBytes = BigInteger.Zero;
                StartEnd.Start = StartEnd.LowerLimitAddress;
                StartEnd.End = StartEnd.UpperLimitAddress;

                StartEnd = v6st.GoToSubnet(this.StartEnd, this.is128Checked);
                BigInteger OnceTotalBytes = BigInteger.Zero;
                BigInteger OnceDnsTotalBytes = BigInteger.Zero;
                string fnamestart = "";

                fnamestart = String.Format("{0:x}", this.StartEnd.Start);
                fnamestart = fnamestart.TrimStart('0');

                if (this.is128Checked == CheckState.Unchecked)
                {
                    fnamestart = fnamestart.PadLeft(16, '0');
                }
                else if (this.is128Checked == CheckState.Checked)
                {
                    fnamestart = fnamestart.PadLeft(32, '0');
                }

                saveDialog.Filter = "Text (*.wordpad)|*.wordpad|Text (*.txt)|*.txt";

                if (this.incomingID == 0 || this.incomingID == 1)
                {
                    saveDialog.FileName = fnamestart + StringsDictionary.KeyValue("SaveAs_FileName_prefix", this.culture)
                        + this.StartEnd.slash + StringsDictionary.KeyValue("SaveAs_FileName_to", this.culture)
                        + this.StartEnd.subnetslash.ToString()
                        + StringsDictionary.KeyValue("SaveAs_FileName_index", this.culture)
                        + this.FromIndex.ToString() + StringsDictionary.KeyValue("SaveAs_FileName_to", this.culture)
                        + this.ToIndex.ToString();
                }
                else if (this.incomingID == 2)
                {
                    OnceTotalBytes = OnceDnsTotalBytes;
                    saveDialog.FileName = StringsDictionary.KeyValue("SaveAs_FileName_ReverseDNS", this.culture)
                        + fnamestart + StringsDictionary.KeyValue("SaveAs_FileName_prefix", this.culture)
                        + this.StartEnd.subnetslash.ToString()
                        + StringsDictionary.KeyValue("SaveAs_FileName_index", this.culture)
                        + this.FromIndex.ToString()
                        + StringsDictionary.KeyValue("SaveAs_FileName_to", this.culture) + this.ToIndex.ToString();
                }

                this.textBox1.BackColor = Color.White;
                this.textBox2.BackColor = Color.White;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    this.progressBar1.Visible = true;
                    this.textBox5.Visible = true;
                    this.cancelButton.Enabled = true;
                    this.label5.Text = "";
                    this.label5.ForeColor = Color.RoyalBlue;

                    backgroundWorker1.RunWorkerAsync();
                    Thread.Sleep(7);
                }
                else
                {
                    this.SaveAs.Enabled = true;
                    this.cancelButton.Enabled = false;
                    this.textBox1.Enabled = true;
                    this.textBox2.Enabled = true;
                }
            }
        }

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar1.Hide();
            this.textBox5.Visible = false;
            StartEnd.Start = StartEnd.LowerLimitAddress;
            StartEnd.End = StartEnd.UpperLimitAddress;

            if (e.Cancelled)
            {
                this.backgroundWorker2.CancelAsync();

                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_label5", this.culture)
                    + saveState.percentage + "% )";

                if (File.Exists(this.saveDialog.FileName))
                {
                    try
                    {
                        File.Delete(this.saveDialog.FileName);
                    }
                    catch (SystemException ioex)
                    {
                        MessageBox.Show(StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_error", this.culture)
                            + ioex.Message,
                            StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_head_file", this.culture),
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
            }
            else if (e.Error != null)
            {
                this.backgroundWorker2.CancelAsync();

                MessageBox.Show(StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_error", this.culture)
                    + e.Error.Message, StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_head_error", this.culture),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                if (File.Exists(this.saveDialog.FileName))
                {
                    try
                    {
                        File.Delete(this.saveDialog.FileName);
                    }
                    catch (SystemException ioex)
                    {
                        MessageBox.Show(StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_error", this.culture)
                            + ioex.Message,
                            StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_head_file", this.culture),
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
            }
            else
            {
                this.backgroundWorker2.CancelAsync();

                this.label5.Text = "";
                this.label5.ForeColor = Color.RoyalBlue;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_label5_1", this.culture)
                    + Environment.NewLine + saveState.SavedLines.ToString() + Environment.NewLine
                    + StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_label5_2", this.culture)
                    + Environment.NewLine + "("
                    + String.Format(CultureInfo.InvariantCulture, "{0:0,0}", this.TotalBytes)
                    + StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_label5_3", this.culture);

                this.SaveAs.Enabled = true;
                this.cancelButton.Enabled = false;
                this.textBox1.Enabled = true;
                this.textBox2.Enabled = true;
            }

            ShowDiskInfo();
        }

        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.textBox5.Text = e.ProgressPercentage.ToString() + "%";

            if (this.backgroundWorker2.IsBusy == false)
                this.backgroundWorker2.RunWorkerAsync();
        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (StartEnd.Start > StartEnd.UpperLimitAddress)
                return;

            var saveAsName = new StreamWriter(saveDialog.FileName);
            string ss = "", se = "";
            howmany = (this.ToIndex - this.FromIndex + 1);
            int perc = 0;
            this.count = 0;
            this.TotalBytes = BigInteger.Zero;

            StartEnd.subnetidx = this.FromIndex;

            if (this.incomingID == 1)
            {
                if (this.is128Checked == CheckState.Unchecked)
                    StartEnd.subnetslash = 64;
                else if (this.is128Checked == CheckState.Checked)
                    StartEnd.subnetslash = 128;
            }

            StartEnd = v6st.GoToSubnet(this.StartEnd, this.is128Checked);

            BigInteger i;
            for (i = 0; i < howmany; i++)
            {
                this.count++;

                if (backgroundWorker1.CancellationPending)
                {
                    this.count--;
                    e.Cancel = true;
                    break;
                }
                else
                {
                    StartEnd = v6st.Subnetting(StartEnd, this.is128Checked);

                    if (this.is128Checked == CheckState.Unchecked)
                    {
                        if (this.incomingID == 0 || this.incomingID == 1)
                        {
                            ss = String.Format("{0:x}", StartEnd.Start);
                            if (ss.Length > 16)
                                ss = ss.Substring(1, 16);
                            ss = v6st.Kolonlar(ss, this.is128Checked);
                            ss = v6st.CompressAddress(ss);

                            se = String.Format("{0:x}", StartEnd.End);
                            if (se.Length > 16)
                                se = se.Substring(1, 16);
                            se = v6st.Kolonlar(se, this.is128Checked);

                            //ss = "s" + StartEnd.subnetidx + "> " + ss + "/" +
                            ss = "p" + StartEnd.subnetidx + "> " + ss + "/" +
                                StartEnd.subnetslash;
                            TotalBytes += ss.Length + 2;
                            saveAsName.WriteLine(ss);

                            if (StartEnd.subnetslash != 64)
                            {
                                se = "e" + StartEnd.subnetidx + "> " + se + "/"
                                    + StartEnd.subnetslash;
                            }
                        }
                        else if (this.incomingID == 2)
                        {
                            String[] sa;
                            int spaces = 0;

                            sa = v6st.DnsRev(StartEnd.Start, StartEnd.subnetslash, this.is128Checked);
                            //sa[0] = "s" + StartEnd.subnetidx + "> " + sa[0];
                            sa[0] = "p" + StartEnd.subnetidx + "> " + sa[0];
                            spaces = sa[0].Split(' ')[0].Length + 1;

                            for (int n = 0; n < 8; n++)
                            {
                                if (sa[n] == null)
                                    break;
                                if (n > 0)
                                    sa[n] = sa[n].PadLeft(sa[n].Length + spaces, ' ');

                                TotalBytes += sa[n].Length + 2;
                                saveAsName.WriteLine(sa[n]);
                            }
                        }
                    }
                    else if (this.is128Checked == CheckState.Checked)
                    {
                        if (this.incomingID == 0 || this.incomingID == 1)
                        {
                            ss = String.Format("{0:x}", StartEnd.Start);
                            if (ss.Length > 32)
                                ss = ss.Substring(1, 32);
                            ss = v6st.Kolonlar(ss, this.is128Checked);
                            ss = v6st.CompressAddress(ss);

                            se = String.Format("{0:x}", StartEnd.End);
                            if (se.Length > 32)
                                se = se.Substring(1, 32);
                            se = v6st.Kolonlar(se, this.is128Checked);

                            //ss = "s" + StartEnd.subnetidx + "> " + ss + "/" +
                            ss = "p" + StartEnd.subnetidx + "> " + ss + "/" +
                                StartEnd.subnetslash;

                            TotalBytes += ss.Length + 2;
                            saveAsName.WriteLine(ss);

                            if (StartEnd.subnetslash != 128)
                            {
                                se = "e" + StartEnd.subnetidx + "> " + se + "/"
                                    + StartEnd.subnetslash;
                            }
                        }
                        else if (this.incomingID == 2)
                        {
                            String[] sa;
                            int spaces = 0;

                            sa = v6st.DnsRev(StartEnd.Start, StartEnd.subnetslash, this.is128Checked);
                            sa[0] = "s" + StartEnd.subnetidx + "> " + sa[0];
                            spaces = sa[0].Split(' ')[0].Length + 1;

                            for (int n = 0; n < 8; n++)
                            {
                                if (sa[n] == null)
                                    break;
                                if (n > 0)
                                    sa[n] = sa[n].PadLeft(sa[n].Length + spaces, ' ');

                                TotalBytes += sa[n].Length + 2;
                                saveAsName.WriteLine(sa[n]);
                            }
                        }
                    }

                    if (StartEnd.Start == StartEnd.UpperLimitAddress
                        || StartEnd.subnetidx == (maxsubnet - 1))
                    {
                        break;
                    }
                    StartEnd.Start = StartEnd.End + BigInteger.One;

                    perc = (int)(i * 100 / howmany);

                    saveState.SavedLines = this.count;
                    saveState.percentage = perc;
                    this.backgroundWorker1.ReportProgress(perc);
                }
            }

            perc = (int)(i * 100 / howmany);
            saveState.SavedLines = this.count;
            saveState.percentage = perc;
            this.backgroundWorker1.ReportProgress(perc);
            saveAsName.Close();
        }

        void bgw2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void bgw2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.backgroundWorker1.IsBusy == true)
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_bgw2_ProgressChanged_label5", this.culture)
                    + this.count.ToString();
        }

        void bgw2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.backgroundWorker2.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
            else
                backgroundWorker2.ReportProgress(1);
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (this.textBox1.Text.Trim() == "")
            {
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                return;
            }

            try
            {
                this.FromIndex = BigInteger.Parse(this.textBox1.Text, NumberStyles.Number);
            }
            catch
            {
                this.textBox1.Text = "";
                this.textBox1.Focus();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                return;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (this.textBox2.Text.Trim() == "")
            {
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                return;
            }

            try
            {
                this.ToIndex = BigInteger.Parse(this.textBox2.Text, NumberStyles.Number);
            }
            catch
            {
                this.textBox2.Text = "";
                this.textBox2.Focus();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e0", this.culture);
                return;
            }

            if (this.ToIndex > (maxsubnet - 1))
            {
                this.textBox2.BackColor = Color.FromKnownColor(KnownColor.Info);
                this.textBox4.BackColor = Color.FromKnownColor(KnownColor.Info);
                this.textBox2.SelectAll();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e1", this.culture);
                return;
            }
            else if (this.ToIndex < this.FromIndex)
            {
                this.textBox2.BackColor = Color.FromKnownColor(KnownColor.Info);
                this.textBox2.SelectAll();
                this.label5.ForeColor = Color.Red;
                this.label5.Text = StringsDictionary.KeyValue("SaveAs_Click_e2", this.culture);
                return;
            }
            else
            {
                this.textBox4.BackColor = Color.FromKnownColor(KnownColor.Control);
                this.label5.Text = "";
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.textBox1.BackColor = Color.White;
            this.label5.Text = "";

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.textBox2.BackColor = Color.White;
            this.label5.Text = "";
            this.textBox4.BackColor = Color.FromKnownColor(KnownColor.Control);
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();

            this.SaveAs.Enabled = true;
            this.cancelButton.Enabled = false;
            this.textBox1.Enabled = true;
            this.textBox2.Enabled = true;

            ShowDiskInfo();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExportToFile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.backgroundWorker1.IsBusy || this.backgroundWorker2.IsBusy)
            {
                this.backgroundWorker1.CancelAsync();
                this.backgroundWorker2.CancelAsync();

                MessageBox.Show(StringsDictionary.KeyValue("SaveAs_ExportToFile_FormClosing_Msg", this.culture)
                    + saveState.percentage.ToString() + "% )",
                    StringsDictionary.KeyValue("SaveAs_ExportToFile_FormClosing_Msg_head", this.culture),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);

                if (File.Exists(this.saveDialog.FileName))
                {
                    try
                    {
                        File.Delete(this.saveDialog.FileName);
                    }
                    catch (SystemException ioex)
                    {
                        MessageBox.Show(StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_error", this.culture)
                            + ioex.Message,
                            StringsDictionary.KeyValue("SaveAs_bgw_RunWorkerCompleted_head_file", this.culture),
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }
                }
            }
        }

        private void ShowDiskInfo()
        {
            this.listView1.Items.Clear();

            var alldisk = this.diskspace.GetDiskSpace();

            alldisk.ForEach(x =>
            {
                string[] list = new string[] { x.Item1, x.Item2 };
                this.listView1.Items.Add(new ListViewItem(list));
            });
        }

        public void SwitchLanguage(CultureInfo culture)
        {
            this.culture = culture;

            this.Text = StringsDictionary.KeyValue("SaveAsText_Form", this.culture);
            this.label4.Text = StringsDictionary.KeyValue("SaveAs_label4.Text", this.culture);
            //
            this.cancelButton.Text = StringsDictionary.KeyValue("SaveAs_cancelButton.Text", this.culture);
            this.columnHeader1.Text = StringsDictionary.KeyValue("SaveAs_columnHeader1.Text", this.culture);
            this.columnHeader2.Text = StringsDictionary.KeyValue("SaveAs_columnHeader2.Text", this.culture);
            this.exitButton.Text = StringsDictionary.KeyValue("SaveAs_exitButton.Text", this.culture);
            this.label10.Text = StringsDictionary.KeyValue("SaveAs_label10.Text", this.culture);
            this.label11.Text = StringsDictionary.KeyValue("SaveAs_label11.Text", this.culture);
            this.label12.Text = StringsDictionary.KeyValue("SaveAs_label12.Text", this.culture);
            this.label2.Text = StringsDictionary.KeyValue("SaveAs_label2.Text", this.culture);
            this.label3.Text = StringsDictionary.KeyValue("SaveAs_label3.Text", this.culture);
            this.label4.Text = StringsDictionary.KeyValue("SaveAs_label4.Text", this.culture);
            this.label5.Text = StringsDictionary.KeyValue("SaveAs_label5.Text", this.culture);
            this.label6.Text = StringsDictionary.KeyValue("SaveAs_label6.Text", this.culture);
            this.SaveAs.Text = StringsDictionary.KeyValue("SaveAs_SaveAs.Text", this.culture);
            this.textBox1.Text = StringsDictionary.KeyValue("SaveAs_textBox1.Text", this.culture);
            this.textBox2.Text = StringsDictionary.KeyValue("SaveAs_textBox2.Text", this.culture);
            //
            this.toolTip1.SetToolTip(this.label8, StringsDictionary.KeyValue("SaveAs_label8.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.label9, StringsDictionary.KeyValue("SaveAs_label9.ToolTip", this.culture));
            this.toolTip1.SetToolTip(this.textBox4, StringsDictionary.KeyValue("SaveAs_textBox4.ToolTip", this.culture));
        }
    }
}
