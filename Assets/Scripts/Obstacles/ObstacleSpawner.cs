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
            // Decide to spawn in 1 or 2 regions (out of 3: left, center, right)
            int numRegionsToFill = Random.Range(1, 3); // 1 or 2

            Camera cam = Camera.main;
            float camTop = cam.transform.position.y + cam.orthographicSize;
            float spawnY = camTop + spawnMargin;

            // Compute horizontal region centers based on camera view if possible; otherwise fallback to world-based spawnRangeX
            float[] regionCenters = new float[3];
            float regionWidth = 0f;
            float halfWidth = cam.orthographicSize * cam.aspect;
            float left = cam.transform.position.x - halfWidth;
            float totalWidth = halfWidth * 2f;
            regionWidth = totalWidth / 3f;
            for (int i = 0; i < 3; i++)
            {
                regionCenters[i] = left + regionWidth * (i + 0.5f);
            }

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
                GameObject obstacle = PoolManager.Instance.GetFromPool(poolKey);

                float x = regionCenters[regionIndex];

                // Place obstacle at computed position and parent under container if exists
                obstacle.transform.position = new Vector3(x, spawnY, 0f);
                obstacle.transform.SetParent(_obstacleContainer, false);

                // Ensure SpriteRenderer is enabled
                var sr = obstacle.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = true;
                    var c = sr.color;
                    if (c.a <= 0f) c.a = 1f;
                    sr.color = c;
                }

                // Preserve prefab scale (do not override)

                // Ensure obstacle tag and BoxCollider2D exist and are configured
                if (obstacle.tag != "Obstacle")
                {
                    obstacle.tag = "Obstacle";
                }
                var box = obstacle.GetComponent<BoxCollider2D>();
                box.isTrigger = false;

                // Set alignment on Obstacle component for future use
                var obsComp = obstacle.GetComponent<Obstacle>();
                obsComp.alignment = (Obstacle.Alignment)regionIndex; // 0->Left,1->Center,2->Right

                // Ensure Rigidbody2D is configured (if present)
                var rb = obstacle.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                // Use Dynamic so physics callbacks with player triggers reliably fire
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}
