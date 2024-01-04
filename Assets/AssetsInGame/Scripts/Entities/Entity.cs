using UnityEngine;
using UnityEngine.UI;
using Slacken.Bases.SO;

namespace Slacken.Entities
{
    public abstract class Entity : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] int maxHealth;
        int health;
        protected int Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                onHealthChange?.Invoke(maxHealth, health);

                if(health <= 0)
                {
                    onDeath?.Invoke();
                }
            }
        }
        public delegate void OnHealthChange(int maxHealth, int health);
        public event OnHealthChange onHealthChange;

        protected int numberOfCoins;
        private Rigidbody2D rb;

        [SerializeField] protected FloatValue timeScale;
        [SerializeField] protected Text coinText;
        [SerializeField] int startCoinsCount;

        [Header("Movement")]
        [SerializeField] float maxSpeed = 25f;
        [SerializeField] float maxAcceleration;
        [SerializeField] float friction;
        [SerializeField] float moveSpeed;
        private Vector2 velocity;

        [Header("Attack")]
        [SerializeField] protected int attackDamage;
        [SerializeField] protected float attackCooldown;
        private float attackTimeLeft;

        protected bool CanAttack { get { return attackTimeLeft <= 0f; } }

        [Header("Death")]
        [SerializeField] protected BoolValue playerDeath;
        public delegate void OnDeath();
        public event OnDeath onDeath;

        #region Initialization
        /// <summary>
        /// The start of the entity. Must be implemented in all entities in order for them to be correctly initialized.
        /// </summary>
        public virtual void Start()
        {
            onDeath += Die;

            // If we have a rigidbody, get it
            if(TryGetComponent(out Rigidbody2D rigidBody2D))
            {
                rb = rigidBody2D;
                rb.velocity = Vector2.zero;
            }
        }
        /// <summary>
        /// The end of the entity. Must be implemented in all entities in order for them to be correctly destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            onDeath -= Die;
        }

        /// <summary>
        /// Whenever the entity is revived, it will trigger that function. Reset the values of the entity so that it can fully function.
        /// </summary>
        public virtual void OnEnable()
        {
            Health = maxHealth;
            numberOfCoins = startCoinsCount;
            coinText.text = numberOfCoins.ToString();
        }
        #endregion

        #region Entity Movement
        /// <summary>
        /// This function is used by entities so that they can turn toward a location.
        /// </summary>
        /// <param name="thingToRotate">The body which will rotate.</param>
        /// <param name="target">The target location.</param>
        public void TurnToward(Transform thingToRotate, Vector2 target)
        {
            // Get the direction
            Vector2 direction = (target - (Vector2)transform.position).normalized;
            // Calculate the angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Rotate the entity
            thingToRotate.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        /// <summary>
        /// This function is used by entities so that they can move around.
        /// </summary>
        /// <param name="moveInput">The input given by the entity.</param>
        public void Move(Vector2 moveInput)
        {
            // Get the input depending on the direction the entity is looking at
            moveInput = transform.TransformDirection(moveInput);
            moveInput.Normalize();

            // Get the wished speed
            float wishSpeed = moveInput.magnitude;
            float currentSpeed = Mathf.Clamp(moveSpeed * timeScale.Value, 0f, maxSpeed);
            float timeMultiplier = (currentSpeed / moveSpeed);
            wishSpeed *= currentSpeed;

            // Accelerate the movement
            Vector2 acceleration = Accelerate(moveInput, wishSpeed, maxAcceleration);

            // If there were no acceleration
            if (acceleration.magnitude < (GameData.minVelocity * timeMultiplier))
            {
                // Decrease the velocity
                velocity -= velocity.normalized * friction * timeMultiplier * Time.fixedDeltaTime;

                // If the velocity is too low
                if (velocity.sqrMagnitude < (GameData.minVelocity * GameData.minVelocity))
                {
                    // Set that it doesn't exist anymore
                    velocity = Vector2.zero;
                }
            }
            else
            {
                // Add the acceleration to the velocity
                velocity += acceleration;
            }

            // Move the entity
            transform.position += (Vector3)velocity * Time.fixedDeltaTime;

            // Since we are making our own movement system, we should prevent the rigidbody from pushing the entity on wrong directions
            if (rb != null) { rb.velocity = Vector2.zero; }
        }

        /// <summary>
        /// Accelerate a vector.
        /// </summary>
        /// <param name="wishDir">The direction the entity wishes to go.</param>
        /// <param name="wishSpeed">The speed the entity wishes to go at.</param>
        /// <param name="accel">The current acceleration.</param>
        private Vector2 Accelerate(Vector2 wishDir, float wishSpeed, float accel)
        {
            // Get the current speed
            float currentSpeed = velocity.magnitude;
            // And then the speed we need to add in order to reach this speed
            float addSpeed = wishSpeed - currentSpeed;

            // If the addSpeed is negative, it means that the current velocity is enough to be at the speed we need.
            if (addSpeed <= 0) { return Vector2.zero; }

            // Get the accelerated speed
            float accelSpeed = accel * wishSpeed * Time.fixedDeltaTime;
            // Clamp it so that it isn't greater than the speed we need to add
            if (accelSpeed > addSpeed)
            {
                accelSpeed = addSpeed;
            }

            // Return the acceleration vector
            return wishDir * accelSpeed;
        }
        #endregion

        #region Entity Behaviour
        public virtual void Update()
        {
            // If there is still time left before being able to attack
            if (attackTimeLeft > 0f)
            {
                // Decrease the cooldown
                attackTimeLeft -= Time.deltaTime * timeScale.Value;

                // Clamp the attack time left
                if(attackTimeLeft <= 0f)
                {
                    attackTimeLeft = 0f;
                }
            }
        }

        /// <summary>
        /// Make the entity take damage.
        /// </summary>
        /// <param name="amount">The amount of damage received.</param>
        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
        }

        /// <summary>
        /// This function lets entities attack others.
        /// </summary>
        public virtual void Attack() { }
        
        /// <summary>
        /// This function should be called everytime an attack is proceed. It activates the attack cooldown.
        /// </summary>
        protected void OnAttackProceed()
        {
            attackTimeLeft = attackCooldown;
        }

        /// <summary>
        /// This function is triggered when the entity dies.
        /// </summary>
        public virtual void Die() { }
        #endregion

        #region Entity Special Functions
        /// <summary>
        /// This function triggers the health change function (without modifying the health) so that other things may update (like the UI for instance).
        /// </summary>
        public void TriggerHealthChange()
        {
            onHealthChange?.Invoke(maxHealth, Health);
        }

        /// <summary>
        /// Add other coins to the current number of coins the entity has.
        /// </summary>
        /// <param name="count">The number of coins to give.</param>
        public void AddNewCoins(int count)
        {
            numberOfCoins += count;
            coinText.text = numberOfCoins.ToString();
        }

        /// <summary>
        /// Regenerate a certain <paramref name="amount"/> of health to the entity.
        /// </summary>
        /// <param name="amount">The amount of health given.</param>
        public void AddHealth(int amount)
        {
            Health += amount;
        }
        #endregion
    }
}
