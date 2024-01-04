using UnityEngine;
using UnityEngine.UI;
using Slacken.Bases.SO;
using Slacken.Bases.Joysticks;

namespace Slacken.Handlers
{
    public class InputHandler : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] Joystick moveJoystick;
        [SerializeField] Joystick rotateJoystick;

        [Header("Dependencies")]
        [SerializeField] CameraSO mainCam;

        [SerializeField] Vector2Value inputs;
        [SerializeField] Vector2Value turnInputs;

        [SerializeField] BoolValue playerWantsToShoot;

        private void Update()
        {
            // Get the right inputs and send them
            Vector2 currentInputs = GetInputs();
            if (currentInputs != inputs.Value)
            {
                inputs.Value = currentInputs;
            }

            // Get the right turn inputs and send them
            Vector2 currentTurnInputs = GetTurnInputs();
            if (currentTurnInputs != turnInputs.Value)
            {
                turnInputs.Value = currentTurnInputs;
            }
        }

        #region Movement
        /// <summary>
        /// Get the inputs of the player depending on the device type.
        /// </summary>
        private Vector2 GetInputs()
        {
            Vector2 inputs = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);

            #if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Z))
            {
                inputs.y = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                inputs.y = -1;
            }
            else
            {
                inputs.y = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                inputs.x = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                inputs.x = -1;
            }
            else
            {
                inputs.x = 0;
            }
            #endif

            return inputs;
        }
        /// <summary>
        /// Get the turning inputs of the player depending on the device type.
        /// </summary>
        private Vector2 GetTurnInputs()
        {
            // Get the input
            Vector2 turnInputs = new Vector2(rotateJoystick.Horizontal, rotateJoystick.Vertical);

            // If the turn input hasn't changed, let it stay the same so that it doesn't automatically reset to 0 and make a weird behaviour
            if(turnInputs == Vector2.zero && this.turnInputs.Value != Vector2.zero)
            {
                turnInputs = this.turnInputs.Value;
            }

            return turnInputs;
        }
        #endregion

        #region Special Functions
        /// <summary>
        /// Set whether or not the player will shoot.
        /// </summary>
        /// <param name="value">If the value is on true, the player will shoot.</param>
        public void SetPlayerShoot(bool value)
        {
            playerWantsToShoot.Value = value;
        }
        #endregion
    }
}
