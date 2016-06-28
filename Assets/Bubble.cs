using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 泡泡脚本
/// </summary>
public class Bubble : MonoBehaviour
{
    // 记录要爆破 掉的 列表(Static 属于类的)  如果长度>=3就可以爆掉
    private static List<Bubble> list;

    // 类型
    private string type;

    // 组件
    private SpriteRenderer sR;
    private Rigidbody2D r2D;
    private CircleCollider2D cC2D;     // 要使用它的半径


    // 当前处于哪个 Tile上
    public Tile tile { get; set; }

    private static readonly WaitForSeconds w = new WaitForSeconds(.04f);


    void Awake()
    {
        sR = GetComponent<SpriteRenderer>();
        r2D = GetComponent<Rigidbody2D>();
        cC2D = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        type = sR.sprite.name;
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果不是 固定网格，  是其他泡泡或者 最上边缘的碰撞体
        if (!tile && (collision.gameObject.CompareTag("C") ||     // todo 网上有说过不要使用 CompareTag比较
            collision.gameObject.GetComponent<Bubble>()))
        {
            // 禁止移动 ，顶到确切位置
            Freeze(collision.contacts[0].point);
            // 处理爆掉
            BubbleBoom();
        }
    }

    // 处理爆掉
    void BubbleBoom()
    {
        if (list == null)
        {
            list = new List<Bubble>(Tile.lines * 10);
        }

        list.Clear();
        list.Add(this);
        // 把与当前类型一样的，相连的添加到list中（深度优先遍历！）
        SameTypeDetect();

        // 如果存在超过三个一样的 就执行销毁操作
        if (list.Count >= 3)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].tile.bubble = null;
                Destroy(list[i].gameObject);
            }

            BubbleShouldFall();
            BubbleFall();
        }
    }


    // 除了 不掉落以外的其他所有都给掉落
    static void BubbleFall()
    {
        Bubble[] bs = FindObjectsOfType<Bubble>();

        for (int i = 0; i < bs.Length; i++)
        {
            // 应该掉落的， 并且现在处理静止的
            if (list.Contains(bs[i]) || !bs[i].r2D.isKinematic)
            {
                continue;
            }
            bs[i].StartCoroutine(bs[i].Falling());
        }
    }

    private IEnumerator Falling()
    {
        // 原来可以这样获取  Layer啊！！！！！！
        gameObject.layer = LayerMask.NameToLayer("FallingBubble");
        yield return null;

        for (int i = 0; i < 25; i++)
        {
            // 每次向下位移  .5f
            transform.Translate(0, -.5f, 0);
            yield return w;
        }

        tile.bubble = null;
        Destroy(gameObject);
    }

    // 检测 所有不会掉落的泡泡
    static void BubbleShouldFall()
    {
        list.Clear();
        Tile[] ts = Tile.GetTops();
        // 遍历所有顶节点， 顶节点 下方的所有节点添加到  list中
        for (int i = 0; i < ts.Length; i++)
        {
            if (ts[i].bubble)
            {
                list.Add(ts[i].bubble);
                // 也是 递归的深度优先遍历
                ts[i].bubble.BubbleDetect();
            }
        }
    }

    // 与 SameTypeDetect 类似
    private void BubbleDetect()
    {
        for (int i = 0; i < 6; i++)
        {
            // 获得这个方位的 临节点
            Tile t = tile.GetSurround(i);

            // 不为空、有泡泡、类型一致、没有存在当前的列表中
            if (t && t.bubble
                && !list.Contains(t.bubble))
            {
                list.Add(t.bubble);
                // 递归
                t.bubble.SameTypeDetect();
            }
        }
    }

    // 把与当前类型一样的，相连的添加到list中（深度优先遍历！） 递归实现方式
    void SameTypeDetect()
    {
        for (int i = 0; i < 6; i++)
        {
            // 获得这个方位的 临节点
            Tile t = tile.GetSurround(i);

            // 不为空、有泡泡、类型一致、没有存在当前的列表中
            if (t && t.bubble && t.bubble.type == type
                && !list.Contains(t.bubble))
            {
                list.Add(t.bubble);
                // 递归
                t.bubble.SameTypeDetect();
            }
        }
    }

    // 黏附处理函数
    void Freeze(Vector2 hitPoint)
    {
        // 查找指定位置 符合半径范围内的  所有碰撞体
        var hits = Physics2D.OverlapCircleAll(hitPoint, cC2D.radius,
            LayerMask.GetMask("Grid"));   // 这个Mask 是固定位置的Tile层

        for (int i = 0; i < hits.Length; i++)
        {
            Tile t = hits[i].GetComponent<Tile>();

            if (t && t.bubble == null)
            {
                r2D.isKinematic = true;
                transform.parent = t.transform;
                transform.localPosition = Vector2.zero;
                t.bubble = this;
                tile = t;
                return;
            }
        }
        Debug.Break();
    }
}
