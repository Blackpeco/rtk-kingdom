# Scenes (Step 1 stubs)

These four scenes exist as minimal 2D camera stubs so the folder layout matches the plan. None of them implement World movement or Battle UI.

| Scene | Purpose in later steps | Step 1 contents |
| --- | --- | --- |
| `Boot.unity` | Entry / splash | Orthographic camera only |
| `CharacterCreate.unity` | Party setup | Orthographic camera only |
| `World.unity` | Overworld | Orthographic camera only |
| `Battle.unity` | Combat | Camera + `DamageFormulaSmokeTest` GameObject |

## Recreate a scene in the Editor (if YAML fails to import)

1. **File → New Scene → Basic (Built-in)** or **2D (URP)**.
2. Keep the Main Camera orthographic.
3. **File → Save As** into `Assets/Scenes/` using the exact names above.
4. For Battle only: **GameObject → Create Empty**, name it `DamageFormulaSmokeTest`, **Add Component → Damage Formula Smoke Test**.
5. **File → Build Settings…** and add the four scenes in Boot / CharacterCreate / World / Battle order.

Play Mode formula checks: open `Battle`, press Play, read the Console for `PASS` / `FAIL`.
