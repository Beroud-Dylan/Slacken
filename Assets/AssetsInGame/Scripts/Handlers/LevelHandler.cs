using UnityEngine;
using Slacken.Bases.SO;
using Slacken.Entities;
using System.Collections.Generic;
using System.Collections;

namespace Slacken.Handlers
{
    public class LevelHandler : MonoBehaviour
    {
        [Header("Level Generation")]
        [SerializeField] SpawnableObjects wall;
        [SerializeField] SpawnableObjects obstacle;
        [SerializeField] float terrainSize;
        private List<GameObject> obstacles = new List<GameObject>();
        private float minDistanceBetweenObstacles;

        [Header("Enemy Generation")]
        [SerializeField] SpawnableEnemies[] enemies;
        [SerializeField] int enemyPoolCount;
        [SerializeField] int startAdvancedWave;
        private GameObject[][] enemiesPool;

        private int enemyLeftCount = 0;
        private int previousNumberOfEnemies = 0;

        [Header("Boosts Generation")]
        [SerializeField] SpawnableObjects[] boosts;
        [SerializeField] int boostsPoolCount;
        [SerializeField] int maximumNumberOfBoostsPerWave;
        private GameObject[][] boostsPool;

        [Header("Dependencies")]
        [SerializeField] CameraSO mainCam;
        [SerializeField] IntValue enemiesKilledCount;
        private int startEnemiesKilled;

        [SerializeField] IntValue waveCount;
        private int bestWaveScore;
        [SerializeField] FloatValue timeScale;

        [SerializeField] SceneTheme sceneTheme;
        [SerializeField] MessageEvent soundPlayMessage;

        #region Debug
        private void OnValidate()
        {
            if(startAdvancedWave <= 2)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE ADVANDED WAVE SHOULD START FROM AT LEAST THE WAVE 3 AND NOT BELOW !");
                #endif

                startAdvancedWave = 3;
            }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            // Reset the SO's values
            enemiesKilledCount.Value = 0;
            waveCount.Value = 1;

            // Get the saved values
            startEnemiesKilled = PlayerPrefs.GetInt("fbihpqsndjnpicjoqjdxpqmjqmdxqzpùjskaqnpdclk", 0);
            bestWaveScore = PlayerPrefs.GetInt("gyebdfijdsqdhyfuiedchsijojhfoubjoosfejhbdjjié", 1);
        }
        private void Start()
        {
            // Calculate the minimum size for the terrain
            Vector2 cameraSize = mainCam.Cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            float maxCameraSize = cameraSize.x > cameraSize.y ? cameraSize.x : cameraSize.y;

            // Verify the terrain size is at least the screen size
            terrainSize = terrainSize < maxCameraSize ? maxCameraSize : terrainSize;

            // Find the minimum distance between obstacles
            float maxDistance = float.MinValue;
            for (int i = 0; i < enemies.Length; i++)
            {
                // Get the longest side
                float side = enemies[i].prefab.transform.localScale.x > enemies[i].prefab.transform.localScale.y ? enemies[i].prefab.transform.localScale.x : enemies[i].prefab.transform.localScale.y;
                // If the side is longer than the maximum distance, update it
                if (side > maxDistance)
                {
                    maxDistance = side;
                }
            }
            // Set the minimum distance between obstacles
            minDistanceBetweenObstacles = maxDistance + 1f;

            // Generate the pools
            GenerateEnemyPool();
            GenerateBoostsPool();

            // Generate the terrain
            GenerateTerrain(terrainSize);

            // Spawn the first wave
            SpawnWave();
        }
        private void OnDestroy()
        {
            // Unsubcribe to every entities' death event
            for (int i = 0; i < enemiesPool.Length; i++)
            {
                for (int j = 0; j < enemiesPool[i].Length; j++)
                {
                    if (enemiesPool[i][j].TryGetComponent(out Entity entity))
                    {
                        entity.onDeath -= OnEnemyDeath;
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.LogError("THE ENTITY NAMED '" + enemiesPool[i][j].name + "' AT THE INDEX : '" + i + "' DIDN'T HAVE AN ENTITY SCRIPT !");
                        #endif
                    }
                }
            }
        }

        /// <summary>
        /// This function generates the enemy pool.
        /// </summary>
        private void GenerateEnemyPool()
        {
            enemiesPool = new GameObject[enemies.Length][];
            for (int i = 0; i < enemiesPool.Length; i++)
            {
                // Generate the current enemy's pool
                enemiesPool[i] = new GameObject[enemyPoolCount];
                for (int j = 0; j < enemiesPool[i].Length; j++)
                {
                    // Generate the boost
                    enemiesPool[i][j] = Instantiate(enemies[i].prefab, enemies[i].parent);
                    enemiesPool[i][j].SetActive(false);

                    // Subscribe to its death event
                    if (enemiesPool[i][j].TryGetComponent(out Entity entity))
                    {
                        entity.onDeath += OnEnemyDeath;
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.LogError("THE ENTITY NAMED '" + enemiesPool[i][j].name + "' AT THE INDEX : '" + i + "' DIDN'T HAVE AN ENTITY SCRIPT !");
                        #endif
                    }
                }
            }
        }

        /// <summary>
        /// This function generates the boosts pool.
        /// </summary>
        private void GenerateBoostsPool()
        {
            boostsPool = new GameObject[boosts.Length][];
            for (int i = 0; i < boostsPool.Length; i++)
            {
                // Generate the current boost's pool
                boostsPool[i] = new GameObject[boostsPoolCount];
                for (int j = 0; j < boostsPool[i].Length; j++)
                {
                    // Generate the boost
                    boostsPool[i][j] = Instantiate(boosts[i].prefab, boosts[i].parent);
                    boostsPool[i][j].SetActive(false);
                }
            }
        }

        /// <summary>
        /// This function generates the terrain of the level (so the walls and the obstacles).
        /// </summary>
        /// <param name="terrainSize">The size of the terrain.</param>
        private void GenerateTerrain(float terrainSize)
        {
            // Modify the background's color
            mainCam.Cam.backgroundColor = sceneTheme.mainTheme.BackgroundColor;

            GenerateWalls(terrainSize);
            GenerateObstacles(terrainSize);
        }

        /// <summary>
        /// This function generates the walls and scale them accordingly to the world size.
        /// </summary>
        /// <param name="terrainSize">The size of the terrain.</param>
        private void GenerateWalls(float terrainSize)
        {
            float screenHeight = mainCam.Cam.ScreenToWorldPoint(new Vector2(0f, Screen.height)).y;
            float screenWidth = mainCam.Cam.ScreenToWorldPoint(new Vector2(Screen.width, 0f)).x;

            // Instantiate the walls, then place them at the right position and scale them
            GameObject top = Instantiate(wall.prefab, wall.parent);
            top.transform.position = new Vector2(0f, terrainSize + (screenHeight / 2f));
            top.transform.localScale = new Vector3((terrainSize + screenWidth) * 2f, screenHeight, top.transform.localScale.z);

            GameObject bottom = Instantiate(wall.prefab, wall.parent);
            bottom.transform.position = new Vector2(0f, -terrainSize - (screenHeight / 2f));
            bottom.transform.localScale = new Vector3((terrainSize + screenWidth) * 2f, screenHeight, bottom.transform.localScale.z);

            GameObject right = Instantiate(wall.prefab, wall.parent);
            right.transform.position = new Vector2(terrainSize + (screenWidth / 2f), 0f);
            right.transform.localScale = new Vector3(screenWidth, (terrainSize + screenHeight) * 2f, right.transform.localScale.z);

            GameObject left = Instantiate(wall.prefab, wall.parent);
            left.transform.position = new Vector2(-terrainSize - (screenWidth / 2f), 0f);
            left.transform.localScale = new Vector3(screenWidth, (terrainSize + screenHeight) * 2f, left.transform.localScale.z);

            // Modify their color
            if (top.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.color = sceneTheme.mainTheme.WallColor;
            }
            if (bottom.TryGetComponent(out renderer))
            {
                renderer.color = sceneTheme.mainTheme.WallColor;
            }
            if (right.TryGetComponent(out renderer))
            {
                renderer.color = sceneTheme.mainTheme.WallColor;
            }
            if (left.TryGetComponent(out renderer))
            {
                renderer.color = sceneTheme.mainTheme.WallColor;
            }
        }

        /// <summary>
        /// This function generates random obstacles in the level.
        /// </summary>
        /// <param name="terrainSize">The size of the terrain.</param>
        private void GenerateObstacles(float terrainSize)
        {
            // Initialize variables
            float size = terrainSize - GameData.maxSizeObstacle;
            obstacles = new List<GameObject>();

            // Get the real number of obstacle
            int numberOfObstacles = Mathf.FloorToInt(Mathf.Pow(terrainSize / (GameData.maxSizeObstacle + minDistanceBetweenObstacles), 2f));

            for (int i = 0; i < numberOfObstacles; i++)
            {
                // Get the scale and the position
                Vector2 scale = new Vector2(Random.Range(GameData.minSizeObstacle, GameData.maxSizeObstacle), Random.Range(GameData.minSizeObstacle, GameData.maxSizeObstacle));
                Vector2 position = GetAvailablePosition(size, scale, isForObstaclesCreation: true, numberOfTries: 25);

                // Create a platform there
                GameObject currentObstacle = Instantiate(obstacle.prefab, position, Quaternion.identity, obstacle.parent);
                currentObstacle.transform.localScale = new Vector3(scale.x, scale.y, currentObstacle.transform.localScale.z);

                // Modify its color
                if(currentObstacle.TryGetComponent(out SpriteRenderer renderer))
                {
                    renderer.color = sceneTheme.mainTheme.ObstacleColor;
                }

                // Add it to the list
                obstacles.Add(currentObstacle);
            }
        }

        /// <summary>
        /// Get an available position on the terrain.
        /// </summary>
        /// <param name="size">The terrain size.</param>
        /// <param name="scale">The scale of the object.</param>
        /// <param name="isForObstaclesCreation">Are we searching a position for the creation of a new obstacle ?</param>
        /// <param name="numberOfTries">The number of allowed tries before "giving up".</param>
        /// <returns></returns>
        private Vector2 GetAvailablePosition(float size, Vector2 scale, bool isForObstaclesCreation, int numberOfTries)
        {
            // Get a random position in the map
            Vector2 position = new Vector2(Random.Range(-size, size), Random.Range(-size, size));

            // If this position is too close to another obstacle
            bool isTooClose = false;
            for (int i = 0; i < obstacles.Count; i++)
            {
                // Get basic values
                Vector2 direction = (Vector2)obstacles[i].transform.position - position;
                float minDstX = (isForObstaclesCreation ? minDistanceBetweenObstacles : 0f) + scale.x + obstacles[i].transform.lossyScale.x;
                float minDstY = (isForObstaclesCreation ? minDistanceBetweenObstacles : 0f) + scale.y + obstacles[i].transform.lossyScale.y;

                // Check if the position is too close from an obstacle
                if (Mathf.Abs(direction.x) <= minDstX && Mathf.Abs(direction.y) <= minDstY)
                {
                    isTooClose = true;
                    break;
                }
            }

            // If this position is too close
            if (isTooClose)
            {
                // If we still have other tries to find an accurate position
                if (numberOfTries > 0)
                {
                    // Restart the process to get the obstacle position
                    return GetAvailablePosition(size, scale, isForObstaclesCreation, numberOfTries - 1);
                }
                else
                {
                    // Return a position far enough so that it doesn't bother the player in game
                    if (isForObstaclesCreation) { return position + new Vector2(size * 2f, size * 2f); }
                    else
                    {
                        #if UNITY_EDITOR
                        Debug.LogError("There were too much obstacles to find a right position !");
                        #endif
                        // Return a null vector
                        return Vector2.zero;
                    }
                }
            }
            else
            {
                return position;
            }
        }
        #endregion

        #region Boosts Functions
        /// <summary>
        /// Spawn random boosts on the map.
        /// </summary>
        private void SpawnBoosts()
        {
            // Don't spawn boost before the advance wave is passed
            if (waveCount.Value < startAdvancedWave) return;

            // Get the number of boosts to spawn this wave
            int numberOfBoosts = Random.Range(0, maximumNumberOfBoostsPerWave + 1);

            for (int i = 0; i < numberOfBoosts; i++)
            {
                // Spawn a random boost index
                int index = Random.Range(0, boosts.Length);

                // Then calculate the terrain spawnable size
                float maxBoostSize = boosts[index].prefab.transform.localScale.x >= boosts[index].prefab.transform.localScale.y ? boosts[index].prefab.transform.localScale.x : boosts[index].prefab.transform.localScale.y;
                float size = terrainSize - maxBoostSize;

                // Get a random available position
                Vector2 position = GetAvailablePosition(size, boosts[index].prefab.transform.lossyScale, isForObstaclesCreation: false, numberOfTries: 5);

                // Spawn the boost
                SpawnBoost(index, position);
            }
        }

        /// <summary>
        /// Spawns a random boost at 
        /// </summary>
        /// <param name="index">The index of the boost to spawn.</param>
        /// <param name="position">The spawn position of the boost.</param>
        private void SpawnBoost(int index, Vector2 position)
        {
            // Check that the index is in the right range
            if(index < 0 || index >= boostsPool.Length)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE GIVEN INDEX '" + index + "' WAS INVALID, BECAUSE IT WASN'T IN THE RANGE [0; " + boostsPool.Length + "[ !");
                #endif
                return;
            }

            // For every possible boosts
            for (int i = 0; i < boostsPool[index].Length; i++)
            {
                // Get the current one
                GameObject boost = boostsPool[index][i];
                // If it isn't active yet, then spawn it
                if (!boost.activeSelf)
                {
                    // So enable it and place it at the right position
                    boost.SetActive(true);
                    boost.transform.position = position;
                    break;
                }
            }
        }
        #endregion

        #region Wave Functions
        /// <summary>
        /// Spawn a new wave, when one wave is finished.
        /// </summary>
        private void SpawnWave()
        {
            // If we have a bugged waveCount value, notify the developper and reset it to 1
            if (waveCount.Value < 1)
            {
                #if UNITY_EDITOR
                Debug.LogError("The wave count shouldn't be below 0 !");
                #endif

                waveCount.Value = 1;
            }
            
            // Get the current number of enemies this wave
            int numberOfEnemies;

            // If we are at a sufficiently advanced wave
            if (waveCount.Value > startAdvancedWave)
            {
                // Get a random number of enemies this wave (so between the last number we had to some offset values depending on the wave count.
                numberOfEnemies = previousNumberOfEnemies + Random.Range(-Mathf.FloorToInt(waveCount.Value / 2), waveCount.Value);

                // Clamp the number of enemies
                numberOfEnemies = Mathf.Clamp(numberOfEnemies, 0, enemyPoolCount);

                // Change the previous number of enemies ONLY if it has more this wave
                if (previousNumberOfEnemies < numberOfEnemies)
                {
                    previousNumberOfEnemies = numberOfEnemies;
                }
            }
            else
            {
                // Else, set that the current number of enemies is the same as the wave count
                numberOfEnemies = waveCount.Value;
                previousNumberOfEnemies = numberOfEnemies;
            }

            // Spawn what needs to be spawned
            SpawnEnemies(numberOfEnemies);
            SpawnBoosts();
        }

        /// <summary>
        /// Spawn a certain <paramref name="count"/> of enemies.
        /// </summary>
        /// <param name="count">The number of enemies to spawn.</param>
        private void SpawnEnemies(int count)
        {
            enemyLeftCount = 0;

            // As long as there are enemies to spawn
            while (count > 0)
            {
                // For every "kind" of enemies
                int maximumIndex = -1;
                for (int j = 0; j < enemies.Length; j++)
                {
                    // Search the maximum index we can have
                    if ((count - enemies[j].weight) >= 0)
                    {
                        maximumIndex = j;
                    }
                }

                // Get the random enemy we will spawn (we use maximumIndex + 1, because Random.Range on intergers never goes toward the maximum)
                int currentIndex = Random.Range(0, maximumIndex + 1);

                // Get a possible spawn point for it
                float maxEnemySize = enemies[currentIndex].prefab.transform.localScale.x >= enemies[currentIndex].prefab.transform.localScale.y ? enemies[currentIndex].prefab.transform.localScale.x : enemies[currentIndex].prefab.transform.localScale.y;
                float size = terrainSize - maxEnemySize;
                Vector2 position = GetAvailablePosition(size, enemies[currentIndex].prefab.transform.lossyScale, isForObstaclesCreation: false, numberOfTries: 5);

                // Spawn the entity
                SpawnEnemy(currentIndex, position);

                // Set the number of enemies left
                enemyLeftCount++;

                // Decrease the enemy count
                count -= enemies[currentIndex].weight;
            }
        }

        /// <summary>
        /// Spawn an enemy in the scene.
        /// </summary>
        /// <param name="index">The enemy type that will spawn.</param>
        /// <param name="position">The position where the enemy will spawn.</param>
        private void SpawnEnemy(int index, Vector2 position)
        {
            // For every possible enemies
            for (int i = 0; i < enemiesPool[index].Length; i++)
            {
                // Get the current one
                GameObject enemy = enemiesPool[index][i];
                // If it isn't active yet, then spawn it
                if (!enemy.activeSelf)
                {
                    // So enable it and place it at the right position
                    enemy.SetActive(true);
                    enemy.transform.position = position;
                    break;
                }
            }
        }

        /// <summary>
        /// This function is triggered whenever an enemy has died.
        /// </summary>
        private void OnEnemyDeath()
        {
            // Decrease the number of enemies left
            enemyLeftCount--;
            enemiesKilledCount.Value += 1;

            // Save the total number of enemies killed
            int total = startEnemiesKilled + enemiesKilledCount.Value;
            PlayerPrefs.SetInt("fbihpqsndjnpicjoqjdxpqmjqmdxqzpùjskaqnpdclk", total);

            // If there are no enemies left, start a new wave
            if (enemyLeftCount <= 0)
            {
                StartCoroutine(EndWave());
            }
        }

        /// <summary>
        /// End the current wave.
        /// </summary>
        private IEnumerator EndWave()
        {
            // Increase the number of wave passed
            waveCount.Value += 1;

            // If the player made a better score
            if(waveCount.Value > bestWaveScore)
            {
                // Save it
                PlayerPrefs.SetInt("gyebdfijdsqdhyfuiedchsijojhfoubjoosfejhbdjjié", waveCount.Value);
            }

            // Play the new wave sound
            soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.NewWave);

            // Wait a little bit
            float timeToWait = GameData.waveTextShowTime;
            for (float i = 0; i < timeToWait; i += Time.fixedDeltaTime * timeScale.Value)
            {
                yield return new WaitForSeconds(Time.fixedDeltaTime * timeScale.Value);
            }

            // Spawn a new wave
            SpawnWave();
        }
        #endregion
    }
}
