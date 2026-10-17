using Mugen3D.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mugen3D.Editor
{
    public class APISettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Project/Mugen3D/APISettingsProvider";

        private string accessKey;
        private string secretKey;
        private bool showSecretKey;

        public APISettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new APISettingsProvider(SETTINGS_PATH, SettingsScope.Project)
            {
                label = "API Settings",
                keywords = new[] { "settings" }
            };

            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            APISettingsEditorStore.Load(out accessKey, out secretKey);
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            GUILayout.Space(15);

            DisplayLogo();

            GUILayout.Space(20);

            var titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(5, 5, 5, 5)
            };
            EditorGUILayout.LabelField("API Configuration", titleStyle);

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Keys are saved in Editor preferences for this project only. They are not written into scenes or Resources, so a player build does not include them. In a build, call APISettingsConfig.SetCredentials before login.",
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.Space(5);
            accessKey = EditorGUILayout.TextField("AccessKey:", accessKey);
            GUILayout.Space(5);
            DrawSecretKeyField();
            GUILayout.Space(15);

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 8, 8),
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            if (GUILayout.Button("Save Settings", buttonStyle, GUILayout.Height(35)))
            {
                if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
                {
                    EditorUtility.DisplayDialog("Warning", "Please fill accessKey and secretKey then save it", "OK");
                    return;
                }

                APISettingsEditorStore.Save(accessKey, secretKey);
            }
        }

        private void DrawSecretKeyField()
        {
            EditorGUILayout.BeginHorizontal();
            // Separate control names so IMGUI does not keep the masked editor when toggling.
            GUI.SetNextControlName(showSecretKey ? "Mugen3D.SecretKey.Plain" : "Mugen3D.SecretKey.Masked");
            secretKey = showSecretKey
                ? EditorGUILayout.TextField("SecretKey:", secretKey)
                : EditorGUILayout.PasswordField("SecretKey:", secretKey);

            string iconName = showSecretKey ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff";
            GUIContent eye = new GUIContent(
                EditorGUIUtility.IconContent(iconName).image,
                showSecretKey ? "Hide SecretKey" : "Show SecretKey");
            if (eye.image == null)
            {
                eye.text = showSecretKey ? "Hide" : "Show";
            }

            var eyeStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(4, 0, 0, 0)
            };
            if (GUILayout.Button(eye, eyeStyle, GUILayout.Width(28), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                showSecretKey = !showSecretKey;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DisplayLogo()
        {
            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(Mugen3DEditorPaths.LogoAssetPath);
            if (logo == null)
            {
                return;
            }

            float logoWidth = 64f;
            float logoHeight = 64f * (logo.height / (float)logo.width);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logo, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}
