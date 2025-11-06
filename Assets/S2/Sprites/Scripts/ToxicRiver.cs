using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RiverCurrentFloat : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";

    [Header("Current (X ���O)")]
    public float currentSpeed = 5f;       // ���y�����t��
    public int initialDirection = 1;      // ��l�y�V�]+1 �k / -1 ���^
    public bool overrideXVelocity = true; // �����]�w x �t�סF�����h�����즳 x�]�u����ϼu�^

    [Header("Bounce on Walls")]
    public LayerMask wallLayer;
    public float checkDistance = 0.1f;
    public float skin = 0.01f;

    [Header("Float on Surface")]
    public Collider2D surfaceCollider;    // �������ݪ��I�����]�i�d�š��Φۤv�^
    public float floatOffset = 0.0f;      // �����a��������@�I�I
    public float floatUpSpeed = 20f;      // �ԦV�������t�ס]���/���^

    // �l�ܶi�����a�G��V + ��l���O
    private class SwimState { public int dir; public float originalGravity; }
    private readonly Dictionary<Rigidbody2D, SwimState> swimmers = new();

    Collider2D riverCol;

    void Awake()
    {
        riverCol = GetComponent<Collider2D>();
        if (!riverCol.isTrigger) riverCol.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var rb = other.attachedRigidbody;
        if (!rb || swimmers.ContainsKey(rb)) return;

        swimmers.Add(rb, new SwimState
        {
            dir = (initialDirection >= 0) ? 1 : -1,
            originalGravity = rb.gravityScale
        });

        // �i���G�������O�A�קK�U�I
        rb.gravityScale = 0f;
        // �ߨ�M�������t��
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var rb = other.attachedRigidbody;
        if (!rb) return;

        if (swimmers.TryGetValue(rb, out var st))
        {
            // �٭쭫�O
            rb.gravityScale = st.originalGravity;
            swimmers.Remove(rb);
        }
    }

    void FixedUpdate()
    {
        if (swimmers.Count == 0) return;

        float surfaceY = GetSurfaceY();

        // �ַ��קK���N�ɦr���ܰ�
        var list = new List<Rigidbody2D>(swimmers.Keys);
        foreach (var rb in list)
        {
            if (!rb) { swimmers.Remove(rb); continue; }

            // ---- ��ϼu�ˬd ----
            var st = swimmers[rb];
            int dir = st.dir;

            var col = rb.GetComponent<Collider2D>();
            if (col)
            {
                var filter = new ContactFilter2D { useLayerMask = true, layerMask = wallLayer, useTriggers = false };
                var hits = new RaycastHit2D[3];
                int hitCount = col.Cast(new Vector2(dir, 0f), filter, hits, checkDistance + skin);
                if (hitCount > 0) st.dir = dir = -dir;
            }
            else
            {
                var hit = Physics2D.Raycast(rb.position, new Vector2(dir, 0f), checkDistance + skin, wallLayer);
                if (hit.collider) st.dir = dir = -dir;
            }

            // ---- �������� ----
            if (overrideXVelocity)
                rb.linearVelocity = new Vector2(dir * currentSpeed, rb.linearVelocity.y);

            // ---- �B�b�����G�� Y �Ԩ� surfaceY + offset ----
            float targetY = surfaceY + floatOffset;
            float newY = Mathf.MoveTowards(rb.position.y, targetY, floatUpSpeed * Time.fixedDeltaTime);
            rb.position = new Vector2(rb.position.x, newY);

            // �T�O���|�ֿn�����t��
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    float GetSurfaceY()
    {
        var src = surfaceCollider ? surfaceCollider : riverCol;
        return src.bounds.max.y; // ���I�����W�t���@����
    }

    void OnDrawGizmosSelected()
    {
        var src = surfaceCollider ? surfaceCollider : GetComponent<Collider2D>();
        if (!src) return;
        float y = src.bounds.max.y + floatOffset;
        Gizmos.color = new Color(0, 0.6f, 1f, 0.4f);
        Gizmos.DrawLine(new Vector3(src.bounds.min.x, y, 0), new Vector3(src.bounds.max.x, y, 0));
    }
}
