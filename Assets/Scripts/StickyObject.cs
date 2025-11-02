using System.Collections.Generic;
using UnityEngine;

public class StickyObject : MonoBehaviour
{
    private List<Transform> stuckBodies = new List<Transform>();
    private List<Vector3> offsets = new List<Vector3>();

    void Update()
    {
        for (int i = stuckBodies.Count - 1; i >= 0; i--)
        {
            if (stuckBodies[i] != null) stuckBodies[i].position = transform.position + offsets[i];
            else
            {
                stuckBodies.RemoveAt(i);
                offsets.RemoveAt(i);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("DeadBody")) return;
        
        Transform bodyTransform = collision.transform;
        if (stuckBodies.Contains(bodyTransform)) return;
        
        stuckBodies.Add(bodyTransform);
        offsets.Add(bodyTransform.position - transform.position);
        
        Rigidbody2D bodyRb = bodyTransform.GetComponent<Rigidbody2D>();
        if (bodyRb != null)
        {
            bodyRb.bodyType = RigidbodyType2D.Kinematic;
            bodyRb.linearVelocity = Vector2.zero;
        }
    }
}
