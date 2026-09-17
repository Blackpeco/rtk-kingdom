# UI polish — how to see it in the Editor

Readability pass only. Formulas, save, Wind skip, LastEnd, and Steps 1–5 loops are unchanged. Offline; no networking.

Play **`Assets/Scenes/Boot.unity`** after compile (or the scene named below). Game view **16:9** (e.g. 1600×900) matches the Battle scaler.

## 1. CharacterCreate — element colors + stat scan

1. Wipe a save if needed: World **เกมใหม่ / ลบเซฟ (F8)**, or delete `ts_online_save.json`.
2. Play **Boot**. You should land on **สร้างตัวละคร**, not the city.
3. Four **wide color buttons**: ดิน (brown) น้ำ (blue) ไฟ (red) ลม (green), Thai on top, English under.
4. Click each element. The **stat strip** under the role line highlights the signature stats (Earth HP/DEF, Water SP/INT, Fire ATK, Wind AGI).
5. Name + **เริ่มเดินทาง** still writes the lead and opens World (same `BeginNewGame` / save).

## 2. World — ground, actors, HUD

Play **World** (or Boot with a save).

| What | How it should read |
| --- | --- |
| Ground | **Left city** = light cobble / warm stone. **Right forest** = deep green. Tan road in the middle. |
| Zone chips | `เมือง (ปลอดภัย)` and `ป่า (สุ่มสู้)` on colored bars at the top of each side. |
| Lead | **Diamond** tinted to the created element, label `ชื่อ` + `หัวหน้า · ธาตุ`. |
| ปาโต้เยา | **Pink round blob** with a face, label `ปาโต้เยา`, still lerps behind the lead. |
| ยายเมือง | **Purple triangle / robe**, label `ยายเมือง`. Near → **กด E เพื่อคุย**. |
| Wanderers | Green **diamond** `หมาป่า`, red **square** `โจร`, blue **circle** `กบ`. Same packs / roam. |
| Decor | Houses / well in the city, dark tree triangles in the forest (no colliders). |

**WorldHUD** (top-left, larger type):

- Name + Thai element in the element color
- **Green HP** and **blue SP** strips for the lead
- Controls hint, **quest line** (gold), last battle result
- **บันทึก (F5) / โหลด (F9) / เกมใหม่ (F8)** larger on the right

## 3. Party panel (P)

1. Press **P** or **ปาร์ตี้ / เลเวลอัพ (P)** (top-right).
2. World strip under the HUD: numbered rows, element color tick, **Lv** + **green HP**.
3. Full panel: taller rows, **หัวหน้า** gold badge on the created lead (still no ออก), ▲ ▼ / เข้า / ออก unchanged.
4. Alloc block: HP green, SP blue, same +HP/+SP/+ATK… buttons.

## 4. Battle — sides, bars, commands, popups

Play **Battle** (test 3v3) or walk into a wanderer.

| What | How it should read |
| --- | --- |
| Arena | Cool floor **ฝ่ายเรา** (left), warm floor **ศัตรู** (right), gold mid line. |
| Units | Players = **diamonds**, enemies = **squares**, element color + Thai ธาตุ under the name. ปาโต้เยา still bottom-left blob. |
| HUD cards | Left / right columns with **ฝ่ายเรา / ศัตรู** headers. Color chip next to the name. **HP green**, **SP blue**, thicker bars. |
| Commands | Larger, more spaced: โจมตีปกติ / สกิล / ปาโต้เยา / สมุนไพร / ป้องกัน / หนี. Same `ChooseCommand` wiring. |
| Popups | Damage numbers bigger. **ข่มธาตุ!** gold + dark plate, **ธาตุต้าน** gray, longer linger. |
| Auto | Top-right **Auto Battle** panel, larger toggles and Heal % − / +. |

Advantage still comes from the existing formula (`E=1.25` / `E=0.80` in the log).

## 5. Placeholders / icons

Runtime actors still use generated sprites in `WorldArt` (Play Mode does not require the PNGs).

On disk (overwrite in the Project window if you want to swap art later):

- `Assets/Art/Icons/{earth,water,fire,wind}.png` — 32×32 symbol tiles (GUIDs kept so `ElementDefinition` refs stay valid)
- `Assets/Art/Placeholders/` — `player_lead`, `patoyo`, `grandma`, `wolf`, `bandit`, `frog`, `city_tile`, `forest_tile`

Regenerate: `python3 tools/generate_placeholders.py`

## Must still work (do not treat as UI)

- Steps 1–5 loops, CharacterCreate → World, encounter → Battle → return
- `python3 tools/verify_formulas.py` → **PASS**
- Wind skip (`QA: Force Wind skip`), F5 / F9 / F8, LastEnd line after a fight
- Auto Attack / Auto Heal / Patoyo charges / Herb

No 3D, no networking, no gacha.

## 6. Layout bugfix (party vs F5/F9/F8 + text clip)

After the readability pass, two layout issues were fixed. Gameplay / save / LastEnd / formulas are unchanged.

### Before → after

| Issue | Before | After |
| --- | --- | --- |
| Party open + short Game view | WorldHUD **บันทึก / โหลด / เกมใหม่** sat on the party modal. At **1024×576**, F8 could steal **เข้า** (wipe save). | Those three **GUI.Button**s are **not drawn** while `PartyMenuOpen`. **F5 / F9 / F8 keys still work.** Toast may still show under the P button (label only). |
| Party header | Title 22px in a 28px row wrapped and lost “ตาในรบเรียง AGI”. | Title “ปาร์ตี้” + Hint subtitle with the full clause. |
| HUD hint | One 22px line; WASD copy wrapped into the gold quest band. | Two reserved lines; shorter copy. HUD is 188px, party strip starts at 208. |
| Party strip names | Long 16-char names spilled into Lv/HP. | Ellipsis in the 250px name column. |
| ยายเมือง dialog | Fixed 560px — off-screen under ~560 width. | `min(560, Screen.width-40)`, buttons shrink. |
| CharacterCreate | **เริ่มเดินทาง** sat on the hint on short heights. | Confirm + hint + error are bottom-anchored; element/stat block shrinks. |
| Alloc +stat row | Six 110px buttons clipped under ~740px panel width; roster could cover alloc on short heights. | Wrap to 2×3 when narrow; party/roster lists scroll above alloc. |

## 7. Party (P) vs ยายเมือง (E)

The party modal and the city quest dialog must not be open together (OnGUI stacking stole เข้า / รับเควสต์ clicks). **Block opening the other** while one is active; close one, then the other works. `MenuOpen` / WASD lock is unchanged. F5/F9/F8 still hide only while the party panel is open.

### Verify in the Editor

`python3 tools/verify_formulas.py` → **PASS** (run from repo root).

Play **Boot** (or World with a save). Check **1024×576**, **1280×720**, and **1600×900**:

1. World, party **closed**: F5 / F9 / F8 buttons still click. Keys still work.
2. Press **P**. Save/new-game **buttons disappear**. Click first roster **เข้า**, ▲ ▼, **ออก**, alloc +HP — must not jump to CharacterCreate. Press **F5** (key) — toast under the P button, no wipe. Esc / ปิดปาร์ตี้ — buttons return.
3. Party header shows the full AGI clause. Strip names with a 16-char create name stay in the left column (`…`).
4. HUD: two hint lines, then gold quest, then LastEnd — no overlap. After a forest fight, LastEnd still appears.
5. ยายเมือง **E**: dialog stays on screen if you shrink the Game view; **รับเควสต์** / **ปิด** still clickable.
6. **F8** (closed party) or Boot wipe → CharacterCreate. Shrink height (~480): **เริ่มเดินทาง** stays below the hint; empty name still errors; confirm still starts World.
7. Boot → create → wanderer → Battle commands / auto → return. F9 load. Sandbox **Battle.unity** then World must **not** fake LastEnd.
8. **P** and ยายเมือง **E** are exclusive: party open → E does nothing and the quest dialog is not drawn; dialog open → P / ปาร์ตี้ does not open the panel. Close one (Esc / ปิด / ปิดปาร์ตี้), then the other works. WASD still locked while either is open. F5/F9/F8 hide-while-party unchanged.
9. Forest wanderer must **not** start Battle while `MenuOpen` (party panel **or** ยายเมือง dialog). They still roam. Close the UI; if you are still overlapping, the fight starts (same save-on-enter / grace lock).
