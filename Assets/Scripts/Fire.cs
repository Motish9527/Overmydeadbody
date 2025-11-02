using System.Collections;
using UnityEngine;

public class fire : MonoBehaviour
{
    public Sprite[] animationSprites;
    public float frameRate = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            if (animationSprites == null || animationSprites.Length == 0) yield break;
            
            spriteRenderer.sprite = animationSprites[currentFrame];
            currentFrame = (currentFrame + 1) % animationSprites.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}
