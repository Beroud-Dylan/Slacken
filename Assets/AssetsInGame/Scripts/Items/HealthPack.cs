using UnityEngine;
using Slacken.Entities;
using Slacken.Bases.SO;

namespace Slacken.Items
{
    public class HealthPack : CollectableItem
    {
        [Header("Health Pack Values")]
        [SerializeField] MessageEvent soundPlayMessage;
        [SerializeField] int value;
        private OnCollected onCollectedFunction;

        #region Initialization
        private void Start()
        {
            // Subscribe to events
            onCollectedFunction = (Entity entity) => IncrementPlayerHealth(entity);
            onCollected += onCollectedFunction;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            onCollected -= onCollectedFunction;
        }
        #endregion

        /// <summary>
        /// Increment the <paramref name="entity"/>'s health if it is the player.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        private void IncrementPlayerHealth(Entity entity)
        {
            // Check if the entity is the player
            if ((entity as Player) != null)
            {
                // Play the boost collected sound
                soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.BoostCollected);
                // Modify the player's health
                entity.AddHealth(value);
                // Set that the health pack has to die
                base.Die();
            }
        }
    }
}
