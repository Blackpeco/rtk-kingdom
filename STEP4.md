# Step 4 — Party generals, level-up, auto battle

Offline only. No networking, gacha, Patoyo, quests, or SaveService (those are Step 5). CharacterCreate stays a stub.

## What to play

1. Open **`Assets/Scenes/World.unity`** (or Play `Boot.unity`).
2. Gold player = lead of the current party (default **จูล่ง**). **WASD / arrows**. Left = เมือง. Right = ป่า.
3. Left HUD lists the live party (HP / SP / Lv). **P** or **ปาร์ตี้ / เลเวลอัพ** opens lineup + stat allocation.
4. Walk into a wanderer → Battle uses **that party** (not a hardcoded 3).
5. Win → EXP (each living party member gets the encounter pot) → if anyone levels, allocate points, then **กลับโลก**.
6. HP / SP / level on the World strip match what you left Battle with.

Direct Play on **`Battle.unity`** still works: `PartyManager` builds the default 3 (จูล่ง กวนอู ขงเบ้ง) vs Wolf / Bandit / Frog. No return to World unless you entered via encounter.

## Default roster

Unlocked 6 sample generals:

| EN | TH | Element | Default party |
| --- | --- | --- | --- |
| Zhao Yun | จูล่ง | Wind | yes |
| Guan Yu | กวนอู | Wind | yes |
| Lu Bu | ลิโป้ | Fire | |
| Zhang Fei | เตียวหุย | Fire | |
| Zhuge Liang | ขงเบ้ง | Water (heal) | yes |
| Yang Xiu | หยางซิว | Earth | |

Party max **5**. Lineup order is the World strip / spawn order. **Turn order is still AGI** (plus allocated AGI).

## EXP / level-up

- `UnitDefinition.expReward` on monsters (Wolf 22, Bandit 24, Frog 20, Golem 28, Bird 16, Soldier 22). Fallback 25 if a monster has 0.
- Curve: `ExpToNext(level) = 20 + level * 15` (Lv1 → 35). Two Forest Wolves = 44 → first level-up.
- Each level: **3 unspent points**. Manual: HP+8, SP+4, ATK/INT/DEF/AGI+2. Cap Lv 20.
- After a win, Battle shows the allocate panel if anyone has points. Same panel is always on the World party window.

## Auto battle (two modes)

Top-right of Battle:

- **Auto Attack** — after 0.35s on that unit’s command turn, Attack the **lowest-HP living enemy**.
- **Auto Heal** — if any ally HP% is **below** the threshold (default **40%**) and this actor has a heal skill + enough SP (ขงเบ้ง: Mend / South Wind Heal), use the cheapest heal on the lowest-% wounded ally. Otherwise fall through to Auto Attack (if on) or wait for a manual command.
- Click a command during the 0.35s delay, or toggle the mode off, to interrupt.

## Click-by-click Unity tests

### 1. Fight → gain EXP → level up allocate points

1. Play `World.unity`. Note party at **Lv1**, full HP.
2. Walk right into the green **Wolf** pack (2× Forest Wolf).
3. Fight and **win** (or Play `Battle.unity` and win the default 3v3).
4. Banner / log: `ได้ EXP 44` (or 66 for the 3-pack Frog). Each member should show **+1 Lv** and **3 pts**.
5. On the level-up panel, pick **จูล่ง** → click **ATK +2** (or HP / AGI). Points decrement; ATK (or HP/AGI) rises.
6. **กลับโลก** (World fight) or **ดำเนินการต่อ** (direct Battle). World strip shows **Lv2** and the new stats.

### 2. Party panel change lineup

1. In World press **P** (or the top-right button).
2. Right column: **เข้า** on **ลิโป้** and **หยางซิว** (party becomes 5).
3. Use **▲ / ▼** on a row — World strip order updates.
4. **ออก** on one member (cannot drop below 1).
5. Walk into a monster. Battle left side matches the new lineup (up to 5 cards).

### 3. Auto Attack / Auto Heal toggles in battle

1. Enter Battle. Top-right: leave both **off**. Issue **โจมตีปกติ** yourself — still works.
2. Enable **Auto Attack**. Next player turn, after ~0.35s, they Attack the lowest-HP enemy without a click.
3. Toggle **Auto Attack** off mid-delay — they wait for you.
4. Let someone drop below 40% HP (or set threshold to **100%** with **+** so any missing HP counts). Enable **Auto Heal**. On **ขงเบ้ง**’s turn they should Mend / South Wind Heal the wounded ally (green `+HP`) if SP ≥ cost.
5. If no one is under the threshold, or the actor has no heal/SP, Auto Heal does nothing and Auto Attack (if on) takes over.

### 4. HP persists World → Battle → World

1. Play World. Strip: e.g. จูล่ง **HP 110/110**.
2. Enter a fight. Take damage (do not heal fully). Note HP on the left HUD.
3. Win, allocate if prompted, return.
4. World strip HP/SP must match the Battle write-back (not reset to max).
5. Enter a second fight. Opening HP/SP/level must match that strip. Allocated ATK/AGI apply.

Lose: HP 0 is stored; the next Battle starts that member at **1 HP** so the party is not permanently wiped. City does **not** rest (so this test stays honest).

## Wind skip + formulas

Unchanged from Step 3: skip is rolled while Wind is active, then duration ticks.  
`python3 tools/verify_formulas.py` must print **PASS**. In Unity: **Tools → TS Online → Run Element Formula Tests**.

## Files

| Path | Role |
| --- | --- |
| `Assets/Scripts/Party/PartyManager.cs` | DontDestroyOnLoad party (max 5) + 6-general roster |
| `Assets/Scripts/Party/PartyMember.cs` | Runtime HP/SP/level/bonus stats |
| `Assets/Scripts/Party/ExpLevelSystem.cs` | EXP curve, grant, spend points |
| `Assets/Scripts/Party/PartyWorldUI.cs` | World strip, lineup, allocate |
| `Assets/Scripts/Battle/AutoBattleController.cs` | Auto Attack / Auto Heal |
| `Assets/Scripts/Battle/BattleBootstrap.cs` | Spawn from party, write-back, EXP, return |
| `Assets/Scripts/Battle/BattleUI.cs` | Auto toggles + post-battle level-up |
| `Assets/Scripts/Battle/BattleUnit.cs` | `Init(..., PartyMember)` applies runtime stats |
| `Assets/Scripts/Map/WorldParty.cs` | Shim → `PartyManager.ActiveDefinitions()` |
| `Assets/Scripts/Data/UnitDefinition.cs` | `expReward` |
| `Assets/Data/Monsters/*.asset` | Stamped `expReward` |
| `Assets/Editor/CreateDefaultDataMenu.cs` | Writes expReward when regenerating data |

## ภาษาไทย

เปิดซีน World กด **P** จัดปาร์ตี้ได้สูงสุด 5 จาก 6 ขุนพล  
ชนะรบได้ EXP แจกแต้มเอง (HP/SP/ATK/INT/DEF/AGI)  
ใน Battle เปิด Auto Attack / Auto Heal มุมขวาบน  
เลือดและเลเวลจำข้ามซีน World ↔ Battle
