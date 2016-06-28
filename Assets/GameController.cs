using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏管理类
/// </summary>
public class GameController : MonoBehaviour
{
    // 单例
    public static GameController gameController;


    // 泡泡的速度
    public float bubbleSpeed = 5f;

    public Tile[] tiles;

    // 网格的下降时间
    public float downTime = 2f;

    // 子弹模板 发射时从这里随机的
    public Bubble[] bubbles;

    void Awake()
    {
        gameController = this;

        tiles = FindObjectsOfType<Tile>();
    }

    void Start()
    {
        InvokeRepeating("DownTheTile", downTime, downTime);
    }

    void DownTheTile()
    {
        Tile.DownTiles();
    }

    public Bubble GetBubble(Vector2 pos)
    {
        int ind = Random.Range(0, bubbles.Length);
        Bubble bub = Instantiate(bubbles[ind], pos, Quaternion.identity) as Bubble;
        return bub;
    }
}
