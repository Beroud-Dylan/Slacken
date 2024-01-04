using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Float Value", menuName = "Slacken/Values/Float")]
    public class FloatValue : ScriptableObject
    {
        [SerializeField] float value;
        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                onValueChange?.Invoke(this.value);
            }
        }
        public delegate void OnValueChange(float value);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
