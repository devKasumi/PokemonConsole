#!/usr/bin/env python3
"""
insert_images_pptx.py

Usage:
  python insert_images_pptx.py --output ./Pokemon_Report.pptx
Options:
  --project-root PATH    (default: current working dir)
  --output PATH          (required) output pptx path
  --image-exts           (optional) comma-separated image extensions (default: .png,.jpg,.jpeg)
  --title                (optional) Presentation title
Behaviour:
  - Looks for Diagram/UnifiedUseCases and subfolders:
      Descriptions, ActivityDiagram, SequenceDiagram, TestCases
  - Also looks for Diagram/ClassDiagram and Diagram/DatabaseDiagram for final images
  - Creates slides in order: General requirement -> Use Case Overview -> per-UC slides -> Database/Class diagrams
"""
import os
import sys
import argparse
import re
from pptx import Presentation
from pptx.util import Inches, Pt

# Optional Pillow for better image sizing; fallback to width-only insert if missing
try:
    from PIL import Image
except Exception:
    Image = None

# ---------------- helpers ----------------
def find_files_by_name_contains(base_dir, contains_list, exts):
    if not os.path.isdir(base_dir):
        return None
    for root, _, files in os.walk(base_dir):
        for f in files:
            name = f.lower()
            if any(c.lower() in name for c in contains_list) and any(name.endswith(e) for e in exts):
                return os.path.join(root, f)
    return None

def find_all_images_in_dir(base_dir, exts):
    out = []
    if not os.path.isdir(base_dir):
        return out
    for root, _, files in os.walk(base_dir):
        for f in sorted(files):
            name = f.lower()
            if any(name.endswith(e) for e in exts):
                out.append(os.path.join(root, f))
    return out

def get_uc_list_from_descriptions(desc_dir):
    """Return sorted list of tuples (uc_index:int, path_to_description_file)."""
    ucs = []
    if not os.path.isdir(desc_dir):
        return ucs
    for f in os.listdir(desc_dir):
        m = re.search(r'uc(\d+)', f.lower())
        if m:
            idx = int(m.group(1))
            ucs.append((idx, os.path.join(desc_dir, f)))
    ucs.sort(key=lambda x: x[0])
    return ucs

def image_slide(prs, title, img_path, slide_layout_blank_idx=6):
    # create a slide and insert image centered and scaled to fit within margins
    if slide_layout_blank_idx >= len(prs.slide_layouts):
        layout_idx = 0
    else:
        layout_idx = slide_layout_blank_idx
    slide = prs.slides.add_slide(prs.slide_layouts[layout_idx])
    # Title at top
    try:
        if slide.shapes.title:
            slide.shapes.title.text = title
    except Exception:
        # add simple textbox title
        tx = slide.shapes.add_textbox(Inches(0.5), Inches(0.2), prs.slide_width - Inches(1.0), Inches(0.5))
        tx.text_frame.text = title

    # compute area available (leave title area)
    left_margin = Inches(0.6); right_margin = Inches(0.6)
    top_reserved = Inches(1.0)
    bottom_margin = Inches(0.6)
    avail_w = prs.slide_width - (left_margin + right_margin)
    avail_h = prs.slide_height - (top_reserved + bottom_margin)

    try:
        if Image:
            img = Image.open(img_path)
            dpi = img.info.get('dpi', (96,96))[0] if isinstance(img.info.get('dpi', None), tuple) else 96
            w_px, h_px = img.size
            width_in = w_px / dpi
            height_in = h_px / dpi
            from pptx.util import Inches as _Inches
            pic_w = _Inches(width_in)
            pic_h = _Inches(height_in)
            # scale to fit
            if pic_w > avail_w or pic_h > avail_h:
                scale = min(avail_w / pic_w, avail_h / pic_h)
                pic_w = int(pic_w * scale)
                pic_h = int(pic_h * scale)
            left = int((prs.slide_width - pic_w) / 2)
            top = int(top_reserved + (avail_h - pic_h) / 2)
            slide.shapes.add_picture(img_path, left, top, width=pic_w, height=pic_h)
        else:
            # no pillow - fit by width
            slide.shapes.add_picture(img_path, left_margin, top_reserved, width=avail_w)
    except Exception as e:
        # fallback - just try width insertion
        try:
            slide.shapes.add_picture(img_path, left_margin, top_reserved, width=avail_w)
        except Exception as ex:
            # if still fails, insert text
            body = slide.shapes.add_textbox(Inches(0.5), Inches(1.5), prs.slide_width - Inches(1.0), Inches(3.0))
            body.text_frame.text = f"Could not insert image {img_path}: {ex}"

def text_slide(prs, title, text):
    layout = prs.slide_layouts[1] if len(prs.slide_layouts) > 1 else prs.slide_layouts[0]
    slide = prs.slides.add_slide(layout)
    if slide.shapes.title:
        slide.shapes.title.text = title
    # find content placeholder
    body = None
    for shape in slide.shapes:
        try:
            if shape.is_placeholder and shape.placeholder_format.type == 1:
                body = shape
                break
        except Exception:
            continue
    if body:
        tf = body.text_frame
        tf.clear()
        p = tf.paragraphs[0]
        p.text = text
        p.font.size = Pt(14)
    else:
        tb = slide.shapes.add_textbox(Inches(0.5), Inches(1.2), prs.slide_width - Inches(1.0), prs.slide_height - Inches(1.5))
        tb.text_frame.text = text

def read_text_file(path, max_lines=40):
    try:
        with open(path, 'r', encoding='utf-8') as fh:
            lines = fh.read().splitlines()
            return "\n".join(lines[:max_lines])
    except Exception:
        return os.path.basename(path)

# ---------------- main flow ----------------
def build_presentation(project_root, output_path, exts, title_text):
    prs = Presentation()
    # Title slide
    title_slide_layout = 0 if len(prs.slide_layouts) > 0 else 1
    slide = prs.slides.add_slide(prs.slide_layouts[title_slide_layout])
    if slide.shapes.title:
        slide.shapes.title.text = title_text or "Project Report"
    # 1) General requirement
    gen_dir = os.path.join(project_root, 'Diagram', 'GeneralRequirements')
    # fallback search within UnifiedUseCases descriptions for general files named "general" or "requirements"
    general_img = None
    if os.path.isdir(gen_dir):
        general_img = find_files_by_name_contains(gen_dir, ['general', 'requirement', 'requirements'], exts)
    if not general_img:
        # try UnifiedUseCases/Descriptions
        uc_desc_dir = os.path.join(project_root, 'Diagram', 'UnifiedUseCases', 'Descriptions')
        general_img = find_files_by_name_contains(uc_desc_dir, ['general', 'requirement', 'requirements'], exts)
    if general_img:
        image_slide(prs, "General Requirements", general_img)
    else:
        # if none, add a placeholder text slide
        text_slide(prs, "General Requirements", "General requirements section: (no image found)")

    # 2) Use Case diagram (overview)
    uu_base = os.path.join(project_root, 'Diagram', 'UnifiedUseCases')
    overview = find_files_by_name_contains(uu_base, ['overview','uc_overview','uc-overview','use case overview','usecase_overview'], exts)
    if overview:
        image_slide(prs, "Use Case Diagram - Overview", overview)
    else:
        # maybe there's a .drawio or .puml named overview; attempt to find a file name then warn (we do not auto-render)
        overview_alt = find_files_by_name_contains(uu_base, ['overview','uc_overview','usecase_overview'], ('.puml', '.plantuml', '.drawio'))
        if overview_alt:
            text_slide(prs, "Use Case Diagram - Overview", f"Found overview source ({os.path.basename(overview_alt)}) but no PNG; please export PNG and re-run.")
        else:
            text_slide(prs, "Use Case Diagram - Overview", "Overview not found")

    # 3) Test cases (per UC)
    desc_dir = os.path.join(project_root, 'Diagram', 'UnifiedUseCases', 'Descriptions')
    act_dir = os.path.join(project_root, 'Diagram', 'UnifiedUseCases', 'ActivityDiagram')
    seq_dir = os.path.join(project_root, 'Diagram', 'UnifiedUseCases', 'SequenceDiagram')
    tc_dir  = os.path.join(project_root, 'Diagram', 'UnifiedUseCases', 'TestCases')

    ucs = []
    if os.path.isdir(desc_dir):
        ucs = []
        for f in os.listdir(desc_dir):
            m = re.search(r'uc(\d+)', f.lower())
            if m:
                idx = int(m.group(1))
                ucs.append((idx, os.path.join(desc_dir, f)))
        ucs.sort(key=lambda x: x[0])
    else:
        # fallback: search for any uc images in ActivityDiagram or SequenceDiagram names
        # collect unique uc indices found
        found = set()
        for base in (act_dir, seq_dir, tc_dir):
            if os.path.isdir(base):
                for root, _, files in os.walk(base):
                    for f in files:
                        m = re.search(r'uc(\d+)', f.lower())
                        if m:
                            found.add(int(m.group(1)))
        ucs = sorted([(i, None) for i in found])

    for idx, desc_path in ucs:
        # Summary / Description (prefer image)
        label = f"UC{idx} - Summary"
        desc_img = None
        # look for exact UC{idx}_Description.png or any image containing uc{idx} and 'description'
        if os.path.isdir(desc_dir):
            desc_img = find_files_by_name_contains(desc_dir, [f"uc{idx}", "description", "desc"], exts)
            if not desc_img:
                # try image with just uc{idx}
                desc_img = find_files_by_name_contains(desc_dir, [f"uc{idx}"], exts)
        if not desc_img:
            # try Activity/Sequence/TestCases folders for a description-like image
            desc_img = find_files_by_name_contains(act_dir, [f"uc{idx}", "description"], exts) or \
                       find_files_by_name_contains(seq_dir, [f"uc{idx}", "description"], exts) or \
                       find_files_by_name_contains(tc_dir, [f"uc{idx}", "description"], exts)
        if desc_img:
            image_slide(prs, label, desc_img)
        else:
            # if description file exists (puml/md/txt) show first lines
            if desc_path and os.path.isfile(desc_path):
                ext = os.path.splitext(desc_path)[1].lower()
                if ext in exts:
                    image_slide(prs, label, desc_path)
                else:
                    txt = read_text_file(desc_path, max_lines=40)
                    text_slide(prs, label, txt)
            else:
                text_slide(prs, label, f"Description for UC{idx} not found")

        # Activity diagram
        act_label = f"UC{idx} - Activity Diagram"
        act_img = None
        # look for UC{idx}_Activity.png
        if os.path.isdir(act_dir):
            act_img = find_files_by_name_contains(act_dir, [f"uc{idx}", "activity"], exts)
        if not act_img:
            # maybe Detailed_* exists or UCn_Activity.png already present under act_dir listing
            act_img = find_files_by_name_contains(uu_base, [f"uc{idx}", "activity"], exts)
        if act_img:
            image_slide(prs, act_label, act_img)
        else:
            text_slide(prs, act_label, f"Activity diagram image not found for UC{idx}")

        # Sequence diagram
        seq_label = f"UC{idx} - Sequence Diagram"
        seq_img = None
        if os.path.isdir(seq_dir):
            seq_img = find_files_by_name_contains(seq_dir, [f"uc{idx}", "sequence"], exts)
        if not seq_img:
            seq_img = find_files_by_name_contains(uu_base, [f"uc{idx}", "sequence"], exts)
        if seq_img:
            image_slide(prs, seq_label, seq_img)
        else:
            text_slide(prs, seq_label, f"Sequence diagram image not found for UC{idx}")

        # Test case (images or text)
        tc_label = f"UC{idx} - Test Case"
        tc_img = None
        if os.path.isdir(tc_dir):
            tc_img = find_files_by_name_contains(tc_dir, [f"uc{idx}", "test"], exts)
        if not tc_img:
            tc_img = find_files_by_name_contains(uu_base, [f"uc{idx}", "testcase", "test_case"], exts)
        if tc_img:
            image_slide(prs, tc_label, tc_img)
        else:
            # try text file under TestCases folder
            if os.path.isdir(tc_dir):
                # search for any file containing uc{idx} regardless ext
                for root, _, files in os.walk(tc_dir):
                    for f in files:
                        if f.lower().find(f"uc{idx}") >= 0:
                            path = os.path.join(root, f)
                            txt = read_text_file(path, max_lines=80)
                            text_slide(prs, tc_label, txt)
                            tc_img = True
                            break
                    if tc_img:
                        break
            if not tc_img:
                text_slide(prs, tc_label, f"Test cases not found for UC{idx}")

    # 4) Database diagram(s) and Class diagram(s)
    db_dir = os.path.join(project_root, 'Diagram', 'DatabaseDiagram')
    class_dir = os.path.join(project_root, 'Diagram', 'ClassDiagram')
    db_images = find_all_images_in_dir(db_dir, exts)
    class_images = find_all_images_in_dir(class_dir, exts)
    if db_images:
        for im in db_images:
            image_slide(prs, "Database Diagram - " + os.path.basename(im), im)
    else:
        text_slide(prs, "Database Diagram", "No database diagrams found")

    if class_images:
        for im in class_images:
            image_slide(prs, "Class Diagram - " + os.path.basename(im), im)
    else:
        text_slide(prs, "Class Diagram", "No class diagrams found")

    # Save
    prs.save(output_path)
    print("Saved:", output_path)
    return output_path

# --------------- CLI ---------------
def parse_args():
    p = argparse.ArgumentParser()
    p.add_argument('--project-root', default=os.getcwd(), help='Project root (default: current dir)')
    p.add_argument('--output', required=True, help='Output PPTX path')
    p.add_argument('--image-exts', default='.png,.jpg,.jpeg', help='Comma-separated image extensions to consider')
    p.add_argument('--title', default='Project Report', help='Presentation title')
    return p.parse_args()

def main():
    args = parse_args()
    exts = tuple(e.strip().lower() for e in args.image_exts.split(',') if e.strip())
    build_presentation(args.project_root, args.output, exts, args.title)

if __name__ == '__main__':
    main()