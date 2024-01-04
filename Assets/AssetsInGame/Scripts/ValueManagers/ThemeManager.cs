using UnityEngine;
using Slacken.Bases.SO;

namespace Slacken.ValueManagers
{
    public class ThemeManager : MonoBehaviour
    {
        [SerializeField] SceneTheme sceneTheme;

        private void Awake()
        {
            // Get a random theme as the scene theme
            int index = Random.Range(0, sceneTheme.Themes.Length);

            // If the game has restarted, use the same theme as before
            if (PlayerPrefs.GetInt("fnjdnfievojokcps^qjzdinjopkpo=iazijpoinfkldqcpiqzijpeiondkscskfoqkj") != 0)
            {
                // Get the theme index
                index = PlayerPrefs.GetInt("rebuzhjaeodàçnjceozjhdincjoejzahedjnionqeoodkx,oks", index);
            }

            // Set the new scene's theme
            sceneTheme.mainTheme = sceneTheme.Themes[index];

            // Save the current scene's theme
            PlayerPrefs.SetInt("rebuzhjaeodàçnjceozjhdincjoejzahedjnionqeoodkx,oks", index);
        }
    }
}
