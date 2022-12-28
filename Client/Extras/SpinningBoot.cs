using UnityEngine;

namespace AOClient.Extras
{
    public sealed class SpinningBoot : MonoBehaviour
    {
        [Min(0.1f)] 
        [SerializeField] 
        private float movementSpeed = 5f;

        [Min(0.1f)] 
        [SerializeField] 
        private float rotationSpeed = 80f;

        [SerializeField] 
        private Vector2 finalPosition = new(3, 2);

        private Vector3 startingPosition;

        private void Start()
        {
            startingPosition = transform.position;
        }

        private void Update()
        {
            if ((Vector2)transform.position != finalPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, finalPosition, movementSpeed * Time.deltaTime);
                Quaternion originalRotation = transform.rotation;
                transform.rotation = originalRotation * Quaternion.AngleAxis(10 * rotationSpeed * Time.deltaTime, Vector3.forward);
            }
            else
            {
                transform.position = startingPosition;
            }
        }
    }
}