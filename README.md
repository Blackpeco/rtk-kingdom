# TS Online — Step 1 (Element + Damage)

Single-player offline Unity scaffold for a TS Online–inspired turn-based game.

**This commit is Step 1 only:** folder layout, ScriptableObject data, `ElementSystem`, `DamageCalculator`, and Editor / Play Mode formula tests.

Out of scope: World map, networking, gacha, TurnManager, Battle UI, saves.

## Open in Unity

1. Install **Unity 2022.3 LTS** or **Unity 6**.
2. Unity Hub → **Open** → select this folder.
3. First import will pull **URP** from `Packages/manifest.json`. Unity 6 may upgrade package versions automatically.
4. After compile finishes, follow **[STEP1.md](STEP1.md)** to run `Tools/TS Online/Run Element Formula Tests`.

Target: 2D URP, offline. If materials look pink, create a URP 2D Renderer via **Assets → Create → Rendering → URP Asset (with 2D Renderer)** and assign it in **Project Settings → Graphics**.

## Verify formulas (30 seconds)

**Editor:** `Tools → TS Online → Run Element Formula Tests`  
**Play Mode:** open `Assets/Scenes/Battle.unity`, press Play.

Look for `PASS` in the Console. Details and the full file list are in `STEP1.md`.
