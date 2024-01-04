using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Slacken.Bases.SO;
using System.Collections;

namespace Slacken.Handlers
{
    public class GameplayMenuHandler : MonoBehaviour
    {
        [Header("Menus")]
        [SerializeField] GameObject gameplayMenu;
        [SerializeField] GameObject pauseMenu;
        [SerializeField] GameObject pausePart;
        [SerializeField] GameObject settingsMenu;
        [SerializeField] GameObject deathMenu;

        [Header("Texts")]
        [SerializeField] Text coinsText;
        [SerializeField] Text deathCoinsText;

        [SerializeField] Text enemiesKilledText;
        [SerializeField] Text deathEnemiesKilledText;

        [SerializeField] Text pauseWaveCountText;
        [SerializeField] Text deathWaveCountText;
        [SerializeField] Text waveText;

        [Header("Dependencies")]
        [SerializeField] FloatValue timeScale;
        [SerializeField] IntValue coinsCount;
        private IntValue.OnValueChange onCoinsCountChange;

        [SerializeField] IntValue enemiesKilledCount;
        private IntValue.OnValueChange onEnemiesKilledCountChange;

        [SerializeField] BoolValue playerDead;
        private BoolValue.OnValueChange onPlayerDeathFunction;

        [SerializeField] IntValue waveCount;
        private IntValue.OnValueChange onWaveCountChange;

        [SerializeField] MessageEvent soundPlayMessage;

        private bool isPaused;

        private void Start()
        {
            // Activate the right menus
            gameplayMenu.SetActive(true);
            pauseMenu.SetActive(false);
            deathMenu.SetActive(false);

            // Show the wave text in the beginning
            StartCoroutine(ModifyWaveText(1));

            // Set the right values for the texts
            ModifyCoinsText(coinsCount.Value);
            ModifyEnemiesKilledText(enemiesKilledCount.Value);

            // Subscribe to events
            onCoinsCountChange = (int value) => ModifyCoinsText(value);
            coinsCount.onValueChange += onCoinsCountChange;

            onEnemiesKilledCountChange = (int value) => ModifyEnemiesKilledText(value);
            enemiesKilledCount.onValueChange += onEnemiesKilledCountChange;

            onPlayerDeathFunction = (bool value) => { if (value) { ActivateDeathMenu(); } };
            playerDead.onValueChange += onPlayerDeathFunction;

            onWaveCountChange = (int value) => StartCoroutine(ModifyWaveText(value));
            waveCount.onValueChange += onWaveCountChange;
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            coinsCount.onValueChange -= onCoinsCountChange;
            enemiesKilledCount.onValueChange -= onEnemiesKilledCountChange;
            playerDead.onValueChange -= onPlayerDeathFunction;
            waveCount.onValueChange -= onWaveCountChange;
        }

        #region Event Functions
        /// <summary>
        /// Modify the coins text to show the right value.
        /// </summary>
        /// <param name="value">The new coins value.</param>
        private void ModifyCoinsText(int value)
        {
            coinsText.text = value.ToString();
            deathCoinsText.text = value.ToString();
        }

        /// <summary>
        /// Modify the enemies killed text to show the right value.
        /// </summary>
        /// <param name="value">The new enemies killed value.</param>
        private void ModifyEnemiesKilledText(int value)
        {
            enemiesKilledText.text = value.ToString();
            deathEnemiesKilledText.text = value.ToString();
        }

        /// <summary>
        /// Activates the death menu when the player is dead.
        /// </summary>
        private void ActivateDeathMenu()
        {
            // Enable the right menus
            deathMenu.SetActive(true);
            gameplayMenu.SetActive(false);
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);
        }

        /// <summary>
        /// Modify the wave text according to the new <paramref name="waveCount"/>.
        /// </summary>
        /// <param name="waveCount">The current wave the player is at.</param>
        private IEnumerator ModifyWaveText(int waveCount)
        {
            // Modify the pause and death's wave text
            pauseWaveCountText.text = waveCount.ToString();
            deathWaveCountText.text = waveCount.ToString();

            // Enable the wave text with the right message
            waveText.text = "WAVE : " + waveCount;
            waveText.enabled = true;

            // Loop during the fading process
            for (float count = 0f; count < GameData.waveTextShowTime; count += Time.fixedDeltaTime * timeScale.Value)
            {
                // Update the alpha
                waveText.color = new Color(waveText.color.r, waveText.color.g, waveText.color.b, 1f - (count / GameData.waveTextShowTime));

                // Wait the needed amount of time
                yield return new WaitForSeconds(Time.fixedDeltaTime * timeScale.Value);
            }

            // Reset the alpha correctly
            waveText.color = new Color(waveText.color.r, waveText.color.g, waveText.color.b, 1f);

            // Disable the wave text
            waveText.enabled = false;
        }
        #endregion

        #region Button Functions
        /// <summary>
        /// Plays the click sound.
        /// </summary>
        private void PlayClickSound() { soundPlayMessage.SendMessage("Clip1" + (int)ClipIndex.ButtonClicked); }

        /// <summary>
        /// Pause/Unpause the game depending on its current state.
        /// </summary>
        public void Pause()
        {
            // Play the click sound
            PlayClickSound();

            // Change the game's state
            isPaused = !isPaused;

            // Stop the time
            Time.timeScale = isPaused ? 0f : 1f;

            // Activate the right menus
            gameplayMenu.SetActive(!isPaused);
            pauseMenu.SetActive(isPaused);
            deathMenu.SetActive(false);

            pausePart.SetActive(isPaused);
            settingsMenu.SetActive(!isPaused);
        }

        /// <summary>
        /// Restart the game.
        /// </summary>
        public void Restart()
        {
            StartCoroutine(RestartCor());
        }
        private IEnumerator RestartCor()
        {
            // Play the click sound
            PlayClickSound();

            // Reset the time scale before loading a new scene
            Time.timeScale = 1f;

            yield return new WaitForSeconds(GameData.timeWaitBeforeSceneChanges);

            // Since we change scene, we should try to save the best score
            SaveBestScore();

            // Set that the game has restarted and doesn't play for the first time
            PlayerPrefs.SetInt("fnjdnfievojokcps^qjzdinjopkpo=iazijpoinfkldqcpiqzijpeiondkscskfoqkj", 1);

            // Restart the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Show/unshow the settings panel.
        /// </summary>
        public void Settings(bool activateSettings)
        {
            // Play the click sound
            PlayClickSound();

            // Activate the right menus
            pausePart.SetActive(!activateSettings);
            settingsMenu.SetActive(activateSettings);
        }

        /// <summary>
        /// Return to the menu.
        /// </summary>
        public void GoToMenu()
        {
            StartCoroutine(GoToMenuCor());
        }
        private IEnumerator GoToMenuCor()
        {
            // Play the click sound
            PlayClickSound();

            // Reset the time scale before loading a new scene
            Time.timeScale = 1f;

            yield return new WaitForSeconds(GameData.timeWaitBeforeSceneChanges);

            // Since we change scene, we should try to save the best score
            SaveBestScore();

            // Go to the menu scene
            SceneManager.LoadScene((int)SceneIndex.MENU);
        }

        /// <summary>
        /// Saves the best player's score if he has reach a better score.
        /// </summary>
        private void SaveBestScore()
        {
            // Since the player has died, save the best score for the coins earned and the enemies killed
            int enemiesKilledBestScore = PlayerPrefs.GetInt("fbihdsijqobifhzeojqoidhqkljopjdenhkljxjqsnhcnjkcijns", 0);
            int coinsEarnedBestScore = PlayerPrefs.GetInt("faezgvuqbhidgufvigbhezhiqonfjdpjàixpoqjdjpfuojxuzqd", 0);

            // If the player has made a better score, save it
            if (enemiesKilledCount.Value > enemiesKilledBestScore)
            {
                PlayerPrefs.SetInt("fbihdsijqobifhzeojqoidhqkljopjdenhkljxjqsnhcnjkcijns", enemiesKilledCount.Value);
            }
            if (coinsCount.Value > coinsEarnedBestScore)
            {
                PlayerPrefs.SetInt("faezgvuqbhidgufvigbhezhiqonfjdpjàixpoqjdjpfuojxuzqd", coinsCount.Value);
            }
        }
        #endregion
    }
}
