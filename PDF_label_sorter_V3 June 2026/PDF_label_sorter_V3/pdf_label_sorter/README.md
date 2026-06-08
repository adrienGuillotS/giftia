# PDF Label Sorter v2.0.0
### Amazon · Temu · Etsy

## What's new in v2.0.0
- ✅ **Etsy tab added** – merge up to 2 Etsy label PDFs into one sorted file
- ✅ Updated app icon (Amazon, Temu, Etsy, eBay corners)
- ✅ Dashboard now shows all three platforms
- ✅ Amazon and Temu tabs unchanged from v1.0.0

---

## Building the .exe on Windows

### Prerequisites
- Python 3.10 or newer (https://python.org)
- pip (included with Python)

### Steps

```bat
REM 1. Open Command Prompt in this folder
REM 2. Run the build script:
build_windows.bat
```

Or manually:

```bat
pip install customtkinter pillow PyPDF2 pyinstaller
python generate_icon.py
pyinstaller --clean PDF_Label_Sorter.spec
```

The finished executable will be at:
```
dist\PDF_Label_Sorter_Amazon_Temu_Etsy_V2.exe
```

---

## Running directly (without building)

```bat
pip install customtkinter pillow PyPDF2
python main.py
```

---

## How it works

| Tab | What you provide | What you get |
|-----|-----------------|--------------|
| **Amazon** | 1 Guide PDF + up to 5 source label PDFs | Sorted output PDF ordered by guide |
| **Temu** | Up to 2 source label PDFs | Single merged PDF |
| **Etsy** | Up to 2 source label PDFs | Single merged PDF |

Output files are saved in the same folder as your first source file,
with a timestamp in the filename (e.g. `Etsy_Sorted_20240604_143022.pdf`).

---

## Files

| File | Purpose |
|------|---------|
| `main.py` | Main application (all UI + logic) |
| `generate_icon.py` | Generates `app_icon.ico` for the exe |
| `PDF_Label_Sorter.spec` | PyInstaller build spec |
| `version_info.txt` | Windows exe metadata |
| `build_windows.bat` | One-click build script |
