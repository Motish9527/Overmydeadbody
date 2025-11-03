using System.Collections;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float detectionRange = 10f;
    public float bulletSpeed = 8f;
    public float bulletLifetime = 5f;
    public float rightShift = 5f;
    
    private Vector3 initialPosition;
    private Vector3 currentStartPosition;
    private Transform player;
    private Rigidbody2D rb;
    private bool hasLaunched = false;
    private bool canDetect = false;
    private bool hasShot = false;
    private float launchTime;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        initialPosition = transform.position;
        currentStartPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (GameManager.Instance != null) GameManager.Instance.RegisterBullet(this);
    }
    
    void Update()
    {
        if (hasLaunched)
        {
            rb.linearVelocity = Vector2.left * bulletSpeed;
            if (Time.time - launchTime >= bulletLifetime) ReturnAndShift();
        }
        else if (canDetect && !hasShot && player != null)
        {
            if (Vector2.Distance(transform.position, player.position) <= detectionRange) Launch();
        }
    }

    void Launch()
    {
        hasLaunched = true;
        launchTime = Time.time;
        if (GameManager.Instance != null) GameManager.Instance.PlayBulletSfx();
    }
    
    void ReturnAndShift()
    {
        hasLaunched = false;
        hasShot = true;
        rb.linearVelocity = Vector2.zero;
        currentStartPosition += Vector3.right * rightShift;   // 把子彈藏起來
        transform.position = currentStartPosition;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLaunched) ReturnAndShift();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasLaunched) ReturnAndShift();
    }
    
    public void ResetBullet()
    {
        hasLaunched = false;
        hasShot = false;
        canDetect = false;
        rb.linearVelocity = Vector2.zero;
        currentStartPosition = initialPosition;
        transform.position = initialPosition;
    }
    
    public void StartDetection()
    {
        canDetect = true;
    }
}
