import os
from PIL import Image, ImageDraw, ImageFont

TXT_DIR = os.path.join('Documentation', 'Screenshots')
OUT_DIR = os.path.join('Documentation', 'Screenshots_genimg')
os.makedirs(OUT_DIR, exist_ok=True)

FONT = None
# Tìm font Mono cho đẹp nếu có (Win: Consolas, Linux: DejaVuMono, else default)
_font_candidates = [
    "consola.ttf", "consolas.ttf",                   # Windows
    "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf", # Linux
    "/usr/share/fonts/dejavu/DejaVuSansMono.ttf"
]
for f in _font_candidates:
    if os.path.exists(f):
        FONT = f
        break
if not FONT:
    FONT = None  # Fallback PIL default

def render_text_image(text, outpath, fontsize=20, line_spacing=6, padding=30):
    lines = text.splitlines() or [""]
    font = ImageFont.truetype(FONT, fontsize) if FONT else ImageFont.load_default()
    # đo size lớn nhất
    maxw = max((font.getlength(l) if hasattr(font, "getlength") else font.getsize(l)[0]) for l in lines)
    lineh = (font.getbbox('A')[3] if hasattr(font, 'getbbox') else font.getsize('A')[1]) + line_spacing
    H = len(lines)*lineh+padding*2
    W = int(maxw) + padding*2
    # khử bé quá
    W = max(W,400); H = max(H,200)
    img = Image.new("RGB", (W, H), (255,255,255))
    d = ImageDraw.Draw(img)
    y = padding
    for l in lines:
        d.text((padding, y), l, font=font, fill=(0,0,0))
        y += lineh
    img.save(outpath)
    return outpath

# Batch convert
generated = []
for f in sorted(os.listdir(TXT_DIR)):
    if f.lower().endswith('.txt'):
        path = os.path.join(TXT_DIR, f)
        with open(path, 'r', encoding='utf-8', errors='ignore') as fh:
            txt = fh.read()
        outimg = os.path.join(OUT_DIR, f.replace('.txt','.png'))
        render_text_image(txt, outimg)
        print("Generated", outimg)
        generated.append(outimg)
print(f"Done: {len(generated)} images written to {OUT_DIR}")