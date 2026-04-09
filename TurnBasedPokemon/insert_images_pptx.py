#!/usr/bin/env python3
"""
insert_images_pptx.py

Build a PPTX by inserting images per UC in order (default UC1..UC18).

Usage:
  python insert_images_pptx.py --output ./Pokemon_Report.pptx
  python insert_images_pptx.py --output ./out.pptx --start 1 --end 18 --project-root "C:\Pokemon\PokemonConsole\TurnBasedPokemon"

It searches under:
  Diagram/UnifiedUseCases/{Descriptions,ActivityDiagram,SequenceDiagram,TestCases}
and also appends images from:
  Diagram/DatabaseDiagram and Diagram/ClassDiagram

It matches filenames containing patterns like:
  uc1, uc01, UC1_Login, Detailed_UC1_Login_Activity, etc.
"""
import os
import re
import argparse
from pptx import Presentation
from pptx.util import Inches, Pt

try:
    from PIL import Image
except Exception:
    Image = None

# ---------------- helpers ----------------
def norm_candidates_for_uc(idx):
    """Return list of filename substrings to match for UC index (covering padded and variants)."""
    return [f"uc{idx}", f"uc_{idx}", f"uc{idx:02d}", f"uc_{idx:02d}", f"-uc{idx}-", f"_{idx}_", f"{idx}"]

def find_files_by_name_contains(base_dir, contains_list, exts):
    """Return first matching file path under base_dir whose name contains any substring in contains_list and endswith one of exts."""
    if not base_dir or not os.path.isdir(base_dir):
        return None
    contains_lower = [c.lower() for c in contains_list]
    for root, _, files in os.walk(base_dir):
        for f in files:
            fn = f.lower()
            if not any(fn.endswith(e) for e in exts):
                continue
            if any(c in fn for c in contains_lower):
                return os.path.join(root, f)
    return None

def find_all_images_in_dir(base_dir, exts):
    """Return sorted list of image paths inside base_dir (recursively)."""
    imgs = []
    if not base_dir or not os.path.isdir(base_dir):
        return imgs
    for root, _, files in os.walk(base_dir):
        for f in sorted(files):
            fn = f.lower()
            if any(fn.endswith(e) for e in exts):
                imgs.append(os.path.join(root, f))
    return imgs

def read_text_file(path, max_lines=80):
    try:
        with open(path, 'r', encoding='utf-8') as fh:
            lines = fh.read().splitlines()
            return "\n".join(lines[:max_lines])
    except Exception:
        return os.path.basename(path)

# ---------------- PPT helpers ----------------
def add_title_slide(prs, title_text):
    layout = prs.slide_layouts[0] if len(prs.slide_layouts) > 0 else prs.slide_layouts[1]
    slide = prs.slides.add_slide(layout)
    if slide.shapes.title:
        slide.shapes.title.text = title_text
    return slide

def add_text_slide(prs, title, body_text):
    layout = prs.slide_layouts[1] if len(prs.slide_layouts) > 1 else prs.slide_layouts[0]
    slide = prs.slides.add_slide(layout)
    if slide.shapes.title:
        slide.shapes.title.text = title
    # find body placeholder if present
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
        p.text = body_text
        p.font.size = Pt(14)
    else:
        tx = slide.shapes.add_textbox(Inches(0.5), Inches(1.2), prs.slide_width - Inches(1.0), prs.slide_height - Inches(1.5))
        tx.text_frame.text = body_text
    return slide

def add_image_slide(prs, title, img_path):
    # choose blank-like layout (index 6 usually), else fallback
    blank_idx = 6 if len(prs.slide_layouts) > 6 else (5 if len(prs.slide_layouts) > 5 else 0)
    slide = prs.slides.add_slide(prs.slide_layouts[blank_idx])
    # title
    try:
        if slide.shapes.title:
            slide.shapes.title.text = title
    except Exception:
        slide.shapes.add_textbox(Inches(0.5), Inches(0.2), prs.slide_width - Inches(1.0), Inches(0.5)).text_frame.text = title

    # available area
    left_margin = Inches(0.6)
    right_margin = Inches(0.6)
    top_reserved = Inches(1.0)
    bottom_margin = Inches(0.6)
    avail_w = prs.slide_width - (left_margin + right_margin)
    avail_h = prs.slide_height - (top_reserved + bottom_margin)

    # try to use PIL to compute image physical size and scale
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
            # no PIL: insert scaled by width
            slide.shapes.add_picture(img_path, left_margin, top_reserved, width=avail_w)
    except Exception:
        # fallback simple
        try:
            slide.shapes.add_picture(img_path, left_margin, top_reserved, width=avail_w)
        except Exception as e:
            add_text_slide(prs, title, f"Could not insert image {img_path}: {e}")

# ---------------- core flow ----------------
def build_ppt(project_root, output, start_uc=1, end_uc=18, title_text="Project Report"):
    exts = ('.png', '.jpg', '.jpeg')
    # common dirs
    uu_base = os.path.join(project_root, 'Diagram', 'UnifiedUseCases')
    desc_dir = os.path.join(uu_base, 'Descriptions')
    act_dir = os.path.join(uu_base, 'ActivityDiagram')
    seq_dir = os.path.join(uu_base, 'SequenceDiagram')
    tc_dir  = os.path.join(uu_base, 'TestCases')
    class_dir = os.path.join(project_root, 'Diagram', 'ClassDiagram')
    db_dir = os.path.join(project_root, 'Diagram', 'DatabaseDiagram')

    prs = Presentation()
    add_title_slide(prs, title_text)

    # General requirement: try Diagram/GeneralRequirements or look in Descriptions for "general"
    gen_dir = os.path.join(project_root, 'Diagram', 'GeneralRequirements')
    gen_img = None
    if os.path.isdir(gen_dir):
        gen_img = find_files_by_name_contains(gen_dir, ['general','requirement','requirements'], exts)
    if not gen_img and os.path.isdir(desc_dir):
        gen_img = find_files_by_name_contains(desc_dir, ['general','requirement','requirements'], exts)
    if gen_img:
        add_image_slide(prs, "General Requirements", gen_img)
    else:
        add_text_slide(prs, "General Requirements", "General requirements: (no image found)")

    # Use Case Overview
    overview = None
    if os.path.isdir(uu_base):
        overview = find_files_by_name_contains(uu_base, ['overview','uc_overview','use case overview','usecase_overview','uc-overview'], exts)
    if overview:
        add_image_slide(prs, "Use Case Diagram - Overview", overview)
    else:
        add_text_slide(prs, "Use Case Diagram - Overview", "Overview image not found (place UC_Overview.png or similar in UnifiedUseCases)")

    # For each UC in requested range
    for uc in range(start_uc, end_uc + 1):
        # Summary / Description
        label = f"UC{uc} - Summary"
        desc_img = None
        # look in Descriptions for image first
        if os.path.isdir(desc_dir):
            patterns = norm_candidates_for_uc(uc) + ['description', 'desc']
            desc_img = find_files_by_name_contains(desc_dir, patterns, exts)
            if not desc_img:
                # look for any file in descriptions that contains uc number regardless of 'description'
                desc_img = find_files_by_name_contains(desc_dir, norm_candidates_for_uc(uc), exts)
        if not desc_img:
            # try Activity/Sequence/TestCases for a description-like image
            for base in (act_dir, seq_dir, tc_dir):
                p = find_files_by_name_contains(base, norm_candidates_for_uc(uc), exts)
                if p:
                    desc_img = p
                    break
        if desc_img:
            add_image_slide(prs, label, desc_img)
        else:
            # if a description text file exists, show text
            desc_text = None
            if os.path.isdir(desc_dir):
                # try to find text/md/puml names for this UC
                for ext in ('.md', '.txt', '.puml', '.plantuml'):
                    candidate = find_files_by_name_contains(desc_dir, norm_candidates_for_uc(uc), (ext,))
                    if candidate:
                        desc_text = read_text_file(candidate)
                        break
            if desc_text:
                add_text_slide(prs, label, desc_text)
            else:
                add_text_slide(prs, label, f"No description found for UC{uc}")

        # Activity
        act_label = f"UC{uc} - Activity Diagram"
        act_img = None
        # try common names in activity dir
        if os.path.isdir(act_dir):
            act_img = find_files_by_name_contains(act_dir, norm_candidates_for_uc(uc) + ['activity'], exts)
            if not act_img:
                act_img = find_files_by_name_contains(act_dir, norm_candidates_for_uc(uc), exts)
        if not act_img:
            act_img = find_files_by_name_contains(uu_base, norm_candidates_for_uc(uc) + ['activity'], exts)
        if act_img:
            add_image_slide(prs, act_label, act_img)
        else:
            add_text_slide(prs, act_label, f"Activity diagram image not found for UC{uc}")

        # Sequence
        seq_label = f"UC{uc} - Sequence Diagram"
        seq_img = None
        if os.path.isdir(seq_dir):
            seq_img = find_files_by_name_contains(seq_dir, norm_candidates_for_uc(uc) + ['sequence'], exts)
            if not seq_img:
                seq_img = find_files_by_name_contains(seq_dir, norm_candidates_for_uc(uc), exts)
        if not seq_img:
            seq_img = find_files_by_name_contains(uu_base, norm_candidates_for_uc(uc) + ['sequence'], exts)
        if seq_img:
            add_image_slide(prs, seq_label, seq_img)
        else:
            add_text_slide(prs, seq_label, f"Sequence diagram image not found for UC{uc}")

        # Test case
        tc_label = f"UC{uc} - Test Case"
        tc_img = None
        if os.path.isdir(tc_dir):
            tc_img = find_files_by_name_contains(tc_dir, norm_candidates_for_uc(uc) + ['test','testcase','case'], exts)
            if not tc_img:
                tc_img = find_files_by_name_contains(tc_dir, norm_candidates_for_uc(uc), exts)
        if not tc_img:
            tc_img = find_files_by_name_contains(uu_base, norm_candidates_for_uc(uc) + ['test','testcase','case'], exts)
        if tc_img:
            add_image_slide(prs, tc_label, tc_img)
        else:
            # try text test case files under TestCases
            if os.path.isdir(tc_dir):
                found_text = None
                for root, _, files in os.walk(tc_dir):
                    for f in files:
                        if any(s in f.lower() for s in norm_candidates_for_uc(uc)):
                            path = os.path.join(root, f)
                            found_text = read_text_file(path)
                            break
                    if found_text:
                        break
                if found_text:
                    add_text_slide(prs, tc_label, found_text)
                else:
                    add_text_slide(prs, tc_label, f"Test cases not found for UC{uc}")
            else:
                add_text_slide(prs, tc_label, f"Test cases not found for UC{uc}")

    # append DB and Class diagrams
    db_images = find_all_images_in_dir(db_dir, exts)
    class_images = find_all_images_in_dir(class_dir, exts)
    if db_images:
        for im in db_images:
            add_image_slide(prs, "Database Diagram - " + os.path.basename(im), im)
    else:
        add_text_slide(prs, "Database Diagram", "No database diagrams found")

    if class_images:
        for im in class_images:
            add_image_slide(prs, "Class Diagram - " + os.path.basename(im), im)
    else:
        add_text_slide(prs, "Class Diagram", "No class diagrams found")

    # save
    prs.save(output)
    print("Saved:", output)
    return output

# ---------------- CLI ----------------
def parse_args():
    p = argparse.ArgumentParser()
    p.add_argument('--project-root', default=os.getcwd(), help='Project root directory (default current dir)')
    p.add_argument('--output', '-o', required=True, help='Output PPTX file path')
    p.add_argument('--start', type=int, default=1, help='Start UC index (default 1)')
    p.add_argument('--end', type=int, default=18, help='End UC index (default 18)')
    p.add_argument('--title', default='Project Report', help='Presentation title')
    return p.parse_args()

def main():
    args = parse_args()
    build_ppt(args.project_root, os.path.abspath(args.output), start_uc=args.start, end_uc=args.end, title_text=args.title)

if __name__ == '__main__':
    main()