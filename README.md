Project Kunai

A fast-paced, high-mobility sci-fi FPS prototype centered around kunai-based teleportation mechanics and fluid traversal.

📸 Overview

Project Kunai is an indie first-person shooter prototype inspired by fast-paced retro shooters and titles like ULTRAKILL. The project focuses on creating a tight, responsive movement system where teleportation is seamlessly integrated into both combat and environment navigation.

This project is currently in development hiatus due to academic priorities, with features, systems, and visual assets planned.

view a short demo: https://youtu.be/7BdlwSwoMno

✨ Core Features

Kunai Teleportation Mechanics: Dynamic projectile throwing coupled with instant player teleportation for aggressive combat positioning and rapid traversal.

Fluid Movement System: Responsive movement fundamentals including dashing, wall jumping, and continuous momentum preservation.

Combat & AI Systems: First-person shooting mechanics integrated with dynamic enemy combat behavior.

Visual & Audio Polish: Custom particle effects, muzzle flashes, and sound effects designed to maximize impact and feedback.

🎮 Controls

Action

Input

Move

W A S D

Look

Mouse

Jump

Space

Dash

Shift

Throw Kunai

F

Teleport

F(kunai thrown)

Primary Attack (Shoot)

Left Mouse Button

Secondary Attack (Charged shot)

Right Mouse Button

Note: Keybindings are subject to iteration during development.

🚧 Project Status

Current Development Stage: [Prototype / Pre-alpha]

Active Focus: nil: academic priorities

Recent Milestone: implemented 2 basic types of enemies(melee and ranged) + damage system

📅 Planned Features(in order of descending priority)

loadout selection: weapon options, background modifiers

Wider Variety of enemies

Full Fidelity Environmental Assets

Procedural + Endless Environmental Generation

View frustum, backface and occlusion culling

LOD 

Full Fidelity enemies

🛠️ Tech Stack & Tools

Engine: Unity 6000.4.4f1

Programming Language: C#

3D Modeling, Texturing & Animation: Blender, Photoshop

Version Control: Git & GitHub

⚙️ Setup and Installation

Prerequisites

Unity Engine 6000.4.4f1

Unity Hub

Instructions

Clone the repository:

git clone https://github.com//project-kunai.git


Open Unity Hub.

Click Add -> Add project from disk and select the root directory of the cloned repository.

Launch the project with the designated Unity version.

Navigate to Assets/Scenes/ and open the main scene to run the prototype in the Editor.

📂 Project Architecture

Project-Kunai/
├── Assets/                        <-- Game assets and Main arm rig
│   ├── Animators/                 <-- Animators
│   ├── image_textures/            <-- Image textures
│   ├── knife_model/               <-- Kunai FBX models and textures
│   ├── Materials/                 <-- Standard and shader materials
│   ├── muzzle flash/              <-- Assets used in the making of the muzzle flash fx
│   ├── prefabs/                   <-- Prefabs
│   │   └── enemies/               <-- Enemy prefabs
│   ├── Scenes/                    <-- Gameplay and testing scenes
│   └── Scripts/                   <-- Source code hierarchy
│       ├── enemy/                 <-- Enemy AI and logic
│       ├── Player/                <-- Player movement and combat controllers
│       └── player-related/        <-- Auxiliary player mechanics (e.g., Kunai logic)
├── Packages/                      <-- Project dependencies
├── ProjectSettings/               <-- Unity configuration files
└── README.md


📝 Design & Technical Notes

Known Issues

Movement feels rigid and does not chain well, retaining momentum is not polished

Enemy Behaviour is rudimentary and variety is limited

No Weapons to choose from

Environment is still in greyboxing/prototype stage



Design Philosophy

Focus on Expressive Movement: Core gameplay prioritizes mechanical skill expression, fluidity, and speed over expansive weapon variety.

👨‍💻 Author & Credits

Developer: Liew Zhe Wei Damien (@liewdamien22-a11y)

Inspirations: ULTRAKILL, classic arena shooters, and high-mobility FPS titles.