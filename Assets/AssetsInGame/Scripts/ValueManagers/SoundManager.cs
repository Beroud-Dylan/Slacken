using UnityEngine;
using UnityEngine.UI;
using Slacken.Bases.SO;
using System.Collections;

namespace Slacken.ValueManagers
{
    public class SoundManager : MonoBehaviour
    {
        [Header("UI For Menu")]
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider soundSlider;

        [Header("Values")]
        [SerializeField] int numberOfAudioSources;
        private AudioSource[][] audioSources;
        private float musicVolume;
        private float soundVolume;

        [Header("Dependencies")]
        [SerializeField] SoundList dataBase;
        [SerializeField] MessageEvent soundPlayMessage;
        private MessageEvent.OnNewMessage newMessageFunction;
        [SerializeField] FloatValue timeScale;
        private FloatValue.OnValueChange onTimeScaleChangeFunction;

        private void Awake()
        {
            // Set the right time scale
            timeScale.Value = GameData.startTimeScale;
        }

        private void Start()
        {
            // Load the values
            musicVolume = PlayerPrefs.GetFloat("zebfuhhçdyfiocejipzojPOJEDHOCIEPIQJCKNX", 1f);
            soundVolume = PlayerPrefs.GetFloat("fezgubhdzihyfoechzoiqhofdzqpejfdozqjhpipfdjpqomnc", 1f);

            // Initialize the audio sources, the indices correspond to :
            // 0 - Sounds affected by the time scale
            // 1 - Music affected by the time scale
            // 2 - Sounds not affected by the time scale
            audioSources = new AudioSource[3][];

            for (int i = 0; i < audioSources.Length; i++)
            {
                audioSources[i] = new AudioSource[numberOfAudioSources];
                string startName = i == 1 ? "MusicMaker_" : "SoundMaker_";

                for (int j = 0; j < numberOfAudioSources; j++)
                {
                    GameObject audioMaker = new GameObject(startName + i);
                    audioMaker.transform.SetParent(transform);
                    audioSources[i][j] = audioMaker.AddComponent<AudioSource>();

                    // If this is for the music
                    if (i == 1)
                    {
                        audioSources[i][j].volume = musicVolume;
                        audioSources[i][j].loop = true;
                        audioSources[i][j].pitch = timeScale.Value;
                    }
                    // Else, if it's for sounds
                    else
                    {
                        audioSources[i][j].volume = soundVolume;
                        audioSources[i][j].loop = false;
                        audioSources[i][j].pitch = i == 0 ? timeScale.Value : 1f;
                    }

                    // Deactivate the gameObject
                    audioMaker.gameObject.SetActive(false);
                }
            }

            // Set the right values for the sliders
            musicSlider.value = musicVolume;
            soundSlider.value = soundVolume;

            // Subscribe to events
            newMessageFunction = (string message) => ReceiveMessage(message);
            soundPlayMessage.onNewMessageReceived += newMessageFunction;

            onTimeScaleChangeFunction = (float value) => OnTimeScaleChange(value);
            timeScale.onValueChange += onTimeScaleChangeFunction;

            // Start the game's music
            soundPlayMessage.SendMessage("Music" + (int)MusicIndex.MainMusic);
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            soundPlayMessage.onNewMessageReceived -= newMessageFunction;
            timeScale.onValueChange -= onTimeScaleChangeFunction;
        }

        #region Sound Modifying Functions
        /// <summary>
        /// This function is called whenever the time scale is changed. It will change the sound and music play speed.
        /// </summary>
        /// <param name="value"></param>
        private void OnTimeScaleChange(float value)
        {
            // Loop through the music and sound clips that are affected by the time scale
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < audioSources[i].Length; j++)
                {
                    // Modify the play speed
                    audioSources[i][j].pitch = value;
                }
            }
        }
        #endregion

        #region Sound Functions
        /// <summary>
        /// This function is called whenever a sound message is received. It plays the right sound
        /// </summary>
        /// <param name="soundMessage">The sound message given.</param>
        private void ReceiveMessage(string soundMessage)
        {
            // If that's a clip that is affected by the time scale
            if (soundMessage.StartsWith("Clip0"))
            {
                int index = int.Parse(soundMessage.Remove(0, 5));
                PlaySoundOnce(index, true);
            }
            // Else, if that's a clip that is NOT affected by the time scale
            else if (soundMessage.StartsWith("Clip1"))
            {
                int index = int.Parse(soundMessage.Remove(0, 5));
                PlaySoundOnce(index, false);
            }
            // Else, if that's music
            else if (soundMessage.StartsWith("Music"))
            {
                int index = int.Parse(soundMessage.Remove(0, 5));
                PlayMusic(index);
            }
            // Else, it shouldn't exist
            else
            {
                #if UNITY_EDITOR
                Debug.LogError("THE MESSAGE GIVEN WAS WRONG : '" + soundMessage + "' !");
                #endif
            }
        }

        /// <summary>
        /// Whenever the volume of the sounds is modified, it will modify the volume of the playing sounds and save it.
        /// </summary>
        public void OnSoundValueChange()
        {
            // Set the right volume and save the value
            soundVolume = soundSlider.value;
            PlayerPrefs.SetFloat("fezgubhdzihyfoechzoiqhofdzqpejfdozqjhpipfdjpqomnc", soundVolume);

            // Reset the volumes of every audio sources
            for (int i = 0; i < numberOfAudioSources; i++)
            {
                audioSources[0][i].volume = soundVolume;
                audioSources[2][i].volume = soundVolume;
            }
        }
        /// <summary>
        /// Whenever the volume of the music is modified, it will modify the volume of the playing musics and save it.
        /// </summary>
        public void OnMusicValueChange()
        {
            // Set the right volume and save the value
            musicVolume = musicSlider.value;
            PlayerPrefs.SetFloat("zebfuhhçdyfiocejipzojPOJEDHOCIEPIQJCKNX", musicVolume);

            // Reset the volumes of every audio sources
            for (int i = 0; i < numberOfAudioSources; i++)
            {
                audioSources[1][i].volume = musicVolume;
            }
        }

        public void PlaySoundOnce(int index, bool isAffectedByTimeScale)
        {
            int subIndex = isAffectedByTimeScale ? 0 : 2;

            for (int i = 0; i < numberOfAudioSources; i++)
            {
                AudioSource source = audioSources[subIndex][i];

                // If one audio maker is available, use it
                if (!source.gameObject.activeSelf)
                {
                    source.gameObject.SetActive(true);
                    source.clip = dataBase.clips[index];
                    source.Play();

                    StartCoroutine(DisableLater(source));
                    break;
                }
            }
        }

        IEnumerator DisableLater(AudioSource source)
        {
            // Since sometimes, we can't detect when a clip has finished to play, use a length that will never be too long
            yield return new WaitUntil(() => !source.isPlaying);

            // Disable the sound source
            source.gameObject.SetActive(false);
        }

        public void PlayMusic(int index)
        {
            for (int i = 0; i < numberOfAudioSources; i++)
            {
                AudioSource source = audioSources[1][i];

                // If one audio maker is available, use it
                if (!source.gameObject.activeSelf)
                {
                    source.gameObject.SetActive(true);
                    source.clip = dataBase.musics[index];
                    source.Play();
                    break;
                }
            }
        }
        #endregion
    }
}
