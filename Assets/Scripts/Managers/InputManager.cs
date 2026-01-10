using UnityEngine;

namespace Duet.Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Returns -1 for left, 0 for none, +1 for right.
        /// A / LeftArrow => left; D / RightArrow => right.
        /// </summary>
        public int GetRotationDirection()
        {
            // Mobile / touch support: touch left half -> -1, right half -> +1
            if (Input.touchCount > 0)
            {
                bool sawLeft = false;
                bool sawRight = false;
                foreach (var t in Input.touches)
                {
                    if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended) continue;
                    if (t.position.x > (Screen.width * 0.5f)) sawRight = true;
                    else sawLeft = true;
                }
                if (sawLeft && !sawRight) return -1;
                if (sawRight && !sawLeft) return 1;
                // if both sides touched, treat as neutral
                return 0;
            }

            // Mouse (for editor / simulator): left click on right/left half
            if (Input.GetMouseButton(0))
            {
                var mp = Input.mousePosition;
                if (mp.x > (Screen.width * 0.5f)) return 1;
                return -1;
            }

            // Keyboard fallback
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                return -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                return 1;
            return 0;
        }
    }
}


