using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Collider2D))]
public class RockSmash : MonoBehaviour
{
    [Header("Top / Rise")]
    public float upOffset = 0f;            // 頂端相對初始位置（設 0 = 初始位置就是頂端）
    public float riseSpeed = 4f;           // 上升速度（單位/秒）
    public float waitAtTop = 0.5f;         // 頂端停留

    [Header("Drop")]
    public float dropSpeed = 12f;          // 下落速度（單位/秒）
    public float waitAtBottom = 0.5f;      // 碰到地面後的停留
    public LayerMask groundMask;           // 會被視為「地面/可撞擊」的圖層
    public float skin = 0.01f;             // 與地面保留的小縫，避免重疊

    [Header("Pattern（每次落下前的等待秒數序列）")]
    public List<float> dropIntervals = new List<float> { 1.0f, 0.6f, 1.5f };
    public bool loopPattern = true;

    [Header("Easing（上升用，可空）")]
    public AnimationCurve riseEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("平台推薦")]
    public bool useKinematicRB2D = true;   // 當成移動平台用，建議開

    Rigidbody2D rb;
    Collider2D col;
    Vector3 topPos;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (useKinematicRB2D)
        {
            if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Start()
    {
        var basePos = transform.position;
        topPos = basePos + new Vector3(0f, upOffset, 0f);
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        // 先回到頂端並停一下
        yield return RiseTo(topPos);

        int i = 0;
        while (true)
        {
            // 頂端等待（可依序列）
            float wait = (dropIntervals != null && dropIntervals.Count > 0)
                         ? Mathf.Max(0f, dropIntervals[i])
                         : waitAtTop;
            yield return new WaitForSeconds(wait);
            if (dropIntervals != null && dropIntervals.Count > 0)
            {
                i++;
                if (i >= dropIntervals.Count)
                {
                    if (loopPattern) i = 0;
                    else yield break;
                }
            }

            // 下落直到碰到 groundMask
            yield return DropUntilHit();

            // 底部停留
            yield return new WaitForSeconds(waitAtBottom);

            // 升回頂端
            yield return RiseTo(topPos);
        }
    }

    IEnumerator DropUntilHit()
    {
        // 持續往下：每幀先計算「這一小段距離」可否撞到東西
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };

        RaycastHit2D[] hits = new RaycastHit2D[4];

        while (true)
        {
            float step = dropSpeed * Time.deltaTime;      // 這一幀想走的距離
            int hitCount = col.Cast(Vector2.down, filter, hits, step + skin);

            if (hitCount > 0)
            {
                // 最近的命中距離
                float minDist = Mathf.Infinity;
                for (int h = 0; h < hitCount; h++)
                    if (hits[h].distance < minDist) minDist = hits[h].distance;

                // 只移到接觸點前的 skin
                float move = Mathf.Max(0f, minDist - skin);
                Vector3 target = transform.position + Vector3.down * move;
                MoveToPositionInstant(target);
                yield break; // 抵達底部（撞到）→ 結束下落
            }
            else
            {
                // 沒撞到，安全下移 step
                Vector3 next = transform.position + Vector3.down * step;
                MoveToPositionInstant(next);
            }
            yield return null;
        }
    }

    IEnumerator RiseTo(Vector3 target)
    {
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, target);
        if (dist < 0.0001f)
        {
            // 已在頂端也給個頂端等待
            if (waitAtTop > 0f) yield return new WaitForSeconds(waitAtTop);
            yield break;
        }

        float duration = Mathf.Max(0.0001f, dist / Mathf.Max(0.001f, riseSpeed));
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);
            if (riseEase != null) k = riseEase.Evaluate(k);

            Vector3 pos = Vector3.Lerp(start, target, k);
            MoveToPositionInstant(pos);
            yield return null;
        }
        MoveToPositionInstant(target);
    }

    void MoveToPositionInstant(Vector3 pos)
    {
        if (useKinematicRB2D && rb) rb.MovePosition(pos);
        else transform.position = pos;
    }

   
}
