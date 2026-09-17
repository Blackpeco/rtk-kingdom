# Step 3 — World movement + Encounter → Battle

Offline only. No networking, gacha, save, or CharacterCreate flow.

## What to play

1. Open **`Assets/Scenes/World.unity`** (or Play `Boot.unity`, which loads World after a short delay).
2. **WASD / arrow keys** move the gold player quad.
3. **Left = เมือง (safe)**. **Right = ป่า**. Walk into a colored wandering monster.
4. Battle starts with the **same 3 generals** vs that pack. If the party panel (**P**) or ยายเมือง dialog (**E**) is open (`MenuOpen`), the overlap is ignored until you close the UI — wanderers still move.
5. Win / lose / escape → after ~1.8s return to World, slightly left of the fight (grace 1.6s so you do not instantly re-trigger). Standing still when grace ends does not start a new fight; walk into a pack. If P/E was open while overlapping, closing the UI still Stay-retries. **F9** while P/E is open on a pack must not leave a Stay-retry that starts Battle after you land in the city and close the UI.

Direct Play on `Battle.unity` still works as the Step 2 3v3 and **does not** return to World (no `EncounterContext`).

## Wind skip fix (Part A)

**Bug:** `TickStatusAtTurnStart` ran before `RollSkipTurn`, so a 1-turn Wind status expired before the 25% skip roll.

**Fix:** `BattleUnit.BeginTurn` rolls skip **while Wind is active**, then ticks duration. One-turn Wind ⇒ exactly one skip check.

Automated: `StatusTurnFlow.SimulateWindTurnStart` in `ElementFormulaSelfTest` + `tools/verify_formulas.py`.

### Play Mode repro (guaranteed skip)

1. Play `Battle.unity` (or enter via World).
2. Hierarchy → an enemy (`E_Forest Wolf` etc.).
3. Gear menu on `BattleUnit` → **QA: Force Wind skip (1 turn)**.
4. When that unit’s turn starts, Console: `เสียเทิร์นจากสถานะลม` and they do not attack.

Natural 25% skip also works now (no longer always 0%).

## Files

| Path | Role |
| --- | --- |
| `Assets/Scripts/Core/StatusTurnFlow.cs` | Skip-then-tick rules + QA forced roll |
| `Assets/Scripts/Map/WorldBootstrap.cs` | Builds city/forest, player, wanderers |
| `Assets/Scripts/Map/PlayerWorldController.cs` | 2D movement |
| `Assets/Scripts/Map/EncounterTrigger.cs` | Overlap → `EncounterContext` → Battle |
| `Assets/Scripts/Map/EncounterContext.cs` | Party / return position handoff |
| `Assets/Scripts/Map/WorldParty.cs` | Minimal 3-general party |
| `Assets/Scripts/Map/WanderingMonster.cs` | Idle roam |
| `Assets/Scripts/Map/WorldHUD.cs` | Hint + last result |
| `Assets/Scripts/Map/WorldCameraFollow.cs` | Camera follow |
| `Assets/Scripts/Map/BootLoader.cs` | Boot → World |
| `BattleBootstrap` / `TurnManager` / `SkillSystem` | Encounter roster, return, wind/SP/attack fixes |

## Inspector

`WorldBootstrap` on the World scene can hold Forest Wolf / Bandit / Frog refs (also filled at runtime from assets / `DefaultEncounter`).

Scenes must stay in **File → Build Settings** (Boot, CharacterCreate, World, Battle) so `LoadScene("Battle")` / `LoadScene("World")` works.

## ภาษาไทย

เปิดซีน `World` เดินขวาเข้าป่า ชนมอนสเตอร์ → เข้า Battle → จบแล้วกลับจุดใกล้เดิม  
แก้สถานะลม: เช็กการเสียเทิร์นก่อนลดจำนวนเทิร์น — ดู `STEP3.md` ด้านบน
