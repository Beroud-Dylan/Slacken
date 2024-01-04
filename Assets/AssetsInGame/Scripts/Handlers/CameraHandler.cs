using UnityEngine;
using Slacken.Bases.SO;

namespace Slacken.Handlers
{
    [RequireComponent(typeof(Camera))]
    public class CameraHandler : MonoBehaviour
    {
        [Header("Values")]
        [SerializeField] Transform bodyToMove;
        [SerializeField] Transform target;
        [SerializeField] [Range(0f, 1f)] float smoothSpeed;
        private Vector3 velocity;

        [Header("Dependencies")]
        [SerializeField] CameraSO mainCam;
        [SerializeField] FloatValue timeScale;
        
        private void Awake()
        {
            // Set the camera
            mainCam.Cam = GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            // Only if there is something to move at, the camera should move
            if(bodyToMove != null)
            {
                // Initialize the desired position
                Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, bodyToMove.position.z);

                // Calculate the smooth position and assign it as the current position
                Vector3 smoothPosition = Vector3.SmoothDamp(bodyToMove.position, desiredPosition, ref velocity, smoothSpeed, float.MaxValue, timeScale.Value * Time.fixedDeltaTime);
                bodyToMove.position = smoothPosition;
            }
        }
    }
}
