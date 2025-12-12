from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path("C:/Projects/CoralLedgerBlue")
ASSETS = [
    (1280, 320, ROOT / "github-header.png"),
    (1200, 630, ROOT / "og-image.png"),
]

def load_font(size, bold=False):
    default = ImageFont.load_default()
    font_paths = [
        "C:/Windows/Fonts/seguisb.ttf",
        "C:/Windows/Fonts/arialbd.ttf",
        "C:/Windows/Fonts/Calibri.ttf",
    ]
    for path in font_paths:
        if Path(path).exists():
            try:
                return ImageFont.truetype(path, size)
            except OSError:
                continue
    return ImageFont.load_default()

def draw_gradient(draw, size):
    width, height = size
    for i in range(height):
        ratio = i / height
        c = (
            int(14 + ratio * (190 - 14)),
            int(27 + ratio * (132 - 27)),
            int(56 + ratio * (190 - 56)),
        )
        draw.line([(0, i), (width, i)], fill=c)

def draw_wave(draw, size, offset, color):
    width, height = size
    points = [
        (0, height * 0.55 + offset),
        (width * 0.15, height * 0.45 + offset),
        (width * 0.35, height * 0.6 + offset),
        (width * 0.55, height * 0.5 + offset),
        (width, height * 0.56 + offset),
    ]
    draw.line(points, fill=color, width=height // 24)

for width, height, path in ASSETS:
    img = Image.new("RGB", (width, height), "#031226")
    draw = ImageDraw.Draw(img)
    draw_gradient(draw, (width, height))
    draw_wave(draw, (width, height), offset=16, color=(24, 150, 210))
    draw_wave(draw, (width, height), offset=-10, color=(40, 221, 255))

    title_font = load_font(int(height * 0.24), bold=True)
    tag_font = load_font(int(height * 0.1))
    text = "CoralLedger Blue"
    bbox = draw.textbbox((0, 0), text, font=title_font)
    text_height = bbox[3] - bbox[1]
    draw.text((30, height * 0.35), text, font=title_font, fill="#F4FBFF")
    tagline = "Marine Intelligence for the Bahamas Blue Economy"
    draw.text((30, height * 0.35 + text_height + 12), tagline, font=tag_font, fill="#D7EBFF")

    accent_box = (width - int(width * 0.35), int(height * 0.15), width - int(width * 0.06), int(height * 0.75))
    draw.rectangle(accent_box, outline="#00C5A1", width=5)
    info_path = [
        (accent_box[0] + 10, accent_box[1] + 10),
        (accent_box[0] + 80, accent_box[1] + 80),
        (accent_box[2] - 10, accent_box[1] + 30),
        (accent_box[2] - 60, accent_box[3] - 20),
    ]
    draw.polygon(info_path, outline="#2EE3FF", fill=None, width=3)
    img.save(path)
