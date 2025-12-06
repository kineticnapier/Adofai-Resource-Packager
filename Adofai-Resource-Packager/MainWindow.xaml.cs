using Microsoft.Win32;

using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

using Path = System.IO.Path;

namespace Adofai_Resource_Packager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ret = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private static void Throw_error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            // ファイル選択
            OpenFileDialog ofd = new();
            ofd.Title = "Select a file";
            ofd.Filter = "adofai file (*.adofai)|*.adofai|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                ret = ofd.FileName;
            }
            else
            {
                ret = string.Empty;
            }

            path.Text = ret;
        }

        private async void Process_Click(object sender, RoutedEventArgs e)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(ret))
            {
                Throw_error("No file selected.");
                return;
            }

            string? directory;
            try
            {
                directory = Path.GetDirectoryName(ret);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    Throw_error("Invalid file path.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Throw_error("Invalid file path.\n" + ex.Message);
                return;
            }

            string[] files;
            try
            {
                // 必要に応じて対象拡張子を絞る
                //var targetExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                //{
                //    ".png", ".ogg", ".wav", ".mp3", ".jpg", ".jpeg"
                //};

                files = Directory.GetFiles(directory);
            }
            catch (Exception ex)
            {
                Throw_error("Failed to enumerate files in directory.\n" + ex.Message);
                return;
            }

            string fileContent;
            try
            {
                fileContent = File.ReadAllText(ret, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Throw_error("Failed to read the selected file.\n" + ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(fileContent))
            {
                MessageBox.Show("The selected file is empty.", "Result",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 重複防止用
            var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string adofaiFileName = Path.GetFileName(ret);

            // ファイル名→フルパスのマップを作成（重複名は最初のものを採用）
            var nameToPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in files)
            {
                string fileName = Path.GetFileName(f);

                if (fileName.Equals(adofaiFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                nameToPath.TryAdd(fileName, f);
            }

            if (nameToPath.Count > 0)
            {
                // 参照判定を1回の走査で済ませるため、すべてのファイル名をまとめて検索する
                string pattern = string.Join("|", nameToPath.Keys.Select(Regex.Escape));
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);

                foreach (Match match in regex.Matches(fileContent))
                {
                    if (nameToPath.TryGetValue(match.Value, out var fullPath) && added.Add(fullPath))
                    {
                        result.Add(fullPath);
                    }
                }
            }
            // .adofai 本体も zip に含める
            if (added.Add(ret))
            {
                result.Add(ret);
            }

            if (result.Count == 0)
            {
                MessageBox.Show("No files were used.", "Result",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 出力先の zip ファイル名をユーザーに選ばせる
            var sfd = new SaveFileDialog
            {
                Title = "Save resource zip",
                Filter = "Zip archive (*.zip)|*.zip",
                FileName = Path.GetFileNameWithoutExtension(ret) + "_resources.zip",
            };

            if (sfd.ShowDialog() != true)
            {
                // キャンセルされたら中断
                return;
            }

            string zipPath = sfd.FileName;
            int totalEntries = result
                .Select(Path.GetFileName)
                .Where(name => name is not null)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            progressBar.Minimum = 0;
            progressBar.Maximum = Math.Max(1, totalEntries);
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            progressText.Visibility = Visibility.Visible;
            progressText.Text = "Creating zip file...";

            try
            {
                await Task.Run(() =>
                {
                    // 既に同名の zip があれば削除（上書き）
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }

                    // ZipArchive を作成モードで開く
                    using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        // zip 内部での重複ファイル名を防ぐためのセット
                        var entryNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        int processed = 0;

                        foreach (var f in result)
                        {
                            if (!File.Exists(f))
                            {
                                // 途中で削除されたなどの場合はスキップ
                                continue;
                            }

                            // zip 内のエントリ名（ここではファイル名のみ）
                            string entryName = Path.GetFileName(f);

                            // 同じファイル名が既に追加されていればスキップ
                            if (!entryNameSet.Add(entryName))
                            {
                                // 必要なら別名にする処理を書くこともできるが、
                                // ここではシンプルにスキップ
                                continue;
                            }

                            // ファイルを zip エントリとして追加
                            zip.CreateEntryFromFile(f, entryName);

                            processed++;
                            Dispatcher.Invoke(() =>
                            {
                                progressBar.Value = processed;
                                progressText.Text = $"Packing files... {processed}/{totalEntries}";
                            }, DispatcherPriority.Background);
                        }
                    }
                });

                progressText.Text = "Completed.";
                MessageBox.Show(
                    "Zip file created:\n" + zipPath,
                    "Completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Throw_error("Failed to create zip file.\n" + ex.Message);
            }
            finally
            {
                progressBar.Value = 0;
                progressBar.Visibility = Visibility.Collapsed;
                progressText.Visibility = Visibility.Collapsed;
            }
        }
    }
}
