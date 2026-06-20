using UnityEngine;

namespace NeuroQuest.World
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        private Rigidbody2D body;
        private Vector2 moveInput;
        private bool movementEnabled = true;

        private void Awake()
        {
            EnsureBody();
        }

        private void Update()
        {
            if (!movementEnabled)
            {
                moveInput = Vector2.zero;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                vertical += 1f;
            }

            moveInput = new Vector2(horizontal, vertical);
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }
        }

        private void FixedUpdate()
        {
            EnsureBody();

            Vector2 nextPosition = body.position + moveInput * moveSpeed * Time.fixedDeltaTime;
            body.MovePosition(nextPosition);
        }

        public void SetMovementEnabled(bool enabled)
        {
            EnsureBody();
            movementEnabled = enabled;

            if (!movementEnabled)
            {
                moveInput = Vector2.zero;
                body.linearVelocity = Vector2.zero;
            }
        }

        private void EnsureBody()
        {
            if (body != null)
            {
                return;
            }

            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }
    }
}
