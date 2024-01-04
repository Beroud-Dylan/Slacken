using UnityEngine;
using Slacken.Bases.SO;
using System.Collections;

namespace Slacken.Entities
{
    [RequireComponent(typeof(LineRenderer))]
    public class Player : Entity
    {
        [Header("Components")]
        [SerializeField] Transform shooter;
        [SerializeField] Transform shootPoint;
        [SerializeField] SpriteRenderer mainBodyRenderer;
        [SerializeField] SpriteRenderer shooterRenderer;
        private LineRenderer lineRenderer;

        [Header("Dependencies")]
        [SerializeField] Vector2Value inputs;
        private Vector2Value.OnValueChange onInputChangeFunction;
        private Vector2 moveInputs;

        [SerializeField] Vector2Value turnInputs;
        private Vector2Value.OnValueChange onTurnInputChangeFunction;
        private Vector2 turnInputsValue;

        [SerializeField] GameObjectSO player;
        private OnDeath onDeathFunction;

        [SerializeField] MessageEvent bulletMessageEvent;
        [SerializeField] BoolValue playerWantsToShoot;
        private BoolValue.OnValueChange playerWantsToShootFunction;

        [SerializeField] CameraSO mainCam;
        private float cameraDiagonalLenght;

        [SerializeField] IntValue numberOfBulletsPerShoot;
        [SerializeField] IntValue coinsCount;
        private IntValue.OnValueChange onCoinsCountChange;

        [SerializeField] SceneTheme sceneTheme;
        [SerializeField] MessageEvent soundPlayMessage;

        private Coroutine attackCoroutine;

        #region Initialization
        private void Awake()
        {
            // Set the right values to the SO
            player.Value = gameObject;
            playerDeath.Value = false;
            numberOfBulletsPerShoot.Value = 1;
        }
        private new void Start()
        {
            // Initialize the entity
            base.Start();

            // Subscribe to events
            onInputChangeFunction = (Vector2 movementInput) => { moveInputs = movementInput; };
            inputs.onValueChange += onInputChangeFunction;

            onTurnInputChangeFunction = (Vector2 turnInput) => { turnInputsValue = turnInput; };
            turnInputs.onValueChange += onTurnInputChangeFunction;

            onDeathFunction = () => { playerDeath.Value = true; };
            onDeath += onDeathFunction;

            playerWantsToShootFunction = (bool value) => { CheckPlayerAttack(value); };
            playerWantsToShoot.onValueChange += playerWantsToShootFunction;

            onCoinsCountChange = (int value) => { coinText.text = value.ToString(); };
            coinsCount.onValueChange += onCoinsCountChange;

            // Get the components
            lineRenderer = GetComponent<LineRenderer>();
            coinText.text = "0";

            // Get values
            cameraDiagonalLenght = mainCam.Cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height) * 2f).magnitude;

            // Set the right values
            mainBodyRenderer.color = sceneTheme.mainTheme.PlayerTheme.MainBodyColor;
            shooterRenderer.color = sceneTheme.mainTheme.PlayerTheme.ShooterColor;
        }
        private new void OnDestroy()
        {
            // Destroy the entity
            base.OnDestroy();

            // Unsubscribe to events
            inputs.onValueChange -= onInputChangeFunction;
            turnInputs.onValueChange -= onTurnInputChangeFunction;
            onDeath -= onDeathFunction;
            playerWantsToShoot.onValueChange -= playerWantsToShootFunction;
            coinsCount.onValueChange -= onCoinsCountChange;
        }
        #endregion

        #region Movement
        private void FixedUpdate()
        {
            // The player shouldn't be able to move or turn if he is dead
            if(playerDeath.Value) 
            {
                lineRenderer.enabled = false;
                return; 
            }

            // Move the player
            base.Move(moveInputs);

            // Turn the player so that he looks at the right target
            Vector2 target = (Vector2)transform.position + turnInputsValue;
            base.TurnToward(shooter, target);

            // Modify the line renderer accordingly
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + ((Vector3)target - transform.position).normalized * cameraDiagonalLenght);
        }
        #endregion

        #region Attack
        /// <summary>
        /// Check if the player can shoot and shoot in this case.
        /// </summary>
        private void CheckPlayerAttack(bool wantsToShoot)
        {
            lineRenderer.enabled = !wantsToShoot;

            // If the player wants to attack, can actually attack and isn't dead yet
            if (wantsToShoot && CanAttack && !playerDeath.Value)
            {
                // Attack
                Attack();

                // Set the attack proceeded
                OnAttackProceed();
            }
        }

        public override void Attack()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            attackCoroutine = StartCoroutine(LaunchBullets());
        }

        public override void TakeDamage(int amount)
        {
            // Play the damage sound
            soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.TakeDamage);
            // Take damage
            base.TakeDamage(amount);
        }

        /// <summary>
        /// The player launches all the bullets he can.
        /// </summary>
        private IEnumerator LaunchBullets()
        {
            WaitForSeconds timeToWait = new WaitForSeconds(attackCooldown / (numberOfBulletsPerShoot.Value * timeScale.Value));

            // Launch as many bullets as the player can
            for (int i = 0; i < numberOfBulletsPerShoot.Value; i++)
            {
                // Play the shoot sound
                soundPlayMessage.SendMessage("Clip0" + (int)ClipIndex.Shoot);
                // Launch a bullet
                LaunchBullet();
                yield return timeToWait;
            }
        }

        /// <summary>
        /// This function is used by the player to launch bullets.
        /// </summary>
        private void LaunchBullet()
        {
            // Get the bullet values
            Vector2 direction = (shootPoint.transform.position - shooter.transform.position).normalized;
            BulletSpawnValues values = new BulletSpawnValues(shootPoint.transform.position, GameData.baseBulletScaleRatio * transform.localScale, direction, sceneTheme.mainTheme.PlayerTheme.BulletColor, attackDamage, isFriendly: true, empty: false);

            // Make the message
            string message = GameData.BulletValuesToMessage(values);

            // Send a bullet
            bulletMessageEvent.SendMessage(message);
        }
        #endregion
    }
}
