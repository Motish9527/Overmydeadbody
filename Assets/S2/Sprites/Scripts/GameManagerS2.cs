using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManagerS2 : MonoBehaviour
{
    public static GameManagerS2 Instance;
    
    // 屍體
    public GameObject deadBodyPrefab;
    public int maxDeadBodies = 5;
    public bool stageClear = false;
    
    // 通關
    public GameObject flagObject;
    public string playerTag = "Player";
    
    // UI
    public GameObject restartButton;
    public GameObject exitButton;
    
    // 玩家
    private Move player;
    private List<GameObject> deadBodies = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 隱藏按鈕
        if (restartButton != null)
            restartButton.SetActive(false);
        if (exitButton != null)
            exitButton.SetActive(false);
            
        player = FindFirstObjectByType<Move>();
        
        // 為 flag 物件設定碰撞檢測
        if (flagObject != null)
        {
            var flagScript = flagObject.GetComponent<FlagS2>();
            if (flagScript == null)
            {
                flagScript = flagObject.AddComponent<FlagS2>();
            }
        }
    }
    
    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
            if (Keyboard.current.escapeKey.wasPressedThisFrame) ExitGame();
        }
    }

    // Stage Clear
    public void OnStageClear()
    {
        if (stageClear) return;
        stageClear = true;
        Debug.Log("Stage Clear! 關卡完成！");
        
        // 顯示按鈕
        if (restartButton != null)
            restartButton.SetActive(true);
        if (exitButton != null)
            exitButton.SetActive(true);
    }

    // Restart (切換回 Stage1)
    public void RestartGame()
    {
        SceneManager.LoadScene("Stage1");
    }

    // Exit (供按鈕調用)
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
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
}
