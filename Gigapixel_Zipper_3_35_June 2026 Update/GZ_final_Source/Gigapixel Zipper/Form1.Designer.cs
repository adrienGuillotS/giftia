namespace Gigapixel_Zipper
{
  partial class Form1
  {
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
      // ── Instantiate all controls ─────────────────────────────────────
      pnlHeader       = new System.Windows.Forms.Panel();
      pnlAppIcon      = new System.Windows.Forms.Panel();
      lblAppChar      = new System.Windows.Forms.Label();
      label3          = new System.Windows.Forms.Label();
      label4          = new System.Windows.Forms.Label();
      btnTabGiga      = new System.Windows.Forms.Button();
      btnTabPhoto     = new System.Windows.Forms.Button();
      pictureBox1     = new System.Windows.Forms.PictureBox();

      pnlBg           = new System.Windows.Forms.Panel();
      pnlCard         = new System.Windows.Forms.Panel();
      pnlContentGiga  = new System.Windows.Forms.Panel();
      pnlContentPhoto = new System.Windows.Forms.Panel();

      pnlBottom       = new System.Windows.Forms.Panel();
      lblStatusDot    = new System.Windows.Forms.Label();
      lblStatus       = new System.Windows.Forms.Label();

      // Tab 1 icons (small coloured panels)
      iconZip1 = MkIcon(); iconApp1 = MkIcon(); iconFilter1 = MkIcon(); iconBatch1 = MkIcon();
      lblIconZip1    = MkIconChar("📂"); lblIconApp1  = MkIconChar("⚙");
      lblIconFilter1 = MkIconChar("▽"); lblIconBatch1 = MkIconChar("☰");

      // Tab 1 functional controls
      label1                   = new System.Windows.Forms.Label();
      textBoxZipLocation       = new System.Windows.Forms.TextBox();
      button1                  = new System.Windows.Forms.Button();
      label5                   = new System.Windows.Forms.Label();
      label2                   = new System.Windows.Forms.Label();
      textBoxGGigapixelLocation = new System.Windows.Forms.TextBox();
      label6                   = new System.Windows.Forms.Label();
      label9                   = new System.Windows.Forms.Label();
      label7                   = new System.Windows.Forms.Label();
      textBoxIgnoredSize       = new System.Windows.Forms.TextBox();   // hidden legacy
      textBoxIgnWidthGiga      = new System.Windows.Forms.TextBox();
      lblIgnXGiga              = new System.Windows.Forms.Label();
      textBoxIgnHeightGiga     = new System.Windows.Forms.TextBox();
      lblIgnPxGiga             = new System.Windows.Forms.Label();
      lblIgnDotGiga            = new System.Windows.Forms.Label();
      label10                  = new System.Windows.Forms.Label();
      label8                   = new System.Windows.Forms.Label();
      textBoxIgnoredWorlds     = new System.Windows.Forms.TextBox();
      labelBatchSize           = new System.Windows.Forms.Label();
      numericUpDownBatchSize   = new System.Windows.Forms.NumericUpDown();
      label1BatchSizeInfo      = new System.Windows.Forms.Label();
      button2                  = new System.Windows.Forms.Button();
      button3                  = new System.Windows.Forms.Button();
      div1Zip = MkDiv(); div1App = MkDiv(); div1Filter = MkDiv(); div1Batch = MkDiv();

      // Tab 2 icons
      p2_iconZip = MkIcon(); p2_iconApp = MkIcon(); p2_iconFilter = MkIcon(); p2_iconBatch = MkIcon();
      p2_lblIconZip    = MkIconChar("📂"); p2_lblIconApp  = MkIconChar("⚙");
      p2_lblIconFilter = MkIconChar("▽"); p2_lblIconBatch = MkIconChar("☰");

      // Tab 2 functional controls
      p2_label1                  = new System.Windows.Forms.Label();
      p2_textBoxZipLocation      = new System.Windows.Forms.TextBox();
      p2_button1                 = new System.Windows.Forms.Button();
      p2_label5                  = new System.Windows.Forms.Label();
      p2_label2                  = new System.Windows.Forms.Label();
      p2_textBoxAppLocation      = new System.Windows.Forms.TextBox();
      p2_label6                  = new System.Windows.Forms.Label();
      p2_label9                  = new System.Windows.Forms.Label();
      p2_label7                  = new System.Windows.Forms.Label();
      p2_textBoxIgnoredSize      = new System.Windows.Forms.TextBox();   // hidden legacy
      p2_textBoxIgnWidth         = new System.Windows.Forms.TextBox();
      p2_lblIgnX                 = new System.Windows.Forms.Label();
      p2_textBoxIgnHeight        = new System.Windows.Forms.TextBox();
      p2_lblIgnPx                = new System.Windows.Forms.Label();
      p2_lblIgnDot               = new System.Windows.Forms.Label();
      p2_label10                 = new System.Windows.Forms.Label();
      p2_label8                  = new System.Windows.Forms.Label();
      p2_textBoxIgnoredWords     = new System.Windows.Forms.TextBox();
      p2_labelBatchSize          = new System.Windows.Forms.Label();
      p2_numericUpDownBatchSize  = new System.Windows.Forms.NumericUpDown();
      p2_label1BatchSizeInfo     = new System.Windows.Forms.Label();
      p2_button2                 = new System.Windows.Forms.Button();
      p2_button3                 = new System.Windows.Forms.Button();
      p2_lblStatus               = new System.Windows.Forms.Label();
      p2_div1Zip = MkDiv(); p2_div1App = MkDiv(); p2_div1Filter = MkDiv(); p2_div1Batch = MkDiv();

      ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
      ((System.ComponentModel.ISupportInitialize)numericUpDownBatchSize).BeginInit();
      ((System.ComponentModel.ISupportInitialize)p2_numericUpDownBatchSize).BeginInit();
      pnlHeader.SuspendLayout(); pnlBg.SuspendLayout(); pnlCard.SuspendLayout();
      pnlContentGiga.SuspendLayout(); pnlContentPhoto.SuspendLayout(); pnlBottom.SuspendLayout();
      this.SuspendLayout();

      // ── Colours & fonts ──────────────────────────────────────────────
      var clrBlue    = Clr(30, 64, 175);    // #1E40AF  header / active
      var clrBlueTab = Clr(48, 92, 206);    // inactive tab bg
      var clrAccent  = Clr(37, 99, 235);    // #2563EB  icons / dividers accent
      var clrIconBg  = Clr(219, 234, 254);  // #DBEAFE  icon bg
      var clrGreen   = Clr(22, 163, 74);    // #16A34A
      var clrRed     = Clr(220, 38, 38);    // #DC2626
      var clrBg      = Clr(240, 243, 248);  // form background
      var clrDiv2    = Clr(226, 232, 240);  // #E2E8F0
      var clrMuted   = Clr(100, 116, 139);  // #64748B
      var clrHint    = Clr(148, 163, 184);  // #94A3B8
      var clrText    = Clr(15, 23, 42);     // #0F172A
      var fntSec     = Fnt("Segoe UI", 8f, B);
      var fntUi      = Fnt("Segoe UI", 9.25f);
      var fntHint    = Fnt("Segoe UI", 8f);
      var fntBtn     = Fnt("Segoe UI", 9.5f, B);
      var fntBrowse  = Fnt("Segoe UI", 8.5f, B);

      // ── HEADER ───────────────────────────────────────────────────────
      pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
      pnlHeader.Height    = 82;
      pnlHeader.BackColor = clrBlue;
      pnlHeader.Name      = "pnlHeader";

      pictureBox1.Visible = false;   // resx cleared

      // App icon box
      pnlAppIcon.Location  = P(14, 14);
      pnlAppIcon.Size      = S(52, 52);
      pnlAppIcon.BackColor = Clr(48, 92, 206);
      pnlAppIcon.Name      = "pnlAppIcon";
      lblAppChar.Dock      = System.Windows.Forms.DockStyle.Fill;
      lblAppChar.Text      = "GZ";
      lblAppChar.Font      = Fnt("Segoe UI", 14f, B);
      lblAppChar.ForeColor = System.Drawing.Color.White;
      lblAppChar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      lblAppChar.Name      = "lblAppChar";
      pnlAppIcon.Controls.Add(lblAppChar);

      label3.AutoSize  = true;
      label3.Font      = Fnt("Segoe UI", 15f, B);
      label3.ForeColor = System.Drawing.Color.White;
      label3.Location  = P(74, 14);
      label3.Text      = "Gigapixel Zipper";

      label4.AutoSize  = true;
      label4.Font      = Fnt("Segoe UI", 8.5f);
      label4.ForeColor = Clr(180, 205, 245);
      label4.Location  = P(76, 44);
      label4.Text      = "Version 3.35  \u2022  Topaz AI Batch Processor";

      // Tab buttons in header
      MkTabBtn(btnTabGiga,  "btnTabGiga",  "Gigapixel AI", 316, 24, 120, 34,
                            System.Drawing.Color.White, clrBlue);
      btnTabGiga.Click += new System.EventHandler(this.btnTabGiga_Click);
      MkTabBtn(btnTabPhoto, "btnTabPhoto", "Photo AI",     444, 24, 108, 34,
                            clrBlueTab, System.Drawing.Color.White);
      btnTabPhoto.Click += new System.EventHandler(this.btnTabPhoto_Click);

      pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[]
        { pictureBox1, pnlAppIcon, label3, label4, btnTabGiga, btnTabPhoto });

      // ── BOTTOM STATUS BAR ───────────────────────────────────────────
      pnlBottom.Dock      = System.Windows.Forms.DockStyle.Bottom;
      pnlBottom.Height    = 32;
      pnlBottom.BackColor = System.Drawing.Color.White;
      pnlBottom.Name      = "pnlBottom";

      lblStatusDot.AutoSize  = true;
      lblStatusDot.Font      = Fnt("Segoe UI", 12f);
      lblStatusDot.ForeColor = clrGreen;
      lblStatusDot.Location  = P(14, 8);
      lblStatusDot.Text      = "\u25cf";

      lblStatus.AutoSize  = true;
      lblStatus.Font      = fntUi;
      lblStatus.ForeColor = clrMuted;
      lblStatus.Location  = P(32, 9);
      lblStatus.Text      = "Ready";


      pnlBottom.Controls.AddRange(new System.Windows.Forms.Control[]
        { lblStatusDot, lblStatus });

      // ── BACKGROUND + CARD ───────────────────────────────────────────
      pnlBg.Dock      = System.Windows.Forms.DockStyle.Fill;
      pnlBg.BackColor = clrBg;
      pnlBg.Name      = "pnlBg";
      pnlBg.Padding   = new System.Windows.Forms.Padding(12, 10, 12, 10);

      pnlCard.Dock      = System.Windows.Forms.DockStyle.Fill;
      pnlCard.BackColor = System.Drawing.Color.White;
      pnlCard.Name      = "pnlCard";

      // ── TAB 1 CONTENT PANEL ─────────────────────────────────────────
      pnlContentGiga.Dock      = System.Windows.Forms.DockStyle.Fill;
      pnlContentGiga.BackColor = System.Drawing.Color.White;
      pnlContentGiga.Name      = "pnlContentGiga";
      pnlContentGiga.Visible   = true;

      // Section 1: ZIP
      SetIcon(iconZip1, "iconZip1", lblIconZip1, clrIconBg, clrAccent, 16, 18);
      SectionLabel(label1, "label1", "ZIP FILES LOCATION", 46, 22, fntSec, clrAccent);
      SetDiv(div1Zip, "div1Zip", 16, 44, 502, clrDiv2);
      SetTB(textBoxZipLocation, "textBoxZipLocation", 16, 52, 400, 32, fntUi, clrText,
            "Select folder containing ZIP files\u2026");
      MkBtn(button1, "button1", "  Browse", 422, 52, 96, 32, clrAccent, fntBrowse);
      button1.Click += new System.EventHandler(this.button1_Click);
      HintLbl(label5, "label5", "Folder that contains all ZIP files to process", 16, 90, fntHint, clrHint);

      // Section 2: App
      SetIcon(iconApp1, "iconApp1", lblIconApp1, clrIconBg, clrAccent, 16, 112);
      SectionLabel(label2, "label2", "APPLICATION PATH", 46, 116, fntSec, clrAccent);
      SetDiv(div1App, "div1App", 16, 138, 502, clrDiv2);
      SetTBMulti(textBoxGGigapixelLocation, "textBoxGGigapixelLocation",
            "C:\\Program Files\\Topaz Labs LLC\\Topaz Gigapixel AI\\Topaz Gigapixel AI.exe",
            16, 146, 502, 46, fntUi, clrText);
      HintLbl(label6, "label6", "Only change if Gigapixel AI is installed in a different location.", 16, 198, fntHint, clrHint);

      // Section 3: Filter
      SetIcon(iconFilter1, "iconFilter1", lblIconFilter1, clrIconBg, clrAccent, 16, 220);
      SectionLabel(label9, "label9", "FILTER \u2014 SKIP IMAGES WHERE", 46, 224, fntSec, clrAccent);
      SetDiv(div1Filter, "div1Filter", 16, 246, 502, clrDiv2);
      // "Size:" label
      InlineLbl(label7, "label7", "Size:", 16, 256, fntUi, clrText);
      SetTB(textBoxIgnWidthGiga, "textBoxIgnWidthGiga", 54, 252, 62, 30, fntUi, clrText, "400");
      textBoxIgnWidthGiga.Text = "400";
      InlineLbl(lblIgnXGiga, "lblIgnXGiga", "\u00d7", 122, 256, fntUi, clrMuted);
      SetTB(textBoxIgnHeightGiga, "textBoxIgnHeightGiga", 136, 252, 62, 30, fntUi, clrText, "400");
      textBoxIgnHeightGiga.Text = "400";
      InlineLbl(lblIgnPxGiga, "lblIgnPxGiga", "px", 204, 256, fntUi, clrMuted);
      InlineLbl(lblIgnDotGiga, "lblIgnDotGiga", "\u2022", 228, 256, fntUi, clrMuted);
      InlineLbl(label10, "label10", "Name:", 244, 256, fntUi, clrText);
      SetTB(textBoxIgnoredWorlds, "textBoxIgnoredWorlds", 290, 252, 228, 30, fntUi, clrText, "preview");
      textBoxIgnoredWorlds.Text = "preview;screenshot_0";
      textBoxIgnoredSize.Visible = false;  // legacy hidden
      label8.Visible = false;

      // Section 4: Batch
      SetIcon(iconBatch1, "iconBatch1", lblIconBatch1, clrIconBg, clrAccent, 16, 296);
      SectionLabel(labelBatchSize, "labelBatchSize", "BATCH SIZE", 46, 300, fntSec, clrAccent);
      SetDiv(div1Batch, "div1Batch", 16, 322, 502, clrDiv2);
      numericUpDownBatchSize.Font      = fntUi;
      numericUpDownBatchSize.Location  = P(16, 330);
      numericUpDownBatchSize.Size      = S(72, 28);
      numericUpDownBatchSize.Minimum   = 1; numericUpDownBatchSize.Maximum = 999;
      numericUpDownBatchSize.Increment = 5; numericUpDownBatchSize.Value   = 50;
      HintLbl(label1BatchSizeInfo, "label1BatchSizeInfo",
              "images per launch.  Reduce if Gigapixel crashes (try 10\u201315).",
              96, 334, fntHint, clrHint);

      // Buttons
      MkBtn(button2, "button2", "\u25b6   Start Processing", 120, 376, 168, 38, clrGreen, fntBtn);
      button2.Click += new System.EventHandler(this.button2_Click);
      MkBtn(button3, "button3", "\u25a0   Stop", 298, 376, 110, 38, clrRed, fntBtn);
      button3.Click += new System.EventHandler(this.button3_Click);

      pnlContentGiga.Controls.AddRange(new System.Windows.Forms.Control[] {
        iconZip1, iconApp1, iconFilter1, iconBatch1,
        label1, label2, label9, labelBatchSize,
        div1Zip, div1App, div1Filter, div1Batch,
        textBoxZipLocation, button1, label5,
        textBoxGGigapixelLocation, label6,
        label7, textBoxIgnWidthGiga, lblIgnXGiga, textBoxIgnHeightGiga,
        lblIgnPxGiga, lblIgnDotGiga, label10, textBoxIgnoredWorlds,
        textBoxIgnoredSize, label8,
        numericUpDownBatchSize, label1BatchSizeInfo,
        button2, button3 });

      // ── TAB 2 CONTENT PANEL ─────────────────────────────────────────
      pnlContentPhoto.Dock      = System.Windows.Forms.DockStyle.Fill;
      pnlContentPhoto.BackColor = System.Drawing.Color.White;
      pnlContentPhoto.Name      = "pnlContentPhoto";
      pnlContentPhoto.Visible   = false;

      // Section 1
      SetIcon(p2_iconZip, "p2_iconZip", p2_lblIconZip, clrIconBg, clrAccent, 16, 18);
      SectionLabel(p2_label1, "p2_label1", "ZIP FILES LOCATION", 46, 22, fntSec, clrAccent);
      SetDiv(p2_div1Zip, "p2_div1Zip", 16, 44, 502, clrDiv2);
      SetTB(p2_textBoxZipLocation, "p2_textBoxZipLocation", 16, 52, 400, 32, fntUi, clrText,
            "Select folder containing ZIP files\u2026");
      MkBtn(p2_button1, "p2_button1", "  Browse", 422, 52, 96, 32, clrAccent, fntBrowse);
      p2_button1.Click += new System.EventHandler(this.p2_button1_Click);
      HintLbl(p2_label5, "p2_label5", "Folder that contains all ZIP files to process", 16, 90, fntHint, clrHint);

      // Section 2
      SetIcon(p2_iconApp, "p2_iconApp", p2_lblIconApp, clrIconBg, clrAccent, 16, 112);
      SectionLabel(p2_label2, "p2_label2", "APPLICATION PATH", 46, 116, fntSec, clrAccent);
      SetDiv(p2_div1App, "p2_div1App", 16, 138, 502, clrDiv2);
      SetTBMulti(p2_textBoxAppLocation, "p2_textBoxAppLocation",
            "C:\\Program Files\\Topaz Labs LLC\\Topaz Photo\\Topaz Photo.exe",
            16, 146, 502, 46, fntUi, clrText);
      HintLbl(p2_label6, "p2_label6", "Only change if Topaz Photo AI is installed in a different location.", 16, 198, fntHint, clrHint);

      // Section 3
      SetIcon(p2_iconFilter, "p2_iconFilter", p2_lblIconFilter, clrIconBg, clrAccent, 16, 220);
      SectionLabel(p2_label9, "p2_label9", "FILTER \u2014 SKIP IMAGES WHERE", 46, 224, fntSec, clrAccent);
      SetDiv(p2_div1Filter, "p2_div1Filter", 16, 246, 502, clrDiv2);
      InlineLbl(p2_label7, "p2_label7", "Size:", 16, 256, fntUi, clrText);
      SetTB(p2_textBoxIgnWidth, "p2_textBoxIgnWidth", 54, 252, 62, 30, fntUi, clrText, "400");
      p2_textBoxIgnWidth.Text = "400";
      InlineLbl(p2_lblIgnX, "p2_lblIgnX", "\u00d7", 122, 256, fntUi, clrMuted);
      SetTB(p2_textBoxIgnHeight, "p2_textBoxIgnHeight", 136, 252, 62, 30, fntUi, clrText, "400");
      p2_textBoxIgnHeight.Text = "400";
      InlineLbl(p2_lblIgnPx, "p2_lblIgnPx", "px", 204, 256, fntUi, clrMuted);
      InlineLbl(p2_lblIgnDot, "p2_lblIgnDot", "\u2022", 228, 256, fntUi, clrMuted);
      InlineLbl(p2_label10, "p2_label10", "Name:", 244, 256, fntUi, clrText);
      SetTB(p2_textBoxIgnoredWords, "p2_textBoxIgnoredWords", 290, 252, 228, 30, fntUi, clrText, "preview");
      p2_textBoxIgnoredWords.Text = "preview;screenshot_0";
      p2_textBoxIgnoredSize.Visible = false;
      p2_label8.Visible = false;

      // Section 4
      SetIcon(p2_iconBatch, "p2_iconBatch", p2_lblIconBatch, clrIconBg, clrAccent, 16, 296);
      SectionLabel(p2_labelBatchSize, "p2_labelBatchSize", "BATCH SIZE", 46, 300, fntSec, clrAccent);
      SetDiv(p2_div1Batch, "p2_div1Batch", 16, 322, 502, clrDiv2);
      p2_numericUpDownBatchSize.Font      = fntUi;
      p2_numericUpDownBatchSize.Location  = P(16, 330);
      p2_numericUpDownBatchSize.Size      = S(72, 28);
      p2_numericUpDownBatchSize.Minimum   = 1; p2_numericUpDownBatchSize.Maximum = 999;
      p2_numericUpDownBatchSize.Increment = 5; p2_numericUpDownBatchSize.Value   = 50;
      HintLbl(p2_label1BatchSizeInfo, "p2_label1BatchSizeInfo",
              "images per launch.  Start with 5.  Increase only if stable.",
              96, 334, fntHint, clrHint);
      MkBtn(p2_button2, "p2_button2", "\u25b6   Start Processing", 120, 376, 168, 38, clrGreen, fntBtn);
      p2_button2.Click += new System.EventHandler(this.p2_button2_Click);
      MkBtn(p2_button3, "p2_button3", "\u25a0   Stop", 298, 376, 110, 38, clrRed, fntBtn);
      p2_button3.Click += new System.EventHandler(this.p2_button3_Click);

      p2_lblStatus.AutoSize  = true;
      p2_lblStatus.Font      = fntUi;
      p2_lblStatus.ForeColor = clrMuted;
      p2_lblStatus.Location  = P(16, 422);
      p2_lblStatus.Text      = "Ready";
      p2_lblStatus.Visible   = false;  // status shown in shared lblStatus

      pnlContentPhoto.Controls.AddRange(new System.Windows.Forms.Control[] {
        p2_iconZip, p2_iconApp, p2_iconFilter, p2_iconBatch,
        p2_label1, p2_label2, p2_label9, p2_labelBatchSize,
        p2_div1Zip, p2_div1App, p2_div1Filter, p2_div1Batch,
        p2_textBoxZipLocation, p2_button1, p2_label5,
        p2_textBoxAppLocation, p2_label6,
        p2_label7, p2_textBoxIgnWidth, p2_lblIgnX, p2_textBoxIgnHeight,
        p2_lblIgnPx, p2_lblIgnDot, p2_label10, p2_textBoxIgnoredWords,
        p2_textBoxIgnoredSize, p2_label8,
        p2_numericUpDownBatchSize, p2_label1BatchSizeInfo,
        p2_button2, p2_button3, p2_lblStatus });

      // ── STACK PANELS ───────────────────────────────────────────────
      pnlCard.Controls.Add(pnlContentPhoto);
      pnlCard.Controls.Add(pnlContentGiga);
      pnlBg.Controls.Add(pnlCard);

      // ── FORM ────────────────────────────────────────────────────────
      this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
      this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor           = Clr(240, 243, 248);
      this.ClientSize          = S(564, 570);
      this.Controls.Add(pnlBg);
      this.Controls.Add(pnlBottom);
      this.Controls.Add(pnlHeader);
      try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                          System.Windows.Forms.Application.ExecutablePath); } catch { }
      this.MaximizeBox  = false;
      this.MaximumSize  = S(580, 609);
      this.MinimumSize  = S(580, 609);
      this.Name         = "Form1";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text         = "Gigapixel Zipper";

      ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
      ((System.ComponentModel.ISupportInitialize)numericUpDownBatchSize).EndInit();
      ((System.ComponentModel.ISupportInitialize)p2_numericUpDownBatchSize).EndInit();
      pnlHeader.ResumeLayout(false); pnlHeader.PerformLayout();
      pnlBg.ResumeLayout(false); pnlCard.ResumeLayout(false);
      pnlContentGiga.ResumeLayout(false); pnlContentGiga.PerformLayout();
      pnlContentPhoto.ResumeLayout(false); pnlContentPhoto.PerformLayout();
      pnlBottom.ResumeLayout(false); pnlBottom.PerformLayout();
      this.ResumeLayout(false);
    }

    // ── Layout helpers ──────────────────────────────────────────────────
    static System.Drawing.Color    Clr(int r,int g,int b) => System.Drawing.Color.FromArgb(r,g,b);
    static System.Drawing.Font     Fnt(string n,float sz,System.Drawing.FontStyle st=System.Drawing.FontStyle.Regular)
        => new System.Drawing.Font(n,sz,st);
    static System.Drawing.Point    P(int x,int y)   => new System.Drawing.Point(x,y);
    static System.Drawing.Size     S(int w,int h)   => new System.Drawing.Size(w,h);
    const  System.Drawing.FontStyle B = System.Drawing.FontStyle.Bold;

    static System.Windows.Forms.Panel MkIcon()
    {
      var p = new System.Windows.Forms.Panel();
      p.Size = S(26,26); return p;
    }
    static System.Windows.Forms.Label MkIconChar(string ch)
    {
      var l = new System.Windows.Forms.Label();
      l.Dock = System.Windows.Forms.DockStyle.Fill;
      l.Text = ch;
      l.Font = Fnt("Segoe UI", 11f);
      l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      return l;
    }
    static System.Windows.Forms.Panel MkDiv()
    {
      var p = new System.Windows.Forms.Panel();
      p.Height = 1; return p;
    }

    static void SetIcon(System.Windows.Forms.Panel pan, string name,
        System.Windows.Forms.Label lbl, System.Drawing.Color bg,
        System.Drawing.Color fg, int x, int y)
    {
      pan.Name      = name;
      pan.Location  = P(x, y);
      pan.BackColor = bg;
      lbl.ForeColor = fg;
      pan.Controls.Add(lbl);
    }
    static void SectionLabel(System.Windows.Forms.Label l, string name, string text,
        int x, int y, System.Drawing.Font f, System.Drawing.Color c)
    {
      l.AutoSize=true; l.Font=f; l.ForeColor=c; l.Location=P(x,y); l.Name=name; l.Text=text;
    }
    static void SetDiv(System.Windows.Forms.Panel p, string name,
        int x, int y, int w, System.Drawing.Color c)
    {
      p.Name=name; p.Location=P(x,y); p.Size=S(w,1); p.BackColor=c;
    }
    static void SetTB(System.Windows.Forms.TextBox tb, string name,
        int x, int y, int w, int h, System.Drawing.Font f,
        System.Drawing.Color fg, string placeholder="")
    {
      tb.Name=name; tb.Font=f; tb.ForeColor=fg; tb.BackColor=System.Drawing.Color.White;
      tb.BorderStyle=System.Windows.Forms.BorderStyle.FixedSingle;
      tb.Location=P(x,y); tb.Size=S(w,h); tb.TabIndex=2;
      if (placeholder!="" && tb.Text=="") tb.Text="";
    }
    static void SetTBMulti(System.Windows.Forms.TextBox tb, string name,
        string text, int x, int y, int w, int h,
        System.Drawing.Font f, System.Drawing.Color fg)
    {
      tb.Name=name; tb.Font=f; tb.ForeColor=fg; tb.BackColor=System.Drawing.Color.White;
      tb.BorderStyle=System.Windows.Forms.BorderStyle.FixedSingle;
      tb.Multiline=true; tb.Text=text;
      tb.Location=P(x,y); tb.Size=S(w,h); tb.TabIndex=5;
    }
    static void HintLbl(System.Windows.Forms.Label l, string name, string text,
        int x, int y, System.Drawing.Font f, System.Drawing.Color c)
    {
      l.AutoSize=true; l.Font=f; l.ForeColor=c; l.Location=P(x,y); l.Name=name; l.Text=text;
    }
    static void InlineLbl(System.Windows.Forms.Label l, string name, string text,
        int x, int y, System.Drawing.Font f, System.Drawing.Color c)
    {
      l.AutoSize=true; l.Font=f; l.ForeColor=c; l.Location=P(x,y); l.Name=name; l.Text=text;
    }
    static void MkBtn(System.Windows.Forms.Button btn, string name, string text,
        int x, int y, int w, int h, System.Drawing.Color back,
        System.Drawing.Font f)
    {
      btn.Name=name; btn.Text=text; btn.Font=f;
      btn.Location=P(x,y); btn.Size=S(w,h);
      btn.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
      btn.FlatAppearance.BorderSize=0;
      btn.BackColor=back; btn.ForeColor=System.Drawing.Color.White;
      btn.UseVisualStyleBackColor=false;
      btn.Cursor=System.Windows.Forms.Cursors.Hand;
    }
    static void MkTabBtn(System.Windows.Forms.Button btn, string name, string text,
        int x, int y, int w, int h,
        System.Drawing.Color back, System.Drawing.Color fore)
    {
      btn.Name=name; btn.Text=text;
      btn.Font=Fnt("Segoe UI",9f,B);
      btn.Location=P(x,y); btn.Size=S(w,h);
      btn.FlatStyle=System.Windows.Forms.FlatStyle.Flat;
      btn.FlatAppearance.BorderSize=1;
      btn.FlatAppearance.BorderColor=System.Drawing.Color.White;
      btn.BackColor=back; btn.ForeColor=fore;
      btn.UseVisualStyleBackColor=false;
      btn.Cursor=System.Windows.Forms.Cursors.Hand;
    }

    #endregion

    // ── Field declarations ──────────────────────────────────────────────
    System.Windows.Forms.Panel  pnlHeader, pnlBg, pnlCard, pnlBottom;
    System.Windows.Forms.Panel  pnlContentGiga, pnlContentPhoto;
    System.Windows.Forms.Panel  pnlAppIcon;
    System.Windows.Forms.Label  lblAppChar, label3, label4;
    System.Windows.Forms.Button btnTabGiga, btnTabPhoto;
    System.Windows.Forms.Label  lblStatusDot, lblStatus;
    System.Windows.Forms.PictureBox pictureBox1;

    // Tab-1 icons
    System.Windows.Forms.Panel p2_iconZip, p2_iconApp, p2_iconFilter, p2_iconBatch;
    System.Windows.Forms.Panel iconZip1, iconApp1, iconFilter1, iconBatch1;
    System.Windows.Forms.Label lblIconZip1, lblIconApp1, lblIconFilter1, lblIconBatch1;
    System.Windows.Forms.Label p2_lblIconZip, p2_lblIconApp, p2_lblIconFilter, p2_lblIconBatch;
    System.Windows.Forms.Panel div1Zip, div1App, div1Filter, div1Batch;
    System.Windows.Forms.Panel p2_div1Zip, p2_div1App, p2_div1Filter, p2_div1Batch;

    // Tab-1 functional
    System.Windows.Forms.Label    label1, label5, label2, label6;
    System.Windows.Forms.Label    label9, label7, label10, label8;
    System.Windows.Forms.Label    labelBatchSize, label1BatchSizeInfo;
    System.Windows.Forms.Label    lblIgnXGiga, lblIgnPxGiga, lblIgnDotGiga;
    System.Windows.Forms.TextBox  textBoxZipLocation, textBoxGGigapixelLocation;
    System.Windows.Forms.TextBox  textBoxIgnoredSize, textBoxIgnoredWorlds;
    System.Windows.Forms.TextBox  textBoxIgnWidthGiga, textBoxIgnHeightGiga;
    System.Windows.Forms.NumericUpDown numericUpDownBatchSize;
    System.Windows.Forms.Button   button1, button2, button3;

    // Tab-2 functional
    System.Windows.Forms.Label    p2_label1, p2_label5, p2_label2, p2_label6;
    System.Windows.Forms.Label    p2_label9, p2_label7, p2_label10, p2_label8;
    System.Windows.Forms.Label    p2_labelBatchSize, p2_label1BatchSizeInfo, p2_lblStatus;
    System.Windows.Forms.Label    p2_lblIgnX, p2_lblIgnPx, p2_lblIgnDot;
    System.Windows.Forms.TextBox  p2_textBoxZipLocation, p2_textBoxAppLocation;
    System.Windows.Forms.TextBox  p2_textBoxIgnoredSize, p2_textBoxIgnoredWords;
    System.Windows.Forms.TextBox  p2_textBoxIgnWidth, p2_textBoxIgnHeight;
    System.Windows.Forms.NumericUpDown p2_numericUpDownBatchSize;
    System.Windows.Forms.Button   p2_button1, p2_button2, p2_button3;
  }
}
