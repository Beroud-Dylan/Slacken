using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Slacken.Bases.SO;
using System.Collections;

namespace Slacken.Handlers
{
    public class MenuHandler : MonoBehaviour
    {
        [Header("Menus")]
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject scoresMenu;
        [SerializeField] GameObject settingsMenu;
        [SerializeField] GameObject aboutMenu;

        [SerializeField] Transform aboutValues;
        private Animator aboutMenuAnim;
        private bool hasAlreadyClicked;

        [Header("Texts")]
        [SerializeField] Text totalCoins;
        [SerializeField] Text totalEnemiesKilled;
        [SerializeField] Text maxCoins;
        [SerializeField] Text maxEnemiesKilled;
        [SerializeField] Text bestWaveCount;

        [Header("Dependencies")]
        [SerializeField] MessageEvent soundPlayMessage;

        private void Start()
        {
            // Set the right menus at the start
            mainMenu.SetActive(true);
            scoresMenu.SetActive(false);
            settingsMenu.SetActive(false);
            aboutMenu.SetActive(false);

            // Get the components
            aboutMenuAnim = aboutMenu.GetComponent<Animator>();

            // Set the texts correctly
            int totalCoinsCount = PlayerPrefs.GetInt("bhusjdoqsdbuhcsqhozidlhçdoipqjhiobhidzijfsinjqdijnc", 0);
            int totalEnemyKilled = PlayerPrefs.GetInt("fbihpqsndjnpicjoqjdxpqmjqmdxqzpùjskaqnpdclk", 0);
            int maxCoinsCount = PlayerPrefs.GetInt("faezgvuqbhidgufvigbhezhiqonfjdpjàixpoqjdjpfuojxuzqd", 0);
            int maxEnemiesKilledCount = PlayerPrefs.GetInt("fbihdsijqobifhzeojqoidhqkljopjdenhkljxjqsnhcnjkcijns", 0);
            int bestWaveCountValue = PlayerPrefs.GetInt("gyebdfijdsqdhyfuiedchsijojhfoubjoosfejhbdjjié", 1);

            totalCoins.text = "TOTAL COINS : " + totalCoinsCount;
            totalEnemiesKilled.text = "TOTAL ENEMIES KILLED : " + totalEnemyKilled;
            maxCoins.text = "MAX COLLECTED COINS : " + maxCoinsCount;
            maxEnemiesKilled.text = "MAX ENEMIES KILLED : " + maxEnemiesKilledCount;
            bestWaveCount.text = "BEST WAVE COUNT : " + bestWaveCountValue;
        }

        private void Update()
        {
            // Check the inputs if the player wants to skip the about menu
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (aboutMenu.activeSelf)
                {
                    if (hasAlreadyClicked)
                    {
                        aboutMenuAnim.enabled = false;
                        aboutValues.localPosition = new Vector3(0, 0, 0);
                        hasAlreadyClicked = false;
                    }

                    hasAlreadyClicked = true;
                }
                else
                {
                    hasAlreadyClicked = false;
                }
            }
        }

        #region Menu Button Functions
        /// <summary>
        /// Plays the click sound.
        /// </summary>
        private void PlayClickSound() { soundPlayMessage.SendMessage("Clip1" + (int)ClipIndex.ButtonClicked); }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Play()
        {
            StartCoroutine(PlayCor());
        }
        private IEnumerator PlayCor()
        {
            // Play the click sound
            PlayClickSound();

            yield return new WaitForSeconds(GameData.timeWaitBeforeSceneChanges);
            
            // Set that the game has started for the first time
            PlayerPrefs.SetInt("fnjdnfievojokcps^qjzdinjopkpo=iazijpoinfkldqcpiqzijpeiondkscskfoqkj", 0);

            // Load the play scene
            SceneManager.LoadScene((int)SceneIndex.PLAY);
        }

        /// <summary>
        /// Show the score menu.
        /// </summary>
        public void Score()
        {
            // Play the click sound
            PlayClickSound();

            // Set the right menus
            mainMenu.SetActive(false);
            scoresMenu.SetActive(true);
            settingsMenu.SetActive(false);
            aboutMenu.SetActive(false);
        }

        /// <summary>
        /// Show the settings menu.
        /// </summary>
        public void Settings()
        {
            // Play the click sound
            PlayClickSound();

            // Set the right menus
            mainMenu.SetActive(false);
            scoresMenu.SetActive(false);
            settingsMenu.SetActive(true);
            aboutMenu.SetActive(false);
        }

        /// <summary>
        /// Show the about menu.
        /// </summary>
        public void About()
        {
            // Play the click sound
            PlayClickSound();

            // Set the animation for the about menu
            aboutMenuAnim.enabled = true;

            // Set the right menus
            mainMenu.SetActive(false);
            scoresMenu.SetActive(false);
            settingsMenu.SetActive(false);
            aboutMenu.SetActive(true);
        }

        /// <summary>
        /// Return to the main menu from any other menus.
        /// </summary>
        public void GoBack()
        {
            // Play the click sound
            PlayClickSound();

            // Set the right menus
            mainMenu.SetActive(true);
            scoresMenu.SetActive(false);
            settingsMenu.SetActive(false);
            aboutMenu.SetActive(false);
        }
        #endregion

        #region External Button Functions
        /// <summary>
        /// Open the page toward the Discord server.
        /// </summary>
        public void GoToDiscord()
        {
            // Play the click sound
            PlayClickSound();

            // Go to that URL page
            Application.OpenURL("https://discord.gg/NNmk9zn");
        }

        /// <summary>
        /// Open the page toward Instagram.
        /// </summary>
        public void GoToInstagram()
        {
            // Play the click sound
            PlayClickSound();

            // Go to that URL page
            Application.OpenURL("https://www.instagram.com/mcdown_forcommunity/");
        }

        /// <summary>
        /// Open the page toward Twitter.
        /// </summary>
        public void GoToTwitter()
        {
            // Play the click sound
            PlayClickSound();

            // Go to that URL page
            Application.OpenURL("https://twitter.com/McDown6");
        }

        /// <summary>
        /// Open the page toward Youtube.
        /// </summary>
        public void GoToYoutube()
        {
            // Play the click sound
            PlayClickSound();

            // Go to that URL page
            Application.OpenURL("https://www.youtube.com/channel/UC6FMYBRrE8xw7GVCs9qEFYA");
        }
        #endregion
    }
}
