# append_diagrams.py
# Usage: python append_diagrams.py --ppt Pokemon_GameProject_Report_auto.pptx

import os, argparse
from pptx import Presentation
from pptx.util import Inches

def add_image_slide(prs, image_path):
    # use blank layout or first layout if blank not available
    blank_slide_layout = prs.slide_layouts[6] if len(prs.slide_layouts) > 6 else prs.slide_layouts[0]
    slide = prs.slides.add_slide(blank_slide_layout)
    # place image centered, full width with preserved aspect
    slide_width = prs.slide_width
    slide_height = prs.slide_height
    # read image size
    from PIL import Image
    img = Image.open(image_path)
    w_px, h_px = img.size
    dpi = img.info.get('dpi', (96,96))[0] if isinstance(img.info.get('dpi', None), tuple) else 96
    # convert px to EMU via Inches: inches = px / dpi
    width_in = w_px / dpi
    height_in = h_px / dpi
    from pptx.util import Inches
    pic_w = Inches(width_in)
    pic_h = Inches(height_in)
    # scale down if larger than slide
    if pic_w > slide_width:
        scale = slide_width / pic_w
        pic_w *= scale
        pic_h *= scale
    if pic_h > slide_height:
        scale = slide_height / pic_h
        pic_h *= scale
        pic_w *= scale
    left = int((slide_width - pic_w) / 2)
    top = int((slide_height - pic_h) / 2)
    slide.shapes.add_picture(image_path, left, top, pic_w, pic_h)

def collect_images(dirs, exts=('.png','.jpg','.jpeg','.svg')):
    imgs = []
    for d in dirs:
        if not os.path.isdir(d):
            continue
        for root,_,files in os.walk(d):
            for f in sorted(files):
                if f.lower().endswith(exts):
                    imgs.append(os.path.join(root, f))
    return imgs

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--ppt', required=True, help='Path to pptx to append to')
    parser.add_argument('--dirs', nargs='*', default=['Diagram\\ClassDiagram','Diagram\\DatabaseDiagram'], help='Relative dirs to search for images')
    args = parser.parse_args()

    ppt_path = args.ppt
    if not os.path.isfile(ppt_path):
        raise SystemExit(f"PPTX not found: {ppt_path}")

    prs = Presentation(ppt_path)
    imgs = collect_images([os.path.abspath(d) for d in args.dirs])
    if not imgs:
        print("No images found in", args.dirs)
    else:
        print("Appending images:", len(imgs))
        for im in imgs:
            print(" ->", im)
            add_image_slide(prs, im)
        prs.save(ppt_path)
        print("Saved:", ppt_path)

if __name__ == '__main__':
    main()