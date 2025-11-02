using System.Collections;
using UnityEngine;

public class barUp : MonoBehaviour
{
    public float moveRange = 3f;
    public float moveSpeed = 3f;
    public float pauseTime = 1f;
    
    private Vector3 startPosition;
    private float topBound;
    private float bottomBound;
    private bool movingUp = true;
    private bool isPaused = false;

    void Start()
    {
        startPosition = transform.position;
        topBound = startPosition.y + moveRange;
        bottomBound = startPosition.y - moveRange;
    }

    void Update()
    {
        if (isPaused) return;

        if (movingUp)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            if (transform.position.y >= topBound)
            {
                movingUp = false;
                StartCoroutine(PauseAtBound());
            }
        }
        else
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            if (transform.position.y <= bottomBound)
            {
                movingUp = true;
                StartCoroutine(PauseAtBound());
            }
        }
    }

    IEnumerator PauseAtBound()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseTime);
        isPaused = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("DeadBody"))
        {
            collision.transform.SetParent(transform);  // 踩上變子物件
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
