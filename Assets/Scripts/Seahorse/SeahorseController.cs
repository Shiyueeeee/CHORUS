using UnityEngine;
using UnityEngine.InputSystem;

namespace Seahorse
{
    public class SeahorseController : MonoBehaviour
    {
        public static bool IsGameplayInputLocked => gameplayInputLockCount > 0;

        public Vector2 Move { get; private set; }
        public PlayerMovementMode MovementMode => movementMode;

        [Tooltip("If true, normalizes diagonal input so speed is consistent.")]
        [SerializeField] private bool normalize = true;
        [SerializeField] private PlayerHealth playerHealth;

        private PlayerMovementMode movementMode;
        private static int gameplayInputLockCount;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            movementMode = PlayerEnterState.movementMode;
        }

        private void Update()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.IsGameOverVisible)
            {
                Move = Vector2.zero;
                return;
            }

            if (GlobalUIRoot.IsModalInputLocked)
            {
                Move = Vector2.zero;
                return;
            }
            if (DialogueManager.IsDialogueActive)
            {
                Move = Vector2.zero;
                return;
            }

            if (IsGameplayInputLocked)
            {
                Move = Vector2.zero;
                return;
            }

            var kb = Keyboard.current;
            if (kb == null)
            {
                Move = Vector2.zero;
                return;
            }

            float x = 0f;
            float y = 0f;

            if (kb.aKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed) x += 1f;
            if (kb.sKey.isPressed) y -= 1f;
            if (kb.wKey.isPressed) y += 1f;

            // vertical only
            if (movementMode == PlayerMovementMode.VerticalOnly)
            {
                x = 0f;
            }
            // disable left
            if (movementMode == PlayerMovementMode.DisableLeft)
            {
                x = Mathf.Clamp(x, 0f, x);
            }

            var v = new Vector2(x, y);
            v = Vector2.ClampMagnitude(v, 1f);

            if (normalize)
                v.Normalize();

            Move = v;
        }

        public static void SetGameplayInputLocked(bool locked)
        {
            if (locked)
            {
                gameplayInputLockCount++;
            }
            else
            {
                gameplayInputLockCount = Mathf.Max(0, gameplayInputLockCount - 1);
            }
        }
    }
}
