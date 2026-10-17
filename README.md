# MugenMorph for Unity

Drive any ARKit-ready 3D character with speech-synchronized facial animation.

Send a WAV clip (microphone or file) to the [Mugen3D](https://mugen3d.ai/) MugenMorph API. The plugin returns blendshape coefficients and plays them back in sync with audio.

Get an **Access Key** and **Secret Key** from the [Developers](https://mugen3d.ai/api/developers/keys) page before running the sample.

> Keys stay in Editor preferences for this project. They are not saved into the package, and a player build does not include them.

---


https://github.com/user-attachments/assets/e621056c-3cb6-45af-ada3-ccb010bbf698


## Features

- Offline MugenMorph: audio in, blendshape frames + optional audio out
- ARKit blendshape playback (FLAME / bone-driven modes are also in the runtime)
- Microphone capture at 16 kHz PCM WAV (`AudioRecord`)
- Editor WAV picker in the sample
- Project Settings UI: **Tools → Mugen3D → API Settings** (also **Edit → Project Settings → Mugen3D → API Settings**)
- Install from Unity Package Manager (Git URL), a `.unitypackage` on [Releases](https://github.com/sumeurai/MugenMorph-For-Unity/releases), or copy into `Assets`
- Protobuf response by default, with JSON fallback

---

## Requirements

| Item | Version / note |
| --- | --- |
| Unity | 2020.3 LTS or newer |
| Render Pipeline | Runtime works on **Built-in, URP, and HDRP**. This package does not include a custom render pipeline. The sample has one scene per pipeline (see [Run the sample](#run-the-sample)) |
| Network | HTTPS access to `https://api.mugen3d.ai/` |
| Character | ARKit blendshape names on the face mesh (see [Character setup](#character-setup)) |
| Audio | Send **WAV** to the API. The sample recorder is **16 kHz**, 16-bit PCM. Playback decodes WAV on every platform. MP3 playback is Windows only |

---

## Folder layout

```
Mugen3D/
├── package.json
├── CHANGELOG.md
├── Runtime/                 # Pipeline-agnostic API + blendshape playback
├── Editor/
├── Plugins/                 # NAudio (Windows MP3). Other platforms decode WAV in C#
├── Third-Party Notices.txt
└── Samples/
    ├── Scripts/             # MugenMorphSample, AudioRecord
    ├── Prefabs/
    │   └── Canvas.prefab    # Record / local-file UI, used by all three scenes
    ├── Scenes/
    │   ├── MugenMorph.unity          # HDRP
    │   ├── MugenMorph_URP.unity      # URP
    │   ├── MugenMorph_BuiltIn.unity  # Built-in
    │   └── MugenMorph/
    │       └── Sky and Fog Settings Profile.asset  # HDRP volume profile for MugenMorph.unity
    └── Models/
        └── ARKit_head.fbx    # Untextured sample face
```

---

## Install

### Package Manager (recommended)

In Unity: **Window → Package Manager → + → Install package from git URL**, then paste:

```
https://github.com/sumeurai/MugenMorph-For-Unity.git?path=/Mugen3D
```

Or add this to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.mugen3d.mugenmorph": "https://github.com/sumeurai/MugenMorph-For-Unity.git?path=/Mugen3D"
  }
}
```

`?path=/Mugen3D` is required because `package.json` lives in the `Mugen3D` folder, not the repository root. Package id is `com.mugen3d.mugenmorph`.

Git packages live in a **read-only** Package Cache. Double-clicking a scene under `Packages/` shows *Opening scene in read-only package*. Use one of:

1. **Tools → Mugen3D → Samples → Open … Scene** — copies the scene to `Assets/Mugen3D Samples/` and opens that copy.
2. Package Manager → **MugenMorph for Unity → Samples → Import**, then open the scene from `Assets/Samples/…`.

### Unity package (GitHub Releases)

Each [release](https://github.com/sumeurai/MugenMorph-For-Unity/releases) includes a `.unitypackage` exported from the `Mugen3D` folder.

1. Download the `.unitypackage` from the latest release.
2. In Unity: **Assets → Import Package → Custom Package…** and select the file (or double-click it).
3. Import into `Assets/Mugen3D` (keep the `.meta` files checked).

This is the same layout as copying the folder into `Assets`. It is **not** a Package Manager install. HDRP: `Assets/Mugen3D/Samples/Scenes/MugenMorph.unity`. URP: `MugenMorph_URP.unity`. Built-in: `MugenMorph_BuiltIn.unity`.

### Copy into Assets

1. Copy the `Mugen3D` folder into your Unity project's `Assets` directory (keep `.meta` files).
2. Open the scene that matches the project's render pipeline: `MugenMorph.unity` (HDRP), `MugenMorph_URP.unity`, or `MugenMorph_BuiltIn.unity`.

### After install

1. Open **Tools → Mugen3D → API Settings** (or **Edit → Project Settings → Mugen3D → API Settings**).
2. Enter Access Key and Secret Key, then **Save Settings**.  
   Keys are stored in Editor preferences for this project, so they survive package updates and are not included in a player build. In a build, call `APISettingsConfig.SetCredentials` before `Login`.

Default endpoints (already set on the sample config):

| Setting | Value |
| --- | --- |
| Base URL | `https://api.mugen3d.ai/` |
| Login | `v1/access/auth` |
| MugenMorph | `v1/audio-to-face/offline-mesh` |

---

## Run the sample

`ARKit_head.fbx` has no textures. Each sample scene uses that pipeline's default lit material: Built-in Standard, URP Lit, or HDRP Lit. The head is grey. Facial animation is the same ARKit path in all three scenes.

Open the scene that matches the project's render pipeline:

| Pipeline | Scene | Scene contents |
| --- | --- | --- |
| **HDRP** | `Samples/Scenes/MugenMorph.unity` | HDRP camera, volume, and sky |
| **URP** | `Samples/Scenes/MugenMorph_URP.unity` | URP camera data |
| **Built-in** | `Samples/Scenes/MugenMorph_BuiltIn.unity` | Built-in camera and lights |

An HDRP scene opened without the HDRP package, or a URP scene opened without URP, reports missing scripts. A material from another pipeline renders pink. Playback code does not depend on a pipeline. In your own scene, register any ARKit face and assign that pipeline's lit material.

Package Manager paths are under `Packages/com.mugen3d.mugenmorph/…`. Copied or `.unitypackage` installs use `Assets/Mugen3D/…`. After a Git install, open the writable copy under `Assets/Mugen3D Samples/` (or `Assets/Samples/…` if you used **Import**).

1. Open the scene for your pipeline.
2. Press Play. `MugenMorphSample` logs in and registers the character as ARKit.
3. Use the UI:
   - **Start Record** / **Stop Record** — capture from the default microphone, then send.
   - **Select Local Audio** — pick a `.wav` in the Editor (or press **J**).
4. After the API returns, the plugin plays audio and drives the face.

---

## Integrate into your scene

Minimal flow: register a character, log in, send WAV bytes. In the Editor, `Login` uses the keys saved under **Tools → Mugen3D → API Settings**. In a player build, call `APISettingsConfig.SetCredentials` first. Do not store those keys in a scene or in `Resources`.

```csharp
using Mugen3D;
using Mugen3D.API;
using Mugen3D.MugenMorph;
using UnityEngine;

public class MyMugenMorphSetup : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer[] faceMeshes;
    [SerializeField] Transform rootBone;

    void Start()
    {
        MugenMorphManager.GetInstance().RegisterModel(
            id: 0,
            sex: Sex.Female,
            skinnedMeshRenderers: faceMeshes,
            RootBone: rootBone,
            motionType: MotionType.ARKit);

        APIManager.Instance.Login();
    }

    public void PlayWav(byte[] wavBytes)
    {
        MugenMorphManager.GetInstance().PlayFromAudio(
            wavBytes,
            onSuccess: () => Debug.Log("MugenMorph started"),
            onError: err => Debug.LogError(err));
    }
}
```

Microphone → WAV → play:

```csharp
audioRecord.StopRecord((base64, wavBytes) =>
{
    MugenMorphManager.GetInstance().PlayFromAudio(wavBytes);
});
```

### Multiple characters

Call `RegisterModel` with a unique `id` per character. Playback goes to every registered model. Use `UnRegisterModel(id)` when a character is destroyed.

### Events

```csharp
var mugenMorph = MugenMorphManager.GetInstance();
mugenMorph.StartSpeechEvent += () => { /* audio started */ };
mugenMorph.StopSpeechEvent  += () => { /* audio finished */ };
mugenMorph.StopMotionEvent  += () => { /* blendshapes finished */ };
mugenMorph.Interrupt(); // cancel queues and fade current expression
```

---

## Character setup

`MotionType.ARKit` maps blendshapes on each registered `SkinnedMeshRenderer` onto the 52 ARKit names. Which rule runs depends on the blendshape name:

1. **Substring (usual case)** — the blendshape name contains an ARKit name, ignoring case.  
   `JawOpen`, `jawOpen`, and `head_lod0_mesh_eyeBlinkLeft` all match `JawOpen` / `EyeBlinkLeft`.
2. **Indexed `Mesh` names** — if the name **contains `Mesh`** (this check is case-sensitive) and looks like `Something.Mesh{n}`, `{n}` is the index into the ARKit list (`Mesh0` = `EyeBlinkLeft`, `Mesh17` = `JawOpen`, …).  
   This is a numeric alias used by some exported meshes. It is **not** Unreal MetaHuman `CTRL_expressions.*` naming. MetaHuman (or any mesh) that already embeds ARKit names in the blendshape string uses rule 1.

Examples of ARKit names: `EyeBlinkLeft`, `JawOpen`, `MouthSmileLeft`, `BrowInnerUp`, `CheekPuff`, …

Each API frame is **61 floats**: 52 ARKit weights, then `HeadYaw` / `HeadPitch` / `HeadRoll` and left/right eye yaw/pitch/roll. The 52 shapes drive the mesh. The extra 9 channels only move bones if those transforms were assigned on the face data.

`MotionType.FLAME` looks for `PExpression_{300+i}` / `NExpression_{300+i}` on the mesh. `MotionType.Bones` is for bone-driven faces that already have pose data in the runtime.

---

## API overview

Callers normally use `PlayFromAudio`. The HTTP layer is:

1. **Login** `POST v1/access/auth`  
   Body: `{ "accessKey", "secretKey" }` → `accessToken`
2. **MugenMorph** `POST v1/audio-to-face/offline-mesh`  
   Header: `Authorization: <token>`, `Accept: application/x-protobuf`  
   Body: `{ "traceId", "data": "<wav-base64>" }`

Response (protobuf preferred; JSON still accepted):

| Field | Meaning |
| --- | --- |
| `fps` | Blendshape frame rate (sample uses 30 if missing) |
| `audio` / `audioKey` | Optional WAV. If empty, the local clip is played |
| `blendshapes` / `emoteKey` | Little-endian float32, **61 values per frame** (52 ARKit + head/eye extras) |

Only one MugenMorph request runs at a time. A new `PlayFromAudio` while a request is in flight is ignored.

---

## Platform notes

- Networking and callbacks run on the Unity main thread.
- `AudioRecord` uses `Microphone` (available on Editor, Windows, and most players with mic permission).
- Local file picker uses `UnityEditor` and is Editor-only.
- WAV playback works on every platform. MP3 playback uses NAudio and is Windows only (Editor and standalone). See [Third-Party Notices.txt](Third-Party%20Notices.txt).
- The MugenMorph request needs a network connection to `https://api.mugen3d.ai/` and a Mugen3D account. Playback waits on that response.
- The sample head has no textures. `MugenMorph.unity` is the HDRP scene, `MugenMorph_URP.unity` is URP, and `MugenMorph_BuiltIn.unity` is Built-in. Open the one that matches the project.
- The first time the Editor loads this package, it sends one anonymous install event to `https://api.mugen3d.ai/v1/event-stats/increment`. The body includes a random id stored in Editor preferences, the Unity version, and the package version. `machineId` and `keyId` are empty. No hardware id and no API key are sent.
- The plugin is MIT-licensed (`com.mugen3d.mugenmorph`). Use of the Mugen3D API is separate (account terms on [mugen3d.ai](https://mugen3d.ai/)).

---

## License

This Unity plugin is [MIT](https://github.com/sumeurai/MugenMorph-For-Unity/blob/main/LICENSE).  
Use of the Mugen3D API is subject to your account terms on [mugen3d.ai](https://mugen3d.ai/).  
This package uses NAudio under the MIT License. See [Third-Party Notices.txt](Third-Party%20Notices.txt).  
Version history: [CHANGELOG.md](CHANGELOG.md).

Issues and contributions are welcome.
