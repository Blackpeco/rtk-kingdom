#!/usr/bin/env python3
"""Regenerate readable chibi-placeholder icons and world sprites (no gameplay data)."""
from __future__ import annotations

import hashlib
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def guid_for(key: str) -> str:
    return hashlib.md5(f"ts-online-step1:{key}".encode()).hexdigest()


def png_chunk(tag: bytes, data: bytes) -> bytes:
    return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)


def write_png_rgba(path: Path, pixels: list[tuple[int, int, int, int]], size: int) -> None:
    raw = b""
    for y in range(size):  # y=0 is the top of the PNG
        raw += b"\x00"
        for x in range(size):
            raw += bytes(pixels[y * size + x])
    ihdr = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    png = b"\x89PNG\r\n\x1a\n" + png_chunk(b"IHDR", ihdr) + png_chunk(b"IDAT", zlib.compress(raw, 9)) + png_chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(png)


def texture_meta(guid: str, ppu: int, max_size: int = 64) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: {max_size}
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {max_size}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {guid[:16]}
    internalID: 1537655125
    vertices: []
    indices: 
    edges: []
    weights: []
  spritePackingTag: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def blank(size: int, color=(0, 0, 0, 0)) -> list[tuple[int, int, int, int]]:
    return [color] * (size * size)


def plot(px, size, x, y, c):
    if 0 <= x < size and 0 <= y < size:
        px[y * size + x] = c


def fill_rect(px, size, x0, y0, x1, y1, c):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            plot(px, size, x, y, c)


def fill_circle(px, size, cx, cy, r, c):
    r2 = r * r
    for y in range(size):
        for x in range(size):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r2:
                plot(px, size, x, y, c)


def fill_diamond(px, size, cx, cy, r, c):
    for y in range(size):
        for x in range(size):
            if abs(x - cx) + abs(y - cy) <= r:
                plot(px, size, x, y, c)


def fill_triangle(px, size, cx, y_top, y_bot, c):
    height = max(1, y_bot - y_top)
    for y in range(y_top, y_bot + 1):
        t = (y - y_top) / height
        half = int(t * (size * 0.36))
        for x in range(cx - half, cx + half + 1):
            plot(px, size, x, y, c)


def outline(px, size, edge):
    out = px[:]
    for y in range(size):
        for x in range(size):
            if px[y * size + x][3] == 0:
                continue
            for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                nx, ny = x + dx, y + dy
                if not (0 <= nx < size and 0 <= ny < size) or px[ny * size + nx][3] == 0:
                    out[y * size + x] = edge
                    break
    return out


def shade(base, hi, lo, x, y, size):
    if y < size * 0.42 and x < size * 0.58:
        return hi
    if y > size * 0.72:
        return lo
    return base


def icon_earth(size=32):
    px = blank(size)
    fill_rect(px, size, 4, 4, 27, 27, (184, 135, 61, 255))
    fill_triangle(px, size, 16, 8, 22, (120, 82, 32, 255))
    fill_rect(px, size, 6, 6, 25, 9, (210, 176, 96, 255))
    return outline(px, size, (70, 46, 18, 255))


def icon_water(size=32):
    px = blank(size)
    fill_rect(px, size, 4, 4, 27, 27, (46, 134, 193, 255))
    # drop
    fill_circle(px, size, 16, 11, 6, (210, 236, 255, 255))
    fill_triangle(px, size, 16, 10, 25, (210, 236, 255, 255))
    fill_circle(px, size, 14, 10, 2, (255, 255, 255, 255))
    return outline(px, size, (16, 52, 92, 255))


def icon_fire(size=32):
    px = blank(size)
    fill_rect(px, size, 4, 4, 27, 27, (200, 58, 42, 255))
    fill_triangle(px, size, 16, 6, 26, (255, 196, 64, 255))
    fill_triangle(px, size, 16, 10, 24, (255, 92, 36, 255))
    fill_circle(px, size, 16, 12, 3, (255, 230, 140, 255))
    return outline(px, size, (88, 18, 12, 255))


def icon_wind(size=32):
    px = blank(size)
    fill_rect(px, size, 4, 4, 27, 27, (72, 186, 118, 255))
    for i, y in enumerate((10, 16, 22)):
        fill_rect(px, size, 7 + i, y, 24, y + 2, (230, 255, 230, 255))
        plot(px, size, 25, y + 1, (230, 255, 230, 255))
        plot(px, size, 26, y, (230, 255, 230, 255))
    return outline(px, size, (20, 72, 40, 255))


def sprite_lead(size=32):
    px = blank(size)
    body = (232, 196, 64, 255)
    hi = (255, 232, 140, 255)
    lo = (140, 96, 24, 255)
    fill_diamond(px, size, 16, 17, 13, body)
    for y in range(size):
        for x in range(size):
            if px[y * size + x] == body:
                px[y * size + x] = shade(body, hi, lo, x, y, size)
    fill_circle(px, size, 16, 10, 5, (255, 224, 186, 255))
    plot(px, size, 14, 10, (40, 24, 16, 255))
    plot(px, size, 18, 10, (40, 24, 16, 255))
    return outline(px, size, (70, 46, 10, 255))


def sprite_patoyo(size=32):
    px = blank(size)
    fill_circle(px, size, 16, 16, 12, (255, 132, 178, 255))
    fill_circle(px, size, 11, 11, 3, (255, 196, 214, 255))
    plot(px, size, 12, 15, (48, 16, 28, 255))
    plot(px, size, 20, 15, (48, 16, 28, 255))
    fill_rect(px, size, 14, 19, 18, 20, (180, 48, 80, 255))
    return outline(px, size, (120, 32, 64, 255))


def sprite_grandma(size=32):
    px = blank(size)
    fill_triangle(px, size, 16, 6, 28, (150, 102, 214, 255))
    fill_circle(px, size, 16, 9, 5, (236, 210, 186, 255))
    fill_rect(px, size, 11, 4, 21, 7, (90, 56, 140, 255))
    plot(px, size, 14, 9, (40, 24, 32, 255))
    plot(px, size, 18, 9, (40, 24, 32, 255))
    return outline(px, size, (56, 28, 92, 255))


def sprite_wolf(size=32):
    px = blank(size)
    fill_diamond(px, size, 16, 17, 12, (64, 196, 110, 255))
    fill_triangle(px, size, 10, 4, 12, (40, 140, 72, 255))
    fill_triangle(px, size, 22, 4, 12, (40, 140, 72, 255))
    plot(px, size, 13, 16, (16, 32, 16, 255))
    plot(px, size, 19, 16, (16, 32, 16, 255))
    return outline(px, size, (16, 64, 28, 255))


def sprite_bandit(size=32):
    px = blank(size)
    fill_rect(px, size, 7, 8, 24, 26, (220, 64, 46, 255))
    fill_rect(px, size, 10, 4, 21, 12, (255, 210, 176, 255))
    fill_rect(px, size, 10, 4, 21, 7, (48, 20, 16, 255))
    plot(px, size, 13, 9, (32, 12, 12, 255))
    plot(px, size, 18, 9, (32, 12, 12, 255))
    return outline(px, size, (88, 16, 12, 255))


def sprite_frog(size=32):
    px = blank(size)
    fill_circle(px, size, 16, 18, 11, (46, 140, 220, 255))
    fill_circle(px, size, 10, 11, 4, (80, 190, 255, 255))
    fill_circle(px, size, 22, 11, 4, (80, 190, 255, 255))
    plot(px, size, 10, 11, (12, 24, 48, 255))
    plot(px, size, 22, 11, (12, 24, 48, 255))
    fill_rect(px, size, 13, 20, 19, 22, (20, 48, 96, 255))
    return outline(px, size, (12, 48, 96, 255))


def tile_city(size=32):
    px = blank(size)
    for y in range(size):
        for x in range(size):
            cell = 8
            ox = cell // 2 if (y // cell) % 2 else 0
            mortar = ((x + ox) % cell == 0) or (y % cell == 0)
            n = ((x * 17 + y * 31) & 255) / 255
            stone = (int(118 + 40 * n), int(112 + 36 * n), int(96 + 28 * n), 255)
            px[y * size + x] = (76, 70, 60, 255) if mortar else stone
    return px


def tile_forest(size=32):
    px = blank(size)
    for y in range(size):
        for x in range(size):
            n = ((x * 13 + y * 29) & 255) / 255
            g = int(28 + 48 * n)
            px[y * size + x] = (18, g, 22, 255)
            if (x + y * 3) % 17 == 0:
                px[y * size + x] = (14, 40, 18, 255)
    return px


def keep_icon_guid(rel: str, existing: str | None) -> str:
    meta = ROOT / (rel + ".meta")
    if meta.exists():
        for line in meta.read_text().splitlines():
            if line.startswith("guid: "):
                return line.split()[1]
    return existing or guid_for(rel)


def write_sprite(rel: str, pixels, size: int, ppu: int, keep_guid: bool = False) -> None:
    path = ROOT / rel
    write_png_rgba(path, pixels, size)
    guid = keep_icon_guid(rel, None) if keep_guid else guid_for(rel)
    (ROOT / (rel + ".meta")).write_text(texture_meta(guid, ppu, max(64, size)), encoding="utf-8")
    print(f"wrote {rel} guid={guid}")


def main() -> None:
    write_sprite("Assets/Art/Icons/earth.png", icon_earth(), 32, 32, keep_guid=True)
    write_sprite("Assets/Art/Icons/water.png", icon_water(), 32, 32, keep_guid=True)
    write_sprite("Assets/Art/Icons/fire.png", icon_fire(), 32, 32, keep_guid=True)
    write_sprite("Assets/Art/Icons/wind.png", icon_wind(), 32, 32, keep_guid=True)

    write_sprite("Assets/Art/Placeholders/player_lead.png", sprite_lead(), 32, 32)
    write_sprite("Assets/Art/Placeholders/patoyo.png", sprite_patoyo(), 32, 32)
    write_sprite("Assets/Art/Placeholders/grandma.png", sprite_grandma(), 32, 32)
    write_sprite("Assets/Art/Placeholders/wolf.png", sprite_wolf(), 32, 32)
    write_sprite("Assets/Art/Placeholders/bandit.png", sprite_bandit(), 32, 32)
    write_sprite("Assets/Art/Placeholders/frog.png", sprite_frog(), 32, 32)
    write_sprite("Assets/Art/Placeholders/city_tile.png", tile_city(), 32, 32)
    write_sprite("Assets/Art/Placeholders/forest_tile.png", tile_forest(), 32, 32)
    print("placeholder sprites ready")


if __name__ == "__main__":
    main()
