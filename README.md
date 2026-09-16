# TS Online — Steps 1–2 (offline)

Single-player Unity scaffold for a TS Online–inspired turn-based game.

- **Step 1:** `ElementSystem`, `DamageCalculator`, ScriptableObject data, Editor formula tests — **[STEP1.md](STEP1.md)**
- **Step 2:** 3v3 Battle test (`TurnManager` + `BattleUI`) in `Battle.unity` — **[STEP2.md](STEP2.md)**

Out of scope: World map, networking, gacha, PartyManager persistence, AutoBattle, saves.

## Open in Unity

1. Install **Unity 2022.3 LTS** or **Unity 6**.
2. Unity Hub → **Open** → this folder.
3. First import pulls URP from `Packages/manifest.json`. Unity 6 may upgrade packages — accept that.
4. After compile: **Tools → TS Online → Run Element Formula Tests**, then Play `Assets/Scenes/Battle.unity`.

2D URP is **not** auto-assigned (hand-written GraphicsSettings would break across editor versions). Click path: **[Assets/Settings/README_URP.md](Assets/Settings/README_URP.md)**. Formula tests and Battle placeholders work on Built-in.

If element icons are Missing after first import: **Tools → TS Online → Create Default Data Assets**.

## Verify (30 seconds)

**Formulas (no Unity):** `python3 tools/verify_formulas.py`  
**Editor:** `Tools → TS Online → Run Element Formula Tests`  
**Play Mode:** `Battle.unity` → Play → 3v3 commands + Console `PASS`

Normal attacks use `DamageRequest.NormalAttack` (E 1.12 / 0.90). `BasicStrike` is a None-element **skill**, not that path.
