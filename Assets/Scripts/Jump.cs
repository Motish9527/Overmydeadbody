using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    public Sprite[] animationSprites;
    public float frameRate = 0.1f;
    public float bounceForce = 15f;
    public float highJumpForce = 25f;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D playerRb;
    private Sprite originalSprite;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = spriteRenderer.sprite;
    }

    void Update()
    {
        if (playerRb != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, highJumpForce);
            StartCoroutine(PlayBounceAnimation());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null) playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                StartCoroutine(PlayBounceAnimation());
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) playerRb = null;
    }

    IEnumerator PlayBounceAnimation()
    {
        if (animationSprites != null && animationSprites.Length > 0)
        {
            foreach (Sprite frame in animationSprites)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(frameRate);
            }
        }
        spriteRenderer.sprite = originalSprite;
    }
}

