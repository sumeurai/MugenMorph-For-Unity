using System;
using System.Text;
using Mugen3D.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Mugen3D.Editor
{
    [Serializable]
    public class EventStatsIncrementReq
    {
        public string bizType;
        public string eventType;
        public string eventKey;
        public string product;
        public string packageName;
        public string version;
        public string fromPage;
        public string repo;
        public string anonId;
        public string sdkVersion;
        public string unityVersion;
        public string machineId;
        public int userId;
        public string keyId;
        public string plan;
        public string apiName;
        public int latencyMs;
        public int callCount7d;
        public bool allBlank;
    }

    [Serializable]
    public class EventStatsIncrementRep
    {
        public EventStatsIncrementRepData data;
        public int code;
        public string msg;
    }

    [Serializable]
    public class EventStatsIncrementRepData
    {
        public string bizType;
        public string bizTypeName;
        public string eventType;
        public string eventTypeName;
        public string eventKey;
        public string statDate;
        public int eventCount;
        public int userCount;
    }

    [InitializeOnLoad]
    static class InstallStatReporter
    {
        const string EventPath = "v1/event-stats/increment";
        const string PrefKeyPrefix = "Mugen3D_InstallStat_";
        const string AnonIdPrefKey = "Mugen3D_AnonId";
        const string DebugLogPrefKey = "Mugen3D_InstallStat_DebugLog";

        const string BizType = "12";
        const string EventType = "2";
        const string EventKey = "sdk/mugenmorph/install_success";
        const string Product = "mugenmorph";
        const string PackageName = "mugenmorph";

        static bool s_Started;

        static bool DebugLogEnabled
        {
            get { return EditorPrefs.GetBool(DebugLogPrefKey, false); }
            set { EditorPrefs.SetBool(DebugLogPrefKey, value); }
        }

        static InstallStatReporter()
        {
            MigrateLegacyPrefs();
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += TryReport;
        }

        static void MigrateLegacyPrefs()
        {
            MigrateBool(DebugLogPrefKey, "SumeruAI_InstallStat_DebugLog");
            MigrateBool(PrefKeyPrefix + Application.dataPath.GetHashCode(), "SumeruAI_InstallStat_" + Application.dataPath.GetHashCode());

            string anonId = EditorPrefs.GetString(AnonIdPrefKey, "");
            if (!string.IsNullOrEmpty(anonId))
            {
                return;
            }

            string legacyAnonId = EditorPrefs.GetString("SumeruAI_AnonId", "");
            if (string.IsNullOrEmpty(legacyAnonId))
            {
                return;
            }

            EditorPrefs.SetString(AnonIdPrefKey, legacyAnonId);
            EditorPrefs.DeleteKey("SumeruAI_AnonId");
        }

        static void MigrateBool(string key, string legacyKey)
        {
            if (EditorPrefs.HasKey(key) || !EditorPrefs.HasKey(legacyKey))
            {
                return;
            }

            EditorPrefs.SetBool(key, EditorPrefs.GetBool(legacyKey, false));
            EditorPrefs.DeleteKey(legacyKey);
        }

        static async void TryReport()
        {
            if (s_Started)
            {
                return;
            }

            string prefKey = PrefKeyPrefix + Application.dataPath.GetHashCode();
            if (EditorPrefs.GetBool(prefKey, false))
            {
                Log("[Mugen3D] Install stat skipped: already reported (" + prefKey + ")");
                return;
            }

            string baseUrl = APISettingsConfig.Instance != null ? APISettingsConfig.Instance.BaseUrl : null;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return;
            }

            s_Started = true;

            EventStatsIncrementReq req = BuildRequest();
            string url = CombineUrl(baseUrl, EventPath);
            string json = JsonUtility.ToJson(req);

            Log("[Mugen3D] Install stat URL: " + url);
            Log("[Mugen3D] Install stat body: " + json);

            using (UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] body = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(body);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");

                UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
                while (!op.isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                string respText = uwr.downloadHandler != null ? uwr.downloadHandler.text : "";
                Log("[Mugen3D] Install stat response: HTTP " + (int)uwr.responseCode + " result=" + uwr.result + " error=" + uwr.error + "\n" + respText);

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    s_Started = false;
                    LogWarning("[Mugen3D] Install stat failed: " + uwr.error);
                    return;
                }

                EventStatsIncrementRep response = JsonUtility.FromJson<EventStatsIncrementRep>(uwr.downloadHandler.text);
                if (response == null || response.code != 200)
                {
                    s_Started = false;
                    string msg = response != null ? response.msg : "Response parse failed";
                    LogWarning("[Mugen3D] Install stat failed: " + msg);
                    return;
                }

                EditorPrefs.SetBool(prefKey, true);
            }
        }

        static void Log(string message)
        {
            if (DebugLogEnabled)
            {
                Debug.Log(message);
            }
        }

        static void LogWarning(string message)
        {
            if (DebugLogEnabled)
            {
                Debug.LogWarning(message);
            }
        }

        static EventStatsIncrementReq BuildRequest()
        {
            var req = new EventStatsIncrementReq
            {
                bizType = BizType,
                eventType = EventType,
                eventKey = EventKey,
                product = Product,
                packageName = PackageName,
                version = "",
                fromPage = "",
                repo = "https://github.com/sumeurai/MugenMorph-For-Unity",
                anonId = GetOrCreateAnonId(),
                sdkVersion = Mugen3DEditorPaths.PackageVersion,
                unityVersion = Application.unityVersion,
                machineId = "",
                userId = 0,
                keyId = "",
                plan = "",
                apiName = "",
                latencyMs = 0,
                callCount7d = 0,
                allBlank = true
            };

            req.allBlank = IsOptionalBlank(req);
            return req;
        }

        static bool IsOptionalBlank(EventStatsIncrementReq req)
        {
            return string.IsNullOrEmpty(req.product)
                   && string.IsNullOrEmpty(req.packageName)
                   && string.IsNullOrEmpty(req.version)
                   && string.IsNullOrEmpty(req.fromPage)
                   && string.IsNullOrEmpty(req.repo)
                   && string.IsNullOrEmpty(req.anonId)
                   && string.IsNullOrEmpty(req.sdkVersion)
                   && string.IsNullOrEmpty(req.unityVersion)
                   && string.IsNullOrEmpty(req.machineId)
                   && req.userId == 0
                   && string.IsNullOrEmpty(req.keyId)
                   && string.IsNullOrEmpty(req.plan)
                   && string.IsNullOrEmpty(req.apiName)
                   && req.latencyMs == 0
                   && req.callCount7d == 0;
        }

        static string GetOrCreateAnonId()
        {
            string anonId = EditorPrefs.GetString(AnonIdPrefKey, "");
            if (!string.IsNullOrEmpty(anonId))
            {
                return anonId;
            }

            anonId = Guid.NewGuid().ToString("N");
            EditorPrefs.SetString(AnonIdPrefKey, anonId);
            return anonId;
        }

        static string CombineUrl(string url, string path)
        {
            if (string.IsNullOrEmpty(url))
            {
                return path;
            }

            if (string.IsNullOrEmpty(path))
            {
                return url;
            }

            return url.TrimEnd('/') + "/" + path.TrimStart('/');
        }
    }
}
