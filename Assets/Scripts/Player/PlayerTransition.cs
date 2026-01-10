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
    }
}

