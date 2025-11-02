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
    private bool hasLaunched = false;
    private bool canDetect = false;
    private bool hasShot = false;
    private float launchTime;
    
    void Start()
    {
        initialPosition = transform.position;
        currentStartPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (GameManager.Instance != null) GameManager.Instance.RegisterBullet(this);
    }
    
    void Update()
    {
        if (hasLaunched)
        {
            transform.position += Vector3.left * bulletSpeed * Time.deltaTime;
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
    }
    
    // 子彈發射完後，藏到牆裡下一輪再出來
    void ReturnAndShift()
    {
        hasLaunched = false;
        hasShot = true;
        currentStartPosition += Vector3.right * rightShift;
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
        currentStartPosition = initialPosition;
        transform.position = initialPosition;
    }
    
    public void StartDetection()
    {
        canDetect = true;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
