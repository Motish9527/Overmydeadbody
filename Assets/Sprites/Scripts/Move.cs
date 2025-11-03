using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(AudioSource))]
public class Move : MonoBehaviour
{
    [Header("Move & Jump")]
    public float moveSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Death / Respawn")]
    public GameObject DeadbodyPrefab;
    public Transform spawnPoint;
    public string hazardTag = "Hazard";

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite walkingSideSprite;
    public bool flipByFacing = true;

    [Header("SFX")]
    public AudioClip jumpClip;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    public AudioClip deathImpactClip;                 
    [Range(0f, 1f)] public float deathImpactVolume = 1f;

    [Header("Death Flash")]
    public float deathFlashDuration = 2f;             
    public float flashHz = 6f;                         
    [Range(0f, 1f)] public float minAlpha = 0.2f;       

    Rigidbody2D rb;
    SpriteRenderer sr;
    AudioSource sfx;

    bool isDying = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (!groundCheck) groundCheck = transform.Find("groundCheck");

        sr = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f; 

        if (idleSprite) sr.sprite = idleSprite;
    }

    void Update()
    {
        if (isDying) return; // 死亡流程中鎖操作

        //移動
        float x = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);

        // 翻面
        if (Mathf.Abs(x) > 0.01f)
        {
            if (walkingSideSprite && sr.sprite != walkingSideSprite)
                sr.sprite = walkingSideSprite;
            if (flipByFacing) sr.flipX = x < 0f;
        }
        else
        {
            if (idleSprite && sr.sprite != idleSprite)
                sr.sprite = idleSprite;
        }

        //跳躍
        if (!groundCheck) return;
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);


        if (grounded && Input.GetKeyDown(KeyCode.W))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            if (jumpClip) sfx.PlayOneShot(jumpClip, jumpVolume);
        }
    }

    // 兩種碰撞皆支援
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(hazardTag)) TryDie();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(hazardTag)) TryDie();
    }

    void TryDie()
    {
        if (isDying) return;
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        isDying = true;

        // 1) 播放死亡撞擊音效（只播一次）
        if (deathImpactClip) sfx.PlayOneShot(deathImpactClip, deathImpactVolume);

        // 2) 記錄死亡位置、停止速度、暫停碰撞避免重複觸發
        Vector3 deathPos = transform.position;
        rb.velocity = Vector2.zero;

        var col = GetComponent<Collider2D>();
        bool oldColEnabled = true;
        if (col) { oldColEnabled = col.enabled; col.enabled = false; }

        // 3) 閃爍（透明度在 1 與 minAlpha 間來回）
        float t = 0f;
        Color baseColor = sr.color;
        while (t < deathFlashDuration)
        {
            t += Time.deltaTime;
            float phase = Mathf.PingPong(t * flashHz, 1f);
            float a = Mathf.Lerp(minAlpha, 1f, phase);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
        // 還原不透明
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        // 4) 生成屍體在死亡點
        if (DeadbodyPrefab)
            Instantiate(DeadbodyPrefab, deathPos, Quaternion.identity);

        // 5) 回到重生點
        transform.position = (spawnPoint ? spawnPoint.position : Vector3.zero);
        rb.velocity = Vector2.zero;

        // 6) 還原碰撞
        if (col) col.enabled = oldColEnabled;

        // 7) 重置外觀
        if (idleSprite) sr.sprite = idleSprite;
        if (flipByFacing) sr.flipX = false;

        isDying = false;
    }

  
}
