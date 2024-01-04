using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Camera Value", menuName = "Slacken/Values/Camera")]
    public class CameraSO : ScriptableObject
    {
        [SerializeField] Camera cam;
        public Camera Cam
        {
            get => cam;
            set
            {
                cam = value;
                onValueChange?.Invoke(cam);
            }
        }
        public delegate void OnValueChange(Camera value);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
