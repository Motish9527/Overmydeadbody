using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;
    public GameObject deadBodyPrefab;
    public int maxDeadBodies = 5;
    public bool stageClear = false;
    private List<GameObject> deadBodies = new List<GameObject>(); 
    private List<Rockhead> rockheads = new List<Rockhead>();
    private List<bullet> bullets = new List<bullet>(); 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RegisterAllRockheads();
        RegisterAllBullets();
        StartAllRockheads();
        StartAllBullets();
    }

    public void OnStageClear()
    {
        stageClear = true;
        Debug.Log("Stage Clear! 關卡完成！");
    }

    public IEnumerator OnPlayerDeath(Vector3 deathPosition, Player player, string deathCause = "")
    {
        SpawnDeadBody(deathPosition, deathCause);
        ResetAllRockheads();
        ResetAllBullets();
        yield return StartCoroutine(player.RespawnAnimation());
        StartAllRockheads();
        StartAllBullets();
    }

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

    public void SpawnDeadBody(Vector3 position, string deathCause = "")
    {
        if (deadBodyPrefab == null) return;

        GameObject deadBody = Instantiate(deadBodyPrefab, position, Quaternion.identity);
        
        // 中彈死亡的 deadbody 可 x 軸移動
        if (deathCause == "Bullet")
        {
            Rigidbody2D rb = deadBody.GetComponent<Rigidbody2D>();
            if (rb != null) rb.constraints = RigidbodyConstraints2D.None;
        }
        
        deadBodies.Add(deadBody);
        
        if (deadBodies.Count > maxDeadBodies)
        {
            Destroy(deadBodies[0]);
            deadBodies.RemoveAt(0);
        }
    }
}
