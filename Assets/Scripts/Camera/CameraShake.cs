using System.Collections;
using UnityEngine;

namespace Camera
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalLocalPosition;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            Instance = this;
            originalLocalPosition = transform.localPosition;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Shake(float duration = 0.25f, float magnitude = 0.15f)
        {
            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);

            shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            shakeRoutine = null;
        }
    }
}
