using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Int Value", menuName = "Slacken/Values/Int")]
    public class IntValue : ScriptableObject
    {
        [SerializeField] int value;
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                onValueChange?.Invoke(this.value);
            }
        }
        public delegate void OnValueChange(int value);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
