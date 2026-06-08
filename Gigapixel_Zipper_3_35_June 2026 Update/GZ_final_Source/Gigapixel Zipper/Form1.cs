using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Gigapixel_Zipper
{
  public partial class Form1 : Form
  {
    [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] static extern bool   GetWindowRect(IntPtr hWnd, out RECT r);
    [DllImport("user32.dll")] static extern bool   SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] static extern bool   ShowWindow(IntPtr hWnd, int nCmd);
    const int SW_RESTORE = 9;
    [DllImport("user32.dll")] static extern bool   SetCursorPos(int x, int y);
    [DllImport("user32.dll")] static extern void   mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    // keybd_event: fires raw OS-level keystrokes regardless of message-queue focus
    [DllImport("user32.dll")] static extern void   keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct RECT { public int Left, Top, Right, Bottom; }
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP   = 0x0004;
    const uint KEYEVENTF_KEYUP      = 0x0002;
    const byte VK_CONTROL           = 0x11;
    const byte VK_A                 = 0x41;
    const byte VK_S                 = 0x53;
    const byte VK_RETURN            = 0x0D;

    // ── Tab 1 state ──
    private static string zip_location = "", gigapixel_location = "", ignored_size = "", ignored_words = "";
    private static int batch_size = 30;
    private Thread _myThread; private bool is_working = false;
    private List<FileZipInfo> _aFileZipInfo = new List<FileZipInfo>();

    // ── Tab 2 state ──
    private static string p2_zip_location = "", p2_app_location = "", p2_ignored_size = "", p2_ignored_words = "";
    private static int p2_batch_size = 30;
    private Thread _myThread2; private bool p2_is_working = false;
    private List<FileZipInfo> _p2FileZipInfo = new List<FileZipInfo>();

    private List<string> zip_ext = new List<string> { "zip" };
    private List<string> img_ext = new List<string> { "jpg", "jpeg", "png" };

    public Form1() { InitializeComponent(); this.Load += Form1_Load; this.FormClosing += Form1_FormClosing; }

    // ════════════════════════════════════════════════════
    //  ZIP helpers
    // ════════════════════════════════════════════════════

    static MemoryStream SanitizeZip(string path)
    {
      byte[] d = File.ReadAllBytes(path); int n = d.Length, i = 0;
      while (i < n - 4)
      {
        bool lf = d[i]==0x50&&d[i+1]==0x4B&&d[i+2]==0x03&&d[i+3]==0x04;
        bool cd = d[i]==0x50&&d[i+1]==0x4B&&d[i+2]==0x01&&d[i+3]==0x02;
        if (!lf && !cd) { i++; continue; }
        if (i + (lf?30:46) > n) break;
        int fl = lf ? d[i+26]|(d[i+27]<<8) : d[i+28]|(d[i+29]<<8);
        int el = lf ? d[i+28]|(d[i+29]<<8) : d[i+30]|(d[i+31]<<8);
        int cl = cd ? d[i+32]|(d[i+33]<<8) : 0;
        int fs = i + (lf?30:46); if (fs+fl > n) break;
        for (int j=fs; j<fs+fl; j++) if (d[j]<0x20||d[j]==0x7F) d[j]=(byte)'_';
        i = fs+fl+el+cl;
      }
      return new MemoryStream(d);
    }

    static void ExtractZipSafe(string zipPath, string destDir)
    {
      string root = Path.GetFullPath(destDir) + Path.DirectorySeparatorChar;
      using (var ms = SanitizeZip(zipPath))
      using (var za = new ZipArchive(ms, ZipArchiveMode.Read))
        foreach (var e in za.Entries)
        {
          if (string.IsNullOrEmpty(e.Name)) continue;
          string dest = Path.GetFullPath(Path.Combine(destDir, e.FullName));
          if (!dest.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;
          string dir = Path.GetDirectoryName(dest);
          if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
          using (var s = e.Open()) using (var f = File.Create(dest)) s.CopyTo(f);
        }
    }

    static void ZipDir(string sourceDir, string zipPath)
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
      ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Optimal, false);
    }

    // ════════════════════════════════════════════════════
    //  TAB 1 – Gigapixel AI (GUI automation, unchanged)
    // ════════════════════════════════════════════════════

    void button1_Click(object s, EventArgs e)
    { using (var d=new FolderBrowserDialog()) if(d.ShowDialog()==DialogResult.OK) textBoxZipLocation.Text=zip_location=d.SelectedPath; }

    void button2_Click(object s, EventArgs e)
    {
      if(is_working){MessageBox.Show("Task already running");return;}
      if(textBoxZipLocation.Text==""||textBoxGGigapixelLocation.Text==""||textBoxIgnWidthGiga.Text==""){MessageBox.Show("Enter required details first");return;}
      is_working=true; zip_location=textBoxZipLocation.Text; gigapixel_location=textBoxGGigapixelLocation.Text;
      ignored_size=textBoxIgnWidthGiga.Text.Trim()+"*"+textBoxIgnHeightGiga.Text.Trim(); ignored_words=textBoxIgnoredWorlds.Text; batch_size=Convert.ToInt32(numericUpDownBatchSize.Value);
      (_myThread=new Thread(main_thread){IsBackground=true}).Start();
    }

    void button3_Click(object s, EventArgs e)
    {
      try { _myThread.Abort(); foreach(var p in Process.GetProcessesByName("Topaz Gigapixel AI")) try{p.Kill();}catch{}
        try{Directory.Delete(Tmp("Temp"),true);}catch{} try{Directory.Delete(Tmp("Temp2"),true);}catch{}
        is_working=false; SetLbl(lblStatus,"Stopped"); } catch {}
    }

    void SetLbl(Label lbl, string text)
    {
      string stamped = DateTime.Now.ToString("HH:mm:ss") + "  " + text;
      if (lbl.InvokeRequired)
        lbl.Invoke(new Action(() => { lbl.Text = stamped; lbl.Refresh(); }));
      else
        { lbl.Text = stamped; lbl.Refresh(); }
    }

    void main_thread()
    {
      try
      {
        SetLbl(lblStatus,"Starting...");
        var rem=new List<string[]>(); var aIgn=ignored_words.Trim().Split(new[]{';'},StringSplitOptions.RemoveEmptyEntries);
        string td=Tmp("Temp"), td2=Tmp("Temp2"); Recreate(td); Recreate(td2); Mkdir(Tmp("Backup")); _aFileZipInfo.Clear();
        foreach(var z in ZipFiles(zip_location))
        { var info=MakeInfo(z,zip_location,_aFileZipInfo); DoBackup(z,info,Tmp("Backup")); ExtractZipSafe(z,Path.Combine(td,info.Id)); CollectImgs(td,td2,info,ignored_size,aIgn,rem); }
        int idx=0; string a="";
        foreach(var f in ImgFiles(td2).ToList()) { if(a!="") a+=" "; a+="\""+f+"\""; if(++idx%batch_size==0){GigaBatch(a);a="";} }
        if(a!="") GigaBatch(a);
        SetLbl(lblStatus,"PNG bit-depth.."); foreach(var f in PngFiles(td)) convert24dp(f);
        foreach(var c in rem) File.Copy(Path.Combine(td2,c[0]),Path.Combine(c[1],Path.GetFileName(c[0])),true);
        ComputeFinalTimestamps(_aFileZipInfo);  // sorts by (Date Modified, Creation Time)
        foreach(var oi in _aFileZipInfo)   // iterate SORTED LIST, not random GUID dirs
        { string _dp=Path.Combine(td,oi.Id); if(!Directory.Exists(_dp)) continue;
          SetLbl(lblStatus,"Zipping: "+oi.NameWithoutExtension);
          { string _dest=Path.Combine(zip_location,oi.PathWithoutRoot,oi.NameWithoutExtension+".zip");
          ZipDir(_dp,_dest);
          try{File.SetLastWriteTime(_dest,oi.FinalTimestamp);}catch{}
          try{File.SetCreationTime(_dest, oi.FinalTimestamp);}catch{} } }
        SetLbl(lblStatus,"Completed"); is_working=false;
      }
      catch(ThreadAbortException){is_working=false;}
      catch(Exception ex){is_working=false;SetLbl(lblStatus,"Error");MessageBox.Show("Gigapixel error:\n\n"+ex.Message+"\n"+ex.StackTrace,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }

    void GigaBatch(string args)
    {
      SetLbl(lblStatus,"Opening Gigapixel.."); foreach(var p in Process.GetProcessesByName("Topaz Gigapixel AI")) try{p.Kill();}catch{}
      var pr=new Process(); pr.StartInfo.FileName="cmd.exe"; pr.StartInfo.RedirectStandardInput=true;
      pr.StartInfo.RedirectStandardOutput=true; pr.StartInfo.CreateNoWindow=true; pr.StartInfo.UseShellExecute=false; pr.Start();
      pr.StandardInput.WriteLine("\""+gigapixel_location+"\" "+args);
      while(!HasWindow("Topaz Gigapixel AI")) Thread.Sleep(1000);
      Thread.Sleep(15000); SetLbl(lblStatus,"Saving..");
      this.Invoke(new Action(()=>SendKeys.SendWait("^(s)"))); Thread.Sleep(2000);
      SetLbl(lblStatus,"Waiting.."); Thread.Sleep(10000);
      while(HasWindow("Processing ")&&!HasWindow("Processing Complete")) Thread.Sleep(1000);
      SetLbl(lblStatus,"Closing Gigapixel.."); foreach(var p in Process.GetProcessesByName("Topaz Gigapixel AI")) try{p.Kill();}catch{}
    }

    // ════════════════════════════════════════════════════
    //  TAB 2 – Topaz Photo AI (GUI automation, bulk)
    //
    //  Key fix: Interaction.AppActivate(processId) is the .NET Framework's
    //  purpose-built function for activating a window before SendKeys.
    //  It uses a different internal mechanism than SetForegroundWindow and
    //  is specifically designed to work with SendKeys across process boundaries.
    //  We pass the PROCESS ID (not window title) to guarantee the right window.
    //  Launch method matches Gigapixel (cmd.exe stdin) for consistency.
    // ════════════════════════════════════════════════════

    void p2_button1_Click(object s, EventArgs e)
    { using(var d=new FolderBrowserDialog()) if(d.ShowDialog()==DialogResult.OK) p2_textBoxZipLocation.Text=p2_zip_location=d.SelectedPath; }

    void p2_button2_Click(object s, EventArgs e)
    {
      if(p2_is_working){MessageBox.Show("Task already running");return;}
      if(p2_textBoxZipLocation.Text==""||p2_textBoxAppLocation.Text==""||p2_textBoxIgnWidth.Text==""){MessageBox.Show("Enter required details first");return;}
      p2_is_working=true; p2_zip_location=p2_textBoxZipLocation.Text; p2_app_location=p2_textBoxAppLocation.Text;
      p2_ignored_size=p2_textBoxIgnWidth.Text.Trim()+"*"+p2_textBoxIgnHeight.Text.Trim(); p2_ignored_words=p2_textBoxIgnoredWords.Text; p2_batch_size=Convert.ToInt32(p2_numericUpDownBatchSize.Value);
      (_myThread2=new Thread(p2_main_thread){IsBackground=true}).Start();
    }

    void p2_button3_Click(object s, EventArgs e)
    {
      try { _myThread2.Abort(); KillPhotoAI();
        try{Directory.Delete(Tmp("Temp_PAI"),true);}catch{} try{Directory.Delete(Tmp("Temp2_PAI"),true);}catch{}
        p2_is_working=false; SetLbl(lblStatus,"Stopped"); } catch {}
    }

    void p2_main_thread()
    {
      try
      {
        SetLbl(lblStatus,"Starting...");
        var rem=new List<string[]>(); var aIgn=p2_ignored_words.Trim().Split(new[]{';'},StringSplitOptions.RemoveEmptyEntries);
        string td=Tmp("Temp_PAI"), td2=Tmp("Temp2_PAI"); Recreate(td); Recreate(td2); Mkdir(Tmp("Backup_PAI")); _p2FileZipInfo.Clear();
        foreach(var z in ZipFiles(p2_zip_location))
        { var info=MakeInfo(z,p2_zip_location,_p2FileZipInfo); DoBackup(z,info,Tmp("Backup_PAI")); ExtractZipSafe(z,Path.Combine(td,info.Id)); CollectImgs2(td,td2,info,p2_ignored_size,aIgn,rem); }
        int idx=0; string a="";
        foreach(var f in ImgFiles(td2).ToList()) { if(a!="") a+=" "; a+="\""+f+"\""; if(++idx%p2_batch_size==0){PhotoAIBatch(a);a="";} }
        if(a!="") PhotoAIBatch(a);
        SetLbl(lblStatus,"PNG bit-depth.."); foreach(var f in PngFiles(td)) convert24dp(f);
        foreach(var c in rem) File.Copy(Path.Combine(td2,c[0]),Path.Combine(c[1],Path.GetFileName(c[0])),true);
        ComputeFinalTimestamps(_p2FileZipInfo);
        foreach(var oi in _p2FileZipInfo)
        { string _dp=Path.Combine(td,oi.Id); if(!Directory.Exists(_dp)) continue;
          SetLbl(lblStatus,"Zipping: "+oi.NameWithoutExtension+".zip");
          { string _dest=Path.Combine(p2_zip_location,oi.PathWithoutRoot,oi.NameWithoutExtension+".zip");
          ZipDir(_dp,_dest);
          try{File.SetLastWriteTime(_dest,oi.FinalTimestamp);}catch{}
          try{File.SetCreationTime(_dest, oi.FinalTimestamp);}catch{} } }
        SetLbl(lblStatus,"Completed"); p2_is_working=false;
      }
      catch(ThreadAbortException){p2_is_working=false;}
      catch(Exception ex){p2_is_working=false;SetLbl(lblStatus,"Error");MessageBox.Show("Photo AI error:\n\n"+ex.Message+"\n"+ex.StackTrace,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }

    void PhotoAIBatch(string args)
    {
      string appName = Path.GetFileNameWithoutExtension(p2_app_location);
      SetLbl(lblStatus, "Launching " + appName + "...");
      KillPhotoAI();
      Thread.Sleep(5000);

      // ── Launch Photo AI, keep direct process reference ──────────
      Process photoProc = Process.Start(new ProcessStartInfo
      {
        FileName        = p2_app_location,
        Arguments       = args,
        UseShellExecute = true
      });

      if (photoProc == null)
      { SetLbl(lblStatus, "Failed to launch " + appName); return; }

      // ── Wait for main window using DIRECT process handle ─────────
      // photoProc.MainWindowHandle is the most reliable detection:
      // it checks the actual OS window handle, no title-string matching.
      // We call proc.Refresh() each iteration to get the latest handle.
      SetLbl(lblStatus, "Waiting for " + appName + " window to appear...");
      int waited = 0;
      IntPtr hwnd = IntPtr.Zero;
      while (waited < 120)
      {
        Thread.Sleep(1000); waited++;
        try
        {
          if (photoProc.HasExited) break;
          photoProc.Refresh();
          hwnd = photoProc.MainWindowHandle;
          if (hwnd != IntPtr.Zero) break;
        }
        catch { break; }
        SetLbl(lblStatus, "Waiting for " + appName + " window... (" + waited + "s)");
      }

      if (hwnd == IntPtr.Zero || photoProc.HasExited)
      {
        SetLbl(lblStatus, appName + " window not found – skipping batch");
        KillPhotoAI(); return;
      }
      SetLbl(lblStatus, appName + " window found. Loading images (12s)...");
      Thread.Sleep(12000);

      // ── Send Ctrl+A and Ctrl+S THREE times, 5-second intervals ──
      // Each attempt: fully activate the window then fire raw keys.
      // Three attempts guarantee at least one lands at the right moment
      // regardless of where Photo AI is in its load cycle.
      for (int pass = 1; pass <= 3; pass++)
      {
        SetLbl(lblStatus, "Keys pass " + pass + "/3 — activating " + appName + " window...");

        // Refresh handle in case window changed
        try { photoProc.Refresh(); hwnd = photoProc.MainWindowHandle; } catch {}
        ActivateWindow(hwnd);   // ShowWindow + SetForeground + mouse click

        Thread.Sleep(500);   // let focus settle fully

        SetLbl(lblStatus, "Keys pass " + pass + "/3 — Ctrl+A (select all)");
        RawCtrl(VK_A);
        Thread.Sleep(800);

        SetLbl(lblStatus, "Keys pass " + pass + "/3 — Ctrl+S (save)");
        RawCtrl(VK_S);
        Thread.Sleep(800);

        SetLbl(lblStatus, "Keys pass " + pass + "/3 — Enter (confirm dialog)");
        RawKey(VK_RETURN);

        if (pass < 3)
        {
          SetLbl(lblStatus, "Keys pass " + pass + "/3 sent. Waiting 5s before next pass...");
          Thread.Sleep(5000);
        }
      }

      // ── Monitor completion with CPU-idle detection ───────────────
      SetLbl(lblStatus, "All key passes done. Monitoring Photo AI completion...");
      bool crashed = WaitForIdleCPU(photoProc, maxMinutes: 120);
      Thread.Sleep(5000);

      if (crashed)
      {
        string report     = BuildCrashReport(photoProc, appName, args);
        string reportPath = Tmp("PhotoAI_CrashReport.txt");
        try { File.WriteAllText(reportPath, report); } catch { }
        SetLbl(lblStatus, "Photo AI crashed – crash report saved");
        this.Invoke(new Action(() =>
        {
          var dlg = new CrashReportDialog(report, reportPath);
          dlg.ShowDialog(this);
        }));
      }
      else
      {
        SetLbl(lblStatus, "Batch complete. Closing " + appName + "...");
        KillPhotoAI();
      }
    }

    bool ProcessExited(Process proc)
    {
      if (proc == null) return false;
      try { return proc.HasExited; } catch { return true; }
    }

    bool WaitForIdleCPU(Process proc, int maxMinutes)
    {
      Thread.Sleep(15000);
      int maxSec=maxMinutes*60, elapsed=0, idleSecs=0;
      int cores=Math.Max(1,Environment.ProcessorCount);
      DateTime lastT=DateTime.Now; TimeSpan lastCpu=TimeSpan.Zero;
      try { lastCpu=proc.TotalProcessorTime; } catch { return true; }
      while (elapsed<maxSec)
      {
        Thread.Sleep(3000); elapsed+=3;
        try
        {
          if (proc.HasExited)
          {
            // Already detected idling → normal completion, process just closed itself
            if (idleSecs > 0) return false;
            // Clean exit code → Photo AI finished and closed itself normally
            try { if (proc.ExitCode == 0) return false; } catch {}
            // Exited with non-zero code while still busy → genuine crash
            return true;
          }
        }
        catch { return true; }
        try
        {
          TimeSpan nowCpu=proc.TotalProcessorTime;
          double wall=(DateTime.Now-lastT).TotalSeconds;
          double pct=wall>0?(nowCpu-lastCpu).TotalSeconds/wall/cores*100.0:0;
          lastCpu=nowCpu; lastT=DateTime.Now;
          if(pct<5.0) idleSecs+=3; else idleSecs=0;
          if(idleSecs>=20) break;
          SetLbl(lblStatus,string.Format("Photo AI processing... {0}m {1:D2}s  (CPU {2:F0}%)",
                                         elapsed/60, elapsed%60, pct));
        }
        catch { break; }
      }
      return false;
    }

    /// <summary>
    /// Activates a window using its handle directly — no name/title matching.
    /// Three-step: ShowWindow (restore if minimised) → SetForegroundWindow
    /// (bring to front) → simulated mouse click at window centre (gives
    /// keyboard focus to the content controls, not just the window chrome).
    /// The mouse click travels through Windows' raw-input stack and cannot
    /// be blocked by the foreground-lock mechanism.
    /// </summary>
    void ActivateWindow(IntPtr hwnd)
    {
      if (hwnd == IntPtr.Zero) return;
      try
      {
        ShowWindow(hwnd, SW_RESTORE);      // un-minimise if needed
        SetForegroundWindow(hwnd);         // bring to front
        Thread.Sleep(200);                 // let OS process the switch

        // Click at window centre — gives focus to interactive content
        RECT rect;
        if (GetWindowRect(hwnd, out rect))
        {
          int cx = (rect.Left + rect.Right) / 2;
          int cy = (rect.Top  + rect.Bottom) / 2;
          SetCursorPos(cx, cy);
          Thread.Sleep(80);
          mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
          Thread.Sleep(80);
          mouse_event(MOUSEEVENTF_LEFTUP,   0, 0, 0, UIntPtr.Zero);
          Thread.Sleep(200);
        }
      }
      catch { }
    }

    void ActivateAndSend(string processName, string keys)
    {
      this.Invoke(new Action(() =>
      {
        // ── 1. Locate the process and its window ──────────────────────
        Process target = Process.GetProcessesByName(processName).FirstOrDefault();
        if (target == null) return;
        IntPtr hwnd = target.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;

        // ── 2. AppActivate: restore + bring to foreground ─────────────
        try { Interaction.AppActivate(target.Id); } catch {}
        Thread.Sleep(300);

        // ── 3. Simulated left-click at window CENTRE content area ─────
        SetLbl(lblStatus, DateTime.Now.ToString("HH:mm:ss") + "  [CLICK] Activating " + processName + "...");
        RECT rect;
        if (GetWindowRect(hwnd, out rect))
        {
          int cx = (rect.Left + rect.Right) / 2;
          int cy = (rect.Top  + rect.Bottom) / 2;   // true centre of window
          SetCursorPos(cx, cy);
          Thread.Sleep(80);
          mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
          Thread.Sleep(80);
          mouse_event(MOUSEEVENTF_LEFTUP,   0, 0, 0, UIntPtr.Zero);
          Thread.Sleep(600);   // let focus settle completely
        }

        // ── 4. Fire keystroke via keybd_event (OS raw-input level) ────
        FireRawKey(keys);
      }));
    }

    /// <summary>Translates a SendKeys-style string to keybd_event calls.</summary>
    void FireRawKey(string keys)
    {
      switch (keys)
      {
        case "^a": case "^(a)":
          RawCtrl(VK_A); break;
        case "^s": case "^(s)":
          RawCtrl(VK_S); break;
        case "{ENTER}": case "~":
          RawKey(VK_RETURN); break;
        default:
          SendKeys.SendWait(keys); break;   // fallback for any other key string
      }
    }

    /// <summary>Fires Ctrl+key via keybd_event at OS level.</summary>
    void RawCtrl(byte vKey)
    {
      keybd_event(VK_CONTROL, 0, 0,              UIntPtr.Zero);   // Ctrl down
      Thread.Sleep(15);
      keybd_event(vKey,       0, 0,              UIntPtr.Zero);   // key down
      Thread.Sleep(15);
      keybd_event(vKey,       0, KEYEVENTF_KEYUP, UIntPtr.Zero);  // key up
      Thread.Sleep(15);
      keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);  // Ctrl up
    }

    /// <summary>Fires a single key via keybd_event at OS level.</summary>
    void RawKey(byte vKey)
    {
      keybd_event(vKey, 0, 0,              UIntPtr.Zero);
      Thread.Sleep(15);
      keybd_event(vKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }


    // ── Tab switching ────────────────────────────────────────────────
    void btnTabGiga_Click(object s, EventArgs e)
    {
      pnlContentGiga.Visible  = true;
      pnlContentPhoto.Visible = false;
      btnTabGiga.BackColor  = System.Drawing.Color.White;
      btnTabGiga.ForeColor  = System.Drawing.Color.FromArgb(30, 64, 175);
      btnTabPhoto.BackColor = System.Drawing.Color.FromArgb(48, 92, 206);
      btnTabPhoto.ForeColor = System.Drawing.Color.White;
      SetLbl(lblStatus, "Ready");
    }
    void btnTabPhoto_Click(object s, EventArgs e)
    {
      pnlContentGiga.Visible  = false;
      pnlContentPhoto.Visible = true;
      btnTabPhoto.BackColor = System.Drawing.Color.White;
      btnTabPhoto.ForeColor = System.Drawing.Color.FromArgb(30, 64, 175);
      btnTabGiga.BackColor  = System.Drawing.Color.FromArgb(48, 92, 206);
      btnTabGiga.ForeColor  = System.Drawing.Color.White;
      SetLbl(lblStatus, "Ready");
    }
    void KillPhotoAI()
    {
      string name = Path.GetFileNameWithoutExtension(p2_app_location);
      foreach (var p in Process.GetProcessesByName(name)) try { p.Kill(); } catch { }
    }

    // ════════════════════════════════════════════════════
    //  Shared helpers
    // ════════════════════════════════════════════════════

    string Tmp(string sub) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sub);
    void Recreate(string d) { if(Directory.Exists(d)) Directory.Delete(d,true); Directory.CreateDirectory(d); }
    void Mkdir(string d) { if(!Directory.Exists(d)) Directory.CreateDirectory(d); }
    IEnumerable<string> ZipFiles(string r) => Directory.EnumerateFiles(r,"*.*",SearchOption.AllDirectories).Where(s=>zip_ext.Contains(Ext(s)));
    IEnumerable<string> ImgFiles(string r) => Directory.EnumerateFiles(r,"*.*",SearchOption.AllDirectories).Where(s=>img_ext.Contains(Ext(s)));
    IEnumerable<string> PngFiles(string r) => Directory.EnumerateFiles(r,"*.*",SearchOption.AllDirectories).Where(s=>Ext(s)=="png");
    string Ext(string p) => Path.GetExtension(p).TrimStart('.').ToLowerInvariant();
    bool HasWindow(string title) { foreach(var p in Process.GetProcesses()) try{if(!string.IsNullOrEmpty(p.MainWindowTitle)&&p.MainWindowTitle.Contains(title)) return true;}catch{} return false; }

    FileZipInfo MakeInfo(string zipPath, string rootDir, List<FileZipInfo> list)
    {
      string rel = Path.GetDirectoryName(zipPath).Substring(rootDir.Length).Trim('\\');
      var info = new FileZipInfo{
        Id=Guid.NewGuid().ToString("D"), FullPath=zipPath, Name=Path.GetFileName(zipPath),
        NameWithoutExtension=Path.GetFileNameWithoutExtension(zipPath), PathWithoutRoot=rel,
        // Capture original timestamps before ANY processing touches this file
        OriginalLastWriteTime = File.GetLastWriteTime(zipPath),
        OriginalCreationTime  = File.GetCreationTime(zipPath)
      };
      list.Add(info); return info;
    }

    void DoBackup(string zipPath, FileZipInfo info, string backupRoot)
    { string bp=Path.Combine(backupRoot,info.PathWithoutRoot); Mkdir(bp); File.Copy(zipPath,Path.Combine(bp,info.Name),true); }

    void CollectImgs(string tempDir, string stageDir, FileZipInfo info, string ignoredSz, string[] ignWords, List<string[]> remember)
    {
      string tz=Path.Combine(tempDir,info.Id);
      foreach(string img in ImgFiles(tz))
      {
        using(Image image=Image.FromFile(img))
        {
          int skip=0; string[] sp=ignoredSz.Split('*');
          if(sp.Length==2&&image.Width.ToString()==sp[0]&&image.Height.ToString()==sp[1]) skip=1;
          if(skip==0) { string fn=Path.GetFileNameWithoutExtension(img).ToLowerInvariant(); foreach(string w in ignWords) if(fn.Contains(w.Trim())){skip=1;break;} }
          if(skip==0)
          { string ld=new DirectoryInfo(Path.GetDirectoryName(img)).Name; string t2=Path.Combine(stageDir,ld); Mkdir(t2);
            string nm=Path.GetFileName(img); remember.Add(new[]{Path.Combine(ld,nm),tz}); File.Copy(img,Path.Combine(t2,nm),true); }
        }
      }
    }

    void CollectImgs2(string tempDir, string stageDir, FileZipInfo info, string ignoredSz, string[] ignWords, List<string[]> remember)
    {
      string tz=Path.Combine(tempDir,info.Id);
      foreach(string img in ImgFiles(tz))
      {
        using(Image image=Image.FromFile(img))
        {
          int skip=0; string[] sp=ignoredSz.Split('*');
          if(sp.Length==2&&image.Width.ToString()==sp[0]&&image.Height.ToString()==sp[1]) skip=1;
          if(skip==0) { string fn=Path.GetFileNameWithoutExtension(img).ToLowerInvariant(); foreach(string w in ignWords) if(fn.Contains(w.Trim())){skip=1;break;} }
          if(skip==0)
          { string ld=new DirectoryInfo(Path.GetDirectoryName(img)).Name; string t2=Path.Combine(stageDir,ld); Mkdir(t2);
            string nm=Path.GetFileName(img); remember.Add(new[]{Path.Combine(ld,nm),tz}); File.Copy(img,Path.Combine(t2,nm),true); }
        }
      }
    }

    void convert24dp(string path)
    {
      try
      { string tmp=Tmp("temp.bin"); File.Copy(path,tmp,true);
        using(var src=new Bitmap(tmp)) using(var dst=new Bitmap(src.Width,src.Height,PixelFormat.Format24bppRgb)) using(var g=Graphics.FromImage(dst))
        { g.DrawImage(src,0,0,src.Width,src.Height); dst.Save(path,ImageFormat.Png); } } catch {}
    }

    // ════════════════════════════════════════════════════
    //  Settings
    // ════════════════════════════════════════════════════


    // ── Zip-location file persistence ───────────────────────────────
    // We use a plain text file next to the exe instead of .NET Settings
    // because Settings reset on every build due to assembly-version changes.
    static string PathsCfg =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_paths.cfg");

    void SaveLastPaths()
    {
      try
      {
        File.WriteAllLines(PathsCfg, new[]
        {
          textBoxZipLocation.Text      ?? "",
          p2_textBoxZipLocation.Text   ?? ""
        });
      }
      catch { }
    }

    void LoadLastPaths()
    {
      try
      {
        if (!File.Exists(PathsCfg)) return;
        string[] lines = File.ReadAllLines(PathsCfg);
        if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
          textBoxZipLocation.Text    = lines[0].Trim();
        if (lines.Length > 1 && !string.IsNullOrWhiteSpace(lines[1]))
          p2_textBoxZipLocation.Text = lines[1].Trim();
      }
      catch { }
    }

    /// <summary>
    /// Assigns a unique FinalTimestamp to every FileZipInfo while preserving
    /// the exact sequence the files were in before processing.
    ///
    /// How it works:
    ///   • The list is NOT re-sorted — it stays in the original discovery order
    ///     (the order Directory.EnumerateFiles found them, which matches
    ///     Windows Explorer's file order for that folder).
    ///   • Files with a unique minute-level timestamp keep their exact original
    ///     timestamp — nothing changes for them.
    ///   • Files that share the same year/month/day/hour/minute get 1-second
    ///     offsets based on their position in the original list:
    ///       first occurrence  → :00  (original time, unchanged)
    ///       second occurrence → :01
    ///       third occurrence  → :02  … and so on.
    ///
    /// Result: after processing, the designer sees exactly the same file
    /// sequence as before, with unique timestamps for any same-minute group.
    /// </summary>
    /// <summary>
    /// Sorts the list by (Date Modified minute, then NTFS Creation Time)
    /// and assigns unique FinalTimestamps.
    ///
    /// WHY CREATION TIME WORKS:
    ///   NTFS stores creation time with 100-nanosecond precision even though
    ///   Explorer only displays minutes.  Files downloaded or created in the
    ///   same minute have slightly different creation times that reflect the
    ///   true original sequence (download order). Sorting within same-minute
    ///   groups by OriginalCreationTime reproduces the original Explorer order.
    ///
    /// WHY WE FIX THE LOOP ORDER TOO:
    ///   The old zipping loop iterated GetDirectories() which returns temp dirs
    ///   sorted by their random GUID names — so timestamps were assigned in a
    ///   completely random order each run.  The loop now iterates _aFileZipInfo
    ///   directly (after this sort), so assignment order matches creation order.
    /// </summary>
    void ComputeFinalTimestamps(System.Collections.Generic.List<FileZipInfo> list)
    {
      // ── Step 1: sort to reproduce Windows Explorer's exact ordering ──
      //
      // Explorer sorts "Date Modified" column using:
      //   Primary   : OriginalLastWriteTime  (full timestamp including seconds)
      //   Tiebreaker: OriginalCreationTime   (file creation / download time)
      //
      // Sorting by both reproduces whatever sequence the user saw before
      // processing, regardless of filename.
      list.Sort((a, b) =>
      {
        int cmp = a.OriginalLastWriteTime.CompareTo(b.OriginalLastWriteTime);
        return cmp != 0 ? cmp
             : a.OriginalCreationTime.CompareTo(b.OriginalCreationTime);
      });

      // ── Step 2: assign unique second-level timestamps ────────────────
      //
      // KEY FIX: ALWAYS use minuteFloor + offset for EVERY file, including
      // the first one (offset = 0).
      //
      // The previous code used `orig` when offset == 0, which kept the
      // original seconds value (e.g. :35).  But later files got
      // minuteFloor + 1 = :01, minuteFloor + 2 = :02 …  This made the
      // "first" file (:35) sort AFTER all the others (:01, :02 …) in
      // Explorer, completely reversing the intended order.
      //
      // Using minuteFloor + offset for all files gives :00, :01, :02 …
      // in strict sorted order — every time, without exception.
      for (int i = 0; i < list.Count; i++)
      {
        DateTime orig = list[i].OriginalLastWriteTime;

        // Count earlier entries (lower index = earlier in sorted sequence)
        // that share the same year/month/day/hour/minute.
        int offset = 0;
        for (int j = 0; j < i; j++)
        {
          DateTime p = list[j].OriginalLastWriteTime;
          if (p.Year  == orig.Year  && p.Month == orig.Month &&
              p.Day   == orig.Day   && p.Hour  == orig.Hour  &&
              p.Minute == orig.Minute)
            offset++;
        }

        // minuteFloor + offset gives :00, :01, :02 … for all files,
        // keeping the sorted order intact with unique timestamps.
        DateTime floor = new DateTime(orig.Year, orig.Month, orig.Day,
                                      orig.Hour, orig.Minute, 0);
        list[i].FinalTimestamp = floor.AddSeconds(offset);
      }
    }


    void Form1_Load(object s, EventArgs e)
    {
      try
      {
        TryLoad(textBoxZipLocation,        Properties.Settings.Default.ZipLocation);
        TryLoad(textBoxGGigapixelLocation,  Properties.Settings.Default.GigapixelLocation);
        var _sz1raw = Properties.Settings.Default.IgnoredSize;
        var _sz1 = (string.IsNullOrEmpty(_sz1raw) ? "400*400" : _sz1raw).Split('*');
        if(_sz1.Length==2){ textBoxIgnWidthGiga.Text=_sz1[0]; textBoxIgnHeightGiga.Text=_sz1[1]; }
        else { textBoxIgnWidthGiga.Text="400"; textBoxIgnHeightGiga.Text="400"; }
        TryLoad(textBoxIgnoredWorlds, Properties.Settings.Default.IgnoredWords, "preview;screenshot_0");
        // IgnoredWords loaded above with default
        if(Properties.Settings.Default.BatchSize>0) numericUpDownBatchSize.Value=Clamp(Properties.Settings.Default.BatchSize,numericUpDownBatchSize.Minimum,numericUpDownBatchSize.Maximum);
        TryLoad(p2_textBoxZipLocation,     Properties.Settings.Default.PhotoAIZipLocation);
        TryLoad(p2_textBoxAppLocation,     Properties.Settings.Default.PhotoAIAppLocation);
        var _sz2raw = Properties.Settings.Default.PhotoAIIgnoredSize;
        var _sz2 = (string.IsNullOrEmpty(_sz2raw) ? "400*400" : _sz2raw).Split('*');
        if(_sz2.Length==2){ p2_textBoxIgnWidth.Text=_sz2[0]; p2_textBoxIgnHeight.Text=_sz2[1]; }
        else { p2_textBoxIgnWidth.Text="400"; p2_textBoxIgnHeight.Text="400"; }
        TryLoad(p2_textBoxIgnoredWords, Properties.Settings.Default.PhotoAIIgnoredWords, "preview;screenshot_0");
        // PhotoAI IgnoredWords loaded above with default
        if(Properties.Settings.Default.PhotoAIBatchSize>0) p2_numericUpDownBatchSize.Value=Clamp(Properties.Settings.Default.PhotoAIBatchSize,p2_numericUpDownBatchSize.Minimum,p2_numericUpDownBatchSize.Maximum);
      } catch {}
    }

    void TryLoad(TextBox tb, string val, string def="") { tb.Text = string.IsNullOrWhiteSpace(val) ? def : val; }

    void Form1_FormClosing(object s, FormClosingEventArgs e)
    {
      SaveLastPaths();   // always save paths to file first
      try
      {
        Properties.Settings.Default.ZipLocation        = textBoxZipLocation.Text?.Trim();
        Properties.Settings.Default.GigapixelLocation   = textBoxGGigapixelLocation.Text?.Trim();
        Properties.Settings.Default.IgnoredSize = textBoxIgnWidthGiga.Text.Trim()+"*"+textBoxIgnHeightGiga.Text.Trim();
        Properties.Settings.Default.IgnoredWords       = textBoxIgnoredWorlds.Text?.Trim();
        Properties.Settings.Default.BatchSize          = Convert.ToInt32(numericUpDownBatchSize.Value);
        Properties.Settings.Default.PhotoAIZipLocation  = p2_textBoxZipLocation.Text?.Trim();
        Properties.Settings.Default.PhotoAIAppLocation   = p2_textBoxAppLocation.Text?.Trim();
        Properties.Settings.Default.PhotoAIIgnoredSize = p2_textBoxIgnWidth.Text.Trim()+"*"+p2_textBoxIgnHeight.Text.Trim();
        Properties.Settings.Default.PhotoAIIgnoredWords = p2_textBoxIgnoredWords.Text?.Trim();
        Properties.Settings.Default.PhotoAIBatchSize   = Convert.ToInt32(p2_numericUpDownBatchSize.Value);
        Properties.Settings.Default.Save();
      } catch {}
    }

    /// <summary>
    /// Collects everything useful about a Photo AI crash and returns it
    /// as a formatted string that is both shown to the user and saved to disk.
    /// </summary>
    string BuildCrashReport(Process proc, string appName, string batchArgs)
    {
      var sb = new System.Text.StringBuilder();
      sb.AppendLine("═══════════════════════════════════════════════════");
      sb.AppendLine("  TOPAZ PHOTO AI – CRASH REPORT");
      sb.AppendLine("  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
      sb.AppendLine("═══════════════════════════════════════════════════");
      sb.AppendLine();

      // ── Exit code ──────────────────────────────────────────────────
      int exitCode = -1;
      try { if (proc != null && proc.HasExited) exitCode = proc.ExitCode; } catch { }
      sb.AppendLine("EXIT CODE: " + exitCode);
      sb.AppendLine(ExplainExitCode(exitCode));
      sb.AppendLine();

      // ── Batch that caused the crash ────────────────────────────────
      sb.AppendLine("IMAGES IN FAILED BATCH:");
      foreach (string f in ParseFileArgs(batchArgs))
        sb.AppendLine("  • " + f);
      sb.AppendLine();

      // ── Photo AI's own log file ────────────────────────────────────
      string logExcerpt = ReadPhotoAILog();
      sb.AppendLine("PHOTO AI LOG (last 40 lines):");
      sb.AppendLine(string.IsNullOrEmpty(logExcerpt)
        ? "  (log file not found – see %AppData%\\Topaz Labs LLC\\Topaz Photo AI\\logs)"
        : logExcerpt);
      sb.AppendLine();

      // ── Windows Application Event Log ─────────────────────────────
      sb.AppendLine("WINDOWS EVENT LOG (last 3 crash entries):");
      sb.AppendLine(ReadWindowsEventLog(appName));
      sb.AppendLine();

      // ── Common causes and fixes ────────────────────────────────────
      sb.AppendLine("COMMON CAUSES AND FIXES:");
      sb.AppendLine();
      sb.AppendLine("  1. TOO MANY IMAGES PER BATCH (most common)");
      sb.AppendLine("     Photo AI runs out of GPU/CPU memory when given");
      sb.AppendLine("     too many large images at once.");
      sb.AppendLine("     → Reduce the Batch Size setting (try 5 or 10).");
      sb.AppendLine();
      sb.AppendLine("  2. INSUFFICIENT GPU MEMORY (VRAM)");
      sb.AppendLine("     Large images at high upscale factors need a lot of VRAM.");
      sb.AppendLine("     → Reduce batch size, or close other GPU-heavy apps.");
      sb.AppendLine("     → In Photo AI Settings > Preferences, lower the");
      sb.AppendLine("       'Maximum Memory Usage' slider.");
      sb.AppendLine();
      sb.AppendLine("  3. CORRUPTED IMAGE FILE IN THE BATCH");
      sb.AppendLine("     One bad image file can crash the whole batch.");
      sb.AppendLine("     → Check the images listed above for corruption.");
      sb.AppendLine("     → Try opening them individually in Photo AI.");
      sb.AppendLine();
      sb.AppendLine("  4. OUTDATED GPU DRIVER");
      sb.AppendLine("     → Update your GPU driver from NVIDIA/AMD/Intel site.");
      sb.AppendLine();
      sb.AppendLine("  5. OUTDATED TOPAZ PHOTO AI");
      sb.AppendLine("     → Check for updates inside Photo AI (Help > Check for Updates).");
      sb.AppendLine();
      sb.AppendLine("NOTE: Images processed before the crash ARE saved.");
      sb.AppendLine("      The output ZIP may be partial – check it for completeness.");
      sb.AppendLine();
      sb.AppendLine("═══════════════════════════════════════════════════");

      return sb.ToString();
    }

    string ExplainExitCode(int code)
    {
      switch (code)
      {
        case  0:   return "  (clean exit – Photo AI finished normally then closed itself)";
        case -1:   return "  (unknown – could not read exit code)";
        case  1:   return "  (general error – check the log below)";
        case unchecked((int)0xC0000005u): return "  (0xC0000005 – ACCESS VIOLATION: GPU/driver crash or corrupted image)";
        default:   return "  (exit code " + code + " – check the Windows Event Log entry below)";
      }
    }

    List<string> ParseFileArgs(string batchArgs)
    {
      var files = new List<string>();
      // args are like: "path1" "path2" – extract content between quotes
      int i = 0;
      while (i < batchArgs.Length)
      {
        if (batchArgs[i] == '"')
        {
          int end = batchArgs.IndexOf('"', i + 1);
          if (end < 0) break;
          files.Add(batchArgs.Substring(i + 1, end - i - 1));
          i = end + 1;
        }
        else i++;
      }
      return files;
    }

    string ReadPhotoAILog()
    {
      // Topaz Photo AI writes logs to %AppData%\Topaz Labs LLC\Topaz Photo AI\logs
      string[] roots = {
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
      };
      foreach (string root in roots)
      {
        string logDir = Path.Combine(root, "Topaz Labs LLC", "Topaz Photo AI", "logs");
        if (!Directory.Exists(logDir)) continue;
        // Pick the most recently modified log file
        string latest = Directory.GetFiles(logDir, "*.*")
                         .Where(f => f.EndsWith(".log") || f.EndsWith(".txt"))
                         .OrderByDescending(f => File.GetLastWriteTime(f))
                         .FirstOrDefault();
        if (latest == null) continue;
        try
        {
          // Read with shared access so Photo AI's lock doesn't block us
          using (var fs = new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          using (var sr = new StreamReader(fs))
          {
            string[] lines = sr.ReadToEnd()
                               .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int start = Math.Max(0, lines.Length - 40);
            return "  (from: " + latest + ")\n" +
                   string.Join("\n", lines.Skip(start).Select(l => "  " + l));
          }
        }
        catch { }
      }
      return null;
    }

    string ReadWindowsEventLog(string appName)
    {
      try
      {
        var sb = new System.Text.StringBuilder();
        using (var log = new System.Diagnostics.EventLog("Application"))
        {
          var crashes = log.Entries.Cast<System.Diagnostics.EventLogEntry>()
            .Where(e =>
              e.TimeGenerated > DateTime.Now.AddMinutes(-10) &&
              (e.Source == "Application Error" || e.Source == "Windows Error Reporting") &&
              e.Message.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(e => e.TimeGenerated)
            .Take(3);

          foreach (var entry in crashes)
          {
            sb.AppendLine("  Time : " + entry.TimeGenerated.ToString("HH:mm:ss"));
            sb.AppendLine("  Event: " + entry.InstanceId);
            // Show first 400 chars of message to keep it readable
            string msg = entry.Message.Replace("\r\n", " ").Replace("\n", " ");
            if (msg.Length > 400) msg = msg.Substring(0, 400) + "...";
            sb.AppendLine("  Msg  : " + msg);
            sb.AppendLine();
          }
        }
        return sb.Length > 0 ? sb.ToString() : "  (no crash entries found in the last 10 minutes)";
      }
      catch (Exception ex)
      {
        return "  (could not read event log: " + ex.Message + ")";
      }
    }

    decimal Clamp(decimal v,decimal mn,decimal mx) => v<mn?mn:v>mx?mx:v;
  }
}
