using UnityEngine;

public class PrefabCreator : MonoBehaviour
{
    [ContextMenu("Create Dot Prefab")]
    private void CreateDotPrefab()
    {
        GameObject dot = new GameObject("Dot");
        dot.tag = "Player";

        // Add SpriteRenderer for visibility
        SpriteRenderer renderer = dot.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite();
        renderer.color = Color.white;

        // Save as prefab
        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(dot, "Assets/Prefabs/Dot.prefab");
        DestroyImmediate(dot);
        #endif
    }

    [ContextMenu("Create Obstacle Prefab")]
    private void CreateObstaclePrefab()
    {
        GameObject obstacle = new GameObject("Obstacle");
        obstacle.tag = "Obstacle";

        // Add SpriteRenderer for visibility
        SpriteRenderer renderer = obstacle.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite();
        renderer.color = Color.red;

        // Add BoxCollider2D
        BoxCollider2D collider = obstacle.AddComponent<BoxCollider2D>();

        // Save as prefab
        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(obstacle, "Assets/Prefabs/Obstacle.prefab");
        DestroyImmediate(obstacle);
        #endif
    }

    private Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                colors[y * 64 + x] = distance <= 32 ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
}
