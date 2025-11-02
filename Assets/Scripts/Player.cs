using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public Vector3 startPosition;
    public Sprite idleSprite; 
    public Sprite walkSprite;
    public GameObject completeObject; // 過關標誌物件
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    void Update()
    {
        if (isDying) return; 
        
        // 移動
        float moveInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput != 0)
        {
            if (walkSprite != null) spriteRenderer.sprite = walkSprite;
            spriteRenderer.flipX = moveInput > 0;
        }
        else if (idleSprite != null) spriteRenderer.sprite = idleSprite;
    }

    // 碰撞
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") && !isDying) StartCoroutine(Die("Trap"));
        if (collision.gameObject.CompareTag("Bullet") && !isDying) StartCoroutine(Die("Bullet"));

        if (completeObject != null && collision.gameObject == completeObject && GameManager.Instance != null)
        {
            GameManager.Instance.OnStageClear();
        }
    }

    IEnumerator Die(string deathCause = "")
    {
        isDying = true;
        Vector3 deathPosition = transform.position;
        rb.linearVelocity = Vector2.zero;
        
        if (GameManager.Instance != null)
        {
            yield return StartCoroutine(GameManager.Instance.OnPlayerDeath(deathPosition, this, deathCause));
        }
        
        isDying = false;
    }
    
    public IEnumerator RespawnAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
    }
}