# TS Online — Steps 1–5 + CharacterCreate (offline)

Single-player Unity scaffold for a TS Online–inspired turn-based game.

## Repository

Permanent Origin repo: **[bankleafa/rtk-online](https://cursor.com/codebase/bankleafa/rtk-online)**

- Clone: `https://origin.cursor.com/bankleafa/rtk-online.git`
- Browse: `https://cursor.com/codebase/bankleafa/rtk-online`

The earlier slug `tmp-31ac7ef40c42fe9f` is **legacy**. Use `rtk-online` for new clones and pushes.

- **Step 1:** Element + damage formulas — **[STEP1.md](STEP1.md)**
- **Step 2:** 3v3 Battle test — **[STEP2.md](STEP2.md)**
- **Step 3:** World walk + forest encounter → Battle → return — **[STEP3.md](STEP3.md)**
- **Step 4:** Party (max 5 of 6 generals), EXP / manual level-up, Auto Attack + Auto Heal — **[STEP4.md](STEP4.md)**
- **Step 5:** Patoyo mascot, one city quest, JSON save/load — **[STEP5.md](STEP5.md)**
- **CharacterCreate:** name + element on fresh Boot — **[CHARACTER_CREATE.md](CHARACTER_CREATE.md)**

Out of scope: networking, gacha, multi-quest journal.

## Open in Unity

1. Install **Unity 2022.3 LTS** or **Unity 6**.
2. Unity Hub → **Open** → this folder.
3. After compile: **Tools → TS Online → Run Element Formula Tests**, then Play **`Boot.unity`**.

No save → CharacterCreate. Existing save → World.  
**F8** in World deletes the save and returns to create.

UI readability (HUD, battle bars, placeholders): **[UI_POLISH.md](UI_POLISH.md)**.

2D URP click path: **[Assets/Settings/README_URP.md](Assets/Settings/README_URP.md)**.  
Missing icons: **Tools → TS Online → Create Default Data Assets**.

## Verify

**Formulas:** `python3 tools/verify_formulas.py`  
**Create:** Boot with no save → name + ดิน/น้ำ/ไฟ/ลม → World HUD shows that name  
**World / Battle:** Patoyo, quest, F5/F9, auto battle. Direct `Battle.unity` stays the test 3v3, does not write the player save, and does not fake a World return via leftover `LastEnd`.

Normal attacks use `DamageRequest.NormalAttack`. `BasicStrike` is a None-element skill.
