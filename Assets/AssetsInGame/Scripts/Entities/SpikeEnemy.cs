using UnityEngine;
using Slacken.Bases.SO;
using System.Collections;

namespace Slacken.Entities
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeEnemy : Entity
    {
        [Header("AI Movement Values")]
        [SerializeField] [Range(2, 100)] int numberOfDirections;
        [SerializeField] float obstacleViewDistance;
        [SerializeField] float fearObjectWeight;
        [SerializeField] LayerMask fearMask;

        private Vector2[] directions;
        private Vector2 desiredDirection;

        [Header("AI Attack Values")]
        [SerializeField] Transform[] shooters;
        [SerializeField] Transform[] shootPoints;
        [SerializeField] SpriteRenderer[] shootersGraphics;
        [SerializeField] float shootDistance;
        private float shootSqrDst;

        [Header("Other")]
        [SerializeField] GameObject particles;
        [SerializeField] GameObject GFXPart;
        [SerializeField] SpriteRenderer mainBodyRenderer;
        private Collider2D col;
        private bool isDead;

        [SerializeField] float turnSpeed;
        private Vector2 turnDirection;
        private float increasingAngle;

        [Header("Dependencies")]
        [SerializeField] GameObjectSO player;
        [SerializeField] MessageEvent bulletMessageEvent;
        [SerializeField] MessageEvent coinSpawnMessageEvent;
        [SerializeField] MessageEvent soundPlayMessage;
        [SerializeField] SceneTheme sceneTheme;
        private EntityTheme theme;

        #region Debug
        [Header("Debug")]
        [SerializeField] bool drawGizmos;

        private void OnDrawGizmos()
        {
            // Draw Gizmos only in play mode
            if (Application.isPlaying && drawGizmos && directions != null)
            {
                // Color the surronding direction
                for (int i = 0; i < directions.Length; i++)
                {
                    Gizmos.color = GetWeight(desiredDirection, directions[i]) > 0f ? Color.green : new Color(125f, 0f, 0f, 1f);
                    Gizmos.DrawLine(transform.position, (Vector2)transform.position + directions[i]);
                }

                // Draw a circle around the enemy to see from where it should "fear" the obstacle
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, obstacleViewDistance);

                // Draw a circle around the enemy where we can see where it can attack
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, shootDistance);
            }
        }
        #endregion

        private new void Start()
        {
            // Start the base of the entity
            base.Start();

            // Initialize values
            shootSqrDst = shootDistance * shootDistance;

            // By default, set that the particles shouldn't be activated
            particles.SetActive(false);

            // Set the right theme
            theme = sceneTheme.mainTheme.SpikeEnemyTheme;
            mainBodyRenderer.color = theme.MainBodyColor;
            particles.GetComponent<ParticleSystem>().startColor = theme.MainBodyColor;
            for (int i = 0; i < shootersGraphics.Length; i++) { shootersGraphics[i].color = theme.ShooterColor; }

            // Get the components
            col = GetComponent<Collider2D>();
        }
        private new void OnEnable()
        {
            base.OnEnable();

            // Set that the enemy is no longer dead
            isDead = false;

            // Enable the needed parts
            GFXPart.SetActive(true);
            if (col != null) { col.enabled = true; }
        }

        public override void Die()
        {
            // Play the death sound
            soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.EnemyDeath);

            // Set that the enemy is dead
            isDead = true;
            // Animate the death
            StartCoroutine(DeathAnimation());

            // Get the coins spawn values
            CoinSpawnValues values = new CoinSpawnValues(transform.position, numberOfCoins, false);
            // Get the message from it
            string message = GameData.CoinSpawnValuesToString(values);
            // Spawn the coins
            coinSpawnMessageEvent.SendMessage(message);
        }
        /// <summary>
        /// Animates the enemy's death by making some particles appear.
        /// </summary>
        IEnumerator DeathAnimation()
        {
            // Disable the unnecessary parts
            GFXPart.SetActive(false);
            col.enabled = false;
            // Start the particle system
            particles.SetActive(true);

            // Wait a bit so that the particles can be seen
            yield return new WaitForSeconds(GameData.particlesTimeWait);

            // Disable the particles and the gameObject
            particles.SetActive(false);
            gameObject.SetActive(false);
        }

        #region Attack Functions
        private new void Update()
        {
            // Update the base of the entity
            base.Update();

            // If the player (or the enemy) is dead, the enemy don't need to attack him anymore
            if (playerDeath.Value || isDead) { return; }

            // If the enemy is in the shooting range and can attack
            if ((player.Value.transform.position - transform.position).sqrMagnitude <= shootSqrDst && CanAttack)
            {
                Attack();
                OnAttackProceed();
            }
        }

        public override void Attack()
        {
            // Play the shoot sound
            soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.Shoot);

            // Shoot from every shooters
            for (int i = 0; i < shootPoints.Length; i++)
            {
                // Get the bullet values
                Vector2 direction = (shootPoints[i].transform.position - shooters[i].transform.position).normalized;
                BulletSpawnValues values = new BulletSpawnValues(shootPoints[i].transform.position, GameData.baseBulletScaleRatio * transform.localScale, direction, theme.BulletColor, attackDamage, isFriendly: false, empty: false);

                // Make the message
                string message = GameData.BulletValuesToMessage(values);

                // Send a bullet
                bulletMessageEvent.SendMessage(message);
            }
        }
        #endregion

        #region Movement
        private void FixedUpdate()
        {
            if (isDead) { return; }

            // Get the direction in which the enemy has to go and go there
            Vector2 choseDirection = GetDirection();
            base.Move(choseDirection);

            // Turn the shooters towards the player
            increasingAngle = (increasingAngle + turnSpeed * timeScale.Value * Time.fixedDeltaTime) % 360f;
            turnDirection = new Vector2(Mathf.Cos(increasingAngle), Mathf.Sin(increasingAngle));
            for (int i = 0; i < shooters.Length; i++)
            {
                // Get the angle
                float angle = (float)(i / (float)shooters.Length) * 360f;
                // Get the new direction
                Vector3 newDir = Quaternion.AngleAxis(angle, Vector3.forward) * turnDirection;
                // Turn the current shooter to that direction
                base.TurnToward(shooters[i], transform.position + newDir);
            }
        }

        private Vector2 GetDirection()
        {
            // Get the desired direction
            Vector2 direction = Vector2.zero;
            Vector2 mainDesiredDirection = (player.Value.transform.position - transform.position).normalized;

            float dstRatio = Mathf.Sqrt(shootSqrDst / (player.Value.transform.position - transform.position).sqrMagnitude);
            desiredDirection = mainDesiredDirection * (1f - Mathf.Clamp01(dstRatio));

            // Calculate the surronding directions
            directions = new Vector2[numberOfDirections];
            for (int i = 0; i < numberOfDirections; i++)
            {
                // Get the angle
                float angle = (float)(i / (float)numberOfDirections) * 360f;
                // Get the direction from the angle
                directions[i] = Quaternion.AngleAxis(angle, Vector3.forward) * mainDesiredDirection;
                // Apply a weight to it
                directions[i] *= GetWeight(mainDesiredDirection, directions[i]);
                // Normalize the direction
                directions[i].Normalize();
            }

            // Add to the current direction all the direction that should influence it
            for (int i = 0; i < numberOfDirections; i++) { direction += directions[i]; }
            direction += desiredDirection;
            direction /= numberOfDirections + 1;

            // Return the direction to take
            return direction.normalized;
        }
        private float GetWeight(Vector2 desiredDirection, Vector2 direction)
        {
            // Calculate the base weight of the direction
            float weight = Mathf.Abs(Vector2.Dot(desiredDirection, direction));

            // Get the collisions
            float newShootDst = shootDistance - 0.5f;
            float maxViewDistance = newShootDst > obstacleViewDistance ? newShootDst : obstacleViewDistance;
            RaycastHit2D[] results = Physics2D.RaycastAll(transform.position, direction, maxViewDistance, fearMask);
            float closestDst = float.MaxValue;

            for (int i = 0; i < results.Length; i++)
            {
                // Determinate if it's an entity or an obstacle
                bool isObstacle = results[i].transform.CompareTag(GameData.WALL_TAG) || results[i].transform.CompareTag(GameData.OBSTACLE_TAG);

                // Depending on the object found, it should be taken into account only if the distance allows it
                if ((isObstacle && results[i].distance <= obstacleViewDistance) || (!isObstacle && results[i].distance <= newShootDst))
                {
                    // If the distance is closer and isn't the enemy itself
                    if (results[i].distance < closestDst && results[i].transform != transform)
                    {
                        // Set the new closest distance
                        closestDst = results[i].distance;
                    }
                }
            }

            // If we found an obstacle to fear other the us
            if (closestDst != float.MaxValue)
            {
                // Decrease the weight accordingly
                weight -= (closestDst / obstacleViewDistance) * (fearObjectWeight + 1f);
            }

            // Return the weight
            return weight;
        }
        #endregion
    }
}
