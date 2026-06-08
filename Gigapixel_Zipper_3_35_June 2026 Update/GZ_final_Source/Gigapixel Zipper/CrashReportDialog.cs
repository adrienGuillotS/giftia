using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gigapixel_Zipper
{
  /// <summary>
  /// Scrollable crash report dialog that shows the full diagnostic report,
  /// lets the user copy it to clipboard, and opens the saved .txt file.
  /// </summary>
  public class CrashReportDialog : Form
  {
    private RichTextBox _rtb;
    private Button _btnCopy, _btnOpenFile, _btnClose;
    private string _reportPath;

    public CrashReportDialog(string reportText, string reportPath)
    {
      _reportPath = reportPath;

      // ── Form ─────────────────────────────────────────────────────
      this.Text            = "Photo AI Crash Report";
      this.Width           = 680;
      this.Height          = 560;
      this.MinimumSize     = new System.Drawing.Size(480, 400);
      this.StartPosition   = FormStartPosition.CenterParent;
      this.FormBorderStyle = FormBorderStyle.Sizable;
      this.Icon            = System.Drawing.SystemIcons.Warning;

      // ── Text area ────────────────────────────────────────────────
      _rtb = new RichTextBox
      {
        Text      = reportText,
        ReadOnly  = true,
        Dock      = DockStyle.Fill,
        Font      = new System.Drawing.Font("Consolas", 8.5f),
        ScrollBars = RichTextBoxScrollBars.Both,
        WordWrap  = false,
        BackColor = System.Drawing.Color.WhiteSmoke
      };

      // ── Button panel ─────────────────────────────────────────────
      var panel = new Panel { Dock = DockStyle.Bottom, Height = 44 };

      _btnCopy = new Button
      { Text = "Copy to Clipboard", Width = 140, Height = 28,
        Left = 8, Top = 8 };
      _btnCopy.Click += (s, e) =>
      {
        Clipboard.SetText(_rtb.Text);
        _btnCopy.Text = "Copied ✓";
      };

      _btnOpenFile = new Button
      { Text = "Open Report File", Width = 130, Height = 28,
        Left = 156, Top = 8,
        Enabled = File.Exists(reportPath) };
      _btnOpenFile.Click += (s, e) =>
      {
        try { Process.Start("notepad.exe", "\"" + reportPath + "\""); }
        catch { }
      };

      _btnClose = new Button
      { Text = "Close", Width = 90, Height = 28,
        Left = 294, Top = 8, DialogResult = DialogResult.OK };

      panel.Controls.AddRange(new Control[] { _btnCopy, _btnOpenFile, _btnClose });

      this.Controls.Add(_rtb);
      this.Controls.Add(panel);
      this.AcceptButton = _btnClose;
    }
  }
}
