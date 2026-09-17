# Step 5 — Patoyo, one city quest, simple save

Offline only. No networking, gacha, CharacterCreate polish, or a multi-quest journal.

## What to play

1. Play **`Assets/Scenes/Boot.unity`** (loads a save if one exists) or **`World.unity`**.
2. A pink circle **ปาโต้เยา** lerps behind the gold lead. It is **not** a party slot (a full 5-general party still has Patoyo).
3. Purple **ยายเมือง** stands in the city (left). Walk near → **E** → accept **ป่าไม่สงบ**.
4. Win **2** forest fights (right). HUD shows `ชนะการรบในป่า 1/2`. Return, **E**, **ส่งเควสต์** → สมุนไพร ×2 + EXP 40.
5. **F5** / **บันทึก** writes JSON. Stop Play, Play **Boot** or **F9** / **โหลด** — party, HP, quest, herbs, and world position come back.

Direct Play on `Battle.unity` still works. Pink Patoyo sits at the bottom-left; command **ปาโต้เยา** heals an ally (2 charges / fight, +22 HP). After the quest, **สมุนไพร** uses a Herb (+16 HP).

## Patoyo

| Place | Behavior |
| --- | --- |
| World | `PatoyoFollower` lerps behind the lead (~0.9 units) |
| Battle | Visual only — **not** in `PartyManager` / not a 6th general |
| Help | Command **ปาโต้เยา** → pick an ally (or auto if only one living) → +22 HP, 2 times per battle |

Fill the party to 5 (P → เข้า) and enter a fight: five HUD cards, Patoyo still in the corner and still healable.

## Quest (one)

1. City left → ยายเมือง → **E** → **รับเควสต์**.
2. Walk right, win any two World encounters (lose / escape do not count).
3. HUD: `กลับไปหายายเมืองเพื่อรับรางวัล`.
4. **E** → **ส่งเควสต์**. Reward: 2× Herb (8-slot bag, stacks) + 40 EXP to each current party member. Completing saves automatically.

## Save / load

File: `Application.persistentDataPath/ts_online_save.json` (Editor: look at the Console `[Save] Wrote …`).

Persists: roster ids, party order, level / EXP / unspent / bonus stats / HP / SP, inventory, quest phase + win count, world X/Y, Auto Attack / Auto Heal / threshold.

| When | What |
| --- | --- |
| **F5** or **บันทึก** | Manual |
| Enter Battle from World | Position + party |
| Leave Battle → World | Party write-back, EXP, quest progress, position |
| Quest accept / complete | Party + quest + bag |
| Direct Play `Battle.unity` | Never writes the player save and never sets `LastEnd` (test 3v3 only) |

**Boot** loads only if the save file exists **and** `TryLoad` succeeds; a missing or corrupt file opens CharacterCreate (the bad file is deleted). Playing `World.unity` directly still calls `HydrateIfNeeded` once per session (does **not** reload over in-memory state when you return from Battle).

### Verify save

1. Accept the quest, win 1 fight, allocate a stat, walk somewhere notable, **F5**.
2. Stop Play. Play **Boot**.
3. You should be near that spot, quest `1/2`, same HP / level / points, Patoyo still following.

**F9** mid-World reloads the file onto the current session (position included).

## Click-by-click

### 1. Patoyo follows + battle help (full party)

1. Play World. Walk — the pink circle trails the gold lead.
2. **P** → add generals until 5 → close.
3. Touch a wanderer. Battle: 5 cards on the left, ปาโต้เยา at bottom-left.
4. Take a hit. **ปาโต้เยา** → pick the wounded ally → pink `+22`. Repeat once more, then the button goes dark.

### 2. City quest

1. From city spawn, walk to the purple NPC, **E**, **รับเควสต์**.
2. Win two forest battles. HUD counter ticks.
3. Return, **E**, **ส่งเควสต์**. HUD: สมุนไพร ×2. Next battle, **สมุนไพร** heals +16.

### 3. Save / load

1. After the reward (or mid-quest), **F5**. Console shows the JSON path.
2. Stop. Play Boot. Confirm quest line, herbs, party vitals, position.
3. Optional: change lineup, **F9** — lineup and HP revert to the file.

## Wind skip + formulas

Unchanged. `python3 tools/verify_formulas.py` must print **PASS**.

## Files

| Path | Role |
| --- | --- |
| `Assets/Scripts/Map/PatoyoFollower.cs` | World follow |
| `Assets/Scripts/Battle/PatoyoHelper.cs` | Charges + battle sprite |
| `Assets/Scripts/Quest/QuestTracker.cs` | One quest state |
| `Assets/Scripts/Quest/CityQuestNpc.cs` | ยายเมือง talk / turn-in |
| `Assets/Scripts/Inventory/InventoryService.cs` | 8-slot bag, Herb |
| `Assets/Scripts/Save/SaveService.cs` | JSON + PlayerPrefs flag |
| `Assets/Scripts/Save/GameSave.cs` | Serializable snapshot |
| `TurnManager` / `BattleUI` | ปาโต้เยา + สมุนไพร commands |
| `BattleBootstrap` | Reset helper, quest win, save |
| `WorldBootstrap` / `WorldHUD` / `BootLoader` | Spawn + hydrate + F5/F9 |

## ภาษาไทย

ปาโต้เยาเดินตามในโลก สีชมพู — ในรบกด **ปาโต้เยา** รักษาได้ 2 ครั้ง ไม่กินช่องปาร์ตี้  
ยายเมืองซ้าย กด **E** รับเควสต์ ชนะป่า 2 ครั้ง กลับมารับสมุนไพร  
**F5** เซฟ  **F9** โหลด  หรือเปิดซีน Boot หากมีไฟล์เซฟ
