using UnityEngine;

public class Fruit : MonoBehaviour
{
    public GameObject whole;
    public GameObject sliced;

    private Rigidbody fruitRigidbody;
    private Collider fruitCollider;
    private ParticleSystem juiceEffect;
    private float previousYPosition;

    public int points = 1;
    private bool isSliced = false;  // Flag to check if the fruit has been sliced

    private void Awake()
    {
        fruitRigidbody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        juiceEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void Slice(Vector3 direction, Vector3 position, float force)
    {
        // Kiểm tra xem trái cây có phải là đầu tiên trong Queue không
        if (GameManager.Instance.FruitQueue.Peek() == gameObject)
        {
            GameManager.Instance.IncreaseScore(points);

            // Xử lý như cũ
            fruitCollider.enabled = false;
            whole.SetActive(false);
            sliced.SetActive(true);
            juiceEffect.Play();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody slice in slices)
            {
                slice.velocity = fruitRigidbody.velocity;
                slice.AddForceAtPosition(direction * force, position, ForceMode.Impulse);
            }

            // Xóa khỏi Queue sau khi đã chém
            GameManager.Instance.FruitQueue.Dequeue();

            if (!isSliced)  // Check if the fruit hasn't already been sliced
            {
                isSliced = true;
                // Add your slicing logic here (e.g., play animations, effects)
                Debug.Log("Fruit sliced!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Slice(blade.Direction, blade.transform.position, blade.sliceForce);
        }
    }

    private void Start()
    {
        // Initialize the previous Y position at the start
        previousYPosition = transform.position.y;
    }

    private void Update()
    {
        // If the fruit is already sliced, no need to check for falling
        if (isSliced)
            return;

        // Get the current position of the fruit
        float currentYPosition = transform.position.y;

        // Check if the fruit is falling (moving down)
        if (currentYPosition < previousYPosition)
        {
            // Convert the fruit's current world position to viewport space
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);

            // Check if the fruit is below the screen (y < 0)
            if (viewportPosition.y < 0)
            {
                Debug.Log("A fruit fell below the screen. Game Over.");
                GameManager.Instance.Explode(); // Trigger game over
                Destroy(gameObject); // Clean up the fruit
            }
        }

        // Update the previous Y position for the next frame
        previousYPosition = currentYPosition;
    }
}
