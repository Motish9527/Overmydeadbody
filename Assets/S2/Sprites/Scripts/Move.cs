using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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
        if (isDying) return; // 死亡流程中不操作

        // 移動 (支援 A/D 和 方向鍵)
        float x = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        }
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // 翻轉
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

        // 跳躍 (支援 W/上方向鍵/空白鍵)
        if (!groundCheck) return;
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (grounded && Keyboard.current != null)
        {
            bool jumpPressed = Keyboard.current.wKey.wasPressedThisFrame ||
                             Keyboard.current.upArrowKey.wasPressedThisFrame ||
                             Keyboard.current.spaceKey.wasPressedThisFrame;
            
            if (jumpPressed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (jumpClip) sfx.PlayOneShot(jumpClip, jumpVolume);
            }
        }
    }

    // ��ظI���Ҥ䴩
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

        // 1) 播放死亡音效
        if (deathImpactClip) sfx.PlayOneShot(deathImpactClip, deathImpactVolume);

        // 2) 記錄死亡位置並停止速度、暫時關閉碰撞
        Vector3 deathPos = transform.position;
        rb.linearVelocity = Vector2.zero;

        var col = GetComponent<Collider2D>();
        bool oldColEnabled = true;
        if (col) { oldColEnabled = col.enabled; col.enabled = false; }

        // 3) 閃爍效果
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
        // 恢復不透明
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        // 4) 通知 GameManager 生成屍體
        if (GameManagerS2.Instance != null)
            GameManagerS2.Instance.SpawnDeadBody(deathPos);

        // 5) 回到重生點
        transform.position = (spawnPoint ? spawnPoint.position : Vector3.zero);
        rb.linearVelocity = Vector2.zero;

        // 6) 恢復碰撞
        if (col) col.enabled = oldColEnabled;

        // 7) 重置外觀
        if (idleSprite) sr.sprite = idleSprite;
        if (flipByFacing) sr.flipX = false;

        isDying = false;
    }

  
}
