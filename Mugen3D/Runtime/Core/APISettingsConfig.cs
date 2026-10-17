using UnityEngine;

namespace Mugen3D.API
{
    public class APISettingsConfig : ScriptableObject
    {
        const string DefaultBaseUrl = "https://api.mugen3d.ai/";
        const string DefaultLogin = "v1/access/auth";
        const string DefaultMugenMorphMesh = "v1/audio-to-face/offline-mesh";

        [SerializeField] private string accessKey;
        [SerializeField] private string secretKey;

        [SerializeField] private string baseUrl;
        [SerializeField] private string login;
        [SerializeField] private string mugenMorphMesh;

        static APISettingsConfig instance;

        public string AccessKey
        {
            get { return accessKey; }
        }

        public string SecretKey
        {
            get { return secretKey; }
        }

        public string BaseUrl
        {
            get { return string.IsNullOrEmpty(baseUrl) ? DefaultBaseUrl : baseUrl; }
        }

        public string LoginUrl
        {
            get { return CombineUrl(BaseUrl, string.IsNullOrEmpty(login) ? DefaultLogin : login); }
        }

        public string MugenMorphMeshUrl
        {
            get { return CombineUrl(BaseUrl, string.IsNullOrEmpty(mugenMorphMesh) ? DefaultMugenMorphMesh : mugenMorphMesh); }
        }

        public static APISettingsConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = CreateInstance<APISettingsConfig>();
                    instance.hideFlags = HideFlags.HideAndDontSave;
                    instance.ApplyBuiltInDefaults();
                }

                return instance;
            }
        }

        public static void SetCredentials(string access, string secret)
        {
            APISettingsConfig config = Instance;
            config.accessKey = access ?? string.Empty;
            config.secretKey = secret ?? string.Empty;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey);
        }

        public void ApplyBuiltInDefaults()
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = DefaultBaseUrl;
            }

            if (string.IsNullOrEmpty(login))
            {
                login = DefaultLogin;
            }

            if (string.IsNullOrEmpty(mugenMorphMesh))
            {
                mugenMorphMesh = DefaultMugenMorphMesh;
            }
        }

        static string CombineUrl(string url, string path)
        {
            if (string.IsNullOrEmpty(url)) return path;
            if (string.IsNullOrEmpty(path)) return url;

            return url.TrimEnd('/') + "/" + path.TrimStart('/');
        }
    }
}
