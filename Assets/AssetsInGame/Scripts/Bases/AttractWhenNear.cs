using UnityEngine;
using Slacken.Bases.SO;

namespace Slacken.Bases
{
    [RequireComponent(typeof(Collider2D))]
    public class AttractWhenNear : MonoBehaviour
    {
        [SerializeField] FloatValue timeScale;
        [SerializeField] Transform whatIsAttracted;
        [SerializeField] LayerMask whatAttracts;
        [SerializeField] [Range(0f, 1f)] float attractForce;

        [HideInInspector] public bool IsAttracted { get => target != null; }
        private Transform target;
        private Vector3 velocity;

        private void OnEnable()
        {
            // Reset the target
            target = null;
        }

        private void FixedUpdate()
        {
            // Go toward the target, if there is any
            if (target != null)
            {
                // Make the object be attracted to the target
                whatIsAttracted.position = Vector3.SmoothDamp(whatIsAttracted.position, target.position, ref velocity, attractForce, float.MaxValue, timeScale.Value * Time.fixedDeltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // If the object can attract us
            bool isAttractor = whatAttracts == (whatAttracts | (1 << collision.gameObject.layer));
            if (isAttractor)
            {
                // The object can only have one target
                if(target == null)
                {
                    // So assign it, only if there is no targets yet
                    target = collision.gameObject.transform;
                }
            }
        }
    }
}
