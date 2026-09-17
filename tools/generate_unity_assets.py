#!/usr/bin/env python3
"""Generate Unity .meta, ScriptableObject YAML, tiny PNGs, and stub scenes."""
from __future__ import annotations

import hashlib
import struct
import zlib
from pathlib import Path

ROOT = Path("/workspace")


def guid_for(key: str) -> str:
    return hashlib.md5(f"ts-online-step1:{key}".encode()).hexdigest()


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def script_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def folder_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def native_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def default_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def scene_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def texture_meta(guid: str) -> str:
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
  maxTextureSize: 64
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
  spritePixelsToUnits: 8
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
    maxTextureSize: 64
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


def png_chunk(tag: bytes, data: bytes) -> bytes:
    return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)


def write_png(path: Path, rgb: tuple[int, int, int], size: int = 8) -> None:
    r, g, b = rgb
    raw = b"".join(b"\x00" + bytes([r, g, b]) * size for _ in range(size))
    ihdr = struct.pack(">IIBBBBB", size, size, 8, 2, 0, 0, 0)
    png = b"\x89PNG\r\n\x1a\n" + png_chunk(b"IHDR", ihdr) + png_chunk(b"IDAT", zlib.compress(raw)) + png_chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(png)


def so_header(script_guid: str, name: str) -> str:
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier: 
"""


def ref_so(guid: str) -> str:
    return f"{{fileID: 11400000, guid: {guid}, type: 2}}"


def write_asset(rel: str, body: str) -> None:
    path = ROOT / rel
    write(path, body)
    write(Path(str(path) + ".meta"), native_meta(guid_for(rel)))


SCRIPTS = [
    "Assets/Scripts/Core/ElementType.cs",
    "Assets/Scripts/Core/DamageKind.cs",
    "Assets/Scripts/Core/SkillCategory.cs",
    "Assets/Scripts/Core/TargetingFlags.cs",
    "Assets/Scripts/Core/ElementStatusKind.cs",
    "Assets/Scripts/Core/ElementStatusSpec.cs",
    "Assets/Scripts/Core/StatusEffectSystem.cs",
    "Assets/Scripts/Core/ElementSystem.cs",
    "Assets/Scripts/Core/DamageRequest.cs",
    "Assets/Scripts/Core/DamageResult.cs",
    "Assets/Scripts/Core/DamageCalculator.cs",
    "Assets/Scripts/Core/ElementFormulaSelfTest.cs",
    "Assets/Scripts/Data/UnitStats.cs",
    "Assets/Scripts/Data/ElementDefinition.cs",
    "Assets/Scripts/Data/SkillDefinition.cs",
    "Assets/Scripts/Data/UnitDefinition.cs",
    "Assets/Scripts/Data/ItemDefinition.cs",
    "Assets/Scripts/Data/EncounterTable.cs",
    "Assets/Scripts/Battle/DamageFormulaSmokeTest.cs",
    "Assets/Scripts/Battle/AutoBattleController.cs",
    "Assets/Scripts/Party/PartyMember.cs",
    "Assets/Scripts/Party/ExpLevelSystem.cs",
    "Assets/Scripts/Party/PartyManager.cs",
    "Assets/Scripts/Party/PartyWorldUI.cs",
    "Assets/Editor/ElementFormulaTestWindow.cs",
    "Assets/Editor/CreateDefaultDataMenu.cs",
]

FOLDERS = [
    "Assets",
    "Assets/Scripts",
    "Assets/Scripts/Core",
    "Assets/Scripts/Map",
    "Assets/Scripts/Battle",
    "Assets/Scripts/Party",
    "Assets/Scripts/Data",
    "Assets/Scripts/UI",
    "Assets/Scripts/AI",
    "Assets/Data",
    "Assets/Data/Generals",
    "Assets/Data/Monsters",
    "Assets/Data/Skills",
    "Assets/Data/Items",
    "Assets/Data/Elements",
    "Assets/Scenes",
    "Assets/Art",
    "Assets/Art/Placeholders",
    "Assets/Art/UI",
    "Assets/Art/Icons",
    "Assets/Editor",
]


def element_asset(name: str, eid: str, en: str, th: str, enum_i: int, color, blurb: str, icon_guid: str, script: str) -> str:
    r, g, b = color
    return so_header(script, name) + f"""  id: {eid}
  displayName: {en}
  displayNameThai: {th}
  element: {enum_i}
  icon: {{fileID: 1537655125, guid: {icon_guid}, type: 3}}
  color: {{r: {r}, g: {g}, b: {b}, a: 1}}
  roleBlurb: {blurb}
"""


def skill_asset(
    name: str,
    sid: str,
    en: str,
    th: str,
    power: float,
    kind: int,
    element: int,
    category: int,
    sp: int,
    targeting: int,
    script: str,
) -> str:
    return so_header(script, name) + f"""  id: {sid}
  displayName: {en}
  displayNameThai: {th}
  power: {power}
  damageKind: {kind}
  element: {element}
  category: {category}
  spCost: {sp}
  targeting: {targeting}
  overrideStatusApplyChance: 0
  statusApplyChance: 0.3
"""


def unit_asset(
    name: str,
    uid: str,
    en: str,
    th: str,
    element: int,
    stats: tuple[int, int, int, int, int, int],
    general: bool,
    monster: bool,
    skill_guids: list[str],
    script: str,
    exp_reward: int = 0,
) -> str:
    hp, sp, atk, intel, defense, agi = stats
    if skill_guids:
        skills = "\n".join(f"  - {ref_so(g)}" for g in skill_guids)
    else:
        skills = "  []"
    return so_header(script, name) + f"""  id: {uid}
  displayName: {en}
  displayNameThai: {th}
  element: {element}
  baseStats:
    hp: {hp}
    sp: {sp}
    atk: {atk}
    intel: {intel}
    def: {defense}
    agi: {agi}
  startingSkills:
{skills}
  isGeneral: {1 if general else 0}
  isMonster: {1 if monster else 0}
  expReward: {exp_reward}
"""


def item_asset(name: str, iid: str, en: str, th: str, desc: str, script: str) -> str:
    return so_header(script, name) + f"""  id: {iid}
  displayName: {en}
  displayNameThai: {th}
  description: {desc}
  stackLimit: 99
  icon: {{fileID: 0}}
"""


def encounter_asset(name: str, eid: str, en: str, monster_guids: list[str], script: str) -> str:
    monsters = "\n".join(f"  - {ref_so(g)}" for g in monster_guids)
    return so_header(script, name) + f"""  id: {eid}
  displayName: {en}
  possibleMonsters:
{monsters}
  minCount: 1
  maxCount: 3
"""


def scene_yaml(extra_objects: str = "", extra_roots: str = "") -> str:
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {{r: 0.5, g: 0.5, b: 0.5, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.212, g: 0.227, b: 0.259, a: 1}}
  m_AmbientEquatorColor: {{r: 0.114, g: 0.125, b: 0.133, a: 1}}
  m_AmbientGroundColor: {{r: 0.047, g: 0.043, b: 0.035, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.478, b: 0.627, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 0}}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {{fileID: 0}}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {{fileID: 0}}
  m_LightingSettings: {{fileID: 0}}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {{fileID: 0}}
--- !u!1 &519420028
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 519420032}}
  - component: {{fileID: 519420031}}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!20 &519420031
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 519420028}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {{r: 0.11, g: 0.12, b: 0.14, a: 0}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 1
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 0
  m_HDR: 1
  m_AllowMSAA: 0
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 0
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!4 &519420032
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 519420028}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -10}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
{extra_objects}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {{fileID: 519420032}}{extra_roots}
"""


BATTLE_EXTRA = """--- !u!1 &880011001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 880011002}
  - component: {fileID: 880011003}
  m_Layer: 0
  m_Name: DamageFormulaSmokeTest
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &880011002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 880011001}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &880011003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 880011001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: SMOKE_GUID, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  runOnStart: 1
  runOnValidate: 0
"""


def main() -> None:
    for folder in FOLDERS:
        d = ROOT / folder
        d.mkdir(parents=True, exist_ok=True)
        write(d.with_suffix(d.suffix + "") / ".gitkeep" if False else d / ".gitkeep", "")
        write(Path(str(d) + ".meta"), folder_meta(guid_for(folder + "/")))

    # Don't leave .gitkeep in folders that already have content; keep them only for empty ones.
    for folder in FOLDERS:
        keep = ROOT / folder / ".gitkeep"
        if keep.exists():
            keep.unlink()

    for rel in SCRIPTS:
        write(ROOT / (rel + ".meta"), script_meta(guid_for(rel)))

    smoke_guid = guid_for("Assets/Scripts/Battle/DamageFormulaSmokeTest.cs")
    element_script = guid_for("Assets/Scripts/Data/ElementDefinition.cs")
    skill_script = guid_for("Assets/Scripts/Data/SkillDefinition.cs")
    unit_script = guid_for("Assets/Scripts/Data/UnitDefinition.cs")
    item_script = guid_for("Assets/Scripts/Data/ItemDefinition.cs")
    encounter_script = guid_for("Assets/Scripts/Data/EncounterTable.cs")

    icons = {
        "earth": ((184, 135, 61), "Assets/Art/Icons/earth.png"),
        "water": ((46, 134, 193), "Assets/Art/Icons/water.png"),
        "fire": ((231, 76, 60), "Assets/Art/Icons/fire.png"),
        "wind": ((88, 214, 141), "Assets/Art/Icons/wind.png"),
    }
    icon_guids = {}
    for key, (rgb, rel) in icons.items():
        write_png(ROOT / rel, rgb)
        g = guid_for(rel)
        icon_guids[key] = g
        write(ROOT / (rel + ".meta"), texture_meta(g))

    # ElementType: None=0 Earth=1 Water=2 Fire=3 Wind=4
    write_asset(
        "Assets/Data/Elements/Earth.asset",
        element_asset("Earth", "earth", "Earth", "ดิน", 1, (0.72, 0.53, 0.24),
                      "Rock and armor. Beats Water (ข่มน้ำ). Opposite of Fire.",
                      icon_guids["earth"], element_script),
    )
    write_asset(
        "Assets/Data/Elements/Water.asset",
        element_asset("Water", "water", "Water", "น้ำ", 2, (0.22, 0.52, 0.82),
                      "Heal and flood. Beats Fire (ข่มไฟ). Opposite of Wind.",
                      icon_guids["water"], element_script),
    )
    write_asset(
        "Assets/Data/Elements/Fire.asset",
        element_asset("Fire", "fire", "Fire", "ไฟ", 3, (0.86, 0.24, 0.18),
                      "Burst and burn. Beats Wind (ข่มลม). Opposite of Earth.",
                      icon_guids["fire"], element_script),
    )
    write_asset(
        "Assets/Data/Elements/Wind.asset",
        element_asset("Wind", "wind", "Wind", "ลม", 4, (0.40, 0.82, 0.55),
                      "Speed and cuts. Beats Earth (ข่มดิน). Opposite of Water.",
                      icon_guids["wind"], element_script),
    )

    # DamageKind Physical=0 Magical=1
    # SkillCategory Attack=0 Heal=1 Buff=2 Wall=3 Stealth=4
    # TargetingFlags SingleAlly=2 AllAllies=4 SingleEnemy=8 AllEnemies=16
    skills = {
        "GaleSlash": ("gale_slash", "Gale Slash", "พายุดาบ", 1.15, 0, 4, 0, 8, 16),
        "GreenDragonStrike": ("green_dragon_strike", "Green Dragon Strike", "มังกรเขียว", 1.45, 0, 4, 0, 10, 8),
        "SkyPiercer": ("sky_piercer", "Sky Piercer", "ทวนทะลุฟ้า", 1.70, 0, 3, 0, 12, 8),
        "SerpentSpearSweep": ("serpent_spear_sweep", "Serpent Spear Sweep", "งูทวนกวาด", 1.20, 0, 3, 0, 9, 16),
        "FloodBolt": ("flood_bolt", "Flood Bolt", "สายฟ้าน้ำ", 1.35, 1, 2, 0, 10, 8),
        "SouthWindHeal": ("south_wind_heal", "South Wind Heal", "ลมใต้ฟื้นชีพ", 1.00, 1, 2, 1, 12, 2),
        "Rockslide": ("rockslide", "Rockslide", "หินถล่ม", 1.30, 1, 1, 0, 11, 16),
        "EarthenWall": ("earthen_wall", "Earthen Wall", "กำแพงดิน", 0.00, 1, 1, 3, 10, 4),
        "WindClaw": ("wind_claw", "Wind Claw", "กรงเล็บลม", 1.05, 0, 4, 0, 4, 8),
        "TorchSlash": ("torch_slash", "Torch Slash", "ดาบคบเพลิง", 1.10, 0, 3, 0, 5, 8),
        "BogSpit": ("bog_spit", "Bog Spit", "น้ำลายบึง", 1.10, 1, 2, 0, 5, 8),
        "StoneFist": ("stone_fist", "Stone Fist", "หมัดหิน", 1.00, 0, 1, 0, 4, 8),
        "DivePeck": ("dive_peck", "Dive Peck", "จิกพุ่ง", 1.00, 0, 4, 0, 3, 8),
        "BasicStrike": ("basic_strike", "Basic Strike (skill)", "สกิลไร้ธาตุ", 1.00, 0, 0, 0, 0, 8),
        "Mend": ("mend", "Mend", "รักษา", 0.90, 1, 0, 1, 6, 2),
    }
    skill_guids = {}
    for file, vals in skills.items():
        rel = f"Assets/Data/Skills/{file}.asset"
        skill_guids[file] = guid_for(rel)
        write_asset(rel, skill_asset(file, *vals, skill_script))

    generals = [
        ("ZhaoYun", "zhao_yun", "Zhao Yun", "จูล่ง", 4, (110, 40, 88, 35, 42, 95), ["GaleSlash", "BasicStrike"]),
        ("GuanYu", "guan_yu", "Guan Yu", "กวนอู", 4, (125, 35, 98, 30, 55, 70), ["GreenDragonStrike", "BasicStrike"]),
        ("LuBu", "lu_bu", "Lu Bu", "ลิโป้", 3, (130, 30, 110, 25, 48, 80), ["SkyPiercer", "BasicStrike"]),
        ("ZhangFei", "zhang_fei", "Zhang Fei", "เตียวหุย", 3, (140, 30, 92, 22, 58, 55), ["SerpentSpearSweep", "BasicStrike"]),
        ("ZhugeLiang", "zhuge_liang", "Zhuge Liang", "ขงเบ้ง", 2, (85, 90, 28, 105, 38, 60), ["FloodBolt", "SouthWindHeal", "Mend"]),
        ("YangXiu", "yang_xiu", "Yang Xiu", "หยางซิว", 1, (95, 80, 32, 92, 50, 52), ["Rockslide", "EarthenWall"]),
    ]
    for file, uid, en, th, element, stats, sk in generals:
        rel = f"Assets/Data/Generals/{file}.asset"
        write_asset(
            rel,
            unit_asset(file, uid, en, th, element, stats, True, False, [skill_guids[s] for s in sk], unit_script),
        )

    monsters = [
        ("ForestWolf", "forest_wolf", "Forest Wolf", "หมาป่า", 4, (55, 15, 42, 12, 18, 70), ["WindClaw"], 22),
        ("MountainBandit", "mountain_bandit", "Mountain Bandit", "โจรภูเขา", 3, (70, 10, 48, 10, 22, 40), ["TorchSlash"], 24),
        ("SwampFrog", "swamp_frog", "Swamp Frog", "กบทึง", 2, (50, 20, 28, 30, 16, 35), ["BogSpit"], 20),
        ("SmallStoneGolem", "small_stone_golem", "Small Stone Golem", "โกเล็มหินเล็ก", 1, (90, 10, 35, 8, 40, 15), ["StoneFist"], 28),
        ("RoofBird", "roof_bird", "Roof Bird", "นกหลังคา", 4, (40, 12, 30, 14, 12, 85), ["DivePeck"], 16),
        ("LostSoldier", "lost_soldier", "Lost Soldier", "ทหารหลงทาง", 3, (65, 12, 40, 16, 24, 38), ["TorchSlash", "BasicStrike"], 22),
    ]
    monster_guids = []
    for file, uid, en, th, element, stats, sk, exp in monsters:
        rel = f"Assets/Data/Monsters/{file}.asset"
        monster_guids.append(guid_for(rel))
        write_asset(
            rel,
            unit_asset(file, uid, en, th, element, stats, False, True, [skill_guids[s] for s in sk], unit_script, exp),
        )

    write_asset(
        "Assets/Data/Items/Herb.asset",
        item_asset("Herb", "herb", "Herb", "สมุนไพร", "Step 1 stub. Restores a little HP later.", item_script),
    )
    write_asset(
        "Assets/Data/ForestEdgeEncounters.asset",
        encounter_asset("ForestEdgeEncounters", "forest_edge", "Forest Edge", monster_guids, encounter_script),
    )

    scenes = {
        "Assets/Scenes/Boot.unity": scene_yaml(),
        "Assets/Scenes/CharacterCreate.unity": scene_yaml(),
        "Assets/Scenes/World.unity": scene_yaml(),
        "Assets/Scenes/Battle.unity": scene_yaml(
            BATTLE_EXTRA.replace("SMOKE_GUID", smoke_guid),
            extra_roots="\n  - {fileID: 880011002}",
        ),
    }
    for rel, body in scenes.items():
        write(ROOT / rel, body)
        write(ROOT / (rel + ".meta"), scene_meta(guid_for(rel)))

    print("Generated Unity metas, assets, icons, and scenes.")
    print("Smoke test script guid:", smoke_guid)
    print("ElementDefinition guid:", element_script)


if __name__ == "__main__":
    main()
