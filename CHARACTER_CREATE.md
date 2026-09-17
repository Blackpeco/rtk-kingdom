# CharacterCreate — name + element

Offline. Boot with **no save** opens this scene instead of World defaults.

## Flow

1. Play **`Assets/Scenes/Boot.unity`**.
2. **No save** → `CharacterCreate.unity` (name + ดิน/น้ำ/ไฟ/ลม).
3. **เริ่มเดินทาง** → World. Created hero is party lead (not one of the 6 generals). Recruit generals later with **P**.
4. **Has save** → World as before (party / quest / position restored).

Direct Play on **`World.unity`** still uses the Step 4 default 3 generals if you never created a hero.  
**`Battle.unity`** always uses the wired test 3v3 (it does not hydrate a save). World encounters use the created lead.

**เริ่มเดินทาง** / `BeginNewGame` clears quest, inventory, and auto-battle prefs (same statics as **F8**), so Disable Domain Reload cannot leave a finished quest + herbs on a new hero.

A save file that fails to load is **deleted**; Boot then opens CharacterCreate instead of trapping you in a default World.

## Starting stats

| Element | Bias | HP | SP | ATK | INT | DEF | AGI | Extra skill |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Earth / ดิน | tank / control | 130 | 35 | 55 | 30 | **70** | 40 | Stone Fist |
| Water / น้ำ | heal / buff | 90 | **95** | 40 | **90** | 40 | 50 | Mend |
| Fire / ไฟ | damage | 95 | 40 | **100** | 35 | **28** | 60 | Torch Slash |
| Wind / ลม | speed | 95 | 50 | 70 | 40 | 38 | **100** | Wind Claw |

Everyone also gets Basic Strike. World avatar color matches the element. HUD title and party strip show the chosen **name**.

## Click-by-click

### 1. Fresh Boot → create

1. Wipe any old save (**F8** / **เกมใหม่** in World, or delete `ts_online_save.json` — see below).
2. Play **Boot**. After ~0.4s you must see **สร้างตัวละคร**, not the city.
3. Type a name (e.g. `มีนา`). Click **Fire / ไฟ**. Confirm ATK 100, DEF 28.
4. **เริ่มเดินทาง**. World title: `มีนา  ไฟ`. Gold-red lead quad. Party strip line 1 is มีนา, not จูล่ง.
5. **P** — roster still has the 6 generals; **เข้า** still works. The created lead shows **หัวหน้า** (no ออก).

Repeat once per element (or check the preview numbers before confirm).

### 2. Save / load keeps name + element

1. After create, walk a bit, **F5**.
2. Stop Play. Play **Boot** again — you skip create and land in World as `มีนา  ไฟ` with the same HP/element.
3. **F9** mid-World also restores name + element.

### 3. Wipe save to re-test create

Any of:

- World: **เกมใหม่ / ลบเซฟ (F8)** — deletes the JSON, resets party/quest, loads CharacterCreate.
- Delete `Application.persistentDataPath/ts_online_save.json` (path is logged on **F5**).
- Trash the JSON so `TryLoad` fails — Boot discards it and opens create.
- Then Play **Boot** — create screen again.

After a Battle-only win, **ดำเนินการต่อ** on the level-up panel writes the allocated points to the save (same as World return).

## Files

| Path | Role |
| --- | --- |
| `Assets/Scripts/Map/CharacterCreateUI.cs` | Name + element UI |
| `Assets/Scripts/Party/CreatedHero.cs` | Stats, skills, runtime `UnitDefinition` |
| `Assets/Resources/CreatedHero/SkillKit.asset` | Starting skills for player builds |
| `Assets/Scripts/Map/BootLoader.cs` | Save → World, else CharacterCreate |
| `SaveService` / `GameSave` | `playerName`, `playerElement`, wipe |
| `PartyManager.BeginNewGame` | Lead + unlocked generals |
| `Assets/Scenes/CharacterCreate.unity` | Hosts the UI |

## ภาษาไทย

เปิด Boot ถ้ายังไม่มีเซฟจะมาหน้าสร้างตัว ใส่ชื่อเลือกธาตุ  
หัวหน้าปาร์ตี้คือตัวที่สร้าง ขุนพล 6 คนยังเรียกได้จากแผง P  
**F8** ลบเซฟแล้วสร้างใหม่
