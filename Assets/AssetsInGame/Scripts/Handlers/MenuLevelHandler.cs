using UnityEngine;
using Slacken.Bases.SO;
using Slacken.Entities;
using System.Collections;

namespace Slacken.Handlers
{
    public class MenuLevelHandler : MonoBehaviour
    {
        [Header("Enemy Generation")]
        [SerializeField] SpawnableObjects enemy;
        [SerializeField] int enemyPoolCount;
        private GameObject[] enemies;

        [Header("Animation")]
        [SerializeField] float minimumTimeBetweenWaves;
        [SerializeField] float timeOffset;
        private Vector2 cameraSize;

        [Header("Dependencies")]
        [SerializeField] CameraSO mainCam;

        #region Initialization
        private void Start()
        {
            // Calculate the minimum size for the terrain
            cameraSize = (Vector2)mainCam.Cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)) + Vector2.one;

            // Generate the enemy pool
            GenerateEnemyPool();

            // Start the animation
            StartCoroutine(Animation());
        }

        /// <summary>
        /// This function generates the enemy pool.
        /// </summary>
        private void GenerateEnemyPool()
        {
            enemies = new GameObject[enemyPoolCount];
            for (int i = 0; i < enemyPoolCount; i++)
            {
                // Generate the entity
                enemies[i] = Instantiate(enemy.prefab, enemy.parent);
                enemies[i].SetActive(false);
            }
        }
        #endregion

        #region Enemies Animation
        /// <summary>
        /// Animates the menu by making enemies pass in front of the camera at random times.
        /// </summary>
        private IEnumerator Animation()
        {
            while (true)
            {
                // Start a wave
                Wave();

                // Wait a certain amount of time
                yield return new WaitForSeconds(GetTime());
            }
        }
        /// <summary>
        /// Returns the time to wait.
        /// </summary>
        private float GetTime() { return minimumTimeBetweenWaves + Random.Range(0f, timeOffset); }

        /// <summary>
        /// Launch the wave of enemies that will pass in front of the camera.
        /// </summary>
        private void Wave()
        {
            // Get the number of enemies this wave
            int numberOfEnemies = enemyPoolCount > (minimumTimeBetweenWaves + timeOffset) ? Random.Range(1, Mathf.CeilToInt(enemyPoolCount / (minimumTimeBetweenWaves + timeOffset))) : 1;

            // "Spawn" them
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Find a spawn point for that group
                Vector2 possibleOffset, targetPosition;
                Vector2 position = GetPosition(out possibleOffset, out targetPosition);

                GameObject enemy = GetAvailableEnemy();
                // Set the current entity's  position
                enemy.transform.position = position + (possibleOffset * Random.Range(-0.5f, 0.5f) / 2);
                // Reset its values
                enemy.GetComponent<EnemyInMenu>().ResetValues(targetPosition, turnSpeed: Random.Range(5f, 10f), speed: Random.Range(5f, 20f));

                // Enable the it
                enemy.SetActive(true);
            }
        }

        /// <summary>
        /// Returns an available enemy to spawn.
        /// </summary>
        private GameObject GetAvailableEnemy()
        {
            for (int i = 0; i < enemyPoolCount; i++)
            {
                if (!enemies[i].activeSelf)
                {
                    return enemies[i];
                }
            }

            #if UNITY_EDITOR
            Debug.LogError("THERE WASN'T ENOUGH TIME FOR THE ENEMY TO RUN THROUGH THE CAMERA ! ");
            #endif

            return enemies[0];
        }

        /// <summary>
        /// Returns a possible position for the enemy to spawn, its target position and a possible offset on its spawn point.
        /// </summary>
        /// <param name="possibleOffset">The possible offset on its spawn point.</param>
        /// <param name="targetPosition">The target position where the enemy will go.</param>
        private Vector2 GetPosition(out Vector2 possibleOffset, out Vector2 targetPosition)
        {
            float sizeX = cameraSize.x, sizeY = cameraSize.y;

            // Find a spawn point for that group
            Vector2 position = new Vector2(Random.Range(-sizeX, sizeX), Random.Range(-sizeY, sizeY));

            // If we are closer to an horizontal corner
            if ((Mathf.Abs(position.x) - sizeX) < (Mathf.Abs(position.y) - sizeY))
            {
                // Set the x value to be in the corner
                position.x = position.x < 0 ? -sizeX : sizeX;
                possibleOffset = Vector2.up;

                // Set the target position on the other corner, with an offset on the Y axis
                targetPosition = new Vector2(-position.x, Random.Range(-sizeY, sizeY));
            }
            // We are closer to the vertical corner
            else
            {
                // Set the y value to be in the corner
                position.y = position.y < 0 ? -sizeY : sizeY;
                possibleOffset = Vector2.right;

                // Set the target position on the other corner, with an offset on the X axis
                targetPosition = new Vector2(Random.Range(-sizeX, sizeX), -position.y);
            }

            // Return the position
            return position;
        }
        #endregion
    }
}
