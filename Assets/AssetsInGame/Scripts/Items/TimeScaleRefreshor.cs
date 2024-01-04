using UnityEngine;
using Slacken.Entities;
using Slacken.Bases.SO;

namespace Slacken.Items
{
    public class TimeScaleRefreshor : CollectableItem
    {
        [Header("Time Scale Refreshor")]
        [SerializeField] MessageEvent timeScaleMessage;
        private OnCollected onCollectedFunction;
        [SerializeField] MessageEvent soundPlayMessage;

        #region Initialization
        private void Start()
        {
            // Subscribe to events
            onCollectedFunction = (Entity entity) => RefreshTimeScale(entity);
            onCollected += onCollectedFunction;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            onCollected -= onCollectedFunction;
        }
        #endregion

        /// <summary>
        /// Refresh the time scale if the <paramref name="entity"/> is the player.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        private void RefreshTimeScale(Entity entity)
        {
            // Check if the entity is the player
            if ((entity as Player) != null)
            {
                // Play the boost collected sound
                soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.BoostCollected);
                // Set the new time scale
                timeScaleMessage.SendMessage(GameData.startTimeScale.ToString());
                // Set that the time scale refreshor has to die
                base.Die();
            }
        }
    }
}
