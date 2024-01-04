using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Bool Value", menuName = "Slacken/Values/Bool")]
    public class BoolValue : ScriptableObject
    {
        [SerializeField] bool value;
        public bool Value
        {
            get => value;
            set
            {
                this.value = value;
                onValueChange?.Invoke(this.value);
            }
        }
        public delegate void OnValueChange(bool value);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
