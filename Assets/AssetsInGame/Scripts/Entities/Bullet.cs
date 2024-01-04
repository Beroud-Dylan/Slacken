using System.Collections;
using UnityEngine;
using Slacken.Bases.SO;

namespace Slacken.Entities
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float maxSpeed;
        [SerializeField] float speed;
        private int damage;
        private Vector2 direction;
        private bool isFriendly;

        [SerializeField] GameObject particles;
        private SpriteRenderer spriteRenderer;
        private Collider2D col;
        private bool isDead;

        [SerializeField] FloatValue timeScale;

        #region Initialization
        /// <summary>
        /// Reset the current bullet's values.
        /// </summary>
        /// <param name="direction">Set its new direction.</param>
        /// <param name="color">The color of the bullet.</param>
        /// <param name="damage">Set its new damage value.</param>
        /// <param name="isFriendly">Set whether or not this bullet is friendly for the player.</param>
        public void ResetValues(Vector2 direction, Color color, int damage, bool isFriendly) 
        {
            // Set the right values
            this.direction = direction;
            this.damage = damage;
            this.isFriendly = isFriendly;

            // By default, the particles should be disabled
            particles.SetActive(false);
            isDead = false;

            // Get the components if needed
            if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }
            if (col == null) { col = GetComponent<Collider2D>(); }

            // Set the right color
            spriteRenderer.color = color;
            particles.GetComponent<ParticleSystem>().startColor = color;

            // Enable the needed components
            spriteRenderer.enabled = true;
            col.enabled = true;
        }
        #endregion

        private void FixedUpdate()
        {
            // If the bullet is "dead", it shouldn't be moving
            if (isDead) { return; }

            // Move at the speed rate
            float currentSpeed = Mathf.Clamp(speed * timeScale.Value, 0f, maxSpeed);
            transform.Translate(direction * currentSpeed * Time.fixedDeltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // If the bullet is "dead", it shouldn't interact with anything anymore
            if (isDead) { return; }

            // If the collision is a wall or an obstacle, then the bullet must "die"
            if(collision.transform.CompareTag(GameData.WALL_TAG) || collision.transform.CompareTag(GameData.OBSTACLE_TAG))
            {
                Die();
                return;
            }

            // If the collision was with an entity
            if (collision.transform.TryGetComponent(out Entity entity))
            {
                // If the bullet is friendly, it should only deal damage to ennemies
                if ((isFriendly && collision.transform.CompareTag(GameData.ENEMY_TAG)) || (!isFriendly && collision.transform.CompareTag(GameData.PLAYER_TAG)))
                {
                    // Apply damage and die
                    entity.TakeDamage(damage);
                    Die();
                }
            }
        }

        /// <summary>
        /// Destroy the current bullet.
        /// </summary>
        private void Die()
        {
            isDead = true;
            StartCoroutine(DeathAnimation());
        }

        /// <summary>
        /// Animate the "death" animation of the bullet by making particles.
        /// </summary>
        IEnumerator DeathAnimation()
        {
            // Disable the unnecessary parts
            spriteRenderer.enabled = false;
            col.enabled = false;
            // Start the particle system
            particles.SetActive(true);

            // Wait a bit so that the particles can be seen
            yield return new WaitForSeconds(GameData.particlesTimeWait);

            // Disable the gameObject
            gameObject.SetActive(false);
        }
    }
}
