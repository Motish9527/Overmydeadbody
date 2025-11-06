using UnityEngine;

public class FlagS2 : MonoBehaviour
{
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (GameManagerS2.Instance != null)
            {
                GameManagerS2.Instance.OnStageClear();
            }
        }
    }
}
