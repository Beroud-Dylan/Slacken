using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Scene Theme Name", menuName = "Slacken/Custom/SceneTheme")]
    public class SceneTheme : ScriptableObject
    {
        public Theme mainTheme;
        [SerializeField] Theme[] themes;
        public Theme[] Themes { get => themes; }
    }
}
