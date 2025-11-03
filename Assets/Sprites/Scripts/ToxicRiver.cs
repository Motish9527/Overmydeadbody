using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class RiverCurrentFloat : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";

    [Header("Current (X 推力)")]
    public float currentSpeed = 5f;       // 水流水平速度
    public int initialDirection = 1;      // 初始流向（+1 右 / -1 左）
    public bool overrideXVelocity = true; // 直接設定 x 速度；關掉則維持原有 x（只做牆反彈）

    [Header("Bounce on Walls")]
    public LayerMask wallLayer;
    public float checkDistance = 0.1f;
    public float skin = 0.01f;

    [Header("Float on Surface")]
    public Collider2D surfaceCollider;    // 水面所屬的碰撞器（可留空→用自己）
    public float floatOffset = 0.0f;      // 讓玩家高於水面一點點
    public float floatUpSpeed = 20f;      // 拉向水面的速度（單位/秒）

    // 追蹤進水玩家：方向 + 原始重力
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

        // 進水：關掉重力，避免下沉
        rb.gravityScale = 0f;
        // 立刻清掉垂直速度
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var rb = other.attachedRigidbody;
        if (!rb) return;

        if (swimmers.TryGetValue(rb, out var st))
        {
            // 還原重力
            rb.gravityScale = st.originalGravity;
            swimmers.Remove(rb);
        }
    }

    void FixedUpdate()
    {
        if (swimmers.Count == 0) return;

        float surfaceY = GetSurfaceY();

        // 快照避免迭代時字典變動
        var list = new List<Rigidbody2D>(swimmers.Keys);
        foreach (var rb in list)
        {
            if (!rb) { swimmers.Remove(rb); continue; }

            // ---- 牆反彈檢查 ----
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

            // ---- 水平推動 ----
            if (overrideXVelocity)
                rb.velocity = new Vector2(dir * currentSpeed, rb.velocity.y);

            // ---- 浮在水面：把 Y 拉到 surfaceY + offset ----
            float targetY = surfaceY + floatOffset;
            float newY = Mathf.MoveTowards(rb.position.y, targetY, floatUpSpeed * Time.fixedDeltaTime);
            rb.position = new Vector2(rb.position.x, newY);

            // 確保不會累積垂直速度
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    float GetSurfaceY()
    {
        var src = surfaceCollider ? surfaceCollider : riverCol;
        return src.bounds.max.y; // 取碰撞器上緣當作水面
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
