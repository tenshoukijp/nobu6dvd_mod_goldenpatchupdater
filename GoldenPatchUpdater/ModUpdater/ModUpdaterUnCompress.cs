using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;


namespace ModUpdater
{
    partial class ModUpdaterForm : Form
    {

        /// <summary>
        ///  GZipファイルの解凍
        /// </summary>
        /// <param name="filename">相対(または絶対)ファイル名</param>
        private void extractGZip(String gzipFileName)
        {

            String inFile = gzipFileName;

                // 入力ファイルは.gzファイルのみ有効
                if (!inFile.ToLower().EndsWith(".gz"))
                {
                    return;
                }

                // ファイル名末尾の「.gz」を削除
                string outFile = inFile.Substring(0, inFile.Length - 3);

                int num;
                byte[] buf = new byte[1024]; // 1Kbytesずつ処理する

                // 入力ストリーム
                FileStream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read);
    
                // 解凍ストリーム
                GZipStream decompStream  = new GZipStream(
                    inStream, // 入力元となるストリームを指定
                    CompressionMode.Decompress); // 解凍（圧縮解除）を指定

                 // 出力ストリーム
                FileStream outStream = new FileStream(outFile, FileMode.Create);

                using (inStream)
                using (outStream)
                using (decompStream)
                {
                    while ((num = decompStream.Read(buf, 0, buf.Length)) > 0)
                    {
                        outStream.Write(buf, 0, num);
                    }
                }

                if (File.Exists(outFile))
                {
                    extractTar(outFile);
                } else {
                    sbProgress.Text = "ファイル解凍中にエラーが発生しました";
                }
            }

        /// <summary>
        ///  GZipファイルの解凍
        /// </summary>
        /// <param name="filename">相対(または絶対)ファイル名</param>
        private void extractTar(String tarFileName)
        {
            String fullTarPath = System.IO.Path.GetFullPath(tarFileName);
            TarInputStream tis1 = new TarInputStream(File.OpenRead(fullTarPath));
            TarArchive archive = TarArchive.CreateInputTarArchive(tis1);

            archive.SetKeepOldFiles(false);
            archive.AsciiTranslate = false;
            archive.SetUserInfo(0, "", 0, "None");

            String fullDirName = System.IO.Path.GetFullPath(strTmpDir);
            archive.ExtractContents(fullDirName);
            archive.Close();
            tis1.Close();


            //ZIP内のエントリを列挙
            ICSharpCode.SharpZipLib.Tar.TarEntry te;

            TarInputStream tis2 = new TarInputStream(File.OpenRead(fullTarPath));
            //ZipEntryを取得
            while ((te = tis2.GetNextEntry()) != null)
            {
                if (te.IsDirectory)
                {
                    //ディレクトリのとき
                    Console.WriteLine("ディレクトリ名 : {0}", te.Name);
                    Console.WriteLine("日時 : {0}", te.ModTime);
                    Console.WriteLine();
                }
                //情報を表示する
                else
                {
                    //ファイルのとき
                    Console.WriteLine("名前 : {0}", te.Name);
                    Console.WriteLine("サイズ : {0} bytes", te.Size);
                    System.IO.File.SetLastWriteTime(strTmpDir + @"\" + te.Name, te.ModTime.ToLocalTime()); // タイムスタンプを修正
                    Console.WriteLine("日時 : {0}", te.ModTime);
                    Console.WriteLine();
                }
            }

            tis2.Close();
        }
    }
}