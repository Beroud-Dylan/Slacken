using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "GameObject Name", menuName = "Slacken/Values/GameObject")]
    public class GameObjectSO : ScriptableObject
    {
        [SerializeField] GameObject value;
        public GameObject Value
        {
            get => value;
            set
            {
                GameObject oldValue = this.value;
                this.value = value;

                onValueChange?.Invoke(oldValue, this.value);
            }
        }
        public delegate void OnValueChange(GameObject oldValue, GameObject newValue);
        public event OnValueChange onValueChange;

        private void OnDestroy()
        {
            // Reset the events if the object gets destroyed
            onValueChange = null;
        }
    }
}
