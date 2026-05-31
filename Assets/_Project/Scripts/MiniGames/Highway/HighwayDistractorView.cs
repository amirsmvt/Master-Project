using UnityEngine;

namespace NeuroQuest.MiniGames.Highway
{
    public class HighwayDistractorView : MonoBehaviour
    {
        private Vector2 direction;
        private float speed;
        private Rect movementBounds;

        public void Setup(Rect bounds, float moveSpeed)
        {
            movementBounds = bounds;
            speed = moveSpeed;
            direction = UnityEngine.Random.insideUnitCircle.normalized;

            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            Vector3 position = transform.position;

            if (position.x < movementBounds.xMin || position.x > movementBounds.xMax)
            {
                direction.x *= -1f;
            }

            if (position.y < movementBounds.yMin || position.y > movementBounds.yMax)
            {
                direction.y *= -1f;
            }

            position.x = Mathf.Clamp(position.x, movementBounds.xMin, movementBounds.xMax);
            position.y = Mathf.Clamp(position.y, movementBounds.yMin, movementBounds.yMax);

            transform.position = position;
        }
    }
}