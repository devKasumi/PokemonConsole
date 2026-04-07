# Pokemon Turn-Based Game - Class Diagrams Index

**Created:** April 7, 2026  
**Last Updated:** April 7, 2026  
**Status:** ✅ Complete Layer-Based Architecture with Detailed Interactions  

---

## 📋 Class Diagram Directory Structure

### 🏗️ OVERVIEW DIAGRAMS

#### Project Class Diagram
- **File:** [ProjectClassDiagram.puml](ClassDiagram/ProjectClassDiagram.puml)  
**Description:** High-level overview of the entire Pokemon game architecture showing all layers and their main components.

#### Layer Interaction Diagram
- **File:** [LayerInteractionDiagram.puml](ClassDiagram/LayerInteractionDiagram.puml)  
**Description:** Detailed diagram showing how classes from different layers interact with each other, including specific relationships and dependencies.

---

### 🎨 PRESENTATION LAYER
- **File:** [PresentationLayer.puml](ClassDiagram/PresentationLayer.puml)  
**Description:** UI screens and their interactions with application services. Includes LoginScreen, BattleScreen, StoryScreen, etc.

---

### ⚙️ APPLICATION LAYER
The Application Layer has been split into specialized service groups for better organization:

#### Battle Services
- **File:** [BattleServices.puml](ClassDiagram/BattleServices.puml)  
**Description:** Core battle mechanics including turn execution, damage calculation, and battle results.

#### Story Services
- **File:** [StoryServices.puml](ClassDiagram/StoryServices.puml)  
**Description:** Story progression, phase management, and NPC interactions.

#### Experience & Healing Services
- **File:** [ExpAndHealingServices.puml](ClassDiagram/ExpAndHealingServices.puml)  
**Description:** Pokemon leveling, experience calculation, and healing mechanics.

#### Catch & Authentication Services
- **File:** [CatchAndAuthServices.puml](ClassDiagram/CatchAndAuthServices.puml)  
**Description:** Pokemon catching mechanics and user authentication services.

#### File & World Generation Services
- **File:** [FileAndWorldGenServices.puml](ClassDiagram/FileAndWorldGenServices.puml)  
**Description:** Save/load functionality and procedural world generation.

#### Network & Matchmaking Services
- **File:** [NetworkAndMatchmakingServices.puml](ClassDiagram/NetworkAndMatchmakingServices.puml)  
**Description:** PvP matchmaking, network communication, and battle room management.

#### Complete Application Layer
- **File:** [ApplicationLayer.puml](ClassDiagram/ApplicationLayer.puml)  
**Description:** All application services in one comprehensive diagram (original file).

---

### 🧠 DOMAIN LAYER
- **File:** [DomainLayer.puml](ClassDiagram/DomainLayer.puml)  
**Description:** Core business entities including Pokemon, Player, Items, and game state management.

---

### 🗄️ INFRASTRUCTURE LAYER
- **File:** [InfrastructureLayer.puml](ClassDiagram/InfrastructureLayer.puml)  
**Description:** Data persistence, repositories, and external service integrations.

---

## 🔗 Layer Architecture Summary

- **Presentation Layer:** Handles user interface and input/output
- **Application Layer:** Contains business logic services and use case implementations
- **Domain Layer:** Core business entities and rules
- **Infrastructure Layer:** Data access, external APIs, and persistence

Each layer depends only on the layer below it, following clean architecture principles.

---

## 📝 Notes

- All diagrams use PlantUML syntax and can be rendered with PlantUML tools
- The Application Layer was split into smaller, focused diagrams for better maintainability
- LayerInteractionDiagram provides the most detailed view of inter-layer communication
- ProjectClassDiagram serves as a high-level architectural overview