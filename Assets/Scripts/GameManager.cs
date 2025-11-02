using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;
    
    // 屍體
    public GameObject deadBodyPrefab;
    public int maxDeadBodies = 5;
    public bool stageClear = false;
    
    // 音樂音效
    [Header("Audio")]
    public AudioClip themeMusic;
    public AudioClip gameClearMusic;
    public AudioClip successSfx;
    public AudioClip jumpSfx;
    public AudioClip[] deadSfxList;
    public AudioClip[] boingSfxList;
    public AudioClip bigJumpSfx;
    public AudioClip rockSfx;
    public AudioClip fireSfx;
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    // Game Objects
    private List<GameObject> deadBodies = new List<GameObject>(); 
    private List<Rockhead> rockheads = new List<Rockhead>();
    private List<bullet> bullets = new List<bullet>();
    private Player player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            musicSource = sources[0];
            sfxSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            musicSource = sources[0];
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        
        sfxSource.loop = false;
    }

    void Start()
    {
        RegisterAllRockheads();
        RegisterAllBullets();

        StartAllRockheads();
        StartAllBullets();
        player = FindFirstObjectByType<Player>();
        PlayThemeMusic();
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
    }

    // Stage Clear
    public void OnStageClear()
    {
        if (stageClear) return;
        stageClear = true;
        Debug.Log("Stage Clear! 關卡完成！");
        StartCoroutine(PlaySuccessSequence());
    }
    
    IEnumerator PlaySuccessSequence()
    {
        if (sfxSource != null && successSfx != null)
        {
            sfxSource.PlayOneShot(successSfx);
            yield return new WaitForSeconds(successSfx.length);
        }
        if (musicSource != null && gameClearMusic != null)
        {
            musicSource.Stop();
            musicSource.loop = false;
            musicSource.clip = gameClearMusic;
            musicSource.Play();
            yield return new WaitForSeconds(gameClearMusic.length);
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Restart
    public void RestartGame()
    {
        foreach (GameObject deadBody in deadBodies)
        {
            if (deadBody != null) Destroy(deadBody);
        }
        deadBodies.Clear();
        
        if (player != null)
        {
            player.transform.position = player.startPosition;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        
        ResetAllRockheads();
        StartAllRockheads();
        ResetAllBullets();
        StartAllBullets();
        stageClear = false;
        PlayThemeMusic();
        
        Debug.Log("Game Restarted! 遊戲已重新開始！");
    }

    // Rockhead
    public void RegisterRockhead(Rockhead rockhead)
    {
        if (!rockheads.Contains(rockhead)) rockheads.Add(rockhead);
    }

    private void RegisterAllRockheads()
    {
        foreach (Rockhead rockhead in FindObjectsByType<Rockhead>(FindObjectsSortMode.None))
        {
            RegisterRockhead(rockhead);
        }
    }
    
    private void StartAllRockheads()
    {
        foreach (Rockhead rockhead in rockheads)
        {
            if (rockhead != null) rockhead.StartDetection();
        }
    }
    
    private void ResetAllRockheads()
    {
        foreach (Rockhead rockhead in rockheads)
        {
            if (rockhead != null) rockhead.ResetToStart();
        }
    }
    
    // Bullet
    public void RegisterBullet(bullet bulletObj)
    {
        if (!bullets.Contains(bulletObj)) bullets.Add(bulletObj);
    }
    
    private void RegisterAllBullets()
    {
        foreach (bullet bulletObj in FindObjectsByType<bullet>(FindObjectsSortMode.None))
        {
            RegisterBullet(bulletObj);
        }
    }
    
    private void StartAllBullets()
    {
        foreach (bullet bulletObj in bullets)
        {
            if (bulletObj != null) bulletObj.StartDetection();
        }
    }
    
    private void ResetAllBullets()
    {
        foreach (bullet bulletObj in bullets)
        {
            if (bulletObj != null) bulletObj.ResetBullet();
        }
    }

    // Deadbody
    public void SpawnDeadBody(Vector3 position)
    {
        if (deadBodyPrefab == null) return;

        GameObject deadBody = Instantiate(deadBodyPrefab, position, Quaternion.identity);
        deadBodies.Add(deadBody);

        if (deadBodies.Count > maxDeadBodies)
        {
            Destroy(deadBodies[0]);
            deadBodies.RemoveAt(0);
        }
    }
    
    // Sounds
    private void PlayThemeMusic()
    {
        if (musicSource != null && themeMusic != null)
        {
            musicSource.clip = themeMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    
    private void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip, volume);
    }
    
    private void PlayRandomSfx(AudioClip[] clips, float volume = 1f)
    {
        if (sfxSource != null && clips != null && clips.Length > 0)
        {
            sfxSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }

    public void PlayJumpSfx() => PlaySfx(jumpSfx, 0.5f);
    public void PlayBoingSfx() => PlayRandomSfx(boingSfxList, 1.5f);
    public void PlayBigJumpSfx() => PlaySfx(bigJumpSfx);
    public void PlayRockSfx() => PlaySfx(rockSfx);
    
     public IEnumerator OnPlayerDeath(Vector3 deathPosition, Player player)
    {
        PlayRandomSfx(deadSfxList, 1.5f);
        SpawnDeadBody(deathPosition);
        ResetAllRockheads();
        ResetAllBullets();
        yield return StartCoroutine(player.RespawnAnimation());
        StartAllRockheads();
        StartAllBullets();
    }
}
