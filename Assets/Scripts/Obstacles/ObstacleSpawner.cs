using UnityEngine;
using Duet.Managers;

namespace Duet.Obstacles
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public string poolKey = "Obstacle";
        public float spawnInterval = 1.5f;
        public float spawnHeight = 6f;
        public float spawnMargin = 1f; // margin above camera top when computing spawn height
        public float spawnRangeX = 4f; // -spawnRangeX to +spawnRangeX
        private float nextSpawnTime;
        private Transform _obstacleContainer;
        [Header("Configs")]
        public ObstacleConfig[] obstacleConfigs;

        private void Start()
        {
            // allow override from GameConfig if present
            var cfg = UnityEngine.Resources.Load<Duet.Config.GameConfig>("GameConfig");
            if (cfg != null)
            {
                spawnInterval = cfg.spawnInterval;
                spawnMargin = cfg.spawnMargin;
                spawnRangeX = cfg.spawnRangeX;
                spawnHeight = cfg.spawnHeight;
            }
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Awake()
        {
            _obstacleContainer = GameObject.Find("Obstacle").transform;
        }

        void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing && Time.time >= nextSpawnTime)
            {
                SpawnObstacle();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        private void SpawnObstacle()
        {
            // Decide to spawn in exactly 1 region (out of 3: left, center, right)
            int numRegionsToFill = 1;

            Camera cam = Camera.main;
            float camTop = cam.transform.position.y + cam.orthographicSize;
            float spawnY = camTop + spawnMargin;

            // Use fixed spawn range width instead of camera-based calculation for consistent obstacle placement
            float[] regionCenters = new float[3];
            float totalWidth = spawnRangeX * 2f; // spawnRangeX is already -spawnRangeX to +spawnRangeX
            float regionWidth = totalWidth / 3f;
            float left = cam.transform.position.x - spawnRangeX; // center the spawn area on camera
            for (int i = 0; i < 3; i++)
            {
                regionCenters[i] = left + regionWidth * (i + 0.5f);
            }
            Debug.Log($"[Spawner] regionCenters: {regionCenters[0]:F2}, {regionCenters[1]:F2}, {regionCenters[2]:F2} (left={left:F2}, regionWidth={regionWidth:F2})");

            // Choose config (bind config -> prefab) first, then choose region(s)
            ObstacleConfig chosenCfg = null;
            if (obstacleConfigs != null && obstacleConfigs.Length > 0)
            {
                float total = 0f;
                foreach (var c in obstacleConfigs) total += c.probability;
                float pick = Random.Range(0f, total);
                float acc = 0f;
                foreach (var c in obstacleConfigs)
                {
                    acc += c.probability;
                    if (pick <= acc)
                    {
                        chosenCfg = c;
                        break;
                    }
                }
                if (chosenCfg == null) chosenCfg = obstacleConfigs[0];
            }

            string key = chosenCfg != null && !string.IsNullOrEmpty(chosenCfg.id) ? chosenCfg.id : (chosenCfg != null && chosenCfg.prefab != null ? chosenCfg.prefab.name : poolKey);

            // Choose distinct regions
            int[] indices = new int[] { 0, 1, 2 };
            // shuffle
            for (int i = 0; i < indices.Length; i++)
            {
                int j = Random.Range(i, indices.Length);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

            // Take first numRegionsToFill indices as spawn regions
            for (int r = 0; r < numRegionsToFill; r++)
            {
                int regionIndex = indices[r];

                // If the chosen config disallows center region and we picked center, try to swap
                if (chosenCfg != null && !chosenCfg.centered && regionIndex == 1)
                {
                    bool swapped = false;
                    for (int k = 0; k < indices.Length; k++)
                    {
                        int candidate = indices[(r + 1 + k) % indices.Length];
                        bool used = false;
                        for (int u = 0; u < r; u++) if (indices[u] == candidate) { used = true; break; }
                        if (used) continue;
                        if (candidate != 1)
                        {
                            regionIndex = candidate;
                            swapped = true;
                            break;
                        }
                    }
                    if (!swapped)
                    {
                        // no valid non-center region found
                        continue;
                    }
                }

                float x = regionCenters[regionIndex];
                // Apply regionOffset from config only for left(0) and right(2) regions.
                // Positive regionOffset -> move toward center; negative -> move away from center.
                if (chosenCfg != null && regionIndex != 1)
                {
                    float appliedOffset = (regionIndex == 0) ? chosenCfg.regionOffset : -chosenCfg.regionOffset;
                    x += appliedOffset;
                }
                string chosenName = "null";
                if (chosenCfg != null)
                {
                    chosenName = !string.IsNullOrEmpty(chosenCfg.id) ? chosenCfg.id : (chosenCfg.prefab != null ? chosenCfg.prefab.name : "(no-prefab)");
                }
                Debug.Log($"[Spawner] chosenCfg={chosenName}, key={key}, regionIndex={regionIndex}, x={x:F2}");

                GameObject obstacle = PoolManager.Instance.GetFromPool(key);
                if (obstacle == null)
                {
                    // If pool doesn't have this key, fall back to instantiating the prefab directly (ensures cfg->prefab binding)
                    if (chosenCfg != null && chosenCfg.prefab != null)
                    {
                        obstacle = Instantiate(chosenCfg.prefab, _obstacleContainer);
                        obstacle.name = key;
                        Debug.Log($"[Spawner] Instantiated prefab '{chosenCfg.prefab.name}' for key '{key}'");
                    }
                }
                if (obstacle == null)
                {
                    Debug.LogError($"[Spawner] GetFromPool/Instantiate returned null for key '{key}'");
                    continue;
                }

                // Place obstacle at computed position and parent under container
                obstacle.transform.position = new Vector3(x, spawnY, 0f);
                // Keep world position when setting parent so spawned world coordinates remain as computed.
                obstacle.transform.SetParent(_obstacleContainer, true);

                // Ensure SpriteRenderer is enabled
                var sr = obstacle.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = true;
                    var c = sr.color;
                    if (c.a <= 0f) c.a = 1f;
                    sr.color = c;
                }

                // apply config overrides if present
                if (chosenCfg != null)
                {
                    if (chosenCfg.overrideLocalScale != Vector3.zero)
                    {
                        obstacle.transform.localScale = chosenCfg.overrideLocalScale;
                    }
                    if (chosenCfg.overrideColliderSize != Vector2.zero)
                    {
                        var bx = obstacle.GetComponent<BoxCollider2D>();
                        bx.size = chosenCfg.overrideColliderSize;
                    }
                }
                // DEBUG_LOG: detailed instance diagnostics (safe to remove later)
                // These logs help verify where the instance is placed vs. where its sprite/children are.
                // Remove or comment out when no longer needed.
                string cfgName = "(none)";
                if (chosenCfg != null)
                {
                    if (!string.IsNullOrEmpty(chosenCfg.id)) cfgName = chosenCfg.id;
                    else if (chosenCfg.prefab != null) cfgName = chosenCfg.prefab.name;
                }
                Debug.Log($"[Spawner DEBUG] spawned '{obstacle.name}' cfg={cfgName}, region={regionIndex} worldX={obstacle.transform.position.x:F2} worldY={obstacle.transform.position.y:F2}");
                if (sr != null)
                {
                    Debug.Log($"[Spawner DEBUG] Sprite bounds center: {sr.bounds.center} spritePivotApproxLocal={sr.sprite.pivot}");
                }
                // log local positions of children to detect prefab internal offsets
                for (int ci = 0; ci < obstacle.transform.childCount; ci++)
                {
                    var child = obstacle.transform.GetChild(ci);
                    Debug.Log($"[Spawner DEBUG] child '{child.name}' localPos={child.localPosition}");
                }
                Debug.Log($"[Spawner DEBUG] instance.localPosition={obstacle.transform.localPosition} instance.localScale={obstacle.transform.localScale}");

                // Ensure obstacle tag and BoxCollider2D exist and are configured
                if (obstacle.tag != "Obstacle")
                {
                    obstacle.tag = "Obstacle";
                }
                var box = obstacle.GetComponent<BoxCollider2D>();
                box.isTrigger = false;

                // Configure Obstacle component directly
                var obsComp = obstacle.GetComponent<Obstacle>();
                if (obsComp != null)
                {
                    obsComp.alignment = (Obstacle.Alignment)regionIndex;
                    obsComp.poolKey = key;
                    obsComp.ApplyConfig(chosenCfg);
                    // Log cfg assignment to instance
                    string cfgId = chosenCfg != null ? (chosenCfg.id ?? (chosenCfg.prefab != null ? chosenCfg.prefab.name : "no-prefab")) : "none";
                    Debug.Log($"[Spawner CFG ASSIGN] '{obstacle.name}' assigned cfg='{cfgId}' poolKey='{key}' region={regionIndex}");
                }

                // Ensure Rigidbody2D is configured (Obstacle script handles the constraints)
                var rb = obstacle.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}
