using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 固定的6边行网格 结构Item 
/// </summary>
public class Tile : MonoBehaviour
{
    // 左右六边形的间隔距离
    private const float interv = 1f;    // 斜边
    private const float d_x = interv * .5f;   // 30°对应的边
    private const float d_y = interv * .866f; // 60°对应的边

    // 行数
    public const int lines = 14;

    // 方位常量 (todo 改为枚举)
    public const int LU = 0;
    public const int RU = 1;
    public const int LL = 2;
    public const int RD = 3;
    public const int LD = 4;
    public const int RR = 5;

    // 方位数组   和方位常量对应
    public static readonly Vector2[] Dirs =
    {
        new Vector2(d_x, d_y),
        new Vector2(d_x, d_y),
        new Vector2(-interv, 0),
        new Vector2(d_x, -d_y),
        new Vector2(-d_x, -d_y),
        new Vector2(interv, 0),
    };

    // 表示周围的网格是哪个网格的数据
    public Tile[] surrounds;

    // 当前位置显示的  泡泡i
    public Bubble bubble;


    // 自身碰撞盒
    private Collider2D c2D;
    // 当前网格是不是 顶部的网格,不会掉落
    private bool isTop; 

    void Awake()
    {
        c2D = GetComponent<Collider2D>();
        surrounds = new Tile[6];
    }

    void Start()
    {
        SetSurrounds();
    }

    // 获取六方位
    void SetSurrounds()
    {
        // 检测之前要把自身的 碰撞盒关掉
        c2D.enabled = false;

        for (int i = 0; i < 6; i++)
        {
            SetSurround(i);
        }

        TopCheck();
        BotttomCheck();
        c2D.enabled = true;
    }

    // 重载
    void SetSurround(int dir)
    {
        SetSurround(transform.position, dir);
    }


    // 获取其中一个（通过射线的方式判断是否有内容）
    void SetSurround(Vector2 pos, int dir)
    {
        // 初始位置， 方向， 距离, 标识自己是哪个物理层级的  magnitude长度， sqrmagnitude是长度的平方
        var hit = Physics2D.Raycast(pos, Dirs[dir], Dirs[dir].magnitude,
            LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)));

        if (hit.collider)
        {
            surrounds[dir] = hit.collider.GetComponent<Tile>();
        }
    }

    public Tile GetSurround(int dir)
    {
        if ((dir == LU || dir == RU) && isTop)// 最上方 的上方 就是没有啊
        {
            return null;
        }
        // 最下方的  下方的排除
        else if ((dir == LD || dir == RD) && (surrounds[dir] != null && surrounds[dir].isTop))
        {
            return null;
        }

        return surrounds[dir];
    }

    void TopCheck()
    {
        if (!surrounds[LU] && !surrounds[RU])   // 说明是最上方的一行
        {
            isTop = true;
            // 把他的左上、右上设置为 对应最下面的网格！  （看了图片就好理解坐标了）
            SetSurround((Vector2)transform.position + Vector2.down * d_y * lines, LU);
            SetSurround((Vector2)transform.position + Vector2.down * d_y * lines, RU);
        }
    }

    void BotttomCheck()
    {
        if (!surrounds[LD] && !surrounds[RD])   // 说明是最下方的一行
        {
            isTop = true;
            SetSurround((Vector2)transform.position + Vector2.up * d_y * lines, LD);
            SetSurround((Vector2)transform.position + Vector2.up * d_y * lines, RD);
        }
    }

    public static Tile[]   GetTops()
    {
        // todo FindObjectsOfType这个使用的应该是反射吧, 应该全局就维护一个Tile列表！
        //Tile[] tiles = FindObjectsOfType<Tile>();    
        var tiles = GameController.gameController.tiles;
        List<Tile> tList = new List<Tile>(10);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].isTop)
            {
                tList.Add(tiles[i]);
            }
        }

        return tList.ToArray();
    }

    // 每走 一步就要导致上方 网格的下降（网格下降、泡泡自然跟着下降）
    public static void DownTiles()
    {
        Tile[] tiles = GameController.gameController.tiles;

        Tile[] tTops = GetTops();
        // 设置新的 Top 
        for (int i = 0; i < tTops.Length; i++)
        {
            if (tTops[i].surrounds[LU])
            {
                tTops[i].surrounds[LU].isTop = true;
            }
            if (tTops[i].surrounds[RU])
            {
                tTops[i].surrounds[RU].isTop = true;
            }
            tTops[i].isTop = false;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].isTop)
            {
                tiles[i].transform.position += Vector3.up * (lines - 1) * d_y;  // 移到当前最上方的位置

                // 生成 泡泡
                Bubble bub = GameController.gameController.GetBubble(tiles[i].transform.position);
                // 固定位置
                bub.GetComponent<Rigidbody2D>().isKinematic = true;
                bub.tile = tiles[i];
                bub.tile.bubble = bub;
                bub.transform.parent = tiles[i].transform;
            }
            else
            {
                tiles[i].transform.position -= Vector3.up * d_y;  // 下降一个格
            }
        }
    }



}
