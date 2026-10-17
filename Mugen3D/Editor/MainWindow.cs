using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mugen3D.Editor
{
    public class MainWindow : UnityEditor.Editor
    {

        [MenuItem("Tools/Mugen3D/API Settings", false, 100)]
        public static void OpenAPISettingsWindow()
        {
            SettingsService.OpenProjectSettings("Project/Mugen3D/APISettingsProvider");
        }

    }
}
