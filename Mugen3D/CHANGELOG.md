# Changelog

All notable changes to this package are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.7] - 2026-10-08

### Added

- `Third-Party Notices.txt` for NAudio (MIT).
- Managed WAV decode on every platform. MP3 playback stays on Windows, through NAudio.

### Changed

- API keys are stored in Editor preferences for the current project. They are not written into `Resources` or scenes, so a player build does not include them. Builds call `APISettingsConfig.SetCredentials` before login.
- Editor entries live under **Tools → Mugen3D**.
- Sample scripts are in the `Mugen3D.Samples` namespace. `NAudioPlayer` is in `Mugen3D`.
- The sample character is the untextured `Samples/Models/ARKit_head.fbx`, with one scene per pipeline (`MugenMorph`, `MugenMorph_URP`, `MugenMorph_BuiltIn`). Each scene uses that pipeline's default lit material. Xandra, its textures, and the CoreRP shaders are no longer part of this package.
- Editor install stat sends an anonymous id, Unity version, and package version. `machineId` and `keyId` are empty.

### Removed

- Shipped `APISettingsConfig` assets. Do not put access keys back into the package.

## [0.0.6] - 2026-09-16

### Added

- Unity Package Manager install (`com.mugen3d.mugenmorph`, display name **MugenMorph for Unity**). Git URL: `?path=/Mugen3D`. API keys save under the project's `Assets` so they survive package updates.
- Built-in and URP Xandra sample scenes (`MugenMorph_BuiltIn.unity`, `MugenMorph_URP.unity`) alongside the existing HDRP `MugenMorph.unity`.
- Pipeline-specific materials (`Materials/HDRP`, `Materials/URP`, `Materials/BuiltIn`) and CoreRP character shaders so the sample runs without the HDRP package.
- Editor import gate that hides HDRP Shader Graphs when HDRP is not installed.
- Package Manager **Samples → Import** and **Mugen3D → Samples**. Git packages are read-only; copy the scene into `Assets` before opening it.

### Changed

- Sample textures live under `Textures/`.
- README covers Git UPM, GitHub Releases `.unitypackage`, and copy-into-Assets.
- Character setup docs describe ARKit name matching; they no longer imply MetaHuman `Mesh.xxx` naming.
- Built-in / URP sample look: URP reads main-light intensity, Fill / Rim contribute, closer skin wrap and hair highlights.

### Fixed

- Runtime no longer calls `UnityEditor` APIs outside `#if UNITY_EDITOR`, so player builds compile.

## [0.0.5] - 2026-08-26

### Fixed

- Sample record / stop buttons call `MugenMorphSample`.
- Reset Xandra prefab pose.

## [0.0.4] - 2026-08-20

### Added

- Protobuf MugenMorph (`offline-mesh`), with JSON fallback.
- `PlayFromAudio`: one WAV in, blendshapes + audio out.
- Local WAV playback when the server returns no audio.
- WAV / MP3 decode via NAudio.
- Xandra HDRP + ARKit sample scene `MugenMorph.unity`.

### Changed

- Request body is now `{ "traceId", "data": " " }`.
- Sample scene is `MugenMorph.unity` (HDRP).

## [0.0.3] - 2026-03-31

### Changed

- Login API updates.
- README updates.
- Removed default Access Key / Secret Key from the shipped config.

## [0.0.2] - 2026-03-07

### Changed

- Replaced the FBX model used in the sample scene.

## [0.0.1] - 2026-03-06

Initial release: MugenMorph runtime, sample scene, and Project Settings API keys.
