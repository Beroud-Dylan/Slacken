using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Sound List", menuName = "Slacken/Custom/SoundList")]
    public class SoundList : ScriptableObject
    {
        public AudioClip[] musics;
        public AudioClip[] clips;
    }
}
