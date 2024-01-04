using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using Slacken.Bases.SO;

namespace Slacken.ValueManagers
{
    public class SlowTimeManager : MonoBehaviour
    {
        const float limitBarRange = 84.37f;

        [Header("Slow Time Values")]
        [SerializeField] float maxSlowDownTime;
        [SerializeField] float slowDownTimeIncrement;
        [SerializeField] float minSlowTimeValue;
        private float slowDownTimeValue;

        private bool isSlowingDownTime = false;
        private float timeScaleToAssign = 1f;

        private Coroutine graduallySlowDownCoroutine;
        private Coroutine graduallyTimeGoBackCoroutine;
        private Coroutine refreshTimeScaleCoroutine;

        [Header("UI Elements")]
        [SerializeField] Slider slowTimeSlider;
        [SerializeField] RawImage fillImage;
        [SerializeField] Transform limitBar;
        [SerializeField] private Gradient colorOverTime;

        [SerializeField] Text timeScaleText;

        [Header("Dependencies")]
        [SerializeField] FloatValue timeScale;
        private FloatValue.OnValueChange onTimeScaleChange;
        [SerializeField] MessageEvent timeScaleMessage;
        private MessageEvent.OnNewMessage onNewTimeScaleAssignmentFunction;
        [SerializeField] CameraSO mainCam;
        [SerializeField] MessageEvent soundPlayMessage;
        [SerializeField] SceneTheme sceneTheme;

        private Bloom bloom;
        private ChromaticAberration chromaticAberration;

        #region Initialization
        private void Awake()
        {
            // Set the right time
            timeScale.Value = GameData.startTimeScale;
            timeScaleToAssign = timeScale.Value;
            Time.timeScale = 1f;
        }
        private void Start()
        {
            // Set the right values
            slowDownTimeValue = maxSlowDownTime;
            slowTimeSlider.value = 1f;

            float offset = minSlowTimeValue / maxSlowDownTime;
            limitBar.localPosition = new Vector3((offset * 2f - 1f) * limitBarRange, limitBar.localPosition.y, limitBar.localPosition.z);

            timeScaleText.text = "Time speed : x" + timeScale.Value;

            // Subscribe to events
            onTimeScaleChange = (float value) => { timeScaleText.text = "Time speed : x" + (float)((int)(value * 1000f) / 1000f); };
            timeScale.onValueChange += onTimeScaleChange;

            onNewTimeScaleAssignmentFunction = (string message) => ModifyTimeScale(message);
            timeScaleMessage.onNewMessageReceived += onNewTimeScaleAssignmentFunction;

            // Get the components
            if(mainCam.Cam.TryGetComponent(out PostProcessVolume volume))
            {
                // Get the settings
                volume.profile.TryGetSettings(out bloom);
                volume.profile.TryGetSettings(out chromaticAberration);

                // Modify them
                bloom.intensity.value = 0f;
                bloom.color.value = sceneTheme.mainTheme.SlowTimeColorEffect;
                chromaticAberration.intensity.value = 0f;
            }
        }
        private void OnDestroy()
        {
            // Unsubscribe to events
            timeScale.onValueChange -= onTimeScaleChange;
            timeScaleMessage.onNewMessageReceived -= onNewTimeScaleAssignmentFunction;
        }
        private void OnValidate()
        {
            if (maxSlowDownTime <= 0f)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE MAXIMUM SLOW DOWN TIME SHOULD BE A STRICLTY POSITIVE NUMBER !");
                #endif

                maxSlowDownTime = 1f;
            }

            if (minSlowTimeValue < 0f)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE MINIMUM VALUE BEFORE USING SLOWING TIME SHOULD BE A POSITIVE OR NULL NUMBER !");
                #endif

                minSlowTimeValue = 0f;
            }

            if (minSlowTimeValue >= maxSlowDownTime)
            {
                #if UNITY_EDITOR
                Debug.LogError("THE MINIMUM VALUE BEFORE USING SLOWING TIME SHOULD BE LESS THAN THE MAXIMUM SLOW DOWN TIME !");
                #endif
                maxSlowDownTime = minSlowTimeValue + 1;
            }
        }
        #endregion

        private void FixedUpdate()
        {
            // If the time is slowed down
            if (isSlowingDownTime)
            {
                // Increment the time scale to assign
                timeScaleToAssign += (GameData.timeIncrementEachSlows / maxSlowDownTime) * Time.fixedDeltaTime;
                // Decrease the time slow value over time
                slowDownTimeValue -= Time.fixedDeltaTime;

                // If the slow down time value is too low, make the time go back to its initial value
                if (slowDownTimeValue < 0f)
                {
                    ChangeSlowTimeState();
                }
            }
            else
            {
                // Else, increment the value so that it gets to its maximum
                slowDownTimeValue = Mathf.Clamp(slowDownTimeValue + Time.fixedDeltaTime * slowDownTimeIncrement, 0f, maxSlowDownTime);
            }

            // Update the slow down time bar
            UpdateSlowTimeBar();
        }

        /// <summary>
        /// Modify the time scale depending on the message received.
        /// </summary>
        /// <param name="message">The received message.</param>
        private void ModifyTimeScale(string message)
        {
            // Get the time scale value if it's possible
            if(float.TryParse(message, out float newTimeScale))
            {
                // If the time is slowed down, put it back to its new time scale
                if (isSlowingDownTime)
                {
                    // Stop the coroutine
                    if (graduallyTimeGoBackCoroutine != null) StopCoroutine(graduallyTimeGoBackCoroutine);
                }
                else
                {
                    // Stop the coroutine
                    if (graduallySlowDownCoroutine != null) StopCoroutine(graduallySlowDownCoroutine);
                }

                // Make the time go its new scale
                refreshTimeScaleCoroutine = StartCoroutine(GraduallyPutTimeScaleToValue(newTimeScale, true));
                // Set the new time scale to assign
                timeScaleToAssign = newTimeScale;
                // Set that the time is no longer slowed down
                isSlowingDownTime = false;
            }
        }

        #region Slow Time
        /// <summary>
        /// Slows down the time or make it go back as it was.
        /// </summary>
        public void ChangeSlowTimeState()
        {
            // Change the value of whether or not the player wants to slow time
            isSlowingDownTime = !isSlowingDownTime && slowDownTimeValue > minSlowTimeValue ? true : false;

            // Stop the coroutines
            if (refreshTimeScaleCoroutine != null) StopCoroutine(refreshTimeScaleCoroutine);

            // If the player wants to slow down time
            if (isSlowingDownTime)
            {
                // Stop the coroutine
                if (graduallyTimeGoBackCoroutine != null) StopCoroutine(graduallyTimeGoBackCoroutine);

                // Slows down time gradually
                graduallySlowDownCoroutine = StartCoroutine(GraduallyPutTimeScaleToValue(timeScale.Value * GameData.playerTimeSlow, false));
            }
            // Else, if the player wants to stop slowing time because the time is already slowed
            else if (!isSlowingDownTime)
            {
                // Stop the coroutine
                if (graduallySlowDownCoroutine != null) StopCoroutine(graduallySlowDownCoroutine);

                // Reset the time scale gradually
                graduallyTimeGoBackCoroutine = StartCoroutine(GraduallyPutTimeScaleToValue(timeScaleToAssign, false));
            }
        }

        /// <summary>
        /// Gradually changes the time scale so that it reaches a <paramref name="timeGoal"/>.
        /// </summary>
        /// <param name="timeGoal">The time goal to reach.</param>
        /// <param name="isForRefresh">Is the current time scale modified to refresh it ?</param>
        /// <returns></returns>
        private IEnumerator GraduallyPutTimeScaleToValue(float timeGoal, bool isForRefresh)
        {
            // Play the right sound
            int clipToPlay = (int)(timeScale.Value > timeGoal ? ClipIndex.TimeSlow1 : ClipIndex.TimeSlow2);
            soundPlayMessage.SendMessage("Clip0" + clipToPlay);

            // Get the time values
            float timeIncrement = (Mathf.Abs(timeScale.Value - timeGoal) / GameData.timeSlowTransitionTime) * Time.fixedDeltaTime;
            WaitForSeconds timeToWait = new WaitForSeconds(Time.fixedDeltaTime);

            // Get effects' values
            float bloomGoal = isForRefresh ? 0f : (timeScale.Value > timeGoal ? GameData.maxBloomIntensity : timeScale.Value < timeGoal ? 0f : bloom.intensity.value);
            float chromaticAberrationGoal = isForRefresh ? 0f : (timeScale.Value > timeGoal ? GameData.maxChromaticAberrationIntensity : timeScale.Value < timeGoal ? 0f : chromaticAberration.intensity.value);

            float bloomIntensityIncrement = (Mathf.Abs(bloom.intensity - bloomGoal) / GameData.timeSlowTransitionTime) * Time.fixedDeltaTime;
            float chromaticAberrationIntensityIncrement = (Mathf.Abs(chromaticAberration.intensity - chromaticAberrationGoal) / GameData.timeSlowTransitionTime) * Time.fixedDeltaTime;

            // If we want to slow down to that value
            if (timeScale.Value > timeGoal)
            {
                // Decrease the current time scale so that the time slows down
                while (timeScale.Value > timeGoal)
                {
                    timeScale.Value -= timeIncrement;
                    bloom.intensity.value += bloomIntensityIncrement;
                    chromaticAberration.intensity.value += chromaticAberrationIntensityIncrement;
                    yield return timeToWait;
                }
            }
            // Else, if we want to make time faster to that value 
            else if (timeScale.Value < timeGoal)
            {
                // Increase the current time scale so that it reaches the right value
                while (timeScale.Value < timeGoal)
                {
                    timeScale.Value += timeIncrement;
                    bloom.intensity.value -= bloomIntensityIncrement;
                    chromaticAberration.intensity.value -= chromaticAberrationIntensityIncrement;
                    yield return timeToWait;
                }
            }
            
            // Clamp them to the right value
            timeScale.Value = timeGoal;
            bloom.intensity.value = bloomGoal;
            chromaticAberration.intensity.value = chromaticAberrationGoal;
        }
        #endregion

        #region Update UI Elements
        /// <summary>
        /// Updates the slow down time bar.
        /// </summary>
        private void UpdateSlowTimeBar()
        {
            // Get the ratio
            float ratio = (float)(slowDownTimeValue / maxSlowDownTime);

            // Assign the color and set the right value for the slider
            fillImage.color = colorOverTime.Evaluate(ratio);
            slowTimeSlider.value = ratio;
        }
        #endregion
    }
}
