using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class NewDialogueExporter
{
    [MenuItem("Tools/Exportar Diálogos (S_TextTriggerSystem)", false, 20)]
    public static void ExportDialogues()
    {
        string savePath = EditorUtility.SaveFilePanel("Guardar CSV de diálogos", "Assets", "Dialogues_Export.csv", "csv");
        if (string.IsNullOrEmpty(savePath)) return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Scene,Owner,Type,Text");

        string[] scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => path.StartsWith("Assets/"))
            .ToArray();

        foreach (string path in scenePaths)
        {
            EditorSceneManager.OpenScene(path);
            string sceneName = Path.GetFileNameWithoutExtension(path);

            // 1. Exportar S_TextTriggerSystem_MA
            foreach (var system in Resources.FindObjectsOfTypeAll<S_TextTriggerSystem_MA>())
            {
                if (system.gameObject.scene.path != path) continue;

                SerializedObject so = new SerializedObject(system);
                SerializedProperty listProp = so.FindProperty("dialogueText");

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    SerializedProperty item = listProp.GetArrayElementAtIndex(i);
                    string text = item.FindPropertyRelative("Text").stringValue;
                    csv.AppendLine($"\"{sceneName}\",\"{system.name}\",\"MainDialogue\",\"{text.Replace("\"", "\"\"")}\"");

                    // Opciones de respuesta
                    SerializedProperty options = item.FindPropertyRelative("options");
                    for (int j = 0; j < options.arraySize; j++)
                    {
                        string answer = options.GetArrayElementAtIndex(j).FindPropertyRelative("Answer").stringValue;
                        csv.AppendLine($"\"{sceneName}\",\"{system.name}\",\"Option\",\"{answer.Replace("\"", "\"\"")}\"");
                    }
                }
            }

            // 2. Exportar S_IncorrectAnswer_MA
            foreach (var incorrect in Resources.FindObjectsOfTypeAll<S_IncorrectAnswer_MA>())
            {
                if (incorrect.gameObject.scene.path != path) continue;

                SerializedObject so = new SerializedObject(incorrect);
                SerializedProperty listProp = so.FindProperty("endDialogue");

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    string text = listProp.GetArrayElementAtIndex(i).stringValue;
                    csv.AppendLine($"\"{sceneName}\",\"{incorrect.name}\",\"IncorrectDialogue\",\"{text.Replace("\"", "\"\"")}\"");
                }
            }
        }

        File.WriteAllText(savePath, csv.ToString(), new UTF8Encoding(true));
        EditorUtility.DisplayDialog("Éxito", "Diálogos exportados correctamente.", "OK");
    }
}