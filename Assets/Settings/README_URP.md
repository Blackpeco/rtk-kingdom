# 2D URP setup (do this once in the Unity Editor)

This repo lists **URP 14** in `Packages/manifest.json` (`com.unity.render-pipelines.universal` + `com.unity.2d.sprite`). There is **no** hand-authored `GraphicsSettings.asset` / pipeline asset — those YAML blobs break across Unity 2022 vs Unity 6.

Script-only tests (`Tools/TS Online/…`, formula smoke test) work on the **Built-in** renderer. Pink 3D materials do not block Battle Play Mode (placeholders are runtime sprites).

## Exact clicks — Unity 2022.3 LTS

1. **Assets → Create → Rendering → URP Asset (with 2D Renderer)**
2. Save as `Assets/Settings/TSOnlineURP.asset` (the 2D Renderer Data appears beside it).
3. **Edit → Project Settings → Graphics**
4. **Scriptable Render Pipeline Settings** → drag `TSOnlineURP`.
5. **Edit → Project Settings → Quality**
6. For each quality level you use, set **Render Pipeline Asset** to the same URP asset.
7. Open `Assets/Scenes/Battle.unity` and press Play.

## Unity 6

Same menu path. If the 2022.3 packages upgrade on first open, accept the upgrade, then create a **fresh** URP + 2D Renderer (do not reuse a 14.x asset on 17.x).

## Element icons missing after first import

**Tools → TS Online → Create Default Data Assets** reassigns `Assets/Art/Icons/{earth,water,fire,wind}.png` onto the four `ElementDefinition` assets.
