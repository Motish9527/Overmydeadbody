using System.Collections;
using UnityEngine;

public class Rockhead : MonoBehaviour
{
    public float detectionRange = 4.8f;
    public float fallSpeed = 10f;
    public float riseSpeed = 5f;
    public float minWaitTime = 0.35f;
    public float maxWaitTime = 1.5f;
    public float resetDelay = 1.5f;
    
    private Vector3 startPosition;
    private bool isFalling = false;
    private bool isResetting = false;
    private bool isTriggered = false;
    private bool canDetect = false;
    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        if (GameManager.Instance != null) GameManager.Instance.RegisterRockhead(this);
    }

    void Update()
    {
        if (isResetting || !canDetect) return;

        if (isTriggered)
        {
            if (!isFalling && !isResetting) StartCoroutine(FallAfterDelay());
            return;
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            isTriggered = true;
            StartCoroutine(FallAfterDelay());
        }
    }

    IEnumerator FallAfterDelay()
    {
        isFalling = true;
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
        
        if (GameManager.Instance != null && IsInCameraView()) GameManager.Instance.PlayRockSfx();
    }
    
    bool IsInCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;
        
        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isFalling)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    Player playerScript = collision.gameObject.GetComponent<Player>();
                    if (playerScript != null) playerScript.StartCoroutine("Die");
                    break;
                }
            }
        }
        
        if (!collision.gameObject.CompareTag("Player"))
        {
            isFalling = false;
            StartCoroutine(ResetPosition());
        }
    }

    IEnumerator ResetPosition()
    {
        if (isResetting) yield break;
        
        isResetting = true;
        isFalling = false;
        yield return new WaitForSeconds(resetDelay);
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        
        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, riseSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = startPosition;
        isFalling = false;
        isResetting = false;
    }

    public void ResetToStart()
    {
        StopAllCoroutines();
        isFalling = false;
        isResetting = false;
        isTriggered = false;
        canDetect = false;
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        
        transform.position = startPosition;
    }
    
    public void StartDetection()
    {
        canDetect = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
