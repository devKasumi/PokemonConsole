#!/usr/bin/env python3
"""
insert_images_pptx.py (patched: robust UC detection)
Usage:
  python insert_images_pptx.py --output ./out.pptx [--start 1 --end 18]
"""
import os, re, argparse
from pptx import Presentation
from pptx.util import Inches, Pt

try:
    from PIL import Image
except Exception:
    Image = None

def add_design_details_section(prs, img_dir):
    from pptx.util import Inches
    # Header slide
    slide = prs.slides.add_slide(prs.slide_layouts[0])
    if slide.shapes.title:
        slide.shapes.title.text = "Design Details"
    img_paths = sorted([os.path.join(img_dir, x) for x in os.listdir(img_dir) if x.lower().endswith('.png')])
    for p in img_paths:
        name = os.path.splitext(os.path.basename(p))[0]
        s = prs.slides.add_slide(prs.slide_layouts[6] if len(prs.slide_layouts)>6 else 0)
        s.shapes.add_textbox(Inches(0.5), Inches(0.2), Inches(8), Inches(0.5)).text_frame.text = name
        l = Inches(0.5); r = Inches(0.5); t = Inches(1.0); b = Inches(0.5)
        aw = prs.slide_width - l - r; ah = prs.slide_height - t - b
        try:
            from PIL import Image
            im = Image.open(p)
            iw, ih = im.size
            scale = min(aw/iw, ah/ih)
            iw = int(iw*scale); ih = int(ih*scale)
            left = int((prs.slide_width - iw)/2)
            top = int(t + (ah - ih)/2)
            s.shapes.add_picture(p, left, top, width=iw, height=ih)
        except Exception:
            s.shapes.add_picture(p, l, t, width=aw)

def find_all_images(base_dir, exts=('.png','.jpg','.jpeg')):
    out=[]
    if not os.path.isdir(base_dir):
        return out
    for root,_,files in os.walk(base_dir):
        for f in sorted(files):
            if f.lower().endswith(exts):
                out.append(os.path.join(root,f))
    return out

def extract_numbers_from_filename(filename):
    return [int(n.lstrip('0') or 0) for n in re.findall(r'\d+', filename)]

def find_matches_for_uc(base_dir, idx, exts=('.png','.jpg','.jpeg','.puml','.plantuml','.md','.txt')):
    """
    Return list of matching files under base_dir for UC idx.
    Heuristics (priority):
     1) token-based 'uc' + idx with optional leading zeros, e.g. 'uc1', 'uc01' (must be standalone token)
     2) numeric token equal to idx (not part of longer number)
     3) boundary-ish matches like _1_, -1-, start/end variants
    This avoids substring matches (so 'uc1' will NOT match 'uc10').
    """
    results = []
    if not os.path.isdir(base_dir):
        return results

    idx_str = str(idx)
    idx_z = rf'0*{idx}'            # allow leading zeros
    # pattern1: uc + idx as a token (no digit immediately before/after)
    pat_uc = re.compile(rf'(?<!\d)uc{idx_z}(?!\d)', re.IGNORECASE)
    # pattern2: numeric token equal to idx (not part of longer number)
    pat_num = re.compile(rf'(?<!\d){idx_str}(?!\d)', re.IGNORECASE)
    # pattern3: boundary-ish like _1_ or -1- or start/end variants
    pat_boundary = re.compile(rf'(^|[_\-\.\s])0*{idx}([_\-\.\s]|$)', re.IGNORECASE)

    for root, _, files in os.walk(base_dir):
        for f in sorted(files):
            lf = f.lower()
            if not lf.endswith(exts):
                continue

            # 1) uc{idx} token (highest priority)
            if pat_uc.search(lf):
                results.append(os.path.join(root, f))
                continue

            # 2) numeric token match (exact number token)
            if pat_num.search(lf):
                results.append(os.path.join(root, f))
                continue

            # 3) boundary-ish matches
            if pat_boundary.search(lf):
                results.append(os.path.join(root, f))
                continue

    seen = set(); uniq = []
    for p in results:
        if p not in seen:
            uniq.append(p); seen.add(p)
    return uniq

# Minimal PPT helper (insertion scaled by width/height)
def add_title(prs, title):
    layout = prs.slide_layouts[0] if len(prs.slide_layouts)>0 else prs.slide_layouts[1]
    slide = prs.slides.add_slide(layout)
    if slide.shapes.title: slide.shapes.title.text = title

def add_text_slide(prs, title, text):
    layout = prs.slide_layouts[1] if len(prs.slide_layouts)>1 else prs.slide_layouts[0]
    slide = prs.slides.add_slide(layout)
    if slide.shapes.title: slide.shapes.title.text = title
    # content placeholder
    body=None
    for shp in slide.shapes:
        try:
            if shp.is_placeholder and shp.placeholder_format.type==1:
                body=shp; break
        except Exception:
            continue
    if body:
        tf = body.text_frame; tf.clear(); tf.paragraphs[0].text = text
    else:
        tb = slide.shapes.add_textbox(Inches(0.5), Inches(1.2), prs.slide_width-Inches(1.0), prs.slide_height-Inches(1.5))
        tb.text_frame.text = text

def add_image_slide(prs, title, img_path):
    blank = 6 if len(prs.slide_layouts)>6 else 5 if len(prs.slide_layouts)>5 else 0
    slide = prs.slides.add_slide(prs.slide_layouts[blank])
    try:
        if slide.shapes.title: slide.shapes.title.text = title
    except Exception:
        slide.shapes.add_textbox(Inches(0.5), Inches(0.2), prs.slide_width-Inches(1.0), Inches(0.5)).text_frame.text = title
    # compute area
    l = Inches(0.6); r = Inches(0.6); top = Inches(1.0); bottom = Inches(0.6)
    avail_w = prs.slide_width - (l + r)
    avail_h = prs.slide_height - (top + bottom)
    try:
        if Image:
            img = Image.open(img_path)
            dpi = img.info.get('dpi', (96,96))[0] if isinstance(img.info.get('dpi', None), tuple) else 96
            w_px, h_px = img.size
            from pptx.util import Inches as _Inches
            pic_w = _Inches(w_px / dpi); pic_h = _Inches(h_px / dpi)
            if pic_w > avail_w or pic_h > avail_h:
                scale = min(avail_w / pic_w, avail_h / pic_h)
                pic_w = int(pic_w * scale); pic_h = int(pic_h * scale)
            left = int((prs.slide_width - pic_w)/2); top_pos = int(top + (avail_h - pic_h)/2)
            slide.shapes.add_picture(img_path, left, top_pos, width=pic_w, height=pic_h)
        else:
            slide.shapes.add_picture(img_path, l, top, width=avail_w)
    except Exception as e:
        try:
            slide.shapes.add_picture(img_path, l, top, width=avail_w)
        except Exception as ee:
            add_text_slide(prs, title, f"Insert failed for {img_path}: {ee}")

def build(project_root, output, start=1, end=18):
    uu = os.path.join(project_root, 'Diagram','UnifiedUseCases')
    desc = os.path.join(uu,'Descriptions'); act = os.path.join(uu,'ActivityDiagram'); seq = os.path.join(uu,'SequenceDiagram'); tc = os.path.join(uu,'TestCases')
    classd = os.path.join(project_root,'Diagram','ClassDiagram'); db = os.path.join(project_root,'Diagram','DatabaseDiagram')
    exts = ('.png','.jpg','.jpeg')

    prs = Presentation()
    add_title(prs, "Project Report")

    # General
    gen = os.path.join(project_root,'Diagram','GeneralRequirements')
    gen_found = None
    if os.path.isdir(gen):
        gen_found = find_all_images(gen, exts)
        if gen_found:
            add_image_slide(prs, "General Requirements", gen_found[0])
    if not gen_found:
        add_text_slide(prs, "General Requirements","No general requirement image found")

    # Overview (search for files containing 'overview' or 'uc_overview')
    overview = None
    if os.path.isdir(uu):
        for root,_,files in os.walk(uu):
            for f in files:
                if 'overview' in f.lower() and f.lower().endswith(exts):
                    overview = os.path.join(root,f); break
            if overview: break
    if overview:
        add_image_slide(prs, "Use Case Diagram - Overview", overview)
    else:
        add_text_slide(prs, "Use Case Diagram - Overview", "Overview not found")

    # per UC
    for uc in range(start, end+1):
        # summary
        label = f"UC{uc} - Summary"
        # look in descriptions by numeric match then contains
        found = []
        if os.path.isdir(desc):
            found = find_matches_for_uc(desc, uc, exts)
        if not found and os.path.isdir(uu):
            found = find_matches_for_uc(uu, uc, exts)
        if found:
            add_image_slide(prs, label, found[0])
        else:
            # try text description files
            txt = None
            if os.path.isdir(desc):
                for root,_,files in os.walk(desc):
                    for f in files:
                        if uc in extract_numbers_from_filename(f) and f.lower().endswith(('.md','.txt','.puml','.plantuml')):
                            txt = os.path.join(root,f); break
                    if txt: break
            if txt:
                add_text_slide(prs, label, read := open(txt, 'r', encoding='utf-8', errors='ignore').read()[:4000])
            else:
                add_text_slide(prs, label, f"No description for UC{uc}")

        # activity
        act_label = f"UC{uc} - Activity Diagram"
        act_found = []
        if os.path.isdir(act):
            act_found = find_matches_for_uc(act, uc, exts)
        if not act_found:
            act_found = find_matches_for_uc(uu, uc, exts)
        if act_found:
            add_image_slide(prs, act_label, act_found[0])
        else:
            add_text_slide(prs, act_label, f"Activity diagram image not found for UC{uc}")

        # sequence
        seq_label = f"UC{uc} - Sequence Diagram"
        seq_found = []
        if os.path.isdir(seq):
            seq_found = find_matches_for_uc(seq, uc, exts)
        if not seq_found:
            seq_found = find_matches_for_uc(uu, uc, exts)
        if seq_found:
            add_image_slide(prs, seq_label, seq_found[0])
        else:
            add_text_slide(prs, seq_label, f"Sequence diagram image not found for UC{uc}")

        # test case
        tc_label = f"UC{uc} - Test Case"
        tc_found = []
        if os.path.isdir(tc):
            tc_found = find_matches_for_uc(tc, uc, exts)
        if not tc_found:
            tc_found = find_matches_for_uc(uu, uc, exts)
        if tc_found:
            add_image_slide(prs, tc_label, tc_found[0])
        else:
            # try text
            textpath=None
            if os.path.isdir(tc):
                for root,_,files in os.walk(tc):
                    for f in files:
                        if uc in extract_numbers_from_filename(f):
                            textpath = os.path.join(root,f); break
                    if textpath: break
            if textpath:
                add_text_slide(prs, tc_label, open(textpath,'r',encoding='utf-8',errors='ignore').read()[:4000])
            else:
                add_text_slide(prs, tc_label, f"Test cases not found for UC{uc}")

    # append db/class
    db_imgs = find_all_images(db, exts); class_imgs = find_all_images(classd, exts)
    if db_imgs:
        for im in db_imgs: add_image_slide(prs, "Database Diagram - "+os.path.basename(im), im)
    else: add_text_slide(prs, "Database Diagram", "No DB diagrams found")
    if class_imgs:
        for im in class_imgs: add_image_slide(prs, "Class Diagram - "+os.path.basename(im), im)
    else: add_text_slide(prs, "Class Diagram","No class diagrams found")

    prs.save(output)
    print("Saved", output)

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument('--project-root', default=os.getcwd())
    ap.add_argument('--output', required=True)
    ap.add_argument('--start', type=int, default=1)
    ap.add_argument('--end', type=int, default=18)
    args = ap.parse_args()
    build(args.project_root, os.path.abspath(args.output), start=args.start, end=args.end)

img_dir = os.path.join(project_root, 'Documentation', 'Screenshots_genimg')
if os.path.isdir(img_dir):
    add_design_details_section(prs, img_dir)