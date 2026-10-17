using Mugen3D;
using Mugen3D.API;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mugen3D.MugenMorph
{
    
    public struct MugenMorph_Audio_Data
    {
        public float[] audio;
        public float time;
        public byte[] audioData;
    }

    public struct MugenMorph_Emote_Data
    {
        public float[] bs;
        public float fps;
    }


    public class MugenMorphManager : MonoBehaviour
    {
        private static MugenMorphManager instance;

        public static MugenMorphManager GetInstance()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("MugenMorphManager");
                instance = go.AddComponent<MugenMorphManager>();
                // DontDestroyOnLoad(go);
            }

            return instance;
        }


        public UnityAction StartSpeechEvent;

        public UnityAction StopSpeechEvent;

        public UnityAction StopMotionEvent;

        private AudioSource audioSource;

        public AudioSource AudioSource
        {
            get { return audioSource; }
        }

        private Coroutine audioCoroutine;

        private List<AudioClip> audioClips = new List<AudioClip>();
        private Queue<MugenMorph_Audio_Data> AudioQueue = new Queue<MugenMorph_Audio_Data>();
        private Queue<MugenMorph_Emote_Data> EmoteQueue = new Queue<MugenMorph_Emote_Data>();
        public Dictionary<int, MugenMorphMgrData> MugenMorphDataDic = new Dictionary<int, MugenMorphMgrData>();
        private Dictionary<string, Transform> FaceBones = new Dictionary<string, Transform>();
        private bool mugenMorphRequesting;


        private void Awake()
        {
            audioSource = this.gameObject.AddComponent<AudioSource>();

        }

        private void Update()
        {
            foreach (var dic in MugenMorphDataDic)
            {
                dic.Value.Update(Time.deltaTime);
            }

            HandleStreamMugenMorphData();
        }

        public void AddAudioFaceData(string emote, string audio, float fps)
        {
            if (!string.IsNullOrEmpty(audio))
            {
                AddAudioData(audio);
            }

            if (!string.IsNullOrEmpty(emote))
            {
                AddEmoteFaceData(emote, fps);
            }
        }

        public void AddAudioData(string audio)
        {
            if (string.IsNullOrEmpty(audio))
            {
                return;
            }

            AddAudioData(Convert.FromBase64String(audio));
        }

        public void AddAudioData(byte[] audioBytes)
        {
            if (audioBytes == null || audioBytes.Length == 0)
            {
                return;
            }

            MugenMorph_Audio_Data mugenMorphMgrData = new MugenMorph_Audio_Data();
            mugenMorphMgrData.audioData = audioBytes;
            mugenMorphMgrData.time = 10.0f;
            AudioQueue.Enqueue(mugenMorphMgrData);
        }

        public void AddEmoteFaceData(string emote, float fps)
        {
            if (string.IsNullOrEmpty(emote))
            {
                return;
            }

            AddEmoteFaceData(Convert.FromBase64String(emote), fps);
        }

        public void AddEmoteFaceData(byte[] blendshapeBytes, float fps)
        {
            if (blendshapeBytes == null || blendshapeBytes.Length == 0)
            {
                return;
            }

            MugenMorph_Emote_Data emoteData = new MugenMorph_Emote_Data();
            emoteData.bs = ConvertByteArrayToFloatArray(blendshapeBytes);
            emoteData.fps = fps;
            EmoteQueue.Enqueue(emoteData);
        }

        public void PlayFromAudio(byte[] wavBytes, Action onSuccess = null, Action<string> onError = null)
        {
            if (mugenMorphRequesting)
            {
                Debug.LogWarning("[MugenMorph] Request already in progress.");
                return;
            }

            if (wavBytes == null || wavBytes.Length == 0)
            {
                onError?.Invoke("audio is empty");
                return;
            }

            mugenMorphRequesting = true;
            Interrupt();
            APIManager.Instance.RequestMugenMorph(wavBytes,
                result =>
                {
                    mugenMorphRequesting = false;
                    ApplyMugenMorphResult(result, wavBytes);
                    onSuccess?.Invoke();
                },
                error =>
                {
                    mugenMorphRequesting = false;
                    Debug.LogError("[MugenMorph] " + error);
                    onError?.Invoke(error);
                });
        }

        public void ApplyMugenMorphResult(MugenMorphProtobufResult result, byte[] fallbackAudio = null)
        {
            if (result == null)
            {
                return;
            }

            if (result.Code != 0 && result.Code != 200)
            {
                Debug.LogError($"[MugenMorph] code={result.Code}, message={result.Message}");
                return;
            }

            bool hasResponseAudio = result.Audio != null && result.Audio.Length > 0;
            if (hasResponseAudio)
            {
                AddAudioData(result.Audio);
            }
            else if (fallbackAudio != null && fallbackAudio.Length > 0)
            {
                Debug.Log($"[MugenMorph] response has no audio, playing local wav {fallbackAudio.Length} bytes");
                AddAudioData(fallbackAudio);
            }

            if (result.Blendshapes != null && result.Blendshapes.Length > 0)
            {
                float fps = result.Fps > 0f ? result.Fps : 30f;
                Debug.Log($"[MugenMorph] a2f frames={result.NumFrames}, fps={fps}, bytes={result.Blendshapes.Length}");
                AddEmoteFaceData(result.Blendshapes, fps);
            }
        }

        public void RegisterModel(int id, Sex sex, SkinnedMeshRenderer[] skinnedMeshRenderers, Transform RootBone,
            MotionType motionType)
        {
            MugenMorphMgrData mugenMorphMgrData = new MugenMorphMgrData();
            FaceBones.Clear();
            CollectBones(RootBone);
            FaceData faceData = new FaceData();
            faceData.Sex = sex;
            if (FaceBones.ContainsKey("head"))
            {
                faceData.Head = FaceBones["head"];
            }

            if (FaceBones.ContainsKey("jaw"))
            {
                faceData.Jaw = FaceBones["jaw"];
            }

            faceData.SkinnedMeshRenderers = skinnedMeshRenderers;
            faceData.motionType = motionType;

            mugenMorphMgrData.Init(faceData);

            if (!MugenMorphDataDic.ContainsKey(id))
            {
                MugenMorphDataDic.Add(id, mugenMorphMgrData);
            }

            mugenMorphMgrData.SpeechOverEvent += OnSpeechOver;
            mugenMorphMgrData.MotionOverEvent += OnMotionOver;
        }

        public void UnRegisterModel(int id)
        {
            if (MugenMorphDataDic.ContainsKey(id))
            {
                MugenMorphDataDic.Remove(id);
            }
        }

        public void Interrupt()
        {
            EmoteQueue.Clear();

            AudioQueue.Clear();

            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
                audioCoroutine = null;
            }

            audioSource.Stop();
            audioClips.Clear();

            foreach (var face in MugenMorphDataDic)
            {
                face.Value.Interrupt();
            }
        }

        private void OnSpeechOver(MugenMorphMgrData mugenMorphMgrData)
        {
            if (StopSpeechEvent != null)
                StopSpeechEvent();
        }

        private void OnMotionOver(MugenMorphMgrData mugenMorphMgrData)
        {
            if (StopMotionEvent != null)
                StopMotionEvent();
        }


        private void HandleStreamMugenMorphData()
        {
            while (AudioQueue.Count > 0)
            {
                MugenMorph_Audio_Data mgrData = AudioQueue.Dequeue();

                AudioClip audioClip = NAudioPlayer.FromAudioData(mgrData.audioData);

                audioClips.Add(audioClip);

                if (audioClips.Count == 1)
                {
                    if (StartSpeechEvent != null)
                        StartSpeechEvent();

                    audioCoroutine = StartCoroutine(PlayAudioSequence());
                }
            }

            while (EmoteQueue.Count > 0)
            {
                MugenMorph_Emote_Data data = EmoteQueue.Dequeue();

                List<float[]> tempEmoteData = new List<float[]>();

                for (int i = 0; i < data.bs.Length; i += 61)
                {
                    float[] eachFrameData = new float[61];
                    for (int j = 0; j < 61; j++)
                    {
                        eachFrameData[j] = data.bs[i + j];
                    }

                    tempEmoteData.Add(eachFrameData);
                }

                EmoteData emoteData = new EmoteData(1.0f / (float)data.fps, tempEmoteData, EMugenMorphDataType.Chat);

                PlayEmoteData(emoteData);
            }

        }


        private IEnumerator PlayAudioSequence()
        {
            for (int i = 0; i < audioClips.Count; i++)
            {
                audioSource.clip = audioClips[i];
                audioSource.Play();

                yield return new WaitForSeconds(audioClips[i].length);
            }

            audioClips.Clear();

            if (StopSpeechEvent != null)
                StopSpeechEvent();
        }


        private float[] ConvertByteArrayToFloatArray(byte[] byteArray)
        {
            if (byteArray.Length % 4 != 0)
                throw new ArgumentException("Byte array length must be divisible by 4.");

            float[] floatArray = new float[byteArray.Length / 4];

            for (int i = 0; i < byteArray.Length; i += 4)
            {
                // If your system uses little endian, you don't need to reverse the byte order
                // if (BitConverter.IsLittleEndian)
                //  Array.Reverse(byteArray, i, 4);

                floatArray[i / 4] = BitConverter.ToSingle(byteArray, i);
                //  Debug.Log(floatArray[i / 4]);
            }

            return floatArray;
        }


        private void CollectBones(Transform curBone)
        {
            for (int i = 0; i < curBone.childCount; i++)
            {
                CollectBones(curBone.GetChild(i));
            }
        }

        public void PlayEmoteData(EmoteData emoteData)
        {
            foreach (var face in MugenMorphDataDic)
            {
                face.Value.StartEmote(emoteData);
            }
        }

    }

}

