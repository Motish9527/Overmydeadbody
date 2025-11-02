using UnityEngine;

public class DeadBody : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(transform);  // 玩家踩上 deadbody 時也變成他的子物件一起移動
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) collision.transform.SetParent(null);
    }
}
