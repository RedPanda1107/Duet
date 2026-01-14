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
        
        [Header("Movement")]
        [Tooltip("是否在水平上做左右摆动（oscillate）。默认 false")]
        public bool horizontalMove = false;
        [Tooltip("水平摆动频率（Hz-like），用于 sin 计算的速度系数（越大摆动越快）。")]
        public float horizontalSpeed = 1f;
        [Tooltip("水平摆动振幅（世界单位），摆动范围为 ±amplitude")]
        public float horizontalAmplitude = 0.5f;
        
        [Header("Burst (Child)")]
        [Tooltip("启用子物体加速突发行为")]
        public bool burstEnabled = false;
        public enum BurstTriggerType { Height, Time }
        [Tooltip("触发类型：按高度或按时间")]
        public BurstTriggerType burstTriggerType = BurstTriggerType.Height;
        [Tooltip("触发值：若按 Height 则为下降高度（世界单位）；若按 Time 则为秒数")]
        public float burstTriggerValue = 2f;
        [Tooltip("加速倍数（乘以父速度）")]
        public float burstMultiplier = 2f;
        [Tooltip("加速持续时长（秒）")]
        public float burstDuration = 0.8f;
        [Tooltip("回归到父速度的持续时间（若为0则瞬间回归）")]
        public float burstRejoinDuration = 0.2f;
        [Tooltip("回归时是否瞬间对齐（true = snap, false = lerp over rejoin duration）")]
        public bool burstRejoinSnap = true;
    }
}

