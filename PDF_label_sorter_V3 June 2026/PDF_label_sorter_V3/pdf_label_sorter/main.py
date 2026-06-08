"""
PDF Label Sorter – Amazon, Temu & Etsy  v2.4.0
"""

import os, json, threading, datetime, re, base64, io, shutil, tempfile, subprocess, sys
import customtkinter as ctk
from tkinter import filedialog, messagebox
from PIL import Image, ImageDraw, ImageFont

try:
    from PyPDF2 import PdfReader, PdfWriter
    from PyPDF2.generic import RectangleObject
except ImportError:
    try:
        from pypdf import PdfReader, PdfWriter
    except ImportError:
        PdfReader = PdfWriter = None

try:
    from reportlab.pdfgen import canvas as rl_canvas
    HAS_RL = True
except ImportError:
    # Auto-install reportlab — required for quantity stamps
    import subprocess, sys
    try:
        subprocess.check_call(
            [sys.executable, "-m", "pip", "install", "reportlab", "--quiet"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL
        )
        from reportlab.pdfgen import canvas as rl_canvas
        HAS_RL = True
    except Exception:
        rl_canvas = None
        HAS_RL = False

ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("blue")

APP_NAME     = "PDF Label Sorter"
APP_VERSION  = "2.4.0"
APP_SUBTITLE = "Amazon · Temu · Etsy"

COL_BG             = "#1a1f2e"
COL_CARD           = "#ffffff"
COL_MAIN           = "#f0f4f8"
COL_ACCENT         = "#f97316"
COL_BLUE           = "#3b82f6"
COL_AMAZ           = "#232f3e"
COL_TEMU           = "#f97316"
COL_ETSY           = "#f16521"
COL_TXT_DARK       = "#1e293b"
COL_TXT_MID        = "#64748b"
COL_BORDER         = "#e2e8f0"
COL_GREEN          = "#22c55e"
COL_SIDEBAR_ACTIVE = "#2563eb"
COL_SIDEBAR_HOVER  = "#1e3a5f"

HISTORY_FILE  = os.path.join(os.path.expanduser("~"), ".pdf_label_sorter_history.json")
SETTINGS_FILE = os.path.join(os.path.expanduser("~"), ".pdf_label_sorter_settings.json")

# ── Embedded logos ────────────────────────────────────────────────────────────
_HERE = os.path.dirname(os.path.abspath(__file__))
_LOGO_CACHE = {}

def _b64_to_ctk(b64_str, size):
    key = (id(b64_str), size)
    if key not in _LOGO_CACHE:
        try:
            raw = base64.b64decode(b64_str)
            img = Image.open(io.BytesIO(raw)).convert("RGBA")
            _LOGO_CACHE[key] = ctk.CTkImage(light_image=img, dark_image=img, size=size)
        except Exception as e:
            _LOGO_CACHE[key] = None
    return _LOGO_CACHE[key]

_AMAZ_LIGHT = _AMAZ_WHITE = _TEMU = _ETSY = None
def _load_logos():
    global _AMAZ_LIGHT, _AMAZ_WHITE, _TEMU, _ETSY
    try:
        import logo_data as LD
        _AMAZ_LIGHT = LD.AMAZON_LIGHT
        _AMAZ_WHITE = LD.AMAZON_WHITE
        _TEMU       = LD.TEMU
        _ETSY       = LD.ETSY
    except Exception as e:
        print(f"Logo load error: {e}")
_load_logos()

# Card sizes (all same fixed width for equal cards)
CARD_LOGO_W = 120   # fixed logo display width for all card logos
def logo_amazon_card():    return _b64_to_ctk(_AMAZ_LIGHT, (CARD_LOGO_W, 36)) if _AMAZ_LIGHT else None
def logo_temu_card():      return _b64_to_ctk(_TEMU,       (CARD_LOGO_W, 88)) if _TEMU       else None
def logo_etsy_card():      return _b64_to_ctk(_ETSY,       (CARD_LOGO_W, 60)) if _ETSY       else None
# Header (inside config panel)
def logo_amazon_hdr():     return _b64_to_ctk(_AMAZ_LIGHT, (110, 33))          if _AMAZ_LIGHT else None
def logo_temu_hdr():       return _b64_to_ctk(_TEMU,       (80, 59))            if _TEMU       else None
def logo_etsy_hdr():       return _b64_to_ctk(_ETSY,       (80, 40))            if _ETSY       else None
# Sidebar (small)
def logo_amazon_sb():      return _b64_to_ctk(_AMAZ_WHITE, (72, 22))            if _AMAZ_WHITE else None
def logo_temu_sb():        return _b64_to_ctk(_TEMU,       (48, 35))            if _TEMU       else None
def logo_etsy_sb():        return _b64_to_ctk(_ETSY,       (40, 20))            if _ETSY       else None

# ── App icon ──────────────────────────────────────────────────────────────────
def make_app_icon():
    size = 256
    img  = Image.new("RGBA",(size,size),(0,0,0,0))
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([0,0,size,size],radius=40,fill="#1a2035")
    for offset,w in [(12,9),(28,7),(44,5)]:
        draw.arc([offset,offset,size-offset,size-offset],start=25,end=295,fill="#f97316",width=w)
        draw.arc([offset,offset,size-offset,size-offset],start=205,end=115,fill="#f97316",width=w)
    cx,cy=128,116
    for i in range(2,-1,-1):
        o=i*7; x1,y1,x2,y2=cx-36+o,cy-42+o,cx+36+o,cy+44+o
        draw.rounded_rectangle([x1,y1,x2,y2],radius=6,fill="#3d4f6b",outline="#5a6f8a",width=1)
        draw.polygon([x2-13,y1,x2,y1+13,x2-13,y1+13],fill="#5a7299")
    try:
        fnt=ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",20)
        draw.text((cx-17,cy-8),"PDF",fill="white",font=fnt)
    except Exception: pass
    draw.polygon([(cx-8,cy-24),(cx-4,cy-33),(cx,cy-24)],fill="#f97316")
    draw.polygon([(cx+2,cy+20),(cx+6,cy+29),(cx+10,cy+20)],fill="#f97316")
    return img

# ── Quantity stamp on PDF page ────────────────────────────────────────────────
def _make_stamp_pdf_bytes(w, h, sx, sy, fsz, qty):
    """
    Build a minimal PDF page containing only the *qty text.
    Works with reportlab (preferred) OR with raw PDF content stream (fallback).
    The raw PDF fallback needs NO external libraries.
    """
    if HAS_RL:
        buf = io.BytesIO()
        c = rl_canvas.Canvas(buf, pagesize=(w, h))
        c.setFont("Helvetica-Bold", fsz)
        c.setFillColorRGB(0, 0, 0)
        c.drawString(sx, sy, f"*{qty}")
        c.save()
        return buf.getvalue()
    else:
        # Pure PDF content stream — no libraries needed
        # Positions: sx,sy in PDF pts from bottom-left
        text  = f"*{qty}"
        # Build a minimal valid PDF manually
        stream = (
            f"BT\n"
            f"/F1 {fsz} Tf\n"
            f"{sx} {sy} Td\n"
            f"({text}) Tj\n"
            f"ET\n"
        ).encode()
        stream_len = len(stream)
        pdf = (
            f"%PDF-1.4\n"
            f"1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"
            f"2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"
            f"3 0 obj\n<< /Type /Page /Parent 2 0 R "
            f"/MediaBox [0 0 {w} {h}] "
            f"/Contents 4 0 R /Resources << /Font << /F1 << /Type /Font "
            f"/Subtype /Type1 /BaseFont /Helvetica-Bold >> >> >> >>\nendobj\n"
            f"4 0 obj\n<< /Length {stream_len} >>\nstream\n"
        ).encode() + stream + b"\nendstream\nendobj\n"
        # Minimal xref
        offsets = []; pos = 0
        for line in pdf.split(b"\n"):
            offsets.append(pos); pos += len(line)+1
        xref = f"xref\n0 5\n0000000000 65535 f \n"
        # simple: just append xref at end
        xref_pos = len(pdf)
        xref_bytes = b"xref\n0 5\n0000000000 65535 f \n" + b"\n".join(
            f"{o:010d} 00000 n ".encode() for o in [9,50,100,160,280]
        ) + b"\ntrailer\n<</Size 5/Root 1 0 R>>\nstartxref\n" + str(xref_pos).encode() + b"\n%%EOF"
        return pdf + xref_bytes


def stamp_qty_on_page(page, qty, ltype):
    """
    Stamp *{qty} bold black directly onto page.
    - ltype pre-detected ('evri'/'rm') before this call — never re-detected.
    - Merges directly on original page object (no clone — clone breaks pypdf 5.x).
    - Works with or without reportlab installed.
    Evri (283x425 pts): x=130, y=88, size=12
    RM   (286x431 pts): x=100, y=83, size=14
    """
    if qty <= 1:
        return page
    try:
        w = float(page.mediabox.width)
        h = float(page.mediabox.height)
    except Exception:
        w, h = 595, 842

    if ltype == 'evri':
        sx, sy, fsz = 130, 88.0, 12
    elif ltype == 'rm':
        sx, sy, fsz = 100, 83, 14
    else:
        sx, sy, fsz = w - 60, 20, 14

    try:
        pdf_bytes  = _make_stamp_pdf_bytes(w, h, sx, sy, fsz, qty)
        stamp_page = PdfReader(io.BytesIO(pdf_bytes)).pages[0]
        page.merge_page(stamp_page)   # direct merge on original — no clone
    except Exception as e:
        pass   # silently skip stamp if it fails

    return page


# ── Shared widgets ────────────────────────────────────────────────────────────
class FileRow(ctk.CTkFrame):
    def __init__(self, master, label, required=True, **kw):
        super().__init__(master, fg_color="transparent", **kw)
        self.path_var = ctk.StringVar()
        self.columnconfigure(1, weight=1)
        ctk.CTkLabel(self, text=label, width=248, anchor="w",
                     text_color=COL_TXT_DARK,
                     font=ctk.CTkFont(size=13)).grid(row=0,column=0,padx=(0,12),sticky="w")
        self.entry = ctk.CTkEntry(self, textvariable=self.path_var,
                                  placeholder_text="Select PDF file...",
                                  fg_color="#f8fafc", border_color=COL_BORDER,
                                  text_color=COL_TXT_DARK,
                                  placeholder_text_color=COL_TXT_MID,
                                  height=38, corner_radius=8)
        self.entry.grid(row=0,column=1,sticky="ew",padx=(0,8))
        ctk.CTkButton(self, text="  Browse", width=90, height=38,
                      corner_radius=8, fg_color="#f1f5f9", hover_color="#e2e8f0",
                      text_color=COL_TXT_DARK, border_width=1, border_color=COL_BORDER,
                      font=ctk.CTkFont(size=12), command=self._browse).grid(row=0,column=2)
    def _browse(self):
        p = filedialog.askopenfilename(filetypes=[("PDF files","*.pdf")])
        if p: self.path_var.set(p)
    def get(self):   return self.path_var.get().strip()
    def clear(self): self.path_var.set("")

class SectionCard(ctk.CTkFrame):
    def __init__(self, master, **kw):
        super().__init__(master, fg_color=COL_CARD, corner_radius=12,
                         border_width=1, border_color=COL_BORDER, **kw)

class LogBox(ctk.CTkTextbox):
    def __init__(self, master, **kw):
        super().__init__(master, fg_color="#f8fafc", border_color=COL_BORDER,
                         border_width=1, text_color=COL_TXT_DARK,
                         font=ctk.CTkFont(family="Courier",size=11),
                         corner_radius=8, **kw)
        self.configure(state="disabled")
    def append(self, msg, tag="info"):
        ts  = datetime.datetime.now().strftime("%H:%M:%S")
        pfx = {"info":"ℹ","ok":"✔","warn":"⚠","error":"✖"}.get(tag,"·")
        self.configure(state="normal")
        self.insert("end",f"[{ts}] {pfx}  {msg}\n")
        self.see("end"); self.configure(state="disabled")
    def clear(self):
        self.configure(state="normal"); self.delete("1.0","end"); self.configure(state="disabled")

def _save_history(platform, guide, sources, output):
    history = []
    if os.path.exists(HISTORY_FILE):
        try:
            with open(HISTORY_FILE) as f: history = json.load(f)
        except Exception: pass
    history.insert(0,{"platform":platform,"guide":guide,
                       "sources":[s for s in (sources or []) if s],
                       "output":output,
                       "timestamp":datetime.datetime.now().isoformat()})
    with open(HISTORY_FILE,"w") as f: json.dump(history[:50],f,indent=2)

def build_log_card(parent, holder):
    lc = SectionCard(parent); lc.pack(fill="x",padx=32,pady=(0,24))
    lr = ctk.CTkFrame(lc,fg_color="transparent"); lr.pack(fill="x",padx=20,pady=(14,6))
    ctk.CTkLabel(lr,text="⟳  Execution Log",
                 font=ctk.CTkFont(size=14,weight="bold"),
                 text_color=COL_TXT_DARK).pack(side="left")
    lb = LogBox(lc,height=150)
    ctk.CTkButton(lr,text="🗑  Clear Log",width=110,height=30,
                  fg_color="#f1f5f9",hover_color="#e2e8f0",text_color=COL_TXT_DARK,
                  border_width=1,border_color=COL_BORDER,font=ctk.CTkFont(size=12),
                  command=lb.clear).pack(side="right")
    lb.pack(fill="x",padx=20,pady=(0,16)); holder.append(lb)

def build_platform_header(card, logo_fn, title):
    hrow = ctk.CTkFrame(card,fg_color="transparent")
    hrow.pack(fill="x",padx=20,pady=(16,8))
    logo = logo_fn()
    if logo:
        ctk.CTkLabel(hrow,text="",image=logo).pack(side="left",padx=(0,12))
    ctk.CTkLabel(hrow,text=title,
                 font=ctk.CTkFont(size=16,weight="bold"),
                 text_color=COL_TXT_DARK).pack(side="left")
    ctk.CTkFrame(card,height=1,fg_color=COL_BORDER).pack(fill="x",padx=20,pady=(0,14))

# ── PDF helpers ───────────────────────────────────────────────────────────────
def extract_order_ids_from_guide(path):
    ids = []
    if not path or not os.path.exists(path): return ids
    reader = PdfReader(path)
    for page in reader.pages:
        txt = page.extract_text() or ""
        ids.extend(re.findall(r'\b(40\d{8})\b', txt))
    return ids

def extract_order_id_evri(txt):
    # "Reference 2 407744546680 AVO2" — order ID may be concatenated with routing
    # Extract exactly 10 digits starting with 40 after "Reference 2"
    m = re.search(r'Reference\s*2\s+(40\d{8})', txt)
    if m: return m.group(1)
    # Fallback: any 10-digit number starting with 40 (word boundary optional)
    m = re.search(r'(40\d{8})', txt)
    return m.group(1) if m else None

def extract_order_id_rm(txt):
    m = re.search(r'Customer\s*Ref[:\s]+\d+\s*/\s*(\d{10})', txt)
    if m: return m.group(1)
    m = re.search(r'/\s*(40\d{8})\b', txt)
    return m.group(1) if m else None

def label_type(txt):
    if "EVRi" in txt or "Evri" in txt or "Reference 2" in txt or "P2G" in txt:
        return "evri"
    if "Royal Mail" in txt or "Customer Ref" in txt:
        return "rm"
    return "unknown"

def _sort_by_guide_with_qty(id_to_pages, guide_ids, log, platform=""):
    """
    Sort label pages to match guide order.
    id_to_pages: dict of oid -> (ltype, page)  ← ltype detected BEFORE cloning
    Quantity = how many times an order ID appears in the guide.
    Output: ONE label page per order, with *qty stamped on it (if qty > 1).
    """
    from collections import Counter
    guide_qty  = Counter(guide_ids)
    seen_order = []; seen_set = set()
    for oid in guide_ids:
        if oid not in seen_set:
            seen_order.append(oid); seen_set.add(oid)

    writer = PdfWriter(); matched = 0; used_oids = set()

    for oid in seen_order:
        if oid not in id_to_pages:
            log(f"  Order {oid} (qty {guide_qty[oid]}) – no label found","warn")
            continue
        qty         = guide_qty[oid]
        ltype, page = id_to_pages[oid]          # unpack pre-detected type
        stamped     = stamp_qty_on_page(page, qty, ltype)
        writer.add_page(stamped)
        matched += 1; used_oids.add(oid)
        qty_str = f" → *{qty} stamped" if qty > 1 else ""
        log(f"  Matched order {oid} ({ltype}){qty_str}","ok")

    for oid, (ltype, page) in id_to_pages.items():
        if oid not in used_oids:
            writer.add_page(page)
            log(f"  Order {oid} not in guide – appended","warn")

    return writer, matched

def _today():
    return datetime.datetime.now().strftime("%Y-%m-%d")

def do_amazon_sort(guide_path, source_paths, log, done_cb):
    def run():
        try:
            if PdfReader is None:
                log("PyPDF2 not installed", "error"); done_cb(False); return

            from amazon_processor import process_amazon_files

            valid_sources = [p for p in (source_paths or []) if p]
            if not guide_path or not os.path.exists(guide_path):
                log("Guide file not found", "error"); done_cb(False); return
            if not valid_sources:
                log("No source files", "error"); done_cb(False); return

            out_dir = os.path.dirname(valid_sources[0])
            out_name = f"Amazon_Sorted_{datetime.datetime.now().strftime('%Y%m%d_%H%M%S')}.pdf"
            out_path = os.path.join(out_dir, out_name)

            def _log_cb(msg: str):
                m = str(msg)
                level = "info"
                upper = m.upper()
                if "ERROR" in upper or "CRITICAL" in upper or "🚨" in m:
                    level = "error"
                elif "WARNING" in upper or "MISSING" in upper or "⚠" in m or "❌" in m:
                    level = "warn"
                elif "✅" in m or "✓" in m:
                    level = "ok"
                log(m, level)

            ok = process_amazon_files(guide_path, valid_sources, out_path, _log_cb)
            if not ok:
                done_cb(False)
                return

            log(f"Saved: {out_name}", "ok")
            _save_history("Amazon", guide_path, source_paths, out_path)
            done_cb(True)
        except Exception as e:
            log(f"Error: {e}", "error"); done_cb(False)
    threading.Thread(target=run,daemon=True).start()

def do_amazon_old_sort(guide_path, source_paths, log, done_cb):
    def run():
        def _safe_done(ok: bool):
            try:
                done_cb(ok)
            except Exception as e:
                log(f"done_cb failed: {e}", "warn")

        try:
            if PdfReader is None:
                log("PyPDF2 not installed", "error"); _safe_done(False); return
            if not guide_path or not os.path.exists(guide_path):
                log("Guide file not found", "error"); _safe_done(False); return

            valid_sources = [p for p in (source_paths or []) if p]
            if not valid_sources:
                log("No source files", "error"); _safe_done(False); return

            out_dir = os.path.dirname(valid_sources[0])
            out_name = f"Amazon_Sorted_{datetime.datetime.now().strftime('%Y%m%d_%H%M%S')}.pdf"
            out_path = os.path.join(out_dir, out_name)

            pdf_home = os.environ.get("PDF_HOME") or os.environ.get("PDF_PROCESS_HOME")
            if not pdf_home:
                candidates = []
                try:
                    candidates.append(os.path.join(os.path.expanduser("~"), "Documents", "PDF Process Latest 3.0 2"))
                except Exception:
                    pass

                exe_dir = os.path.dirname(getattr(sys, "executable", "") or "")
                if exe_dir:
                    candidates.append(os.path.join(exe_dir, "PDF Process Latest 3.0 2"))
                    candidates.append(os.path.join(os.path.dirname(exe_dir), "PDF Process Latest 3.0 2"))

                script_dir = os.path.dirname(os.path.abspath(__file__))
                candidates.append(os.path.join(script_dir, "PDF Process Latest 3.0 2"))
                candidates.append(os.path.join(os.path.dirname(script_dir), "PDF Process Latest 3.0 2"))
                candidates.append(os.path.join(os.path.dirname(os.path.dirname(script_dir)), "PDF Process Latest 3.0 2"))

                for c in candidates:
                    if not c or not os.path.isdir(c):
                        continue
                    if os.path.isfile(os.path.join(c, "lib", "pdf-process-1.0.jar")):
                        pdf_home = c
                        break

            if pdf_home:
                log(f"Legacy tool path: {pdf_home}", "info")

            lib_dir = os.path.join(pdf_home, "lib") if pdf_home else None
            has_java_tool = bool(
                pdf_home
                and os.path.isdir(lib_dir)
                and os.path.isfile(os.path.join(pdf_home, "lib", "pdf-process-1.0.jar"))
            )

            if has_java_tool:
                log("Starting Amazon Old (Java legacy processor)…", "info")
                with tempfile.TemporaryDirectory(prefix="amazon_old_") as work_dir:
                    guide_dst = os.path.join(work_dir, "guide.pdf")
                    input_dst = os.path.join(work_dir, "input.pdf")
                    out_dst = os.path.join(work_dir, "output.pdf")

                    img_src = os.path.join(pdf_home, "sharp_warning.png")
                    if os.path.exists(img_src):
                        try:
                            shutil.copyfile(img_src, os.path.join(work_dir, "sharp_warning.png"))
                        except Exception as e:
                            log(f"Could not copy sharp_warning.png: {e}", "warn")
                    else:
                        log("sharp_warning.png not found in PDF_HOME; legacy processor may fail", "warn")

                    shutil.copyfile(guide_path, guide_dst)

                    log("Merging source PDFs into input.pdf…", "info")
                    merged = PdfWriter()
                    for sp in valid_sources:
                        log(f"Reading source: {os.path.basename(sp)}", "info")
                        r = PdfReader(sp)
                        for p in r.pages:
                            merged.add_page(p)
                        log(f"  → {len(r.pages)} pages added", "ok")
                    with open(input_dst, "wb") as f:
                        merged.write(f)

                    conf_dir = os.path.join(work_dir, "conf")
                    os.makedirs(conf_dir, exist_ok=True)
                    with open(os.path.join(conf_dir, "config.properties"), "w", encoding="utf-8") as f:
                        f.write("input.labelFileName=input.pdf\n")
                        f.write("input.guideFileName=guide.pdf\n")
                        f.write("output.fileName=output.pdf\n")

                    classpath = os.path.join(lib_dir, "*")
                    cmd = ["java", "-cp", classpath, "com.skee.pdfprocess.Processor"]
                    try:
                        proc = subprocess.run(
                            cmd,
                            cwd=work_dir,
                            capture_output=True,
                            text=True,
                            check=False,
                        )
                    except FileNotFoundError:
                        log("Java not found on this machine (java command missing)", "error")
                        _safe_done(False)
                        return

                    if proc.stdout:
                        for line in proc.stdout.splitlines():
                            if line.strip():
                                log(line.strip(), "info")
                    if proc.stderr:
                        for line in proc.stderr.splitlines():
                            if line.strip():
                                log(line.strip(), "warn")

                    if proc.returncode != 0:
                        log(f"Legacy processor failed (exit {proc.returncode})", "error")
                        _safe_done(False)
                        return

                    if not os.path.exists(out_dst):
                        log("Legacy processor did not produce output.pdf", "error")
                        _safe_done(False)
                        return

                    shutil.copyfile(out_dst, out_path)
                    log(f"Saved: {out_name}", "ok")
                    _save_history("Amazon Old", guide_path, source_paths, out_path)
                    _safe_done(True)
                    return

            log("Legacy Java processor not available; using fallback merge.", "warn")
            log(f"Reading guide: {os.path.basename(guide_path)}", "info")
            gr = PdfReader(guide_path)
            log(f"  → {len(gr.pages)} guide pages", "ok")

            writer = PdfWriter()
            for p in gr.pages:
                writer.add_page(p)

            for sp in valid_sources:
                log(f"Reading source: {os.path.basename(sp)}", "info")
                r = PdfReader(sp)
                for p in r.pages:
                    writer.add_page(p)
                log(f"  → {len(r.pages)} pages added", "ok")

            with open(out_path, "wb") as f:
                writer.write(f)
            log(f"Saved: {out_name}", "ok")
            _save_history("Amazon Old", guide_path, source_paths, out_path)
            _safe_done(True)
        except Exception as e:
            log(f"Error: {e}", "error"); _safe_done(False)

    threading.Thread(target=run, daemon=True).start()

def do_temu_sort(guide_path, source_paths, log, done_cb):
    def run():
        try:
            if PdfReader is None:
                log("PyPDF2 not installed", "error"); done_cb(False); return

            import pdf_extraction_v3 as temu_processor

            valid_sources = [p for p in (source_paths or []) if p]
            if not guide_path or not os.path.exists(guide_path):
                log("Guide file not found", "error"); done_cb(False); return
            if not valid_sources:
                log("No source files", "error"); done_cb(False); return

            out_dir = os.path.dirname(valid_sources[0])
            out_name = f"OutputLabel_{_today()}.pdf"
            out_path = os.path.join(out_dir, out_name)

            def _log_cb(msg: str):
                m = str(msg)
                level = "info"
                upper = m.upper()
                if "ERROR" in upper or "CRITICAL" in upper or "🚨" in m:
                    level = "error"
                elif "WARNING" in upper or "MISSING" in upper or "⚠" in m or "❌" in m:
                    level = "warn"
                elif "✅" in m or "✓" in m:
                    level = "ok"
                log(m, level)

            # pdf_extraction_v3 is a standalone Tk app; silence popups for this app.
            try:
                old_showinfo = getattr(temu_processor.messagebox, "showinfo", None)
                old_showerror = getattr(temu_processor.messagebox, "showerror", None)
                if old_showinfo:
                    temu_processor.messagebox.showinfo = lambda *a, **k: None
                if old_showerror:
                    temu_processor.messagebox.showerror = lambda *a, **k: None
                temu_processor.process_files(guide_path, valid_sources[:2], out_path, _log_cb)
            finally:
                if old_showinfo:
                    temu_processor.messagebox.showinfo = old_showinfo
                if old_showerror:
                    temu_processor.messagebox.showerror = old_showerror

            log(f"Saved: {out_name}", "ok")
            _save_history("Temu", guide_path, source_paths, out_path)
            done_cb(True)
        except Exception as e:
            log(f"Error: {e}", "error"); done_cb(False)
    threading.Thread(target=run,daemon=True).start()

def do_etsy_sort(guide_path, source_paths, log, done_cb):
    def run():
        try:
            if PdfReader is None: log("PyPDF2 not installed","error"); done_cb(False); return
            guide_ids = []
            if guide_path and os.path.exists(guide_path):
                log(f"Reading guide: {os.path.basename(guide_path)}","info")
                guide_ids = extract_order_ids_from_guide(guide_path)
                log(f"  → {len(guide_ids)} order IDs","ok")
            else:
                log("No guide – merging in file order","warn")
            id_to_pages = {}; unmatched = []
            for sp in source_paths:
                if not sp: continue
                log(f"Reading: {os.path.basename(sp)}","info")
                r = PdfReader(sp); ec = rm = uk = 0
                for page in r.pages:
                    txt = page.extract_text() or ""
                    lt  = label_type(txt)
                    if lt=="evri":  oid=extract_order_id_evri(txt); ec+=1
                    elif lt=="rm":  oid=extract_order_id_rm(txt);   rm+=1
                    else:
                        m=re.search(r'(40\d{8})',txt)
                        oid=m.group(1) if m else None; uk+=1
                    if oid: id_to_pages[oid] = (lt, page)  # store ltype+page
                    else:   unmatched.append(page)
                log(f"  → {len(r.pages)} pages  (Evri:{ec}  RM:{rm}  Other:{uk})","ok")
            if guide_ids:
                writer, matched = _sort_by_guide_with_qty(id_to_pages,guide_ids,log,"Etsy")
            else:
                writer = PdfWriter(); matched = 0
                for pages in id_to_pages.values():
                    for p in pages: writer.add_page(p); matched += 1
            for p in unmatched: writer.add_page(p)
            log(f"Matched {matched} unique orders, {len(unmatched)} unmatched appended","info")
            valid = [s for s in source_paths if s]
            if not valid: log("No source files","error"); done_cb(False); return
            out_dir  = os.path.dirname(valid[0])
            out_name = f"OutputLabel_{_today()}.pdf"   # ← new naming
            out_path = os.path.join(out_dir,out_name)
            with open(out_path,"wb") as f: writer.write(f)
            log(f"Saved: {out_name}","ok")
            log(f"Location: {out_path}","info")
            _save_history("Etsy",guide_path,source_paths,out_path)
            done_cb(True)
        except Exception as e: log(f"Error: {e}","error"); done_cb(False)
    threading.Thread(target=run,daemon=True).start()

# ═══════════════════════════════════════════════════════════════════════════════
# Dashboard  – equal-size cards using grid with uniform column weights
# ═══════════════════════════════════════════════════════════════════════════════
class DashboardPanel(ctk.CTkFrame):
    def __init__(self, master, switch_cb, **kw):
        super().__init__(master,fg_color=COL_MAIN,**kw)
        self.switch_cb = switch_cb; self._build()

    def _build(self):
        ctk.CTkLabel(self,text="PDF Label Sorting Tool",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Sort and organise your marketplace order labels with ease.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,20))

        # Grid container – equal columns
        grid_outer = ctk.CTkFrame(self,fg_color="transparent")
        grid_outer.pack(fill="x",padx=32,pady=(0,20))
        for col in range(4):
            grid_outer.columnconfigure(col, weight=1, uniform="card")

        platforms = [
            (logo_amazon_card,"Amazon","Up to 5 label files","#1a2535",COL_AMAZ,"amazon"),
            (logo_amazon_card,"Amazon Old","Legacy processing","#1a2535",COL_AMAZ,"amazon_old"),
            (logo_temu_card,  "Temu",  "Up to 2 label files",COL_TEMU, COL_TEMU,"temu"),
            (logo_etsy_card,  "Etsy",  "Up to 2 label files",COL_ETSY, COL_ETSY,"etsy"),
        ]
        for col,(logo_fn,name,desc,btn_col,hover_col,key) in enumerate(platforms):
            card = SectionCard(grid_outer)
            card.grid(row=0,column=col,padx=(0 if col==0 else 10,0),pady=4,sticky="nsew")

            # Fixed-height logo area so all cards have same proportions
            logo_frame = ctk.CTkFrame(card,fg_color="transparent",height=100)
            logo_frame.pack(fill="x",pady=(20,0)); logo_frame.pack_propagate(False)

            logo = logo_fn()
            if logo:
                ctk.CTkLabel(logo_frame,text="",image=logo).place(relx=0.5,rely=0.5,anchor="center")
            else:
                ctk.CTkLabel(logo_frame,text=name[0],width=50,height=50,
                             fg_color=hover_col,corner_radius=10,
                             font=ctk.CTkFont(size=22,weight="bold"),
                             text_color="white").place(relx=0.5,rely=0.5,anchor="center")

            ctk.CTkLabel(card,text=name,
                         font=ctk.CTkFont(size=14,weight="bold"),
                         text_color=COL_TXT_DARK).pack(pady=(10,2))
            ctk.CTkLabel(card,text=desc,
                         font=ctk.CTkFont(size=11),
                         text_color=COL_TXT_MID).pack(pady=(0,10))
            ctk.CTkButton(card,text=f"Open {name}",
                          fg_color=btn_col,hover_color=hover_col,
                          text_color="white",height=36,
                          corner_radius=8,
                          command=lambda k=key:self.switch_cb(k)).pack(
                              fill="x",padx=20,pady=(0,20))

        # Quick start
        qs = SectionCard(self); qs.pack(fill="x",padx=32,pady=(0,20))
        ctk.CTkLabel(qs,text="Quick Start Guide",
                     font=ctk.CTkFont(size=15,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=20,pady=(18,8))
        for s in [
            "1.  Select your marketplace tab (Amazon, Amazon Old, Temu or Etsy).",
            "2.  Select the Guide PDF (order list) – required for sorting.",
            "3.  Select up to 5 source label PDFs (Amazon) or 2 (Temu/Etsy).",
            "4.  For Etsy: supports both Evri and Royal Mail labels in any mix.",
            "5.  Click Start Processing – sorted PDF saved alongside first source.",
            "ℹ   Orders appearing multiple times in the guide are repeated with *qty stamp on label.",
        ]:
            ctk.CTkLabel(qs,text=s,anchor="w",font=ctk.CTkFont(size=12),
                         text_color=COL_TXT_MID).pack(anchor="w",padx=20,pady=1)
        ctk.CTkFrame(qs,height=14,fg_color="transparent").pack()


# ═══════════════════════════════════════════════════════════════════════════════
# Platform panels
# ═══════════════════════════════════════════════════════════════════════════════
class AmazonPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,status_cb,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw)
        self.status_cb=status_cb; self._build()
    def _build(self):
        ctk.CTkLabel(self,text="PDF Label Sorting Tool",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Sort and organise your marketplace order labels with ease.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,16))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=(0,16))
        build_platform_header(card,logo_amazon_hdr,"Amazon Configuration")
        inner=ctk.CTkFrame(card,fg_color="transparent")
        inner.pack(fill="x",padx=20,pady=(0,16)); inner.columnconfigure(0,weight=1)
        self.guide_row=FileRow(inner,"Guide File (PDF)",required=True)
        self.guide_row.grid(row=0,column=0,sticky="ew",pady=5)
        self.source_rows=[]
        for i,lbl in enumerate(["Source Labels #1","Source Labels #2 (Optional)",
                                  "Source Labels #3 (Optional)","Source Labels #4 (Optional)",
                                  "Source Labels #5 (Optional)"]):
            r=FileRow(inner,lbl,required=(i==0))
            r.grid(row=i+1,column=0,sticky="ew",pady=5); self.source_rows.append(r)
        self.start_btn=ctk.CTkButton(self,text="▶   Start Amazon Processing",
                                     height=52,corner_radius=10,
                                     fg_color=COL_ACCENT,hover_color="#ea6c00",
                                     text_color="white",font=ctk.CTkFont(size=15,weight="bold"),
                                     command=self._start)
        self.start_btn.pack(fill="x",padx=32,pady=(0,16))
        holder=[]; build_log_card(self,holder); self.log=holder[0]
    def _start(self):
        guide=self.guide_row.get(); valid=[r.get() for r in self.source_rows if r.get()]
        if not guide: messagebox.showwarning("Missing","Please select a Guide PDF."); return
        if not valid: messagebox.showwarning("Missing","Please select at least one source file."); return
        self.start_btn.configure(state="disabled",text="⏳  Processing…")
        self.status_cb("Processing Amazon labels…"); self.log.append("Starting Amazon sort…","info")
        def done(ok):
            self.start_btn.configure(state="normal",text="▶   Start Amazon Processing")
            if ok: self.status_cb("✔  Amazon complete"); self.log.append("Complete!","ok"); messagebox.showinfo("Done","Amazon labels sorted!")
            else:  self.status_cb("✖  Amazon failed")
        do_amazon_sort(guide,valid,self.log.append,done)


class AmazonOldPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,status_cb,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw)
        self.status_cb=status_cb; self._build()
    def _build(self):
        ctk.CTkLabel(self,text="PDF Label Sorting Tool",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Sort and organise your marketplace order labels with ease.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,16))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=(0,16))
        build_platform_header(card,logo_amazon_hdr,"Amazon Old Configuration")
        inner=ctk.CTkFrame(card,fg_color="transparent")
        inner.pack(fill="x",padx=20,pady=(0,16)); inner.columnconfigure(0,weight=1)
        self.guide_row=FileRow(inner,"Guide File (PDF)",required=True)
        self.guide_row.grid(row=0,column=0,sticky="ew",pady=5)
        self.source_row=FileRow(inner,"Source Labels (PDF)",required=True)
        self.source_row.grid(row=1,column=0,sticky="ew",pady=5)
        self.start_btn=ctk.CTkButton(self,text="▶   Start Amazon Old Processing",
                                     height=52,corner_radius=10,
                                     fg_color=COL_ACCENT,hover_color="#ea6c00",
                                     text_color="white",font=ctk.CTkFont(size=15,weight="bold"),
                                     command=self._start)
        self.start_btn.pack(fill="x",padx=32,pady=(0,16))
        holder=[]; build_log_card(self,holder); self.log=holder[0]
    def _start(self):
        guide=self.guide_row.get(); src=self.source_row.get(); valid=[src] if src else []
        if not guide: messagebox.showwarning("Missing","Please select a Guide PDF."); return
        if not valid: messagebox.showwarning("Missing","Please select at least one source file."); return
        self.start_btn.configure(state="disabled",text="⏳  Processing…")
        self.status_cb("Processing Amazon Old labels…"); self.log.append("Starting Amazon Old sort…","info")
        def done(ok):
            self.start_btn.configure(state="normal",text="▶   Start Amazon Old Processing")
            if ok: self.status_cb("✔  Amazon Old complete"); self.log.append("Complete!","ok"); messagebox.showinfo("Done","Amazon Old labels sorted!")
            else:  self.status_cb("✖  Amazon Old failed")
        do_amazon_old_sort(guide,valid,self.log.append,done)


class TemuPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,status_cb,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw)
        self.status_cb=status_cb; self._build()
    def _build(self):
        ctk.CTkLabel(self,text="PDF Label Sorting Tool",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Sort and organise your marketplace order labels with ease.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,16))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=(0,16))
        build_platform_header(card,logo_temu_hdr,"Temu Configuration")
        inner=ctk.CTkFrame(card,fg_color="transparent")
        inner.pack(fill="x",padx=20,pady=(0,16)); inner.columnconfigure(0,weight=1)
        self.guide_row=FileRow(inner,"Guide File (PDF)",required=False)
        self.guide_row.grid(row=0,column=0,sticky="ew",pady=5)
        self.source_rows=[]
        for i,lbl in enumerate(["Source Labels #1","Source Labels #2 (Optional)"]):
            r=FileRow(inner,lbl,required=(i==0))
            r.grid(row=i+1,column=0,sticky="ew",pady=5); self.source_rows.append(r)
        self.start_btn=ctk.CTkButton(self,text="▶   Start Temu Processing",
                                     height=52,corner_radius=10,
                                     fg_color=COL_TEMU,hover_color="#e06200",
                                     text_color="white",font=ctk.CTkFont(size=15,weight="bold"),
                                     command=self._start)
        self.start_btn.pack(fill="x",padx=32,pady=(0,16))
        holder=[]; build_log_card(self,holder); self.log=holder[0]
    def _start(self):
        guide=self.guide_row.get(); valid=[r.get() for r in self.source_rows if r.get()]
        if not valid: messagebox.showwarning("Missing","Please select at least one source file."); return
        self.start_btn.configure(state="disabled",text="⏳  Processing…")
        self.status_cb("Processing Temu labels…"); self.log.append("Starting Temu sort…","info")
        def done(ok):
            self.start_btn.configure(state="normal",text="▶   Start Temu Processing")
            if ok: self.status_cb("✔  Temu complete"); self.log.append("Complete!","ok"); messagebox.showinfo("Done","Temu labels sorted!")
            else:  self.status_cb("✖  Temu failed")
        do_temu_sort(guide or None,valid,self.log.append,done)


class EtsyPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,status_cb,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw)
        self.status_cb=status_cb; self._build()
    def _build(self):
        ctk.CTkLabel(self,text="PDF Label Sorting Tool",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Sort and organise your marketplace order labels with ease.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,16))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=(0,16))
        build_platform_header(card,logo_etsy_hdr,"Etsy Configuration")
        inner=ctk.CTkFrame(card,fg_color="transparent")
        inner.pack(fill="x",padx=20,pady=(0,16)); inner.columnconfigure(0,weight=1)
        self.guide_row=FileRow(inner,"Guide File (PDF)",required=False)
        self.guide_row.grid(row=0,column=0,sticky="ew",pady=5)
        self.source_rows=[]
        # ── info box REMOVED as requested ──
        for i,lbl in enumerate(["Source Labels #1  (Evri or Royal Mail)",
                                  "Source Labels #2  (Optional)"]):
            r=FileRow(inner,lbl,required=(i==0))
            r.grid(row=i+1,column=0,sticky="ew",pady=5); self.source_rows.append(r)
        # No note/info box here
        self.start_btn=ctk.CTkButton(self,text="▶   Start Etsy Processing",
                                     height=52,corner_radius=10,
                                     fg_color=COL_ETSY,hover_color="#d4521a",
                                     text_color="white",font=ctk.CTkFont(size=15,weight="bold"),
                                     command=self._start)
        self.start_btn.pack(fill="x",padx=32,pady=(0,16))
        holder=[]; build_log_card(self,holder); self.log=holder[0]
    def _start(self):
        guide=self.guide_row.get(); valid=[r.get() for r in self.source_rows if r.get()]
        if not valid: messagebox.showwarning("Missing","Please select at least one source file."); return
        self.start_btn.configure(state="disabled",text="⏳  Processing…")
        self.status_cb("Processing Etsy labels…"); self.log.append("Starting Etsy sort…","info")
        if not guide: self.log.append("No guide – merging in file order","warn")
        def done(ok):
            self.start_btn.configure(state="normal",text="▶   Start Etsy Processing")
            if ok: self.status_cb("✔  Etsy complete"); self.log.append("Complete!","ok"); messagebox.showinfo("Done","Etsy labels sorted!")
            else:  self.status_cb("✖  Etsy failed")
        do_etsy_sort(guide or None,valid,self.log.append,done)


# ═══════════════════════════════════════════════════════════════════════════════
# History / Settings / About
# ═══════════════════════════════════════════════════════════════════════════════
class HistoryPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw); self._build()
    def _build(self):
        ctk.CTkLabel(self,text="Processing History",
                     font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        ctk.CTkLabel(self,text="Your recent label sorting sessions.",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=32,pady=(0,16))
        history=[]
        if os.path.exists(HISTORY_FILE):
            try:
                with open(HISTORY_FILE) as f: history=json.load(f)
            except Exception: pass
        if not history:
            ctk.CTkLabel(self,text="No history yet.",font=ctk.CTkFont(size=13),
                         text_color=COL_TXT_MID).pack(padx=32,pady=40); return
        for entry in history[:20]:
            card=SectionCard(self); card.pack(fill="x",padx=32,pady=(0,10))
            row=ctk.CTkFrame(card,fg_color="transparent"); row.pack(fill="x",padx=16,pady=12)
            colour={"Amazon":COL_AMAZ,"Amazon Old":COL_AMAZ,"Temu":COL_TEMU,"Etsy":COL_ETSY}.get(entry["platform"],COL_BLUE)
            ctk.CTkLabel(row,text=entry["platform"],fg_color=colour,corner_radius=6,
                         text_color="white",width=100,height=26,
                         font=ctk.CTkFont(size=11,weight="bold")).pack(side="left",padx=(0,12))
            ts=entry.get("timestamp","")[:19].replace("T"," ")
            ctk.CTkLabel(row,text=ts,font=ctk.CTkFont(size=11),text_color=COL_TXT_MID).pack(side="left")
            out=os.path.basename(entry.get("output",""))
            ctk.CTkLabel(row,text=f"→ {out}",font=ctk.CTkFont(size=11),
                         text_color=COL_TXT_DARK).pack(side="right")

class SettingsPanel(ctk.CTkScrollableFrame):
    def __init__(self,master,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw); self._build()
    def _build(self):
        ctk.CTkLabel(self,text="Settings",font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=(16,0))
        ctk.CTkLabel(card,text="Default Output Directory",
                     font=ctk.CTkFont(size=13,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=20,pady=(16,4))
        ctk.CTkLabel(card,text="Leave blank to save alongside the first source file (default).",
                     font=ctk.CTkFont(size=12),text_color=COL_TXT_MID,
                     anchor="w").pack(anchor="w",padx=20,pady=(0,12))
        row=ctk.CTkFrame(card,fg_color="transparent"); row.pack(fill="x",padx=20,pady=(0,16))
        row.columnconfigure(0,weight=1); self.out_var=ctk.StringVar()
        ctk.CTkEntry(row,textvariable=self.out_var,placeholder_text="(same folder as source)",
                     fg_color="#f8fafc",border_color=COL_BORDER,text_color=COL_TXT_DARK,
                     height=38,corner_radius=8).grid(row=0,column=0,sticky="ew",padx=(0,8))
        ctk.CTkButton(row,text="Browse",width=90,height=38,corner_radius=8,
                      fg_color="#f1f5f9",hover_color="#e2e8f0",text_color=COL_TXT_DARK,
                      border_width=1,border_color=COL_BORDER,
                      command=self._browse).grid(row=0,column=1)
        ctk.CTkButton(self,text="Save Settings",height=42,corner_radius=8,
                      fg_color=COL_BLUE,hover_color="#2563eb",text_color="white",
                      font=ctk.CTkFont(size=13,weight="bold"),
                      command=self._save).pack(anchor="w",padx=32,pady=16)
    def _browse(self):
        p=filedialog.askdirectory()
        if p: self.out_var.set(p)
    def _save(self):
        with open(SETTINGS_FILE,"w") as f: json.dump({"output_dir":self.out_var.get()},f)
        messagebox.showinfo("Saved","Settings saved.")

class AboutPanel(ctk.CTkFrame):
    def __init__(self,master,**kw):
        super().__init__(master,fg_color=COL_MAIN,**kw); self._build()
    def _build(self):
        ctk.CTkLabel(self,text="About",font=ctk.CTkFont(size=26,weight="bold"),
                     text_color=COL_TXT_DARK,anchor="w").pack(anchor="w",padx=32,pady=(28,4))
        card=SectionCard(self); card.pack(fill="x",padx=32,pady=16)
        ctk.CTkLabel(card,text=f"PDF Label Sorter  v{APP_VERSION}",
                     font=ctk.CTkFont(size=18,weight="bold"),
                     text_color=COL_TXT_DARK).pack(pady=(24,4))
        ctk.CTkLabel(card,text="Amazon · Temu · Etsy",
                     font=ctk.CTkFont(size=13),text_color=COL_TXT_MID).pack(pady=(0,16))
        ctk.CTkLabel(card,
                     text=("• Amazon – sort label pages against a guide order file.\n"
                           "• Temu   – sort/merge PDFs by order ID; qty stamp applied (*N).\n"
                           "• Etsy   – sort Evri + Royal Mail labels; qty stamp applied (*N).\n"
                           "  Output: OutputLabel_YYYY-MM-DD.pdf (Temu & Etsy)\n\n"
                           "Quantity: if an order appears N times in guide → label repeated N times\n"
                           "and *N is stamped on each label page."),
                     font=ctk.CTkFont(size=12),text_color=COL_TXT_MID,
                     justify="left").pack(padx=24,pady=(0,24))


# ═══════════════════════════════════════════════════════════════════════════════
# Main Window
# ═══════════════════════════════════════════════════════════════════════════════
class App(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.title(f"{APP_NAME} – {APP_SUBTITLE}")
        self.geometry("1060x720"); self.minsize(860,620)
        self.configure(fg_color=COL_MAIN)
        try:
            icon_img=make_app_icon()
            from PIL import ImageTk
            self._icon_ref=ImageTk.PhotoImage(icon_img.resize((32,32)))
            self.iconphoto(True,self._icon_ref)
        except Exception: pass
        self._active_key="dashboard"; self._panels={}; self._nav_btns={}
        self._build_sidebar(); self._build_main(); self._show_panel("dashboard")

    def _add_nav(self, key, icon_txt, logo_fn, logo_size, name_txt=None, name_colour=None):
        """
        Sidebar nav row.
        Plain items: icon + text in button.
        Marketplace items: logo image only by default.
        """
        btn = ctk.CTkButton(
            self.sidebar, text="", anchor="w",
            height=46, corner_radius=8,
            fg_color="transparent", hover_color=COL_SIDEBAR_HOVER,
            text_color="#cbd5e1", font=ctk.CTkFont(size=13),
            command=lambda k=key: self._show_panel(k))
        btn.pack(fill="x", padx=12, pady=2)
        self._nav_btns[key] = btn

        if logo_fn:
            # Logo (optionally followed by text)
            logo = logo_fn()
            if logo:
                img_lbl = ctk.CTkLabel(btn, text="", image=logo,
                                       fg_color="transparent", cursor="hand2")
                img_lbl.place(relx=0, rely=0.5, x=14, anchor="w")
                img_lbl.bind("<Button-1>", lambda e,k=key: self._show_panel(k))
            if icon_txt:
                btn.configure(text=icon_txt)
        else:
            btn.configure(text=icon_txt)

    def _build_sidebar(self):
        self.sidebar=ctk.CTkFrame(self,width=230,corner_radius=0,fg_color=COL_BG)
        self.sidebar.pack(side="left",fill="y"); self.sidebar.pack_propagate(False)

        # Header
        lf=ctk.CTkFrame(self.sidebar,fg_color="transparent"); lf.pack(fill="x",padx=16,pady=(20,8))
        ctk.CTkLabel(lf,text="≡",width=40,height=40,fg_color=COL_BLUE,
                     corner_radius=10,font=ctk.CTkFont(size=22,weight="bold"),
                     text_color="white").pack(side="left",padx=(0,10))
        lt=ctk.CTkFrame(lf,fg_color="transparent"); lt.pack(side="left")
        ctk.CTkLabel(lt,text=APP_NAME,font=ctk.CTkFont(size=14,weight="bold"),
                     text_color="white",anchor="w").pack(anchor="w")
        ctk.CTkLabel(lt,text=APP_SUBTITLE,font=ctk.CTkFont(size=10),
                     text_color="#94a3b8",anchor="w").pack(anchor="w")
        ctk.CTkFrame(self.sidebar,height=1,fg_color="#2d3748").pack(fill="x",padx=16,pady=(8,12))

        # Nav rows
        # key, icon_text, logo_fn, logo_size, name_text, name_colour
        self._add_nav("dashboard","⌂  Dashboard",None,None,None,None)
        self._add_nav("amazon",   "",logo_amazon_sb,(72,22))
        self._add_nav("amazon_old","Amazon Old",logo_amazon_sb,(72,22))
        self._add_nav("temu",     "",logo_temu_sb,  (48,35))
        self._add_nav("etsy",     "",logo_etsy_sb,  (40,20))
        self._add_nav("history",  "⏱  History",     None,None,None,None)
        self._add_nav("settings", "⚙  Settings",    None,None,None,None)
        self._add_nav("about",    "ℹ  About",        None,None,None,None)

        # Status dot
        bot=ctk.CTkFrame(self.sidebar,fg_color="transparent")
        bot.pack(side="bottom",fill="x",padx=12,pady=12)
        self.status_dot=ctk.CTkLabel(bot,text="●",font=ctk.CTkFont(size=12),text_color=COL_GREEN)
        self.status_dot.pack(side="left")
        self.status_lbl=ctk.CTkLabel(bot,text="Ready to process",
                                     font=ctk.CTkFont(size=11),text_color="#94a3b8")
        self.status_lbl.pack(side="left",padx=4)

        # Footer
        self.footer=ctk.CTkFrame(self,height=30,corner_radius=0,fg_color="#e8ecf0")
        self.footer.pack(side="bottom",fill="x"); self.footer.pack_propagate(False)
        self.footer_status=ctk.CTkLabel(self.footer,text="● Status:  Idle",
                                        font=ctk.CTkFont(size=11),text_color=COL_TXT_MID)
        self.footer_status.pack(side="left",padx=16)
        ctk.CTkLabel(self.footer,
                     text=f"Version {APP_VERSION}    |    Ready to sort your labels",
                     font=ctk.CTkFont(size=11),text_color=COL_TXT_MID).pack(side="right",padx=16)

    def _build_main(self):
        self.main_area=ctk.CTkFrame(self,fg_color=COL_MAIN,corner_radius=0)
        self.main_area.pack(side="left",fill="both",expand=True)
        def scb(msg): self.after(0,lambda:self._set_status(msg))
        self._panels["dashboard"]=DashboardPanel(self.main_area,switch_cb=self._show_panel)
        self._panels["amazon"]   =AmazonPanel(self.main_area,status_cb=scb)
        self._panels["amazon_old"]=AmazonOldPanel(self.main_area,status_cb=scb)
        self._panels["temu"]     =TemuPanel(self.main_area,status_cb=scb)
        self._panels["etsy"]     =EtsyPanel(self.main_area,status_cb=scb)
        self._panels["history"]  =HistoryPanel(self.main_area)
        self._panels["settings"] =SettingsPanel(self.main_area)
        self._panels["about"]    =AboutPanel(self.main_area)

    def _show_panel(self,key):
        if self._active_key in self._panels: self._panels[self._active_key].pack_forget()
        if self._active_key in self._nav_btns:
            self._nav_btns[self._active_key].configure(fg_color="transparent",text_color="#cbd5e1")
        self._active_key=key
        self._panels[key].pack(fill="both",expand=True)
        if key in self._nav_btns:
            self._nav_btns[key].configure(fg_color=COL_SIDEBAR_ACTIVE,text_color="white")

    def _set_status(self,msg):
        colour=COL_GREEN if("complete" in msg.lower() or "✔" in msg) else \
               "#ef4444" if("fail" in msg.lower() or "✖" in msg) else COL_ACCENT
        self.status_lbl.configure(text=msg[:45])
        self.status_dot.configure(text_color=colour)
        self.footer_status.configure(text=f"● Status:  {msg}")

if __name__=="__main__":
    App().mainloop()
