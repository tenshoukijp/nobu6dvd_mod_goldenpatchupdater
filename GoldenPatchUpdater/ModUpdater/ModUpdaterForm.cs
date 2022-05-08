using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModUpdater
{

    partial class ModUpdaterForm : Form
    {
        Button btnUpdate;           // 更新ボタン
        DataGridView dgvUpdate;     // グリッドビューボタン
        // データグリッドビューのタイトル
        static String[] colTitle = new String[] { "ファイル名", "ファイル更新日時", "バージョン", "ファイルサイズ", "比較結果" };

        bool ControlsAdded;


        StatusBar sbProgress;       // ステータスバー

        bool isSholdBeReload;       // GoldenPatchUpdater.iniが更新されているので、もう一度。

        public ModUpdaterForm()
        {
            // 自分自身のセッティング
            DefaultFormSettings();

            // コンポーネントの生成
            btnUpdate = new Button();
            btnUpdate.Click += new EventHandler(btnUpdate_Click);

            dgvUpdate = new DataGridView();
            sbProgress = new StatusBar();

            UpdateGUIControls();

            this.Resize += new EventHandler(Form_Resize);
            this.Shown += new EventHandler(Form_Shown);
            this.Closed += new EventHandler(Form_Closed);
        }


        /// <summary>
        ///  最初のフォームのデフォルト設定
        /// </summary>
        private void DefaultFormSettings()
        {
            this.Width = 700;
            this.Height = 400;
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.MaximizeBox = false;
            // このプロジェクトのアセンブリのタイプを取得。
            System.Reflection.Assembly prj_assebmly = GetType().Assembly;
            System.Resources.ResourceManager r = new System.Resources.ResourceManager(String.Format("{0}.ModUpdaterRes", prj_assebmly.GetName().Name), prj_assebmly);

            System.Drawing.Icon iconform = (System.Drawing.Icon)(r.GetObject("icon"));
            this.Icon = iconform;


        }

        /// <summary>
        ///  フォー上に乗るGUIの設定
        /// </summary>
        private void UpdateGUIControls()
        {

            // 最初と更新時共通の処理
            btnUpdate.Left = 10;
            btnUpdate.Top = 10;
            btnUpdate.Width = 100;
            btnUpdate.Height = 30;
            btnUpdate.Text = "更新";

            sbProgress.Left = 100;
            sbProgress.Height = 20;
            dgvUpdate.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUpdate.RowHeadersVisible = false;
            dgvUpdate.Left = 10;
            dgvUpdate.Width = this.ClientSize.Width - 20;
            dgvUpdate.Height = this.ClientSize.Height - 20 - btnUpdate.Height - sbProgress.Height;
            dgvUpdate.Top = btnUpdate.Bottom + 10;


            // １回だけの処理
            if (!ControlsAdded)
            {

                foreach (String title in colTitle)
                {
                    DataGridViewTextBoxColumn dgvtbc = new DataGridViewTextBoxColumn();

                    dgvtbc.HeaderText = title;
                    dgvUpdate.Columns.Add(dgvtbc);
                }


                // コンポーネントの配置
                this.Controls.Add(btnUpdate);
                this.Controls.Add(dgvUpdate);
                this.Controls.Add(sbProgress);
                ControlsAdded = true;

            }

        }

        /// <summary>
        /// フォームをリサイズした時
        /// </summary>
        private void Form_Resize(Object o, EventArgs e)
        {
            UpdateGUIControls();
        }

        private void Form_Shown(Object o, EventArgs e)
        {
            // この後の処理が重たいのでここで全部描画する。
            this.Refresh();

            // 重い
            LoadIniFile();

            Timer timer = new Timer();
            timer.Tick += new EventHandler(Form_Tick);
            timer.Interval = 500;
            timer.Enabled = true; // timer.Start()と同じ

        }

        private void Form_Tick(Object o, EventArgs e)
        {
            if (isSholdBeReload)
            {
                isSholdBeReload = false;
                LoadIniFile();
            }
        }

        private void Form_Closed(Object o, EventArgs e)
        {
            // テンポラリディレクトリの削除
            if (System.IO.Directory.Exists(strTmpDir))
            {

                Cancel_Click(o, e);
                DeleteDirectory(new System.IO.DirectoryInfo(strTmpDir));
            }
        }

        List<String> updateTargetFiles;

        private void DgvDataImport()
        {

            if (updateTargetFiles == null)
            {
                updateTargetFiles = new List<String>();
            }

            updateTargetFiles.Clear();

            //全ての行の背景色を白色にする
            dgvUpdate.RowsDefaultCellStyle.BackColor = System.Drawing.Color.White;
            dgvUpdate.Rows.Clear();

            foreach (String filename in lstUpdateFiles)
            {
                String srcFullPath = strTmpDir + @"\" + filename;
                String dstFullPath = filename;
                // 本当に存在するならば
                if (System.IO.File.Exists(srcFullPath))
                {
                    // "ファイル名", "ファイル更新日時", "バージョン", "ファイルサイズ", "処理" };

                    // まず、新しくダウンロードしてきたファイルの情報
                    DateTime srcDtUpdate = System.IO.File.GetLastWriteTime(srcFullPath);
                    System.IO.FileInfo srcFI = new System.IO.FileInfo(srcFullPath);
                    long srcFileSize = srcFI.Length;
                    System.Diagnostics.FileVersionInfo srcVI = System.Diagnostics.FileVersionInfo.GetVersionInfo(srcFullPath);
                    String srcVersion = "";
                    if (srcVI.FileVersion != null)
                    {
                        srcVersion = string.Format("{0}.{1}.{2}.{3}", srcVI.FileMajorPart, srcVI.FileMinorPart, srcVI.FileBuildPart, srcVI.FilePrivatePart);
                    }
                    else
                    {
                        srcVersion = "-";
                    }


                    if (!System.IO.File.Exists(dstFullPath))
                    {
                        dgvUpdate.Rows.Add(filename, srcDtUpdate.ToString(), srcVersion, srcFileSize.ToString(), "新規");
                        // 更新対象として、追加する。
                        updateTargetFiles.Add(filename);

                        btnUpdate.Enabled = true; // 更新対象が１つでもあった。
                        continue;
                    }

                    // 現在天翔記フォルダにある対象のファイル
                    DateTime dstDtUpdate = System.IO.File.GetLastWriteTime(dstFullPath);
                    System.IO.FileInfo dstFI = new System.IO.FileInfo(dstFullPath);
                    long dstFileSize = dstFI.Length;
                    System.Diagnostics.FileVersionInfo dstVI = System.Diagnostics.FileVersionInfo.GetVersionInfo(dstFullPath);
                    String dstVersion = "";
                    if (dstVI.FileVersion != null)
                    {
                        dstVersion = string.Format("{0}.{1}.{2}.{3}", dstVI.FileMajorPart, dstVI.FileMinorPart, dstVI.FileBuildPart, dstVI.FilePrivatePart);
                    }
                    else
                    {
                        dstVersion = "-";
                    }

                    // バージョンが違うか、もしくは、よりファイルが新しいなら
                    if (srcVI.FileVersion != dstVI.FileVersion || srcDtUpdate > dstDtUpdate)
                    {
                        if (filename == iniFileName)
                        {
                            sbProgress.Text = "「" + iniFileName + "」 が更新されています。情報を再構築します。";
                            try
                            {
                                System.IO.File.Copy(srcFullPath, dstFullPath, true);
                                isSholdBeReload = true;
                            }
                            catch (Exception err)
                            {
                                sbProgress.Text = err.Message;
                                MessageBox.Show(err.Message);
                            }
                        }
                        dgvUpdate.Rows.Add(filename, srcDtUpdate.ToString(), srcVersion, srcFileSize.ToString(), "更新");
                        // 更新対象として、追加する。
                        updateTargetFiles.Add(filename);

                        btnUpdate.Enabled = true; // 更新対象が１つでもあった。

                    }
                    else if (srcDtUpdate.ToString() == dstDtUpdate.ToString())
                    {
                        dgvUpdate.Rows.Add(filename, srcDtUpdate.ToString(), srcVersion, srcFileSize.ToString(), "同じ");
                    }
                    // 天翔記フォルダの方がタイムスタンプが新しい
                    else if (srcDtUpdate < dstDtUpdate)
                    {
                        dgvUpdate.Rows.Add(filename, srcDtUpdate.ToString(), srcVersion, srcFileSize.ToString(), "ローカルの方が新しい");
                    }
                }
            }

            if (btnUpdate.Enabled)
            {
                sbProgress.Text = "更新ファイルがあります！";
            }
            else
            {
                sbProgress.Text = "更新ファイルはありません。";
            }

        }

        void btnUpdate_Click(Object o, EventArgs e)
        {
            bool hasError = false;
            foreach (String filename in updateTargetFiles)
            {
                String srcFullPath = strTmpDir + @"\" + filename;
                String dstFullPath = filename;

                try
                {
                    System.IO.File.Copy(srcFullPath, dstFullPath, true);
                }
                catch (Exception err)
                {
                    hasError = true;
                    sbProgress.Text = "ファイルコピー時にエラーが発生しました。コピー出来ません。";
                    MessageBox.Show("ファイルコピー時にエラーが発生しました。コピー出来ません。\n" + err.Message);

                }
            }

            // 金パッチ独特の処理
            if (!hasError)
            {
                if (!System.IO.File.Exists("n6pwav.dll"))
                {
                    try
                    {
                        System.IO.File.Copy("n6pkwav.dll", "n6pwav.dll"); // コピー
                    }
                    catch (Exception err)
                    {
                        hasError = true;
                        sbProgress.Text = "ファイルコピー時にエラーが発生しました。コピー出来ません。";
                        MessageBox.Show("ファイルコピー時にエラーが発生しました。コピー出来ません。\n" + err.Message);
                    }
                }
            }

            if (!hasError)
            {
                MessageBox.Show("全ての更新が反映されました！", "更新完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isSholdBeReload = true;
            }
        }
    }
}

