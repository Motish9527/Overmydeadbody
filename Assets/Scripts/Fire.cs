using System.Collections;
using UnityEngine;

public class fire : MonoBehaviour
{
    public Sprite[] animationSprites;
    public float frameRate = 0.1f;
    public float audioVolume = 0.3f;
    public float stopDelay = 0.5f;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private AudioSource audioSource;
    private bool isPlayingSound = false;
    private Coroutine stopSoundCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = audioVolume;
        StartCoroutine(PlayAnimation());
    }
    
    void Update()
    {
        if (IsInCameraView())
        {
            if (stopSoundCoroutine != null)
            {
                StopCoroutine(stopSoundCoroutine);
                stopSoundCoroutine = null;
            }
            if (!isPlayingSound && GameManager.Instance != null && GameManager.Instance.fireSfx != null)
            {
                audioSource.clip = GameManager.Instance.fireSfx;
                audioSource.Play();
                isPlayingSound = true;
            }
        }
        else if (isPlayingSound && stopSoundCoroutine == null)
        {
            stopSoundCoroutine = StartCoroutine(StopSoundAfterDelay());
        }
    }
    
    IEnumerator StopSoundAfterDelay()
    {
        yield return new WaitForSeconds(stopDelay);
        audioSource.Stop();
        isPlayingSound = false;
        stopSoundCoroutine = null;
    }
    
    bool IsInCameraView()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;
        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
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
