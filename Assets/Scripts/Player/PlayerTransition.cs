using System;
using System.Collections;
using UnityEngine;

namespace Duet.Player
{
    /// <summary>
    /// Handles the start transition: move to start position, ease rotation to zero, and scale change.
    /// Attach to PlayerPivot.
    /// </summary>
    public class PlayerTransition : MonoBehaviour
    {
        public bool IsTransitioning { get; private set; } = false;

        /// <summary>
        /// Start transition.
        /// </summary>
        /// <param name="targetPosition">World position to move to.</param>
        /// <param name="duration">Seconds for the transition.</param>
        /// <param name="targetUniformScale">Final uniform scale (e.g. 80).</param>
        /// <param name="onComplete">Callback when finished.</param>
        public void StartTransition(Vector3 targetPosition, float duration, float targetUniformScale, Action onComplete = null)
        {
            if (IsTransitioning) return;
            StartCoroutine(TransitionCoroutine(targetPosition, duration, targetUniformScale, onComplete));
        }

        /// <summary>
        /// Start reverse transition back to menu state.
        /// </summary>
        /// <param name="menuPosition">Menu position (usually same as start position).</param>
        /// <param name="menuRotation">Menu rotation angle (Z-axis).</param>
        /// <param name="menuScale">Menu scale (player scale from config).</param>
        /// <param name="duration">Seconds for the transition.</param>
        /// <param name="onComplete">Callback when finished.</param>
        public void StartReturnToMenuTransition(Vector3 menuPosition, float menuRotation, float menuScale, float duration, Action onComplete = null)
        {
            if (IsTransitioning) return;
            Debug.Log($"[PlayerTransition] Starting return to menu transition: pos={menuPosition}, rot={menuRotation}, scale={menuScale}, duration={duration}");
            StartCoroutine(ReturnToMenuTransitionCoroutine(menuPosition, menuRotation, menuScale, duration, onComplete));
        }

        private IEnumerator TransitionCoroutine(Vector3 targetPosition, float duration, float targetUniformScale, Action onComplete)
        {
            IsTransitioning = true;

            // Disable player input controller if present
            var torque = GetComponent<PlayerTorqueController>();
            if (torque != null) torque.enabled = false;

            // Try to read rotation speed from PlayerAutoRotate if present
            var autoRotate = GetComponent<PlayerAutoRotate>();
            float rotateSpeed = 0f;
            if (autoRotate != null)
            {
                rotateSpeed = autoRotate.rotationSpeed;
                // disable auto rotate to let transition control rotation
                autoRotate.enabled = false;
            }

            Vector3 startPos = transform.position;
            float startScale = transform.localScale.x;
            // initial z angle
            float startAngle = transform.eulerAngles.z;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // easing: ease out cubic
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                // Position interpolation
                transform.position = Vector3.Lerp(startPos, targetPosition, ease);

                // Simulate continued rotation at rotateSpeed (clockwise if positive) then ease to 0
                // simulatedAngle = startAngle + (-rotateSpeed * elapsed)
                float simulatedAngle = startAngle + (-rotateSpeed * elapsed);
                // interpolate simulatedAngle -> 0 using easing (use LerpAngle to handle wrap)
                float currentAngle = Mathf.LerpAngle(simulatedAngle, 0f, ease);
                transform.eulerAngles = new Vector3(0f, 0f, currentAngle);

                // Scale interpolation (uniform)
                float currentScale = Mathf.Lerp(startScale, targetUniformScale, ease);
                transform.localScale = Vector3.one * currentScale;

                yield return null;
            }

            // Finalize exact values
            transform.position = targetPosition;
            transform.eulerAngles = Vector3.zero;
            transform.localScale = Vector3.one * targetUniformScale;

            // Re-enable player controller
            if (torque != null) torque.enabled = true;

            // Ensure autoRotate is disabled (we want player control)
            if (autoRotate != null) autoRotate.enabled = false;

            IsTransitioning = false;
            onComplete?.Invoke();
        }

        private IEnumerator ReturnToMenuTransitionCoroutine(Vector3 menuPosition, float menuRotation, float menuScale, float duration, Action onComplete)
        {
            IsTransitioning = true;
            Debug.Log($"[PlayerTransition] Return transition started: from pos={transform.position}, rot={transform.eulerAngles.z}, scale={transform.localScale.x}");

            // Disable player input controller if present
            var torque = GetComponent<PlayerTorqueController>();
            if (torque != null) torque.enabled = false;

            // Disable auto rotate during transition
            var autoRotate = GetComponent<PlayerAutoRotate>();
            if (autoRotate != null) autoRotate.enabled = false;

            Vector3 startPos = transform.position;
            float startScale = transform.localScale.x;
            float startAngle = transform.eulerAngles.z;

            Debug.Log($"[PlayerTransition] Transitioning to: pos={menuPosition}, rot={menuRotation}, scale={menuScale}");

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // easing: ease out cubic
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                // Position interpolation
                transform.position = Vector3.Lerp(startPos, menuPosition, ease);

                // Rotation interpolation (from current angle to menu rotation)
                float currentAngle = Mathf.LerpAngle(startAngle, menuRotation, ease);
                transform.eulerAngles = new Vector3(0f, 0f, currentAngle);

                // Scale interpolation (from current scale to menu scale)
                float currentScale = Mathf.Lerp(startScale, menuScale, ease);
                transform.localScale = Vector3.one * currentScale;

                if (elapsed % 0.1f < Time.deltaTime) // Log every ~0.1 seconds
                {
                    Debug.Log($"[PlayerTransition] Transition progress: {t:F2}, pos={transform.position}, rot={transform.eulerAngles.z:F1}, scale={transform.localScale.x:F2}");
                }

                yield return null;
            }

            // Finalize exact values
            transform.position = menuPosition;
            transform.eulerAngles = new Vector3(0f, 0f, menuRotation);
            transform.localScale = Vector3.one * menuScale;

            // Re-enable auto rotate for menu state
            if (autoRotate != null) autoRotate.enabled = true;

            Debug.Log($"[PlayerTransition] Return transition completed: pos={transform.position}, rot={transform.eulerAngles.z}, scale={transform.localScale.x}");

            IsTransitioning = false;
            onComplete?.Invoke();
        }
    }
}

