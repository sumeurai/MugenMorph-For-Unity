using System;
using System.IO;
using Mugen3D;
using Mugen3D.API;
using Mugen3D.MugenMorph;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Mugen3D.Samples
{
    public class MugenMorphSample : MonoBehaviour
    {

        [SerializeField] private SkinnedMeshRenderer[] skinnedMeshes;

        [SerializeField] private Transform rootBone;

        [SerializeField] private AudioRecord audioRecord;

        public void Bind(SkinnedMeshRenderer[] meshes, Transform root, AudioRecord record)
        {
            if (meshes != null && meshes.Length > 0)
            {
                skinnedMeshes = meshes;
            }

            if (root != null)
            {
                rootBone = root;
            }

            if (record != null)
            {
                audioRecord = record;
            }
        }


        void Start()
        {
            if (!HasMeshes())
            {
                SkinnedMeshRenderer[] found = FindObjectsOfType<SkinnedMeshRenderer>();
                if (found != null && found.Length > 0)
                {
                    skinnedMeshes = found;
                    if (rootBone == null)
                    {
                        rootBone = found[0].rootBone != null ? found[0].rootBone : found[0].transform;
                    }
                }
            }

            MugenMorphManager.GetInstance().RegisterModel(0, Sex.Male, skinnedMeshes, rootBone, MotionType.ARKit);

            APIManager.Instance.Login();
        }

        bool HasMeshes()
        {
            if (skinnedMeshes == null || skinnedMeshes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < skinnedMeshes.Length; i++)
            {
                if (skinnedMeshes[i] != null)
                {
                    return true;
                }
            }

            return false;
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                SelectLocalAudioInEditor();
            }
        }

        public void StartRecord()
        {
            if (audioRecord)
            {
                audioRecord.StartRecord();
            }
        }

        public void StopRecord()
        {
            if (audioRecord)
            {
                audioRecord.StopRecord((base64, _) =>
                {
                    MugenMorphManager.GetInstance().PlayFromAudio(Convert.FromBase64String(base64));
                });
            }
        }

        public void SelectLocalAudioInEditor()
        {
    #if UNITY_EDITOR
            string filepath = EditorUtility.OpenFilePanel("select wav file", "", "wav");

            if (string.IsNullOrEmpty(filepath))
            {
                return;
            }

            byte[] wavBytes = File.ReadAllBytes(filepath);
            MugenMorphManager.GetInstance().PlayFromAudio(wavBytes);
    #endif
        }
    }
}
