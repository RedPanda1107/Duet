using UnityEngine;

namespace Duet.Obstacles
{
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "Duet/Obstacle Config", order = 2)]
    public class ObstacleConfig : ScriptableObject
    {
        [Header("Prefab")]
        public GameObject prefab;

        [Header("Layout")]
        [Tooltip("是否在对应区域居中")]
        public bool centered = true;
        [Tooltip("在区域内偏向左边（负）或右边（正），0 表示完全居中")]
        public float regionOffset = 0f;

        [Header("Rotation")]
        [Tooltip("是否在自身中心旋转")]
        public bool centerRotate = false;
        [Tooltip("中心旋转速度（度/秒）")]
        public float rotationSpeed = 0f;

        [Header("Spawn")]
        [Tooltip("用于加权选择的概率权重（>0）")]
        public float probability = 1f;

        [Header("Meta")]
        [Tooltip("可选 ID 或名称，用作区分和注册 key")]
        public string id;
        
        [Header("Size")]
        [Tooltip("如果非零，则在生成时覆盖实例的 localScale（Vector3）。留空（0,0,0）表示使用 prefab 的默认 scale。")]
        public Vector3 overrideLocalScale = Vector3.zero;
        [Tooltip("如果非零，则在生成时覆盖 BoxCollider2D 的 size（宽，高）。留空（0,0）表示不变。")]
        public Vector2 overrideColliderSize = Vector2.zero;
    }
}

