using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Vector2 Value", menuName = "Slacken/Values/Vector2")]
    public class Vector2Value : ScriptableObject
    {
        [SerializeField] Vector2 value;
        public Vector2 Value
        {
            get => value;
            set
            {
                this.value = value;
                onValueChange?.Invoke(this.value);
            }
        }
        public delegate void OnValueChange(Vector2 value);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
