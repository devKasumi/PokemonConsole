"""
Script to generate PowerPoint presentation for Pokemon Console Game
Includes: Requirements, Diagrams, Database Design, and Architecture
"""

from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.dml.color import RGBColor

# Create presentation
prs = Presentation()
prs.slide_width = Inches(10)
prs.slide_height = Inches(7.5)

# Define color scheme
TITLE_COLOR = RGBColor(25, 118, 210)  # Blue
ACCENT_COLOR = RGBColor(244, 67, 54)  # Red
TEXT_COLOR = RGBColor(33, 33, 33)    # Dark Gray

def add_title_slide(prs, title, subtitle):
    """Add a title slide"""
    slide = prs.slides.add_slide(prs.slide_layouts[6])  # Blank layout
    background = slide.background
    fill = background.fill
    fill.solid()
    fill.fore_color.rgb = TITLE_COLOR
    
    # Title
    title_box = slide.shapes.add_textbox(Inches(0.5), Inches(2.5), Inches(9), Inches(1.5))
    title_frame = title_box.text_frame
    title_frame.text = title
    title_frame.word_wrap = True
    for paragraph in title_frame.paragraphs:
        paragraph.font.size = Pt(54)
        paragraph.font.bold = True
        paragraph.font.color.rgb = RGBColor(255, 255, 255)
    
    # Subtitle
    subtitle_box = slide.shapes.add_textbox(Inches(0.5), Inches(4.2), Inches(9), Inches(1))
    subtitle_frame = subtitle_box.text_frame
    subtitle_frame.text = subtitle
    for paragraph in subtitle_frame.paragraphs:
        paragraph.font.size = Pt(24)
        paragraph.font.color.rgb = RGBColor(255, 255, 255)

def add_content_slide(prs, title, content_list):
    """Add a content slide with bullet points"""
    slide = prs.slides.add_slide(prs.slide_layouts[6])  # Blank
    
    # Title
    title_box = slide.shapes.add_textbox(Inches(0.5), Inches(0.3), Inches(9), Inches(0.7))
    title_frame = title_box.text_frame
    title_frame.text = title
    title_frame.paragraphs[0].font.size = Pt(44)
    title_frame.paragraphs[0].font.bold = True
    title_frame.paragraphs[0].font.color.rgb = TITLE_COLOR
    
    # Add line separator
    line_shape = slide.shapes.add_shape(1, Inches(0.5), Inches(1.05), Inches(9), Inches(0))
    line_shape.line.color.rgb = ACCENT_COLOR
    line_shape.line.width = Pt(2)
    
    # Content
    content_box = slide.shapes.add_textbox(Inches(0.8), Inches(1.3), Inches(8.5), Inches(5.8))
    text_frame = content_box.text_frame
    text_frame.word_wrap = True
    
    for i, item in enumerate(content_list):
        if i == 0:
            p = text_frame.paragraphs[0]
        else:
            p = text_frame.add_paragraph()
        p.text = item
        p.level = 0
        p.font.size = Pt(18)
        p.font.color.rgb = TEXT_COLOR
        p.space_before = Pt(6)
        p.space_after = Pt(6)

def add_table_slide(prs, title, table_data):
    """Add a slide with a table"""
    slide = prs.slides.add_slide(prs.slide_layouts[6])
    
    # Title
    title_box = slide.shapes.add_textbox(Inches(0.5), Inches(0.3), Inches(9), Inches(0.7))
    title_frame = title_box.text_frame
    title_frame.text = title
    title_frame.paragraphs[0].font.size = Pt(40)
    title_frame.paragraphs[0].font.bold = True
    title_frame.paragraphs[0].font.color.rgb = TITLE_COLOR
    
    line = slide.shapes.add_shape(1, Inches(0.5), Inches(1.05), Inches(9), Inches(0))
    line.line.color.rgb = ACCENT_COLOR
    line.line.width = Pt(2)
    
    # Table
    rows, cols = len(table_data), len(table_data[0])
    left = Inches(0.6)
    top = Inches(1.3)
    width = Inches(8.8)
    height = Inches(5.5)
    
    table_shape = slide.shapes.add_table(rows, cols, left, top, width, height).table
    
    for i, row_data in enumerate(table_data):
        for j, cell_data in enumerate(row_data):
            cell = table_shape.cell(i, j)
            cell.text = str(cell_data)
            for paragraph in cell.text_frame.paragraphs:
                paragraph.font.size = Pt(10)
                if i == 0:  # Header row
                    paragraph.font.bold = True
                    paragraph.font.color.rgb = RGBColor(255, 255, 255)
                    cell.fill.solid()
                    cell.fill.fore_color.rgb = TITLE_COLOR
                else:
                    paragraph.font.color.rgb = TEXT_COLOR

# Slide 1: Title
add_title_slide(prs, "Pokemon Console Game", "Project Requirements & Architecture")

# Slide 2: Project Overview
add_content_slide(prs, "Project Overview", [
    "• A console-based Pokemon adventure game",
    "• Features: Registration, Login, Story exploration, Battles, Item management",
    "• Turn-based battle system with NPC trainers, Gym Leaders, Elite Four, Champion",
    "• Save/Load game progress to database",
    "• PvP support with battle matchmaking",
    "• Inventory system for items (Healing, Capture) and Pokemon team management"
])

# Slide 3: Core Features
add_content_slide(prs, "Core Features", [
    "1. User Authentication - Login and registration system",
    "2. Story Mode - Multiple phases with locations and NPCs",
    "3. Battle System - Turn-based combat with type advantages",
    "4. Pokemon Management - Team building and evolution",
    "5. Item System - Healing items, Pokeballs, quest items",
    "6. Experience & Leveling - Pokemon gain EXP and level up",
    "7. Pokedex - Track encountered and caught Pokemon",
    "8. Save/Load - Manual game progress persistence",
    "9. PvP Mode - Online player vs player battles"
])

# Slide 4: Actors & Use Cases
add_content_slide(prs, "Main Actors & Use Cases", [
    "Actors:",
    "  • Player - Main actor, explores world and battles",
    "  • NPC - Gym Leaders, Elite Four, Champion, Trainers",
    "",
    "Key Use Cases (18 total):",
    "  • UC1: Register / Login",
    "  • UC2-UC11: Story exploration, battles, items, NPC interaction",
    "  • UC12: Pokemon level up and evolution",
    "  • UC13: Heal team at Pokemon Center",
    "  • UC14-UC16: Challenge Gym Leader / League / Trainer",
    "  • UC17: Select starter Pokemon",
    "  • UC18: Save/Load game"
])

# Slide 5: System Architecture
add_content_slide(prs, "System Architecture", [
    "Layered Architecture:",
    "",
    "▸ Presentation Layer - Console UI (LoginScreen, BattleScreen, StoryScreen)",
    "▸ Application Layer - Business logic (BattleService, StoryService)",
    "▸ Domain Layer - Core entities (Player, Pokemon, NPC, Items)",
    "▸ Infrastructure Layer - Persistence (DB repos, File I/O)",
    "",
    "Key Services:",
    "  • GameSession - Manages user, player, and battle state",
    "  • BattleService - Handles turn-based combat logic",
    "  • StoryService - World generation and narrative management"
])

# Slide 6: Database Structure
add_content_slide(prs, "Database Structure (7 Tables)", [
    "Dynamic Data (User Progress):",
    "  • users - User accounts (Id, Username, Password)",
    "  • players - Player progress (UserId, CurrentPhase, Badges, Inventory)",
    "  • pokemons - Individual Pokemon (PlayerId, Level, HP, Moves, Status)",
    "",
    "Static Data (Game Configuration):",
    "  • pokedex - Pokemon species and base stats",
    "  • items - Item definitions (Healing, Capture, General)",
    "  • game_phases - Story chapters",
    "  • locations - Areas with wild encounters and NPCs"
])

# Slide 7: Database ERM (table details)
table_data_users = [
    ["Field", "Type", "Key", "Constraint"],
    ["Id", "INT", "PK", "AUTO_INCREMENT"],
    ["Username", "VARCHAR(50)", "UQ", "NOT NULL, UNIQUE"],
    ["Password", "VARCHAR(255)", "", "NOT NULL"]
]
add_table_slide(prs, "Table: users", table_data_users)

# Slide 8: Database - players table
table_data_players = [
    ["Field", "Type", "Key", "Constraint"],
    ["UserId", "INT", "PK,FK", "FOREIGN KEY (users.Id)"],
    ["Name", "VARCHAR(100)", "", ""],
    ["CurrentPhase", "INT", "", "DEFAULT 1"],
    ["CurrentLocation", "VARCHAR(100)", "", ""],
    ["Badges", "TEXT", "", "JSON array"],
    ["IsChampion", "TINYINT(1)", "", "DEFAULT 0"],
    ["PvPWins", "INT", "", "DEFAULT 0"]
]
add_table_slide(prs, "Table: players", table_data_players)

# Slide 9: Database - pokemons table
table_data_pokemons = [
    ["Field", "Type", "Key", "Constraint"],
    ["Id", "INT", "PK", "AUTO_INCREMENT"],
    ["PlayerId", "INT", "FK", "FOREIGN KEY (players)"],
    ["SpeciesId", "INT", "", "References pokedex"],
    ["Level", "INT", "", "NOT NULL"],
    ["CurrentHP", "INT", "", "NOT NULL"],
    ["MaxHP", "INT", "", "NOT NULL"],
    ["IsFainted", "TINYINT(1)", "", "0=False, 1=True"],
    ["MovesData", "TEXT", "", "JSON array"]
]
add_table_slide(prs, "Table: pokemons", table_data_pokemons)

# Slide 10: Class Diagram Overview
add_content_slide(prs, "Class Diagram - Key Classes", [
    "Core Entities:",
    "  • User - Account data (Username, Password)",
    "  • Player - Game progress (Name, Phase, Location, Team, Inventory)",
    "  • Pokemon - Individual Pokemon (Level, HP, Stats, Moves, Status)",
    "  • PokemonSpecies - Species data (Name, BaseStats, Moveset, CatchRate)",
    "",
    "Support Classes:",
    "  • Item, HealingItem, CaptureItem - Inheritance hierarchy",
    "  • Inventory - Manages item collection",
    "  • NPC, Trainer - Battle opponents"
])

# Slide 11: Services Architecture
add_content_slide(prs, "Core Services", [
    "GameSession",
    "  • Manages current user, player, battle state",
    "  • Handles login, load/save progress",
    "",
    "BattleService",
    "  • Turn-based battle execution",
    "  • Damage calculation, experience rewards",
    "",
    "StoryService",
    "  • World exploration and movement",
    "  • Wild Pokemon encounters, NPC battles",
    "  • Gym Leader and League challenges"
])

# Slide 12: Relationships & Flow
add_content_slide(prs, "Data Relationships", [
    "users (1) ──────→ (1) players",
    "      └─→ Foreign Key CASCADE",
    "",
    "players (1) ──────→ (*) pokemons",
    "        └─→ Foreign Key CASCADE",
    "",
    "pokemons (*) ────→ (1) pokedex (species reference)",
    "",
    "game_phases (1) ─→ (*) locations",
    "          └─→ Foreign Key CASCADE",
    "",
    "All JSON fields support complex nested data structures"
])

# Slide 13: Testing Plan
add_content_slide(prs, "Testing Strategy", [
    "Unit Tests:",
    "  • Login validation, Pokemon stat calculations",
    "  • Damage formula, experience gain, leveling",
    "",
    "Integration Tests:",
    "  • Battle sequences, NPC encounters",
    "  • Save/load persistence to database",
    "",
    "Test Coverage Areas:",
    "  • Authentication and authorization",
    "  • Battle mechanics and type advantages",
    "  • Item usage and inventory management",
    "  • NPC interactions and dialogue flow"
])

# Slide 14: Installation & Setup
add_content_slide(prs, "Installation & Deployment", [
    "Requirements:",
    "  • .NET SDK (Version 9.0+)",
    "  • MySQL Server for database persistence",
    "  • Visual Studio Code or Visual Studio",
    "",
    "Setup Steps:",
    "  1. Clone repository",
    "  2. Run PokemonDB.sql to initialize database",
    "  3. Configure connection string in appsettings.json",
    "  4. Build and run: dotnet run",
    "  5. Launch console application"
])

# Slide 15: Summary
add_content_slide(prs, "Project Summary", [
    "✓ Comprehensive Pokemon console game with 18 use cases",
    "✓ Layered architecture with clean separation of concerns",
    "✓ Database design supporting 7 tables (dynamic + static data)",
    "✓ Turn-based battle system with type advantages",
    "✓ NPC interactions including Gym Leaders, Elite Four, Champion",
    "✓ Player progression with Pokemon team management",
    "✓ Save/Load functionality for progress persistence",
    "✓ Ready for implementation and deployment"
])

# Save presentation
output_path = r"c:\Pokemon\PokemonConsole\TurnBasedPokemon\Pokemon_GameProject_Report.pptx"
prs.save(output_path)
print(f"✓ PowerPoint presentation created successfully!")
print(f"✓ Saved to: {output_path}")
print(f"✓ Total slides: {len(prs.slides)}")
