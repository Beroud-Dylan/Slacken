using UnityEngine;
using Slacken.Bases;
using Slacken.Bases.SO;
using Slacken.Entities;

namespace Slacken.Items
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Coin : CollectableItem
    {
        [Header("Coin Values")]
        [SerializeField] MessageEvent soundPlayMessage;
        [SerializeField] IntValue coinsCount;
        public int value;
        private OnCollected onCollectedFunction;

        [Header("Spawn Values")]
        [SerializeField] FloatValue timeScale;
        [SerializeField] AttractWhenNear attractor;
        [SerializeField] float spawnRange;
        [SerializeField] [Range(0f, 1f)] float spawnForce;

        private Vector3 target;
        private Vector3 velocity;

        #region Debug
        [Header("Debug")]
        [SerializeField] bool drawGizmos;

        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(transform.position, spawnRange);
            }
        }
        #endregion

        #region Initialization
        private void Start()
        {
            // Subscribe to events
            onCollectedFunction = (Entity entity) => OnCoinCollected(entity);
            onCollected += onCollectedFunction;
        }
        private new void OnEnable()
        {
            base.OnEnable();

            // Add a force whenever the coin spawns
            Vector3 direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            target = transform.position + direction * spawnRange;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            onCollected -= onCollectedFunction;
        }
        #endregion

        #region Coin Functions
        private void FixedUpdate()
        {
            // Move the coin toward a target position, so that it adds a little force when spawned
            if (transform.position != target && !attractor.IsAttracted)
            {
                transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, spawnForce, float.MaxValue, timeScale.Value * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Modify <paramref name="entity"/>'s properties depending on itself.
        /// </summary>
        /// <param name="entity">The entity whose properties are going to be modified.</param>
        private void OnCoinCollected(Entity entity)
        {
            // Check if the entity is the player
            if((entity as Player) != null)
            {
                // Play the coins collected sound
                soundPlayMessage.SendMessage("Clip1" + (int)ClipIndex.CoinsCollected);
                // Increase the number of coins of the player
                coinsCount.Value += value;
            }
            else
            {
                // Else, since it's an enemy, boost its stats
                entity.AddNewCoins(value);
            }

            // Set that the coin has to die
            base.Die();
        }
        #endregion
    }
}
