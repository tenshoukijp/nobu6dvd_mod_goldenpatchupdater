using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ModUpdater
{
    partial class ModUpdaterForm : Form
    {

        InifileUtils iniFile;       // ini ファイル
        
        static String iniFileName = "GoldenPatchUpdater.ini";
        static String URL = "";
        static String strTmpDir = "UpdateArchive";

        List<String> lstUpdateFiles;

        /// <summary>
        /// メインとなる処理
        /// </summary>
        private void LoadIniFile()
        {
            btnUpdate.Enabled = false;
            isSholdBeReload = false;

            if (lstUpdateFiles == null)
            {
                lstUpdateFiles = new List<String>();
            }
            lstUpdateFiles.Clear(); // 一度リセット

            if (System.IO.File.Exists(iniFileName))
            {
                iniFile = new InifileUtils(iniFileName);
                Text = "「" + iniFile.getValueString("Archive", "Title") + "」の更新をチェック";

                URL = iniFile.getValueString("Archive", "URL");

                for (int i = 0; i < 100; i++)
                {
                    String targetKeyName = String.Format("F{0:00}", i);
                    String targetFileName = iniFile.getValueString("UpdateFiles", targetKeyName);
                    if (targetFileName != String.Empty)
                    {
                        lstUpdateFiles.Add(targetFileName);
                    }
                }
            }
            else
            {
                sbProgress.Text = iniFileName + "の読み込み中にエラーが発生しました。";
                this.Close();
            }

            // テンポラリディレクトリの作成
            if (!System.IO.Directory.Exists(strTmpDir))
            {
                System.IO.Directory.CreateDirectory(strTmpDir);
            }

            DownloadArchive();

            // テンポラリディレクトリの削除
            if (System.IO.Directory.Exists(strTmpDir))
            {
                // DeleteDirectory( new System.IO.DirectoryInfo(strTmpDir) );
            }

        }

        /// ----------------------------------------------------------------------------
        /// <summary>
        ///     指定したディレクトリをすべて削除します。</summary>
        /// <param name="hDirectoryInfo">
        ///     削除するディレクトリの DirectoryInfo。</param>
        /// ----------------------------------------------------------------------------
        public static void DeleteDirectory(System.IO.DirectoryInfo hDirectoryInfo)
        {
            // すべてのファイルの読み取り専用属性を解除する
            foreach (System.IO.FileInfo cFileInfo in hDirectoryInfo.GetFiles())
            {
                if ((cFileInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
                {
                    cFileInfo.Attributes = System.IO.FileAttributes.Normal;
                }
            }

            // サブディレクトリ内の読み取り専用属性を解除する (再帰)
            foreach (System.IO.DirectoryInfo hDirInfo in hDirectoryInfo.GetDirectories())
            {
                DeleteDirectory(hDirInfo);
            }

            // このディレクトリの読み取り専用属性を解除する
            if ((hDirectoryInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
            {
                hDirectoryInfo.Attributes = System.IO.FileAttributes.Directory;
            }

            // このディレクトリを削除する
            hDirectoryInfo.Delete(true);
        }


    }
}