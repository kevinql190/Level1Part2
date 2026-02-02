using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public GameObject wholeObject;
    public GameObject brokenObject;
    public Rigidbody wholeRb;

    [Header("Fall Settings")]
    public Vector3 fallDirection = Vector3.forward;
    public float fallForce = 300f;

    [Header("Break Settings")]
    public float breakAfterSeconds = 1f; // Backup timer - break after this many seconds even if no ground hit
    public Vector3 slideDirection = Vector3.forward;
    public float slideSpeed = 3f;

    private bool hasBeenHit = false;
    private bool hasBroken = false;
    private float hitTime = 0f;

    void Start()
    {
        if (wholeRb == null)
        {
            Debug.LogError("ERROR: Whole Rb is not assigned!");
            return;
        }

        wholeRb.isKinematic = false;
        wholeRb.constraints = RigidbodyConstraints.FreezeAll;
        wholeRb.useGravity = true;

        if (brokenObject != null)
        {
            brokenObject.SetActive(false);
        }

        Debug.Log("Ready - will break when hitting ground OR after timer");
    }

    void Update()
    {
        // Backup timer - break after X seconds even if ground collision not detected
        if (hasBeenHit && !hasBroken && Time.time - hitTime >= breakAfterSeconds)
        {
            Debug.Log("### TIMER REACHED - BREAKING NOW!");
            BreakApart();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("!!! COLLISION with: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");

        // Hit by sphere - start falling
        if (!hasBeenHit && collision.gameObject.CompareTag("Breaker"))
        {
            Debug.Log(">>> HIT BY SPHERE - FALLING!");
            hasBeenHit = true;
            hitTime = Time.time;

            wholeRb.constraints = RigidbodyConstraints.None;
            wholeRb.useGravity = true;
            wholeRb.AddForce(fallDirection.normalized * fallForce + Vector3.down * 100f);
        }

        // Hit ground AFTER being hit - BREAK NOW
        if (hasBeenHit && !hasBroken && collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("### HIT GROUND - BREAKING NOW!");
            BreakApart();
        }
        else if (hasBeenHit && !hasBroken)
        {
            Debug.Log("--- Hit something but tag is '" + collision.gameObject.tag + "' not 'Ground'");
        }
    }

    void BreakApart()
    {
        Debug.Log("========== BREAKING INTO PIECES ==========");
        hasBroken = true;

        wholeObject.SetActive(false);
        brokenObject.SetActive(true);

        Collider[] pieceColliders = brokenObject.GetComponentsInChildren<Collider>();
        Rigidbody[] pieces = brokenObject.GetComponentsInChildren<Rigidbody>();

        Debug.Log("Found " + pieces.Length + " pieces to scatter");

        // Disable collisions between pieces
        for (int i = 0; i < pieceColliders.Length; i++)
        {
            for (int j = i + 1; j < pieceColliders.Length; j++)
            {
                Physics.IgnoreCollision(pieceColliders[i], pieceColliders[j], true);
            }
        }

        // Setup each piece
        foreach (Rigidbody rb in pieces)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.mass = 10f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;

            // Zero out
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Slide horizontally
            Vector3 slideVelocity = slideDirection.normalized * slideSpeed;
            slideVelocity.y = -1f;
            rb.linearVelocity = slideVelocity;

            // Tumble forward
            Vector3 dominoTumble = Vector3.Cross(Vector3.up, slideDirection.normalized) * 2f;
            rb.angularVelocity = dominoTumble;
        }

        Debug.Log("========== PIECES NOW SLIDING ==========");
    }
}