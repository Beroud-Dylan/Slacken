using UnityEngine;
using UnityEngine.UI;
using Slacken.Bases.SO;
using Slacken.Entities;

namespace Slacken.ValueManagers
{
    public class HealthManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private RawImage fillImage;
        [SerializeField] private Slider healthSlider;

        [Header("Custom")]
        [SerializeField] [Range(0f, 0.5f)] private float colorOffset;
        [SerializeField] private Color startFillColor;
        [SerializeField] private Color endFillColor;

        [Header("Dependencies")]
        [SerializeField] BoolValue playerDead;
        private BoolValue.OnValueChange playerDeadFunction;

        [SerializeField] GameObjectSO player;
        private GameObjectSO.OnValueChange playerChangeGameObjectFunction;

        private Entity playerEntity;
        private Entity.OnHealthChange playerHealthFunction;

        #region Initialization
        private void Start()
        {
            // Get the player entity
            if(player.Value.TryGetComponent(out Entity entity))
            {
                // Set the player entity
                playerEntity = entity;

                // Subscribe to player entity events
                playerHealthFunction = (int maxHealth, int health) => OnPlayerHealthChanged(maxHealth, health);
                playerEntity.onHealthChange += playerHealthFunction;

                // Update the UI by resetting the health
                playerEntity.TriggerHealthChange();
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("THE GIVEN PLAYER HASN'T AN ENTITY CLASS, WHICH SHOULDN'T BE POSSIBLE !");
                #endif
            }

            // Subscribe to events
            playerDeadFunction = (bool isDead) => { if (isDead) { OnPlayerDeath(); } };
            playerDead.onValueChange += playerDeadFunction;

            playerChangeGameObjectFunction = (GameObject oldPlayer, GameObject newPlayer) => OnPlayerChangeSubscription(oldPlayer, newPlayer);
            player.onValueChange += playerChangeGameObjectFunction;
        }

        private void OnDestroy()
        {
            // Unsubscribe to events
            playerDead.onValueChange -= playerDeadFunction;
            player.onValueChange -= playerChangeGameObjectFunction;
            playerEntity.onHealthChange -= playerHealthFunction;
        }
        #endregion

        #region Health Functions
        /// <summary>
        /// This function is called whenever the player changes somehow. It changes the subscription.
        /// </summary>
        /// <param name="oldPlayer">The old player's gameObject.</param>
        /// <param name="newPlayer">The new player's gameObject.</param>
        private void OnPlayerChangeSubscription(GameObject oldPlayer, GameObject newPlayer)
        {
            // Get the new player's stats
            if (newPlayer != null && newPlayer.TryGetComponent(out Entity newEntity))
            {
                // Subscribe to the new player's stats
                playerEntity = newEntity;
                playerEntity.onHealthChange += playerHealthFunction;

                // Update the UI by resetting the health
                playerEntity.TriggerHealthChange();
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("The new player's body doesn't exist or doesn't have the component 'Entity' on it !");
                #endif
            }

            // Get the old player's stats
            if (oldPlayer != null && oldPlayer.TryGetComponent(out Entity oldEntity))
            {
                // Unsubscribe to the old player's stats
                oldEntity.onHealthChange -= playerHealthFunction;
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("The old player's body didn't exist or didn't have the component 'Entity' on it !");
                #endif
            }
        }

        /// <summary>
        /// This function is called whenever the player's health changes. It updates the UI elements related to it.
        /// </summary>
        /// <param name="maxHealth">The maximum player's health.</param>
        /// <param name="currentHealth">The current player's health.</param>
        private void OnPlayerHealthChanged(int maxHealth, int currentHealth)
        {
            // Get the ratio
            float ratio = Mathf.Clamp01((float)(currentHealth / (float)maxHealth) - colorOffset);

            // Get the lerped color
            Color currentColor = Color.Lerp(startFillColor, endFillColor, 1f - ratio);

            // Assign the color and set the right value for the slider
            fillImage.color = currentColor;
            healthSlider.value = (float)(currentHealth / (float)maxHealth);
        }

        /// <summary>
        /// This function is called once the player dies and disables the health bar.
        /// </summary>
        private void OnPlayerDeath()
        {
            // Deactivate the health bar if the player is dead
            gameObject.SetActive(false);
        }
        #endregion
    }
}
