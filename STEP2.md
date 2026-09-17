# Step 2 — 3v3 Battle test (offline)

Thai + English. World map is **Step 3** (`STEP3.md`). Party / EXP / AutoBattle are **Step 4** (`STEP4.md`). Networking and saves remain out of scope.

## What you can play

Open `Assets/Scenes/Battle.unity` and press **Play**.

- Left: Zhao Yun / จูล่ง (Wind), Guan Yu / กวนอู (Wind), Zhuge Liang / ขงเบ้ง (Water)
- Right: Forest Wolf / หมาป่า (Wind), Mountain Bandit / โจรภูเขา (Fire), Swamp Frog / กบทึง (Water)
- Turns sort by **AGI descending**. Your units wait for commands; enemies auto-**Attack** the lowest-HP living ally.
- Commands: **โจมตีปกติ** / **สกิล** / **ไอเทม (stub)** / **ป้องกัน** / **หนี (30%)**
- Gold popup **ข่มธาตุ!** = advantage. Gray **ธาตุต้าน** = disadvantage. Log also prints `E=1.25` / `E=0.80` / `E=1.12`.

To see ข่ม clearly: have **Zhuge Liang** use **Flood Bolt** (Water magic) on **Mountain Bandit** (Fire) → skill E **1.25**.  
Zhao Yun **normal attack** vs Small Stone Golem is not in this default roster; vs Forest Wolf (same Wind) E stays **1.00**. Guan Yu vs Swamp Frog is opposite (Wind↔Water) → E **1.00**.

## Inspector wiring

Select **BattleBootstrap** in the Battle scene:

| Field | Default |
| --- | --- |
| Player Party | ZhaoYun, GuanYu, ZhugeLiang |
| Enemy Party | ForestWolf, MountainBandit, SwampFrog |
| Encounter Config | `Resources/Battle/DefaultEncounter` |
| Run Formula Smoke Test | off (the `DamageFormulaSmokeTest` object still runs on Play) |

Swap any `UnitDefinition` from `Assets/Data/Generals` or `Monsters`. If refs are missing, Play Mode in the Editor loads the same defaults via `AssetDatabase`, or `Resources.Load("Battle/DefaultEncounter")`.

After first clone, if icons are Missing: **Tools → TS Online → Create Default Data Assets**. URP pipeline: `Assets/Settings/README_URP.md`.

## How to test (Play Mode)

1. Unity 2022.3 LTS or Unity 6 → open this project → wait for compile.
2. Double-click `Assets/Scenes/Battle.unity`.
3. Press Play. Console should still show the Step 1 `Element formula self-test: … PASS` from `DamageFormulaSmokeTest`.
4. When a player’s turn banner appears, click **โจมตีปกติ**, pick an enemy. Confirm HP bar drops and a popup appears.
5. On Zhuge’s turn click **สกิล** → **Flood Bolt** → Bandit. Look for gold **ข่มธาตุ!** and `E=1.25`.
6. **South Wind Heal** / **Mend** on an ally: green `+HP`, E stays 1.00.
7. **ป้องกัน** then let an enemy hit that unit — incoming damage is halved (min 1).
8. Fight until one side is wiped, or **หนี**.

Editor formula menu is unchanged: **Tools → TS Online → Run Element Formula Tests**.

## Files (Step 2)

| Path | Role |
| --- | --- |
| `Assets/Scripts/Battle/BattleUnit.cs` | Runtime HP/SP, element, defend, light status |
| `Assets/Scripts/Battle/TurnManager.cs` | AGI order, commands, win/lose |
| `Assets/Scripts/Battle/SkillSystem.cs` | SP, DamageCalculator, heal, skip E on support |
| `Assets/Scripts/Battle/SkillResolveResult.cs` | Hit records |
| `Assets/Scripts/Battle/BattleCommand.cs` | Attack/Skill/Item/Defend/Escape |
| `Assets/Scripts/Battle/BattleUI.cs` | Runtime uGUI + OnGUI popups |
| `Assets/Scripts/Battle/BattleBootstrap.cs` | 3v3 spawn |
| `Assets/Scripts/Battle/BattleWorldView.cs` | Colored quad placeholders |
| `Assets/Scripts/Battle/BattleTestConfig.cs` | Roster SO |
| `Assets/Resources/Battle/DefaultEncounter.asset` | Default 3v3 refs |
| `Assets/Scenes/Battle.unity` | Camera + smoke test + bootstrap |

`StatusEffectSystem.TryApply` now rolls the 30% chance (no stacking). Burn ticks a little HP at turn start. Full status combat is still light.

## QA bugfixes shipped with this step

- `DamageCalculator` clamps M to **[0, 0.15]** via `ElementSystem.ClampMastery`
- `GetLearnCostMultiplier` returns **-1** when `!CanLearnSkill`
- Forest Wolf Thai **หมาป่า**
- BasicStrike labeled as a **None-element skill**; true normal attack = `DamageRequest.NormalAttack`
- Create Default Data Assets reassigns element icons
- Self-test + `tools/verify_formulas.py`: M clamp, opposite learn -1, magic Base with DEF

## Wind skip (fixed in Step 3)

Turn start now calls `BattleUnit.BeginTurn`: **roll skip while Wind is active**, then tick duration.  
Play Mode: on an enemy `BattleUnit`, gear menu **QA: Force Wind skip (1 turn)** — next turn they pass. See STEP3.md.

## ภาษาไทย — เปิดซีนรบ

1. เปิดโปรเจกต์ใน Unity แล้วเปิดซีน `Battle`
2. กด Play — ซ้ายขุนพล 3 / ขวามอนสเตอร์ 3
3. กด **โจมตีปกติ** หรือ **สกิล** แล้วเลือกเป้า
4. ข้อความทอง **ข่มธาตุ!** = ได้เปรียบธาตุ / เทา **ธาตุต้าน** = เสียเปรียบ
5. ลอง Flood Bolt ของขงเบ้งใส่โจรภูเขา (น้ำข่มไฟ) ควรเห็น E=1.25
