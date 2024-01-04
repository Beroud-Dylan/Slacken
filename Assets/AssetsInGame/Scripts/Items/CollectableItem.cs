using UnityEngine;
using Slacken.Entities;
using System.Collections;

namespace Slacken.Items
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class CollectableItem : MonoBehaviour
    {
        [Header("Collected Values")]
        [SerializeField] GameObject particles;
        private SpriteRenderer spriteRenderer;
        private Collider2D col;

        public delegate void OnCollected(Entity entity);
        public event OnCollected onCollected;

        #region Initialization
        public virtual void OnEnable()
        {
            // Get the component if needed
            if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }
            if (col == null) { col = GetComponent<Collider2D>(); }

            // Enable the needed components
            spriteRenderer.enabled = true;
            col.enabled = true;

            // By default the particle should be disabled
            particles.SetActive(false);
        }
        #endregion

        #region Collected Functions
        public virtual void OnTriggerEnter2D(Collider2D collision)
        {
            // If this is an entity
            if (collision.transform.TryGetComponent(out Entity entity))
            {
                // Trigger the event
                onCollected?.Invoke(entity);
            }
        }
        public virtual void OnCollisionEnter2D(Collision2D collision)
        {
            // If this is an entity
            if (collision.transform.TryGetComponent(out Entity entity))
            {
                // Trigger the event
                onCollected?.Invoke(entity);
            }
        }

        /// <summary>
        /// Destroy the current collectable item.
        /// </summary>
        protected void Die()
        {
            StartCoroutine(DeathAnimation());
        }

        /// <summary>
        /// Play a "death" animation for the collected item.
        /// </summary>
        /// <returns></returns>
        IEnumerator DeathAnimation()
        {
            // Disable the unnecessary parts
            spriteRenderer.enabled = false;
            col.enabled = false;

            // Start the particle system
            particles.SetActive(true);

            // Wait a bit so that the particles can be seen
            yield return new WaitForSeconds(GameData.particlesTimeWait);

            // Disable the particles and the gameObject
            particles.SetActive(false);
            gameObject.SetActive(false);
        }
        #endregion
    }
}