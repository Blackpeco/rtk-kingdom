# TS Online — Steps 1–4 (offline)

Single-player Unity scaffold for a TS Online–inspired turn-based game.

- **Step 1:** Element + damage formulas — **[STEP1.md](STEP1.md)**
- **Step 2:** 3v3 Battle test — **[STEP2.md](STEP2.md)**
- **Step 3:** World walk + forest encounter → Battle → return — **[STEP3.md](STEP3.md)**
- **Step 4:** Party (max 5 of 6 generals), EXP / manual level-up, Auto Attack + Auto Heal — **[STEP4.md](STEP4.md)**

Out of scope until Step 5: Patoyo, quests, SaveService, CharacterCreate flow, networking, gacha.

## Open in Unity

1. Install **Unity 2022.3 LTS** or **Unity 6**.
2. Unity Hub → **Open** → this folder.
3. After compile: **Tools → TS Online → Run Element Formula Tests**, then Play **`World.unity`** (or `Boot.unity`).

2D URP click path: **[Assets/Settings/README_URP.md](Assets/Settings/README_URP.md)**.  
Missing icons: **Tools → TS Online → Create Default Data Assets**.

## Verify

**Formulas:** `python3 tools/verify_formulas.py`  
**Battle only:** Play `Battle.unity` (default party + auto toggles)  
**World loop:** Play `World.unity` → **P** to edit party → walk right → fight → allocate points → confirm HP/Lv on the World strip

Normal attacks use `DamageRequest.NormalAttack`. `BasicStrike` is a None-element skill.
