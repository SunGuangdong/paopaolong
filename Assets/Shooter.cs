using UnityEngine;
using System.Collections;

/// <summary>
/// 泡泡发射  脚本
/// </summary>
public class Shooter : MonoBehaviour
{
    //// 子弹模板 发射时从这里随机的
    //public Bubble[] bubbles;

    // 锁
    public bool canShoot = true;


    // 即将被发射的
    private Bubble bub;
    private Rigidbody2D bub_r2D;



    void Start()
    {
        // 获取一个泡泡放到发射台是上
        GetBubble();
    }
    

    void Update()
    {
        if (canShoot && Input.GetMouseButtonUp(0) && bub != null)   // 其实这个顺序 也是建议注意的
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 保证 发射角度都是斜向上的 
            if (target.y < bub.transform.position.y + .5f)      
            {
                target.y = bub.transform.position.y + .5f;
            }

            // 方向  单位矢量
            Vector2 dir = (target - (Vector2)bub.transform.position).normalized;

            // 速度矢量 = 方向单位矢量 * 标量大小
            bub_r2D.velocity = dir * GameController.gameController.bubbleSpeed;

            ResetBubble();

            // 1s 在获取一个泡泡放到发射台是上
            Invoke("GetBubble", 1f);      
        }
    }

    void GetBubble()
    {
        GetBubble(transform.position);
    }

    // 随机生成一个泡泡
    void GetBubble(Vector2 pos)
    {
        var bub = GameController.gameController.GetBubble(pos);
        bub_r2D = bub.GetComponent<Rigidbody2D>();
    }

    void ResetBubble()
    {
        bub = null;
        bub_r2D = null;
    }
}
