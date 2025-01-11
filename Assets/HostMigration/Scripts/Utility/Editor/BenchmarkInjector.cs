using UnityEditor;
using System.IO;
using System.Text;
using UnityEngine;

public class SecretInfoFieldInjector : EditorWindow
{
    private int numberOfFields = 10;
    private string targetScriptPath = "Assets/HostMigration/Scripts/TopSecretServerInfo.cs";

    [MenuItem("Tools/Inject Secret Info Fields")]
    public static void ShowWindow()
    {
        GetWindow<SecretInfoFieldInjector>("Secret Info Field Injector");
    }

    private void OnGUI()
    {
        GUILayout.Label("Secret Info Field Injector", EditorStyles.boldLabel);
        numberOfFields = EditorGUILayout.IntField("Number of Fields:", numberOfFields);
        targetScriptPath = EditorGUILayout.TextField("Script Path:", targetScriptPath);

        if (GUILayout.Button("Inject Fields"))
        {
            InjectFields(numberOfFields, targetScriptPath);
        }
    }

    private void InjectFields(int count, string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            Debug.LogError($"Script not found at path: {scriptPath}");
            return;
        }

        string script = File.ReadAllText(scriptPath);
        int startMarkerIndex = script.IndexOf("// Secret Info Fields Start");
        int endMarkerIndex = script.IndexOf("// Secret Info Fields End");

        if (startMarkerIndex == -1 || endMarkerIndex == -1)
        {
            Debug.LogError("Could not find the markers in the script. Make sure the script has '// Secret Info Fields Start' and '// Secret Info Fields End' comments.");
            return;
        }

        StringBuilder fieldBuilder = new StringBuilder();
        for (int i = 1; i <= count; i++)
        {
            fieldBuilder.AppendLine($"    [ReadOnly] public byte SecretInfo{i};");
        }

        // Inject fields between the markers
        string newScript = script.Substring(0, startMarkerIndex + "// Secret Info Fields Start".Length) +
                           "\n" + fieldBuilder.ToString() +
                           script.Substring(endMarkerIndex);

        File.WriteAllText(scriptPath, newScript);
        AssetDatabase.Refresh();
        Debug.Log($"Injected {count} Secret Info fields into {scriptPath}");
    }
}
