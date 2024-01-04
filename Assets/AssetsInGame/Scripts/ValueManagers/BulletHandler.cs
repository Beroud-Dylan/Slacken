using UnityEngine;
using Slacken.Bases.SO;
using Slacken.Entities;

namespace Slacken.ValueManagers
{
    public class BulletHandler : MonoBehaviour
    {
        [Header("Bullet Values")]
        [SerializeField] Transform bulletParent;
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] int bulletPoolCount;
        private GameObject[] bullets;

        [Header("Dependencies")]
        [SerializeField] MessageEvent bulletMessageEvent;
        private MessageEvent.OnNewMessage bulletMessageFunction;

        #region Initialization
        private void Start()
        {
            // Initialize the bullets
            bullets = new GameObject[bulletPoolCount];
            for (int i = 0; i < bulletPoolCount; i++)
            {
                bullets[i] = Instantiate(bulletPrefab, bulletParent);
                bullets[i].SetActive(false);
            }

            // Subscribe to events
            bulletMessageFunction = (string message) => OnNewBulletMessage(message);
            bulletMessageEvent.onNewMessageReceived += bulletMessageFunction;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            bulletMessageEvent.onNewMessageReceived -= bulletMessageFunction;
        }
        private void OnValidate()
        {
            if(bulletPoolCount <= 0)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE BULLET POOL COUNT SHOULD BE A STRICTLY POSITIVE NUMBER !");
                #endif

                bulletPoolCount = 1;
            }
        }
        #endregion

        /// <summary>
        /// This function is called whenever an entity tries to spawn a bullet. The message contains the necessary informations.
        /// </summary>
        /// <param name="message">The given message.</param>
        private void OnNewBulletMessage(string message)
        {
            // Get the bullet values
            BulletSpawnValues values = GameData.MessageToBulletValues(message);

            // If there were real values
            if (!values.empty)
            {
                // Send them to place a bullet
                PlaceBullet(values);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("COULDN'T INTERPRETE THE MESSAGE : '" + message + "' AS A BULLET SPAWN VALUES !");
                #endif
            }
        }

        /// <summary>
        /// Place a bullet according to the bullet values given.
        /// </summary>
        /// <param name="values">The bullet values given.</param>
        private void PlaceBullet(BulletSpawnValues values)
        {
            bool couldPlaceBullet = false;

            // Search an available bullet
            for (int i = 0; i < bullets.Length; i++)
            {
                GameObject bullet = bullets[i];
                // If the current bullet is available
                if (!bullet.activeSelf)
                {
                    // Enable it and place it at the right position
                    bullet.transform.position = values.position;
                    bullet.transform.localScale = values.scale;
                    bullet.GetComponent<Bullet>().ResetValues(values.direction, values.color, values.damage, values.isFriendly);
                    bullet.SetActive(true);

                    // Set that we were able to place a bullet
                    couldPlaceBullet = true;
                    break;
                }
            }

            // if we weren't able to place a bullet, then the pool size wasn't enough
            if (!couldPlaceBullet)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE POOL SIZE WASN'T ENOUGH : THERE ARE NO BULLETS AVAILABLE TO SHOOT RIGHT NOW !");
                #endif
            }
        }
    }
}
