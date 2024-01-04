using UnityEngine;

namespace Slacken
{
    public static class GameData
    {
        #region Constant Values
        public const float minVelocity = 0.5f;
        public const float scaleRatio = 3.125f;
        public const float timeWaitBeforeSceneChanges = 0.1f;

        // TAGS
        public const string PLAYER_TAG = "Player";
        public const string ENEMY_TAG = "Enemy";
        public const string WALL_TAG = "Wall";
        public const string OBSTACLE_TAG = "Obstacle";

        // OBSTACLES
        public const float minSizeObstacle = 1f;
        public const float maxSizeObstacle = 2f;

        // LEVEL GENERATION
        public const int maxNewNumberEnemies = 5;
        public const float startTimeScale = 1f;

        // POST PROCESSING VALUES
        public const float maxChromaticAberrationIntensity = 0.35f;
        public const float maxBloomIntensity = 10f;

        // OTHER
        public const float particlesTimeWait = 1f;
        public const float waveTextShowTime = 1f;

        public const float playerTimeSlow = 0.1f;
        public const float timeSlowTransitionTime = 0.5f;
        public const float timeIncrementEachSlows = 0.05f;

        public const float baseBulletScaleRatio = 0.333333333f; // The base bullet scale is 0.25f for an entity of 0.75f, so the scale ratio is 0.25f/0.75f = 1/3f
        #endregion

        #region Message Translator
        const char separator = '|';
        public static string BulletValuesToMessage(BulletSpawnValues values)
        {
            // Create the message
            string message = values.position.AsString();
            message += separator;

            message += values.scale.AsString();
            message += separator;

            message += values.direction.AsString();
            message += separator;

            message += values.color.AsString();
            message += separator;

            message += values.damage;
            message += separator;

            message += values.isFriendly;

            // Return it
            return message;
        }
        public static BulletSpawnValues MessageToBulletValues(string message)
        {
            // Get the different arguments
            string[] args = message.Split(separator);

            if (args.Length == 6)
            {
                Vector2 position = args[0].ToVector2();
                Vector2 scale = args[1].ToVector2();
                Vector2 direction = args[2].ToVector2();
                Color color = args[3].ToColor();
                int damage;
                bool isFriendly;

                // Try to get the values
                if (int.TryParse(args[4], out damage) && bool.TryParse(args[5], out isFriendly))
                {
                    // Return the completed values
                    return new BulletSpawnValues(position, scale, direction, color, damage, isFriendly, empty: false);
                }
            }

            // Return empty values
            return new BulletSpawnValues(Vector2.zero, Vector2.zero, Vector2.zero, Color.white, 0, isFriendly: false, empty: true);
        }

        public static string CoinSpawnValuesToString(CoinSpawnValues values)
        {
            // Create the message
            string message = values.position.AsString();
            message += separator;
            message += values.count;

            // Return it
            return message;
        }
        public static CoinSpawnValues MessageToCoinValues(string message)
        {
            // Get the different arguments
            string[] args = message.Split(separator);

            if (args.Length == 2)
            {
                Vector2 position = args[0].ToVector2();
                int count;

                // Try to get the count
                if (int.TryParse(args[1], out count))
                {
                    // Return the completed values
                    return new CoinSpawnValues(position, count, empty: false);
                }
            }

            // Return empty values
            return new CoinSpawnValues(Vector2.zero, 0, empty: true);
        }

        public static string AsString(this Color color)
        {
            return "(" + color.r + "; " + color.g + "; " + color.b + "; " + color.a + ")";
        }
        public static Color ToColor(this string message)
        {
            // Split the message in different parts
            string[] parts = message.Replace('(', ' ').Replace(')', ' ').Split(';');

            // Verify that it was able to split it correctly
            if (parts.Length == 4)
            {
                // Then we extract the values from it, if it is possible
                if (float.TryParse(parts[0], out float r) && float.TryParse(parts[1], out float g) && float.TryParse(parts[2], out float b) && float.TryParse(parts[3], out float a))
                {
                    // We then return it as a color
                    return new Color(r, g, b, a);
                }
            }

            #if UNITY_EDITOR
            Debug.LogError("THE MESSAGE PROVIDED '" + message + "' DIDN'T CONTAIN ANY INFORMATIONS TO BE TRANSLATED AS A COLOR !");
            #endif
            return Color.white;
        }

        public static string AsString(this Vector2 vector2)
        {
            return "(" + vector2.x + "; " + vector2.y + ")";
        }
        public static Vector2 ToVector2(this string message)
        {
            // Split the message in different parts
            string[] parts = message.Replace('(', ' ').Replace(')', ' ').Split(';');

            // Verify that it was able to split it correctly
            if (parts.Length == 2)
            {
                float x = 0f, y = 0f;

                // Then we extract the values from it, if it is possible
                if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y))
                {
                    // We then return it as a Vector2
                    return new Vector2(x, y);
                }
            }

            #if UNITY_EDITOR
            Debug.LogError("THE MESSAGE PROVIDED '" + message + "' DIDN'T CONTAIN ANY INFORMATIONS TO BE TRANSLATED AS A VECTOR 2 !");
            #endif
            return Vector2.zero;
        }
        #endregion
    }

    public struct BulletSpawnValues
    {
        public Vector2 position;
        public Vector2 scale;
        public Vector2 direction;
        public Color color;
        public int damage;

        public bool isFriendly;
        public bool empty;

        public BulletSpawnValues(Vector2 position, Vector2 scale, Vector2 direction, Color color, int damage, bool isFriendly, bool empty)
        {
            this.position = position;
            this.scale = scale;
            this.direction = direction;
            this.color = color;
            this.damage = damage;

            this.isFriendly = isFriendly;
            this.empty = empty;
        }
    }

    public struct CoinSpawnValues
    {
        public Vector2 position;
        public int count;
        public bool empty;

        public CoinSpawnValues(Vector2 position, int count, bool empty)
        {
            this.position = position;
            this.count = count;
            this.empty = empty;
        }
    }

    [System.Serializable]
    public struct SpawnableObjects
    {
        public GameObject prefab;
        public Transform parent;
    }

    [System.Serializable]
    public struct SpawnableEnemies
    {
        public GameObject prefab;
        public Transform parent;
        public int weight;
    }

    public enum MusicIndex
    {
        MainMusic = 0
    }

    public enum ClipIndex
    {
        ButtonClicked  = 0,
        CoinsCollected = 1,
        Shoot          = 2,
        TakeDamage     = 3,
        TimeSlow1      = 4,
        TimeSlow2      = 5,
        EnemyDeath     = 6,
        NewWave        = 7,
        BoostCollected = 8,
    }

    public enum SceneIndex
    {
        MENU = 0,
        PLAY = 1
    }
}