using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DesignerSuite.App.Pages
{
    public partial class PdfLabelSorterPage : Page
    {
        private Process _runningProcess;
        private List<string> _sources = new();

        public PdfLabelSorterPage()
        {
            InitializeComponent();
        }

        private void BrowseToolPath_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select python.exe or packaged PDF Label Sorter CLI exe"
            };

            if (ofd.ShowDialog() == true)
            {
                ToolPathTextBox.Text = ofd.FileName;
            }
        }

        private void BrowseGuide_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select guide PDF"
            };

            if (ofd.ShowDialog() == true)
            {
                GuidePathTextBox.Text = ofd.FileName;
            }
        }

        private void BrowseSources_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select label PDF(s)",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                _sources = ofd.FileNames.ToList();
                SourcesTextBox.Text = string.Join("; ", _sources);

                if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text) && _sources.Count > 0)
                {
                    try
                    {
                        var firstDir = Path.GetDirectoryName(_sources[0]) ?? "";
                        if (!string.IsNullOrWhiteSpace(firstDir))
                        {
                            OutputPathTextBox.Text = Path.Combine(firstDir, $"OutputLabel_{DateTime.Now:yyyy-MM-dd}.pdf");
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select output PDF",
                FileName = "OutputLabel.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                OutputPathTextBox.Text = sfd.FileName;
            }
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            if (_runningProcess != null)
            {
                AppendLog("A process is already running.");
                return;
            }

            var guide = (GuidePathTextBox.Text ?? string.Empty).Trim();
            var output = (OutputPathTextBox.Text ?? string.Empty).Trim();
            var tool = (ToolPathTextBox.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(guide) || !File.Exists(guide))
            {
                AppendLog("Guide PDF not found.");
                return;
            }

            if (_sources.Count == 0 || _sources.Any(s => string.IsNullOrWhiteSpace(s) || !File.Exists(s)))
            {
                AppendLog("Please select at least one valid source PDF.");
                return;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                AppendLog("Please select an output path.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tool))
            {
                AppendLog("Tool path is required (python or packaged CLI exe)." );
                return;
            }

            var selected = PlatformCombo.SelectedItem as ComboBoxItem;
            var platform = (selected?.Tag?.ToString() ?? "amazon").Trim();

            RunButton.IsEnabled = false;

            try
            {
                await RunCliAsync(tool, platform, guide, _sources, output);
            }
            finally
            {
                RunButton.IsEnabled = true;
            }
        }

        private Task RunCliAsync(string toolPath, string platform, string guidePath, List<string> sources, string outputPath)
        {
            return Task.Run(() =>
            {
                try
                {
                    var isPython = Path.GetFileName(toolPath).Equals("python.exe", StringComparison.OrdinalIgnoreCase)
                        || Path.GetFileName(toolPath).Equals("python", StringComparison.OrdinalIgnoreCase)
                        || toolPath.EndsWith("python", StringComparison.OrdinalIgnoreCase)
                        || toolPath.EndsWith("python.exe", StringComparison.OrdinalIgnoreCase);

                    var args = new List<string>();

                    if (isPython)
                    {
                        var cliScript = LocatePythonCliScript();
                        if (cliScript == null)
                        {
                            Dispatcher.Invoke(() => AppendLog("Could not locate pdf_label_sorter cli.py script."));
                            return;
                        }

                        args.Add(Quote(cliScript));
                    }

                    args.Add("--platform");
                    args.Add(platform);
                    args.Add("--guide");
                    args.Add(Quote(guidePath));
                    args.Add("--output");
                    args.Add(Quote(outputPath));
                    args.Add("--sources");
                    foreach (var s in sources)
                    {
                        args.Add(Quote(s));
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = toolPath,
                        Arguments = string.Join(" ", args),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                    };

                    var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
                    _runningProcess = p;

                    p.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            Dispatcher.Invoke(() => AppendLog(e.Data));
                        }
                    };

                    p.ErrorDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            Dispatcher.Invoke(() => AppendLog(e.Data));
                        }
                    };

                    Dispatcher.Invoke(() => AppendLog($"Running: {psi.FileName} {psi.Arguments}"));

                    if (!p.Start())
                    {
                        Dispatcher.Invoke(() => AppendLog("Failed to start process."));
                        return;
                    }

                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();

                    Dispatcher.Invoke(() => AppendLog($"Exit code: {p.ExitCode}"));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"Error: {ex.Message}"));
                }
                finally
                {
                    _runningProcess?.Dispose();
                    _runningProcess = null;
                }
            });
        }

        private string LocatePythonCliScript()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                var candidates = new[]
                {
                    Path.Combine(baseDir, "Tools", "pdf_label_sorter", "cli.py"),
                    Path.Combine(baseDir, "pdf_label_sorter", "cli.py"),
                    Path.Combine(baseDir, "cli.py"),
                };

                foreach (var c in candidates)
                {
                    if (File.Exists(c))
                    {
                        return c;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string Quote(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "\"\"";
            }

            if (value.Contains(' '))
            {
                return "\"" + value.Replace("\"", "\\\"") + "\"";
            }

            return value;
        }

        private void AppendLog(string line)
        {
            LogTextBox.AppendText(line + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }
    }
}
