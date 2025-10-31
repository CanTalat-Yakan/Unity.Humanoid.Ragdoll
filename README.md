# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Humanoid Ragdoll

> Quick overview: One‑click Humanoid ragdoll generation with in‑scene gizmos to edit colliders, joints, and centers of mass.

A complete physics ragdoll for a Humanoid avatar can be generated in seconds. Rigidbodies, colliders, and configurable joints are added automatically, mass is distributed intelligently, and dedicated Scene gizmos are available to fine‑tune collider shapes, joint limits, and rigidbody centers of mass.

![screenshot](Documentation/Screenshot.png)

## Features
- One‑click setup
  - Generate a full ragdoll for Humanoid avatars from a single component
  - Optional tips: auto‑add hands and feet parts
- Smart physics defaults
  - Total mass distribution across limbs and torso
  - Gravity, kinematic, trigger, drag, and angular drag settings
  - ConfigurableJoint angular drive (spring, damper, max force, acceleration)
  - Collision detection mode selection
- In‑scene editing gizmos
  - Edit modes: Colliders, Joints, Center of Mass
  - Scene handles to adjust shapes, joint limits, and COM per rigidbody
  - Symmetry‑aware bone lookup to assist bilateral editing
- Safe removal
  - Remove all ragdoll components and helper nodes in one click
- Lightweight and focused
  - Editor‑only; no runtime dependencies

## Requirements
- Unity Editor 6000.0+ (Editor‑only; the included asmdef targets the Editor)
- A valid Humanoid `Avatar` assigned to the target GameObject
- The component should be on the character root that corresponds to the Avatar

## Usage
1) Add and configure
   - Select your character root in the Hierarchy
   - Add `RagdollBuilder` and assign a Humanoid `Avatar`
   - Expand “Settings” and adjust:
     - Total Weight, Create Hands & Feet (tips)
     - Use Gravity, As Trigger, Is Kinematic
     - Rigidbody Drag and Angular Drag
     - Angular Drive: Position Spring, Position Damper, Maximum Force, Use Acceleration
     - Collision Detection Mode

2) Create the ragdoll
   - With the object selected, click “Create Ragdoll” (or select the object to auto‑initialize)
   - Colliders, Rigidbodies, and Configurable Joints are added to the appropriate bones

3) Edit in Scene view
   - Toggle one edit mode at a time:
     - Edit Colliders: adjust collider shapes/rotations via handles
     - Edit Joints: adjust joint limits and related settings via handles
     - Edit Center of Mass: reposition rigidbody COM points
   - Tip: The tool reacts to selection changes; gizmos are shown when the object is selected

4) Remove ragdoll (optional)
   - Click “Remove Ragdoll” to delete all ragdoll components and helper nodes

## How It Works
- Part detection and setup
  - Finds key Humanoid bones and creates part objects: pelvis/chest boxes, limb capsules, head sphere, and optional hands/feet
  - Distributes total mass across parts using an internal weight calculator
- Physics configuration
  - Adds Rigidbodies with your gravity/drag/kinematic settings
  - Creates Colliders and rotates helper nodes for precise alignment
  - Connects parts with Configurable Joints and applies your angular drive values
  - Supports selectable collision detection modes
- Gizmo editing
  - A Scene gizmo drawer renders context‑aware handles for the active edit mode
  - Uses a symmetry map of left/right bones to assist consistent adjustments

## Notes and Limitations
- Humanoid only: The assigned Avatar must be Humanoid; otherwise initialization logs an error
- Editor‑only: Ships with an Editor asmdef; this tool is not intended for runtime
- One‑time apply: Core settings should be set before creation; after creation, use gizmos to refine. To re‑apply core settings, Remove and Create again
- Helper nodes: The tool may create internal helper transforms (e.g., for collider rotation) that are cleaned up on removal
- Selection‑driven: Editing modes are shown only while the object is selected in the Editor

## Files in This Package
- `Editor/RagdollBuilder.cs` – Main component (create/edit/remove, edit modes, selection handling)
- `Editor/Data/RagdollData.cs` – Serialized settings (mass, gravity, drive, drag, collision mode, tips)
- `Editor/Data/RagdollPartData.cs` – Per‑part data structure
- `Editor/Factory/RagdollFactory.cs` – Builds and clears ragdoll parts and wiring
- `Editor/Factory/RagdollPartFactory.cs` – Creates and attaches colliders/rigidbodies per bone
- `Editor/Factory/RagdollPhysicsConfigurator.cs` – Joint/collider configuration utilities
- `Editor/Gizmo/RagdollGizmoDrawer.cs` – Scene gizmo window and drawing logic
- `Editor/Gizmo/ColliderGizmo.cs`, `JointGizmo.cs`, `RigidbodyGizmo.cs` – Handles for collider/joint/COM editing
- `Editor/Helper/RagdollHelper.cs`, `RagdollWeightCalculator.cs` – Utilities and mass distribution
- `Editor/UnityEssentials.HumanoidRagdoll.Editor.asmdef` – Editor assembly definition

## Tags
unity, unity-editor, ragdoll, humanoid, physics, collider, configurable-joint, gizmo, editor-tool, mass, center-of-mass
