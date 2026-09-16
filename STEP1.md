# Step 1 — Element system + damage formulas

English below. ภาษาไทยอยู่ด้านล่าง

This folder is a compilable Unity 2022 LTS / Unity 6 (2D URP) scaffold. Step 2 adds an offline 3v3 Battle test — see **[STEP2.md](STEP2.md)**. World map, networking, servers, and gacha are still out of scope.

---

## How to open

1. Install Unity Hub + **Unity 2022.3 LTS** or **Unity 6**.
2. **Add** / **Open** this repository root (the folder that contains `Assets/` and `Packages/`).
3. Wait for the first script compile. Ignore empty reserved folders (`Scripts/Map`, `Scripts/UI`, `Scripts/AI`).
4. If Unity asks to upgrade packages (Unity 6 opening a 2022.3 manifest), accept the upgrade.
5. Optional 2D URP pipeline: follow **`Assets/Settings/README_URP.md`** (create a 2D URP asset, assign it in Project Settings → Graphics / Quality). Script-only tests work on Built-in. First open may need **Tools → TS Online → Create Default Data Assets** to reassign element icons.

### If ScriptableObject fields look empty

Sample `.asset` YAML is already under `Assets/Data/`. If a script GUID mismatch leaves Missing Script:

1. Menu **Tools → TS Online → Create Default Data Assets**
2. Confirm Console: `Default data assets written under Assets/Data/`
3. Inspect `Assets/Data/Elements`, `Skills`, `Generals`, `Monsters`

You can also create assets by hand: **right-click** in the Project window → **Create → TS Online → Data → …**

### If a scene will not open

See `Assets/Scenes/README.md`. Recreate **File → New Scene**, save as `Boot`, `CharacterCreate`, `World`, or `Battle`. For Battle, add an empty GameObject with **Damage Formula Smoke Test**.

---

## Run the Editor test (required)

**Exact menu path:** `Tools → TS Online → Run Element Formula Tests`

Alternate window: `Tools → TS Online → Element Formula Test Window` then click **Run Tests**.

### What to look for

The Console (and the window) must print `PASS` lines, then:

`Element formula self-test: N passed, 0 failed. PASS`

Checks include:

1. **Skill E (ข่ม / แพ้)**
   - Earth vs Water = **1.25**
   - Water vs Earth = **0.80**
   - Earth vs Fire (opposite) = **1.00**
   - Water vs Wind (opposite) = **1.00**
   - same element / `None` = **1.00**
   - full cycle: Water>Fire, Fire>Wind, Wind>Earth
2. **Normal attack (half strength)**
   - advantage **1.12**, disadvantage **0.90**, else **1.00**
3. **Sample Final damage** with RNG fixed at **1.00** (mid of 0.95–1.05)
   - Physical `ATK=100`, `Power=1`, `DEF=40` → Base `80`
   - Earth skill vs Water → Final **100** (`80 * 1.25`)
   - Heal with Earth vs Water → E stays **1.00**, Final **80**
   - Normal attack Earth vs Water → Final **89.6** (`80 * 1.12`)
   - Same skill + crit → Final **150** (`80 * 1.25 * 1.5`)
   - Mastery input **0.50** is clamped to **0.15** → Final **115** (`80 * 1.25 * 1.15`)
   - Magic `INT=100`, `Power=1`, `DEF=40` → Base **90**; Water vs Fire → Final **112.5**
   - Opposite learn cost (`Earth` vs `Fire`) → **-1** (cannot learn; never 1.5)

Any `FAIL` line means a formula constant drifted. Do not “fix” the test — fix `ElementSystem` / `DamageCalculator`.

---

## Play Mode test (no Editor-only code)

1. Open `Assets/Scenes/Battle.unity`.
2. Hierarchy should already contain **DamageFormulaSmokeTest**.
3. Press **Play**.
4. Console should log the same PASS/FAIL report (`DamageFormulaSmokeTest` on Start). The same scene now also runs the Step 2 3v3 test (`BattleBootstrap`) — see STEP2.md.

To add it yourself: empty GameObject → **Add Component → Damage Formula Smoke Test**. Context menu on the component: **Run Element Formula Checks**. `OnValidate` can also log in Edit Mode if `runOnValidate` is enabled.

---

## Formulas (must match — do not invent alternatives)

### Cycle (advantage / ข่ม)

- Earth (ดิน) beats Water (น้ำ)
- Water beats Fire (ไฟ)
- Fire beats Wind (ลม)
- Wind beats Earth

### Opposites (no damage bonus)

- Earth ↔ Fire
- Water ↔ Wind

### Skill E (when the skill has an element)

| Relation | E |
| --- | --- |
| Advantage (ข่ม) | 1.25 |
| Disadvantage (แพ้) | 0.80 |
| Same / opposite / no element | 1.00 |

### Normal attack E (unit element, half strength)

| Relation | E |
| --- | --- |
| Advantage | 1.12 |
| Disadvantage | 0.90 |
| Else | 1.00 |

Heal / Buff / Wall / Stealth: **E is always 1.00**.

### Three layers (never mix roles)

1. Unit innate element
2. Skill element (primary for skill damage)
3. Temporary elemental status on the target (data/types only in Step 1)

### Statuses (stubbed in `StatusEffectSystem`)

- Earth: AGI −20% for 2 turns
- Water: Wet — next Fire hit ×1.15, clears burn, 2 turns
- Fire: Burn 4% HP/turn, heal −30%, 2 turns
- Wind: 25% chance to lose next turn, 1 turn
- Apply chance 30%; cannot stack multiple element statuses

### Mastery / resist

- Mastery M = +1% per 20 uses of that element skill, cap 15%
- Resist R from gear, cap 20%

### Damage

```
Physical Base = ATK * SkillPower - DEF * 0.5
Magic/INT Base = INT * SkillPower - DEF * 0.25
Final = max(1, Base) * E * (1+M) * (1-R) * Crit * Random(0.95, 1.05)
Crit = 1.5 if crit else 1.0
```

`DamageCalculator` takes an injectable `RandomRange`. Editor tests use `DamageCalculator.Deterministic(1.0f)`.

### Cross-element learning (`ElementSystem`)

- Own element: can learn, cost ×1.0
- Adjacent: can learn, cost ×1.5
- Opposite: **cannot** learn

Adjacent pairs: Earth↔Water/Wind, Water↔Earth/Fire, Fire↔Water/Wind, Wind↔Fire/Earth.

---

## Files created

### Runtime C#

| Path | Role |
| --- | --- |
| `Assets/Scripts/Core/ElementType.cs` | None, Earth, Water, Fire, Wind |
| `Assets/Scripts/Core/DamageKind.cs` | Physical, Magical |
| `Assets/Scripts/Core/SkillCategory.cs` | Attack, Heal, Buff, Wall, Stealth, Other |
| `Assets/Scripts/Core/TargetingFlags.cs` | Self / ally / enemy flags |
| `Assets/Scripts/Core/ElementSystem.cs` | ข่ม/แพ้, E, mastery, resist, learning |
| `Assets/Scripts/Core/DamageCalculator.cs` | Physical vs magic pipeline |
| `Assets/Scripts/Core/DamageRequest.cs` | Inputs |
| `Assets/Scripts/Core/DamageResult.cs` | Final + advantage flags |
| `Assets/Scripts/Core/ElementFormulaSelfTest.cs` | Shared PASS/FAIL matrix |
| `Assets/Scripts/Core/StatusEffectSystem.cs` | Status data stub |
| `Assets/Scripts/Core/ElementStatusKind.cs` | Status enum |
| `Assets/Scripts/Core/ElementStatusSpec.cs` | Status description |
| `Assets/Scripts/Data/ElementDefinition.cs` | SO |
| `Assets/Scripts/Data/SkillDefinition.cs` | SO (power lives here, not in combat scripts) |
| `Assets/Scripts/Data/UnitDefinition.cs` | SO |
| `Assets/Scripts/Data/UnitStats.cs` | HP SP ATK INT DEF AGI |
| `Assets/Scripts/Data/ItemDefinition.cs` | Stub SO |
| `Assets/Scripts/Data/EncounterTable.cs` | Stub SO |
| `Assets/Scripts/Battle/DamageFormulaSmokeTest.cs` | Play Mode logger |

### Editor

| Path | Menu |
| --- | --- |
| `Assets/Editor/ElementFormulaTestWindow.cs` | `Tools/TS Online/Run Element Formula Tests` and `…/Element Formula Test Window` |
| `Assets/Editor/CreateDefaultDataMenu.cs` | `Tools/TS Online/Create Default Data Assets` |

### Sample data

- Elements: `Assets/Data/Elements/{Earth,Water,Fire,Wind}.asset`
- Skills: physical+element (`GaleSlash`, `SkyPiercer`, …), magical (`FloodBolt`, `Rockslide`), heal (`SouthWindHeal`, `Mend` — E must stay 1), wall (`EarthenWall`). **`BasicStrike` is a None-element SKILL** (E=1.00). True normal attacks must use `DamageRequest.NormalAttack` (half-strength 1.12 / 0.90 / 1.00).
- Generals: Zhao Yun / จูล่ง (Wind), Guan Yu / กวนอู (Wind), Lu Bu / ลิโป้ (Fire), Zhang Fei / เตียวหุย (Fire), Zhuge Liang / ขงเบ้ง (Water), Yang Xiu / หยางซิว (Earth)
- Monsters: Forest Wolf (Wind), Mountain Bandit (Fire), Swamp Frog (Water), Small Stone Golem (Earth), Roof Bird (Wind), Lost Soldier (Fire)
- Item stub: `Assets/Data/Items/Herb.asset`
- Encounter stub: `Assets/Data/ForestEdgeEncounters.asset`
- Icons: `Assets/Art/Icons/{earth,water,fire,wind}.png`

### Scenes

`Assets/Scenes/{Boot,CharacterCreate,World,Battle}.unity` — Boot/World/Create are camera stubs. Battle includes the smoke-test object **and** the Step 2 3v3 bootstrap (see STEP2.md).

Reserved empty folders: `Assets/Scripts/Map`, `UI`, `AI`; `Assets/Art/Placeholders`, `Art/UI`.

---

## ภาษาไทย — เปิดโปรเจกต์และตรวจสูตร

1. เปิดโฟลเดอร์นี้ใน Unity 2022.3 LTS หรือ Unity 6
2. รอ compile เสร็จ
3. เมนู **Tools → TS Online → Run Element Formula Tests**
4. ดู Console ต้องขึ้น **PASS** ทั้งชุด โดยเฉพาะ
   - ดินข่มน้ำ = 1.25
   - น้ำแพ้ดิน = 0.80
   - ดิน vs ไฟ (ตรงข้าม) = 1.00
   - โจมตีปกติข่ม = 1.12 / แพ้ = 0.90
   - ฮีลต้องได้ E = 1.00 เสมอ
5. หรือเปิดซีน `Battle` กด Play — สคริปต์ `DamageFormulaSmokeTest` จะ log ชุดเดียวกัน

ถ้า asset ขึ้น Missing Script: ใช้ **Tools → TS Online → Create Default Data Assets**

ชั้นธาตุสามชั้นห้ามปนกัน: ธาตุติดตัวหน่วย / ธาตุสกิล / สถานะธาตุชั่วคราวบนเป้า
