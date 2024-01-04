using UnityEngine;
using Slacken.Bases.SO;
using Slacken.Items;

namespace Slacken.ValueManagers
{
    public class CoinManager : MonoBehaviour
    {
        [Header("Coin Values")]
        [SerializeField] SpawnableObjects[] spawnableCoins;
        private int[] coinsThreshold;
        [SerializeField] int coinsPoolCount;
        private GameObject[][] coins;

        [Header("Dependencies")]
        [SerializeField] IntValue coinsCount;
        private IntValue.OnValueChange onCoinsCountChange;
        private int startCoinCount;
        [SerializeField] MessageEvent coinSpawnEvent;
        private MessageEvent.OnNewMessage onCoinSpawnEventTriggered;

        #region Initialization
        private void Awake()
        {
            // Reset the SO's values
            coinsCount.Value = 0;
        }
        private void Start()
        {
            // Get the start coin count value
            startCoinCount = PlayerPrefs.GetInt("bhusjdoqsdbuhcsqhozidlhçdoipqjhiobhidzijfsinjqdijnc", 0);

            // Initialize the pool
            coins = new GameObject[spawnableCoins.Length][];
            coinsThreshold = new int[spawnableCoins.Length];

            // For each spawnable coins
            for (int i = 0; i < spawnableCoins.Length; i++)
            {
                // Create the coins values
                coinsThreshold[i] = spawnableCoins[i].prefab.GetComponent<Coin>().value;

                // Create a pool of coins
                coins[i] = new GameObject[coinsPoolCount];
                for (int j = 0; j < coinsPoolCount; j++)
                {
                    coins[i][j] = Instantiate(spawnableCoins[i].prefab, spawnableCoins[i].parent);
                    coins[i][j].SetActive(false);
                }
            }

            // Subscribe to events
            onCoinsCountChange = (int value) => SaveCoins(value);
            coinsCount.onValueChange += onCoinsCountChange;

            onCoinSpawnEventTriggered = (string message) => OnCoinMessageReceived(message);
            coinSpawnEvent.onNewMessageReceived += onCoinSpawnEventTriggered;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            coinsCount.onValueChange -= onCoinsCountChange;
            coinSpawnEvent.onNewMessageReceived -= onCoinSpawnEventTriggered;
        }
        #endregion

        #region Coins Functions
        /// <summary>
        /// This function is called whenever we try to spawn a coin. We receive a message with the necessary values.
        /// </summary>
        /// <param name="message">The given message containing the necessary values.</param>
        private void OnCoinMessageReceived(string message)
        {
            CoinSpawnValues values = GameData.MessageToCoinValues(message);
            if (!values.empty)
            {
                SpawnCoins(values.position, values.count);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("THE COIN SPAWN VALUES GIVEN COULDN'T BE READ FROM THE MESSAGE '" + message + "' !");
                #endif
            }
        }

        /// <summary>
        /// Spawn a coin at a certain position with a certain count value.
        /// </summary>
        /// <param name="position">The position where the coin will spawn.</param>
        /// <param name="count">The new count value of the coin spawned.</param>
        private void SpawnCoins(Vector2 position, int count)
        {
            // Get the array of coins to spawn per threshold
            int[] numberOfCoinsPerThreshold = new int[spawnableCoins.Length];
            for (int i = spawnableCoins.Length - 1; i >= 0; i--)
            {
                // Get the current number of coins
                int number = Mathf.FloorToInt(count / (float)coinsThreshold[i]);
                numberOfCoinsPerThreshold[i] = number;

                // Decrease the left count
                count -= number * coinsThreshold[i];
            }

            // Spawn the right number of coins per threshold
            for (int i = 0; i < spawnableCoins.Length; i++)
            {
                // Get the number of coins to spawn this threshold
                int numberToSpawn = numberOfCoinsPerThreshold[i];
                // For every possible coin there
                for (int j = 0; j < coins[i].Length; j++)
                {
                    // If we have spawned every needed coins, then break out of the loop
                    if (numberToSpawn <= 0) { break; }

                    // Get the current one
                    GameObject coin = coins[i][j];

                    // If it isn't active in the scene, then we can spawn it
                    if (!coin.activeSelf)
                    {
                        // Spawn the coin
                        coin.transform.position = position;
                        coin.SetActive(true);

                        // Decrease the number of coins to spawn left
                        numberToSpawn--;
                    }
                }

                // If we couldn't spawn every coins, tell it to the developper
                if(numberToSpawn > 0 && numberOfCoinsPerThreshold[i] != 0)
                {
                    #if UNITY_EDITOR
                    Debug.LogError("WE COULDN'T SPAWN EVERY COINS TO MAKE UP TO THE RIGHT COUNT, SINCE WE NEEDED MORE COINS AT THE INDEX '" + i + "' !");
                    #endif
                }
            }
        }

        /// <summary>
        /// Save the current number of coins.
        /// </summary>
        /// <param name="coins">The current number of coins.</param>
        private void SaveCoins(int coins)
        {
            int totalCoinValue = startCoinCount + coins;
            PlayerPrefs.SetInt("bhusjdoqsdbuhcsqhozidlhçdoipqjhiobhidzijfsinjqdijnc", totalCoinValue);
        }
        #endregion
    }
}
