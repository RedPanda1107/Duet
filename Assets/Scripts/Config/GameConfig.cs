using UnityEngine;

namespace Duet.Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Duet/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Player")]
        public Vector3 playerStartPosition = new Vector3(0f, -3.5f, 0f);
        public float playerScale = 150f;
        public float dotDistance = 2f;

        [Header("Menu Player")]
        [Tooltip("Player position when in menu state.")]
        public Vector3 menuPlayerPosition = new Vector3(0f, 0f, 0f);
        [Tooltip("Player scale when in menu state (actual localScale, e.g. 150 = 150x150).")]
        public float menuPlayerScale = 150f;
        public float rotateSpeed = 180f;
        public float torqueForce = 50f;
        public float maxAngularVelocity = 360f;
        public float angularDrag = 2f;

        [Header("Obstacle")]
        public float obstacleFallSpeed = 2f;
        public int obstaclePoolInitialCount = 20;
        public float spawnInterval = 1.5f;
        public float spawnMargin = 1f;
        public float spawnRangeX = 4f;
        public float spawnHeight = 6f;
        
        [Header("Transition")]
        [Tooltip("Duration in seconds for the player start transition (move/rotate/scale).")]
        public float transitionDuration = 1f;
        [Tooltip("Target uniform localScale applied to the player at the end of the transition.")]
        public float transitionTargetScale = 110f;
        [Tooltip("Player rotation angle (Z-axis) when returning to menu.")]
        public float menuReturnRotation = 180f;
    }
}


