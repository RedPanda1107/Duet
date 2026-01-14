using UnityEngine;

namespace Duet.Obstacles
{
    [DisallowMultipleComponent]
    public class ObstacleMovement : MonoBehaviour
    {
        private ObstacleConfig config;
        private Rigidbody2D rb;
        private float fallSpeed;
        private float baseX;
        private bool baseXSet = false;
        private float horizontalPhase;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void SetConfig(ObstacleConfig cfg, float fallSpeedValue)
        {
            config = cfg;
            fallSpeed = fallSpeedValue;
            // randomize phase for this instance
            horizontalPhase = Random.Range(0f, Mathf.PI * 2f);
            baseXSet = false;
            // reset burst trigger state if any
            burstTriggered = false;
            startTime = Time.time;
            baseY = 0f;
            string cfgName = "null";
            string horiz = "n/a";
            if (cfg != null)
            {
                cfgName = !string.IsNullOrEmpty(cfg.id) ? cfg.id : (cfg.prefab != null ? cfg.prefab.name : "(no-prefab)");
                horiz = cfg.horizontalMove.ToString();
            }
            Debug.Log($"[Movement.SetConfig] {gameObject.name} config={cfgName} fallSpeed={fallSpeed} horizontalMove={horiz}");
        }

        private void FixedUpdate()
        {
            if (Duet.Managers.GameManager.Instance == null || Duet.Managers.GameManager.Instance.CurrentState != Duet.Managers.GameState.Playing)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            if (config != null && config.horizontalMove)
            {
                if (!baseXSet)
                {
                    baseX = rb.position.x;
                    baseXSet = true;
                    // capture spawn baseY for height-based triggers
                    baseY = rb.position.y;
                    Debug.Log($"[Movement] {gameObject.name} captured baseX={baseX:F2} baseY={baseY:F2}");
                }
                float t = Time.time;
                float offset = Mathf.Sin(t * config.horizontalSpeed + horizontalPhase) * config.horizontalAmplitude;
                float targetX = baseX + offset;
                float newY = rb.position.y + (-fallSpeed) * Time.fixedDeltaTime;
                rb.MovePosition(new Vector2(targetX, newY));
                Debug.Log($"[Movement] {gameObject.name} MovePosition targetX={targetX:F2} newY={newY:F2} offset={offset:F2}");
            }
            else
            {
                rb.velocity = Vector2.down * fallSpeed;
            }
            // check burst trigger (only once)
            if (!burstTriggered && config != null && config.burstEnabled)
            {
                if (config.burstTriggerType == ObstacleConfig.BurstTriggerType.Height)
                {
                    float fallen = baseY - rb.position.y;
                    Debug.Log($"[Burst Check] {gameObject.name} fallen={fallen:F2} trigger={config.burstTriggerValue:F2}");
                    if (fallen >= config.burstTriggerValue)
                    {
                        TriggerChildBurst();
                        burstTriggered = true;
                    }
                }
                else // Time
                {
                    if (Time.time - startTime >= config.burstTriggerValue)
                    {
                        TriggerChildBurst();
                        burstTriggered = true;
                    }
                }
            }
        }

        private void TriggerChildBurst()
        {
            // find child burst component in children (exclude self)
            // Use reflection to find child component named "ObstacleChildBurst" to avoid compile-time dependency issues
            Component childBurstComp = null;
            var comps = GetComponentsInChildren<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == "ObstacleChildBurst")
                {
                    childBurstComp = c;
                    break;
                }
            }
            if (childBurstComp != null)
            {
                var method = childBurstComp.GetType().GetMethod("StartBurst");
                if (method != null)
                {
                    method.Invoke(childBurstComp, new object[] { config.burstMultiplier, config.burstDuration, config.burstRejoinDuration, config.burstRejoinSnap, fallSpeed });
                }
            }
        }
        // burst trigger support
        private bool burstTriggered = false;
        private float startTime = 0f;
        private float baseY = 0f;
    }
}

 