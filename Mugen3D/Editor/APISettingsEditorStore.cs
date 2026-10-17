using System.Security.Cryptography;
using System.Text;
using Mugen3D.API;
using UnityEditor;
using UnityEngine;

namespace Mugen3D.Editor
{
    static class APISettingsEditorStore
    {
        const string AccessPref = "Mugen3D.AccessKey.";
        const string SecretPref = "Mugen3D.SecretKey.";
        const string LegacyAccessPref = "SumeruAI.AccessKey.";
        const string LegacySecretPref = "SumeruAI.SecretKey.";
        const string LegacyAsset = "Assets/SumeruAI/Resources/APISettingsConfig.asset";
        const string LegacyBackup = "Assets/SumeruAI/Resources/APISettingsConfig-backup.asset";
        const string MovedAsset = "Assets/Mugen3D/Resources/APISettingsConfig.asset";
        const string MovedBackup = "Assets/Mugen3D/Resources/APISettingsConfig-backup.asset";

        static string Scope
        {
            get
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
                    var id = new StringBuilder(16);
                    for (int i = 0; i < 8; i++)
                    {
                        id.Append(hash[i].ToString("x2"));
                    }

                    return id.ToString();
                }
            }
        }

        [InitializeOnLoadMethod]
        static void RegisterPlayModeApply()
        {
            EditorApplication.delayCall += ApplyToRuntime;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyToRuntime()
        {
            MigrateLegacyAssets();
            string access;
            string secret;
            Load(out access, out secret);
            if (string.IsNullOrEmpty(access) || string.IsNullOrEmpty(secret))
            {
                return;
            }

            APISettingsConfig.SetCredentials(access, secret);
        }

        public static void Load(out string accessKey, out string secretKey)
        {
            string scope = Scope;
            accessKey = ReadMigratedPref(AccessPref, LegacyAccessPref, scope);
            secretKey = ReadMigratedPref(SecretPref, LegacySecretPref, scope);
        }

        static string ReadMigratedPref(string pref, string legacyPref, string scope)
        {
            string value = EditorPrefs.GetString(pref + scope, string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            string legacy = EditorPrefs.GetString(legacyPref + scope, string.Empty);
            if (string.IsNullOrEmpty(legacy))
            {
                return string.Empty;
            }

            EditorPrefs.SetString(pref + scope, legacy);
            EditorPrefs.DeleteKey(legacyPref + scope);
            return legacy;
        }

        public static void Save(string accessKey, string secretKey)
        {
            string scope = Scope;
            EditorPrefs.SetString(AccessPref + scope, accessKey ?? string.Empty);
            EditorPrefs.SetString(SecretPref + scope, secretKey ?? string.Empty);
            APISettingsConfig.SetCredentials(accessKey, secretKey);
        }

        static void MigrateLegacyAssets()
        {
            string access;
            string secret;
            Load(out access, out secret);
            if (!string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(secret))
            {
                ClearAsset(LegacyAsset);
                ClearAsset(LegacyBackup);
                ClearAsset(MovedAsset);
                ClearAsset(MovedBackup);
                return;
            }

            APISettingsConfig legacy = LoadLegacy(LegacyAsset);
            if (legacy == null || !legacy.IsValid())
            {
                legacy = LoadLegacy(LegacyBackup);
            }

            if (legacy == null || !legacy.IsValid())
            {
                legacy = LoadLegacy(MovedAsset);
            }

            if (legacy == null || !legacy.IsValid())
            {
                legacy = LoadLegacy(MovedBackup);
            }

            if (legacy != null && legacy.IsValid())
            {
                Save(legacy.AccessKey, legacy.SecretKey);
            }

            ClearAsset(LegacyAsset);
            ClearAsset(LegacyBackup);
            ClearAsset(MovedAsset);
            ClearAsset(MovedBackup);
        }

        static APISettingsConfig LoadLegacy(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<APISettingsConfig>(assetPath);
        }

        static void ClearAsset(string assetPath)
        {
            APISettingsConfig asset = LoadLegacy(assetPath);
            if (asset == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
            if (!path.StartsWith("Assets/"))
            {
                return;
            }

            SerializedObject so = new SerializedObject(asset);
            SerializedProperty access = so.FindProperty("accessKey");
            SerializedProperty secret = so.FindProperty("secretKey");
            if (access != null)
            {
                access.stringValue = string.Empty;
            }

            if (secret != null)
            {
                secret.stringValue = string.Empty;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }
    }
}
