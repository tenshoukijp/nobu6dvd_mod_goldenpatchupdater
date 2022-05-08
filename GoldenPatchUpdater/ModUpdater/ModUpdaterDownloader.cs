using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;


namespace ModUpdater
{
    partial class ModUpdaterForm : Form
    {       //WebClientフィールド
        System.Net.WebClient downloadClient = null;

        String strGZipFileName;

        private void DownloadArchive()
        {
            downloadClient = null;
            String filename = System.IO.Path.GetFileName(URL);
            sbProgress.Text = "サイトに接続しています。しばらくお待ちください";
            strGZipFileName = strTmpDir + @"\" + filename;
            DownloadArchiveFile(URL, strGZipFileName);
        }


        //Button1のClickイベントハンドラ
        private void DownloadArchiveFile(String SrcURL, String DsttFilePatth)
        {
            //Button1.Enabled = false;
            // Button2.Enabled = true;

            //ダウンロードしたファイルの保存先
            string fileName = DsttFilePatth;
            //ダウンロード基のURL
            Uri u = new Uri(SrcURL);

            //WebClientの作成
            if (downloadClient == null)
            {
                downloadClient = new System.Net.WebClient();
                //イベントハンドラの作成
                downloadClient.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(downloadClient_DownloadProgressChanged);
                downloadClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(downloadClient_DownloadFileCompleted);
            }

            //非同期ダウンロードを開始する
            downloadClient.DownloadFileAsync(u, fileName);
        }

        //CancelのClickイベントハンドラ
        private void Cancel_Click(object sender, EventArgs e)
        {
            //非同期ダウンロードをキャンセルする
            if (downloadClient != null)
            {
                downloadClient.CancelAsync();
            }
        }

        private void downloadClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            String output = String.Format("{0}% ({1}byte 中 {2}byte) ダウンロードが終了しました。", e.ProgressPercentage, e.TotalBytesToReceive, e.BytesReceived);
            sbProgress.Text = output;
        }

        private void downloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                String output = "キャンセルされました。";
                sbProgress.Text = output;

            }
            else if (e.Error != null)
            {
                String output = String.Format("エラー:{0}", e.Error.Message);
                sbProgress.Text = output;

            }
            else
            {
                String output = "準備が完了しました。";
                sbProgress.Text = output;

                extractGZip(strGZipFileName);

                DgvDataImport();
            }

            //Button1.Enabled = true;
            //Button2.Enabled = false;
        }

    }

}