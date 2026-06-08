"""
generate_icon.py
Run this once before building with PyInstaller to create app_icon.ico
"""
import sys, os
from PIL import Image, ImageDraw

def make_icon():
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Background rounded rectangle (dark navy)
    draw.rounded_rectangle([0, 0, size, size], radius=40, fill="#1a2035")

    # Corner badges
    draw.rounded_rectangle([0, 0, 56, 56],   radius=14, fill="#232f3e")   # Amazon
    draw.rounded_rectangle([200, 0, 256, 56], radius=14, fill="#f97316")  # Temu
    draw.rounded_rectangle([0, 200, 90, 256], radius=14, fill="#1e2d40")  # Etsy
    draw.rounded_rectangle([170,200,256, 256],radius=14, fill="#1e2d40")  # eBay

    # Swirl rings
    for offset, w in [(12, 9), (28, 7), (44, 5)]:
        draw.arc([offset, offset, size-offset, size-offset],
                 start=25, end=295, fill="#f97316", width=w)
        draw.arc([offset, offset, size-offset, size-offset],
                 start=205, end=115, fill="#f97316", width=w)

    # PDF document stack
    cx, cy = 128, 116
    for i in range(2, -1, -1):
        o = i * 7
        x1, y1, x2, y2 = cx-36+o, cy-42+o, cx+36+o, cy+44+o
        draw.rounded_rectangle([x1, y1, x2, y2], radius=6,
                                fill="#3d4f6b", outline="#5a6f8a", width=1)
        fold = 13
        draw.polygon([x2-fold, y1, x2, y1+fold, x2-fold, y1+fold], fill="#5a7299")

    # "PDF" text
    try:
        from PIL import ImageFont
        fnt  = ImageFont.truetype("arial.ttf", 22)
        fnt2 = ImageFont.truetype("arial.ttf", 11)
    except Exception:
        fnt = fnt2 = None

    if fnt:
        draw.text((cx-19, cy-8),  "PDF",  fill="white",   font=fnt)
        draw.text((14, 14),       "a",    fill="white",   font=fnt)
        draw.text((213, 14),      "T",    fill="white",   font=fnt)
        draw.text((10, 208),      "Etsy", fill="#f16521", font=fnt2)
        draw.text((175, 208),     "ebay", fill="#e53238", font=fnt2)
    else:
        draw.text((cx-19, cy-8),  "PDF",  fill="white")
        draw.text((14, 14),       "a",    fill="white")
        draw.text((213, 14),      "T",    fill="white")

    # Sort arrows
    draw.polygon([(cx-8, cy-24),(cx-4,cy-33),(cx, cy-24)], fill="#f97316")
    draw.polygon([(cx+2, cy+20),(cx+6,cy+29),(cx+10,cy+20)], fill="#f97316")

    return img

if __name__ == "__main__":
    img = make_icon()
    # Save as ICO with multiple sizes
    sizes = [(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)]
    imgs  = [img.resize(s, Image.LANCZOS) for s in sizes]
    imgs[0].save("app_icon.ico", format="ICO", sizes=sizes,
                 append_images=imgs[1:])
    img.save("app_icon.png")
    print("app_icon.ico and app_icon.png generated successfully.")
