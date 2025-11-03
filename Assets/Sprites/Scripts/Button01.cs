using UnityEngine;
using System.Collections;

public class Button01 : MonoBehaviour
{
    [Header("Trigger Mode")]
    public string playerTag = "Player";
    public bool useTrigger = true;    

    [Header("Cube Move (pixels)")]
    public float moveDownPixels = 15f;   
    public float pixelsPerUnit = 100f;   

    [Header("Stairs Move (world units on X)")]
    public Transform stair01;            
    public Transform stair02;            
    public float stair01DeltaX = -1f;    
    public float stair02DeltaX = 1f;    

    [Header("Tween")]
    public float cubeDuration = 0.15f;  // 回彈時間
    public float stairDuration = 0.30f;  // 縮回時間
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 內部狀態
    Vector3 cubeStart, cubeEnd;
    Vector3 s1Start, s1End;
    Vector3 s2Start, s2End;
    int overlappingCount = 0;       
    Coroutine tweenCo;

    void Start()
    {
        // 起點
        cubeStart = transform.position;
        s1Start = stair01 ? stair01.position : Vector3.zero;
        s2Start = stair02 ? stair02.position : Vector3.zero;

        // 終點
        float worldDown = moveDownPixels / Mathf.Max(1f, pixelsPerUnit);
        cubeEnd = cubeStart + Vector3.down * worldDown;
        s1End = stair01 ? s1Start + new Vector3(stair01DeltaX, 0f, 0f) : Vector3.zero;
        s2End = stair02 ? s2Start + new Vector3(stair02DeltaX, 0f, 0f) : Vector3.zero;
    }

    // Trigger 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        if (other.CompareTag(playerTag)) OnPlayerEnter();
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (!useTrigger) return;
        if (other.CompareTag(playerTag)) OnPlayerExit();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (useTrigger) return;
        if (col.collider.CompareTag(playerTag)) OnPlayerEnter();
    }
    void OnCollisionExit2D(Collision2D col)
    {
        if (useTrigger) return;
        if (col.collider.CompareTag(playerTag)) OnPlayerExit();
    }

    void OnPlayerEnter()
    {
        overlappingCount++;
        if (overlappingCount == 1) SetPressed(true); 
    }

    void OnPlayerExit()
    {
        overlappingCount = Mathf.Max(0, overlappingCount - 1);
        if (overlappingCount == 0) SetPressed(false); 
    }

   
    void SetPressed(bool pressed)
    {
        if (tweenCo != null) StopCoroutine(tweenCo);
        tweenCo = StartCoroutine(TweenTo(pressed));
    }

    //?????
    IEnumerator TweenTo(bool pressed)
    {
    
        Vector3 cFrom = transform.position;
        Vector3 cTo = pressed ? cubeEnd : cubeStart;

        Vector3 s1From = stair01 ? stair01.position : Vector3.zero;
        Vector3 s1To = stair01 ? (pressed ? s1End : s1Start) : Vector3.zero;

        Vector3 s2From = stair02 ? stair02.position : Vector3.zero;
        Vector3 s2To = stair02 ? (pressed ? s2End : s2Start) : Vector3.zero;

        float tCubeDur = Mathf.Max(0.0001f, cubeDuration);
        float tStairDur = Mathf.Max(0.0001f, stairDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(tCubeDur, tStairDur); // 同步時間軸
            float eCube = ease.Evaluate(Mathf.Clamp01(t * (tCubeDur / Mathf.Max(tCubeDur, tStairDur))));
            float eStair = ease.Evaluate(Mathf.Clamp01(t * (tStairDur / Mathf.Max(tCubeDur, tStairDur))));

            transform.position = Vector3.Lerp(cFrom, cTo, eCube);
            if (stair01) stair01.position = Vector3.Lerp(s1From, s1To, eStair);
            if (stair02) stair02.position = Vector3.Lerp(s2From, s2To, eStair);

            yield return null;
        }

        // 對齊到終點
        transform.position = cTo;
        if (stair01) stair01.position = s1To;
        if (stair02) stair02.position = s2To;

        tweenCo = null;
    }

}