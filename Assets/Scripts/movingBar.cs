using UnityEngine;

public class movingBar : MonoBehaviour
{
    public float moveRange = 3f;
    public float moveSpeed = 3f;
    
    private Vector3 startPosition;
    private float leftBound;
    private float rightBound;
    private bool movingRight = true;

    void Start()
    {
        startPosition = transform.position;
        leftBound = startPosition.x - moveRange;
        rightBound = startPosition.x + moveRange;
    }

    void Update()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            if (transform.position.x >= rightBound) movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
            if (transform.position.x <= leftBound) movingRight = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("DeadBody"))
        {
            collision.transform.SetParent(transform);   // 上平台後變為子物件跟隨平台移動
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("DeadBody"))
        {
            collision.transform.SetParent(null);
        }
    }
}
