#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Duet.Config;

[InitializeOnLoad]
public static class CreateDefaultGameConfig
{
    static CreateDefaultGameConfig()
    {
        // Delay call to allow editor to finish initialization
        EditorApplication.delayCall += EnsureExists;
    }

    private static void EnsureExists()
    {
        if (Resources.Load<GameConfig>("GameConfig") != null)
            return;

        // Ensure Resources folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        var cfg = ScriptableObject.CreateInstance<GameConfig>();
        AssetDatabase.CreateAsset(cfg, "Assets/Resources/GameConfig.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("[GameConfig] Created default Assets/Resources/GameConfig.asset");
    }
}
#endif


