using UnityEngine;

namespace Slacken.Bases.SO
{
    [CreateAssetMenu(fileName = "Theme Name", menuName = "Slacken/Custom/Theme")]
    public class Theme : ScriptableObject
    {
		[Header("Environment")]
		[SerializeField] Color backgroundColor;
		public Color BackgroundColor { get => backgroundColor; }

		public Color WallColor { get => wallColor; }
		[SerializeField] Color wallColor;

		public Color ObstacleColor { get => obstacleColor; }
		[SerializeField] Color obstacleColor;
		

		[Header("Entities")]
		[SerializeField] EntityTheme playerTheme;
		public EntityTheme PlayerTheme { get => playerTheme; }

		public EntityTheme EnemyTheme { get => enemyTheme; }
		[SerializeField] EntityTheme enemyTheme;
		public EntityTheme FastEnemyTheme { get => fastEnemyTheme; }
		[SerializeField] EntityTheme fastEnemyTheme;
		public EntityTheme FatEnemyTheme { get => fatEnemyTheme; }
		[SerializeField] EntityTheme fatEnemyTheme;
		public EntityTheme SpikeEnemyTheme { get => spikeEnemyTheme; }
		[SerializeField] EntityTheme spikeEnemyTheme;

		[Header("Effects")]
		[SerializeField] Color slowTimeColorEffect;
		public Color SlowTimeColorEffect { get => slowTimeColorEffect; }
	}

	[System.Serializable]
	public struct EntityTheme
    {
		[SerializeField] Color mainBodyColor;
		public Color MainBodyColor { get => mainBodyColor; }

		[SerializeField] Color shooterColor;
		public Color ShooterColor { get => shooterColor; }

		[SerializeField] Color bulletColor;
		public Color BulletColor { get => bulletColor; }
	}
}
