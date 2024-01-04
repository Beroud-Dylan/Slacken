using UnityEngine;

namespace Slacken.Entities
{
    public class EnemyInMenu : MonoBehaviour
    {
        [SerializeField] Transform shooter;
        [SerializeField] float stopingRange;
        private float turnSpeed;
        private float speed;

        private Vector2 targetPosition;

        public void ResetValues(Vector2 targetPosition, float turnSpeed, float speed)
        {
            this.targetPosition = targetPosition;
            this.turnSpeed = turnSpeed;
            this.speed = speed;
        }

        private void Update()
        {
            // If we are in range of the stopping point
            if (transform.position.x >= (targetPosition.x - stopingRange) && transform.position.x <= (targetPosition.x + stopingRange) && transform.position.y >= (targetPosition.y - stopingRange) && transform.position.y <= (targetPosition.y + stopingRange))
            {
                // Deactivate ourself
                gameObject.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            // Get the direction vector
            Vector3 direction = ((Vector3)targetPosition - transform.position).normalized;

            // Move toward that direction
            transform.position = Vector2.MoveTowards(transform.position, transform.position + direction, speed * Time.fixedDeltaTime);

            // Get the angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Rotate accordingly
            shooter.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
}
