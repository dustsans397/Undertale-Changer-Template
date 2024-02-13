using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Log;
using System.Collections.Generic;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
/// <summary>
/// 控制战斗内玩家(心)的相关属性
/// </summary>
public class BattlePlayerController : MonoBehaviour
{
    float saveTime;
    public float speed, speedWeightX, speedWeightY;//速度与权重(按X乘以倍数)，速度测试为3，权重0.5f
    private float speedWeight = 0.5f;
    public float hitCD, hitCDMax;//无敌时间
    public bool isMoving;//用于蓝橙骨判断：玩家是否真的在移动
    public float timeInterpolation = -0.225f;

    public enum PlayerDirEnum
    {
        up,
        down,
        left,
        right,
        nullDir
    };

    public PlayerDirEnum playerDir;//方向
    public Vector3 moving;
    public bool isJump;//是否处于“跳起”状态
    public float jumpAcceleration;//跳跃的加速度
    public float jumpRayDistance;//射线距离
    public float jumpRayDistanceForBoard;

    private Rigidbody2D rigidBody;
    public CircleCollider2D collideCollider;//圆形碰撞体
    private SpriteRenderer spriteRenderer;
    public BattleControl.PlayerColor playerColor;//含有属性的颜色 读取BattleControl中的enum PlayerColor.颜色变换通过具体变换的函数来执行
    private Tween missAnim = null;

    public Volume hitVolume;

    //LayerMask mask;
    private void Start()
    {
        speedWeightX = 1;
        speedWeightY = 1;
        jumpRayDistance = 0.2f;
        jumpRayDistanceForBoard = 0.2f;
        jumpAcceleration = 1.25f;
        playerColor = BattleControl.PlayerColor.red;
        playerDir = PlayerDirEnum.down;
        rigidBody = GetComponent<Rigidbody2D>();
        collideCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitVolume = GetComponent<Volume>();
        hitVolume.weight = 0;
        //mask = 1 << 6;
        MainControl.instance.PlayerControl.missTime = 0;

        lineRenderer = GetComponent<LineRenderer>();
    }
    LineRenderer lineRenderer;
    private void Update()
    {
        if (!MainControl.instance.OverworldControl.noSFX && hitVolume.weight > 0)
            hitVolume.weight -= Time.deltaTime;

        if (MainControl.instance.PlayerControl.hp <= 0)
        {
            MainControl.instance.PlayerControl.hp = MainControl.instance.PlayerControl.hpMax;

            if (!(MainControl.instance.PlayerControl.isDebug && MainControl.instance.PlayerControl.invincible))
            {
                spriteRenderer.color = Color.red;
                MainControl.instance.OverworldControl.playerDeadPos = transform.position;
                MainControl.instance.OverworldControl.pause = true;
                TurnController.instance.KillIEnumerator();
                MainControl.instance.SwitchScene("Gameover", false);
            }
            else
                MainControl.instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
        }

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        if (MainControl.instance.PlayerControl.missTime >= 0)
        {
            MainControl.instance.PlayerControl.missTime -= Time.deltaTime;
            if (missAnim == null && MainControl.instance.PlayerControl.missTimeMax >= 0.4f)
                missAnim = spriteRenderer.DOColor(MainControl.instance.BattleControl.playerMissColorList[(int)playerColor], 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            if (missAnim != null)
            {
                missAnim.Kill();
                missAnim = null;
                spriteRenderer.color = MainControl.instance.BattleControl.playerColorList[(int)playerColor];
            }
        }

        //Debug
        if (MainControl.instance.PlayerControl.isDebug)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[0], 0);
            else if (Input.GetKeyDown(KeyCode.Keypad2))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[1], (BattleControl.PlayerColor)1);
            else if (Input.GetKeyDown(KeyCode.Keypad6))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5);

            if (Input.GetKeyDown(KeyCode.I))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, 0);
            else if (Input.GetKeyDown(KeyCode.K))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)1);
            else if (Input.GetKeyDown(KeyCode.J))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)2);
            else if (Input.GetKeyDown(KeyCode.L))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)3);

            if (Input.GetKeyDown(KeyCode.P))
                MainControl.instance.PlayerControl.hp = 0;
        }
    }

    private void FixedUpdate()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
        if (!TurnController.instance.isMyTurn)
            Moving();
    }
    private void Moving()
    {
        Vector2 dirReal = new Vector2();
        switch (playerDir)
        {
            case PlayerDirEnum.up:
                dirReal = Vector2.up;
                break;

            case PlayerDirEnum.down:
                dirReal = Vector2.down;
                break;

            case PlayerDirEnum.left:
                dirReal = Vector2.left;
                break;

            case PlayerDirEnum.right:
                dirReal = Vector2.right;
                break;
        }
        Ray2D ray = new Ray2D(transform.position, dirReal);
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, jumpRayDistance);

        Ray2D rayF = new Ray2D(transform.position, dirReal * -1);
        Debug.DrawRay(rayF.origin, rayF.direction, Color.red);
        RaycastHit2D infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance);//反向检测(顶头)

        //------------------------移动------------------------
        switch (playerColor)
        {
            case BattleControl.PlayerColor.red:
                if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                {
                    speedWeightX = speedWeight;
                    speedWeightY = speedWeight;
                }
                else
                {
                    speedWeightX = 1;
                    speedWeightY = 1;
                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                {
                    moving = new Vector3(moving.x, 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                {
                    moving = new Vector3(moving.x, -1);
                }
                else
                    moving = new Vector3(moving.x, 0);

                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                    moving = new Vector3(moving.x, 0);

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                {
                    moving = new Vector3(1, moving.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                {
                    moving = new Vector3(-1, moving.y);
                }
                else
                    moving = new Vector3(0, moving.y);

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    moving = new Vector3(0, moving.y);
                break;

            case BattleControl.PlayerColor.orange:
                /*同时按下两个方向键有bug
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    moving = new Vector3(moving.x, 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    moving = new Vector3(moving.x, -1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    moving = new Vector3(moving.x, 0);
                }

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    moving = new Vector3(1, moving.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    moving = new Vector3(-1, moving.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                    moving = new Vector3(0, moving.y);

                while (moving == Vector3.zero || (moving.x != 0 && moving.y != 0))
                    moving = new Vector3(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
                */
                break;

            case BattleControl.PlayerColor.yellow:
                break;

            case BattleControl.PlayerColor.green:
                break;

            case BattleControl.PlayerColor.cyan:
                break;

            case BattleControl.PlayerColor.blue:
                RaycastHit2D infoForBoard = Physics2D.Raycast(transform.position, dirReal, jumpRayDistanceForBoard);
                if (infoForBoard.collider != null)
                {
                    GameObject obj = infoForBoard.collider.gameObject;
                    if (obj.transform.CompareTag("board") && !obj.transform.GetComponent<EdgeCollider2D>().isTrigger && infoForBoard.collider == obj.transform.GetComponent<EdgeCollider2D>() && obj.transform.GetComponent<BoardController>().canMove)
                    {
                        jumpRayDistance = 0;
                        isJump = false;
                        transform.SetParent(infoForBoard.transform);
                    }
                    else
                        transform.SetParent(null);
                }
                else
                {
                    transform.SetParent(null);
                }

                int gravity = 10;
                if (rigidBody.gravityScale != gravity)
                    rigidBody.gravityScale = gravity;
                switch (playerDir)
                {
                    case PlayerDirEnum.up:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightX = speedWeight;
                        }
                        else
                        {
                            speedWeightX = 1;
                        }

                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        Physics2D.gravity = new Vector2(0, 9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);

                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            moving = new Vector3(0, moving.y);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1) && !isJump && moving.y == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moving = new Vector3(moving.x, -2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.y < -0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(moving.x, -0.55f);
                            
                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moving.y += Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * timeInterpolation;
                        break;

                    case PlayerDirEnum.down:////////////////////////////////////////////////
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightX = speedWeight;
                        }
                        else
                        {
                            speedWeightX = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        Physics2D.gravity = new Vector2(0, -9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            moving = new Vector3(0, moving.y);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && !isJump && moving.y == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moving = new Vector3(moving.x, 2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;


                            DebugLogger.Log(Time.time - saveTime,DebugLogger.Type.war,"#FF0000");
                            saveTime = Time.time; ;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.y > 0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(moving.x, 0.55f);

                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moving.y -= Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * timeInterpolation;
                        break;

                    case PlayerDirEnum.left:////////////////////////////////////////////////
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightY = speedWeight;
                        }
                        else
                        {
                            speedWeightY = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 270);
                        Physics2D.gravity = new Vector2(-9.8f, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                        {
                            moving = new Vector3(moving.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            moving = new Vector3(moving.x, -1);
                        }
                        else
                            moving = new Vector3(moving.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moving = new Vector3(moving.x, 0);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !isJump && moving.x == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moving = new Vector3(2.15f, moving.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.x > 0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(0.55f, moving.y);
                            
                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moving.x -= Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.x = 0;
                        }
                        jumpAcceleration += Time.deltaTime * timeInterpolation;
                        break;

                    case PlayerDirEnum.right:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightY = speedWeight;
                        }
                        else
                        {
                            speedWeightY = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        Physics2D.gravity = new Vector2(9.8f, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                        {
                            moving = new Vector3(moving.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            moving = new Vector3(moving.x, -1);
                        }
                        else
                            moving = new Vector3(moving.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moving = new Vector3(moving.x, 0);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) && !isJump && moving.x == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moving = new Vector3(-2.15f, moving.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.x < -0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(-0.55f, moving.y);
                            
                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moving.x += Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.x = 0;
                        }
                        jumpAcceleration += Time.deltaTime * timeInterpolation;
                        break;
                }

                break;

            case BattleControl.PlayerColor.purple:
                break;

            default:
                break;
        }


        //蓝橙骨所用的是否移动判定
        Vector2 dirMoveX = new Vector2();
        Vector2 dirMoveY = new Vector2();
        bool isMoveX = false, isMoveY = false;
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
        {
            dirMoveY = Vector2.up;
            isMoveY = true;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
        {
            dirMoveY = Vector2.down;
            isMoveY = true;
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
            isMoveY = false;

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
        {
            dirMoveX = Vector2.left;
            isMoveX = true;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
        {
            dirMoveX = Vector2.right;
            isMoveX = true;
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
            isMoveX = false;

        Ray2D rayMoveX = new Ray2D(transform.position, dirMoveX);
        Debug.DrawRay(rayMoveX.origin, rayMoveX.direction, Color.green);
        RaycastHit2D infoMoveX = Physics2D.Raycast(transform.position, dirMoveX, 0.2f);
        Ray2D rayMoveY = new Ray2D(transform.position, dirMoveY);
        Debug.DrawRay(rayMoveY.origin, rayMoveY.direction, new Color(0, 0.5f, 0, 1));
        RaycastHit2D infoMoveY = Physics2D.Raycast(transform.position, dirMoveY, 0.2f);

        if (isMoveX && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("frame") || infoMoveX.collider.gameObject.CompareTag("board")))
            isMoving = false;

        if (isMoveX || isMoveY)
        {
            bool x = (isMoveX || isMoveY) && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("frame") || infoMoveX.collider.gameObject.CompareTag("board"));
            bool y = (isMoveX || isMoveY) && infoMoveY.collider != null && (infoMoveY.collider.gameObject.CompareTag("frame") || infoMoveY.collider.gameObject.CompareTag("board"));
            if (x && !y && (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1)))
                x = y;
            if (y && !x && (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1)))
                y = x;

            isMoving = !(x || y);

            /*
            //DebugLogger.Log("X:" + x);
            //DebugLogger.Log("Y:" + y);
            */
        }
        else
        {
            if (playerColor == BattleControl.PlayerColor.blue && jumpRayDistance != 0)
                isMoving = true;
            else
                isMoving = false;
        }

        float movingSave = 0;
        if (playerColor == BattleControl.PlayerColor.blue && isJump)
        {
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    movingSave = moving.y;
                    rigidBody.gravityScale = 0;
                    break;

                case PlayerDirEnum.down:
                    movingSave = moving.y;
                    rigidBody.gravityScale = 0;
                    break;

                case PlayerDirEnum.left:
                    movingSave = moving.x;
                    rigidBody.gravityScale = 0;
                    break;

                case PlayerDirEnum.right:
                    movingSave = moving.x;
                    rigidBody.gravityScale = 0;
                    break;
            }
        }

        moving.x = MainControl.instance.JudgmentNumber(false, moving.x, -5);
        moving.y = MainControl.instance.JudgmentNumber(false, moving.y, -5);
        moving.x = MainControl.instance.JudgmentNumber(true, moving.x, 5);
        moving.y = MainControl.instance.JudgmentNumber(true, moving.y, 5);

        Vector3 newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime, speedWeightY * speed * moving.y * Time.deltaTime);//速度参考：3
        Vector3 checkPos = CheckPoint(newPos, 0.175f + BoxController.instance.width / 2);
        if (newPos == checkPos)
            rigidBody.MovePosition(newPos);
        else
            transform.localPosition = checkPos;
        //transform.position = transform.position + new Vector3(speed * moving.x * Time.deltaTime, speed * moving.y * Time.deltaTime);//蓝心会有巨大偏差 不采用
        if (movingSave != 0)
            moving.y = movingSave;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //蓝心碰板子确保再次可以跳
        if (collision.transform.CompareTag("board") && collision.transform.GetComponent<EdgeCollider2D>().IsTouching(collideCollider) && playerColor == BattleControl.PlayerColor.blue)
        {
            jumpRayDistance = 0;
            isJump = false;
        }
    }

    /*
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board" && collision.transform.GetComponent<BoardController>().canMove && playerColor == BattleControl.PlayerColor.blue)
        {
            transform.SetParent(collision.transform);
        }
    }
    */

    /// <summary>
    /// 掉出
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.blue && !isJump)
        {
            jumpRayDistance = 0.2f;
            isJump = true;
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    moving = new Vector3(moving.x, -0.55f);
                    break;

                case PlayerDirEnum.down:
                    moving = new Vector3(moving.x, 0.55f);
                    break;

                case PlayerDirEnum.left:
                    moving = new Vector3(0.55f, moving.y);
                    break;

                case PlayerDirEnum.right:
                    moving = new Vector3(-0.55f, moving.y);
                    break;
            }
        }
    }

    /// <summary>
    /// 通过渐变动画将玩家的颜色改变。
    /// 若time小于等于0 则不会有渐变动画；
    /// 若PlayerColor输入为nullColor，则不会更改玩家的实际颜色属性。
    /// </summary>
    public void ChangePlayerColor(Color aimColor, BattleControl.PlayerColor aimPlayerColor, float time = 0.1f, PlayerDirEnum dir = PlayerDirEnum.nullDir)
    {
        if (time <= 0)
            spriteRenderer.color = aimColor;
        else
        {
            spriteRenderer.DOColor(aimColor, time).SetEase(Ease.InOutSine);
        }
        if (playerColor != aimPlayerColor)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.SetParent(null);
            playerColor = aimPlayerColor;
            moving = Vector3.zero;
            isJump = false;
            jumpAcceleration = 1.25f;
            rigidBody.gravityScale = 0;

            if (aimPlayerColor == BattleControl.PlayerColor.blue)
            {
                jumpRayDistance = 0.2f;
                isJump = true;
                switch (playerDir)
                {
                    case PlayerDirEnum.up:
                        moving = new Vector3(moving.x, -0.55f);
                        break;

                    case PlayerDirEnum.down:
                        moving = new Vector3(moving.x, 0.55f);
                        break;

                    case PlayerDirEnum.left:
                        moving = new Vector3(0.55f, moving.y);
                        break;

                    case PlayerDirEnum.right:
                        moving = new Vector3(-0.55f, moving.y);
                        break;
                }
            }
        }
        if (dir != PlayerDirEnum.nullDir)
        {
            transform.SetParent(null);
            playerDir = dir;
            jumpRayDistance = 0.2f;
            isJump = true;
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    moving = new Vector3(moving.x, -0.55f);
                    break;

                case PlayerDirEnum.down:
                    moving = new Vector3(moving.x, 0.55f);
                    break;

                case PlayerDirEnum.left:
                    moving = new Vector3(0.55f, moving.y);
                    break;

                case PlayerDirEnum.right:
                    moving = new Vector3(-0.55f, moving.y);
                    break;
            }
        }
    }
    // 判断点是否在多边形内
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool isInside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
             (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    // 计算点到线段的最近点（垂足）
    private Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 line = end - start;
        float len = line.magnitude;
        line.Normalize();

        Vector2 v = point - start;
        float d = Vector2.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return start + line * d;
    }

    // 计算位移后的垂点位置
    private Vector2 CalculateDisplacedPoint(Vector2 nearestPoint, Vector2 point, Vector2 lineStart, Vector2 lineEnd, float displacement)
    {
        // 计算线段方向向量
        Vector2 lineDirection = (lineEnd - lineStart).normalized;
        // 计算垂线方向（即线段方向的逆时针90度旋转）
        Vector2 perpendicularDirection = new Vector2(-lineDirection.y, lineDirection.x);

        // 计算位移后的位置
        return nearestPoint + perpendicularDirection * -displacement;
    }
    /// //////////////////////////////////////
    public List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
    {
        if (vertices == null || vertices.Count < 3) return null; // 需要至少三个顶点来构成多边形

        List<Vector2> offsetVertices = new List<Vector2>();
        List<Vector2> intersectionPoints = new List<Vector2>();

        int count = vertices.Count;
        for (int i = 0; i < count; i++)
        {
            // 获取当前边的两个顶点
            Vector2 currentVertex = vertices[i];
            Vector2 nextVertex = vertices[(i + 1) % count]; // 环形列表，确保最后一个顶点连接到第一个顶点

            // 计算边的方向向量并旋转90度（垂直）
            Vector2 edgeDirection = (nextVertex - currentVertex).normalized;
            Vector2 perpendicularDirection = new Vector2(-edgeDirection.y, edgeDirection.x);

            // 平移顶点
            Vector2 offsetCurrentVertex = currentVertex + perpendicularDirection * offset;
            Vector2 offsetNextVertex = nextVertex + perpendicularDirection * offset;

            // 存储平移后的顶点，稍后用于计算交点
            offsetVertices.Add(offsetCurrentVertex);
            offsetVertices.Add(offsetNextVertex);

            // 计算交点
            if (i > 0) // 从第二条边开始计算交点
            {
                bool foundIntersection = LineLineIntersection(out Vector2 intersection, offsetVertices[i * 2 - 2], offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection)
                {
                    intersectionPoints.Add(intersection);
                }
            }
        }

        // 计算首尾两条边的交点
        bool foundFinalIntersection = LineLineIntersection(out Vector2 finalIntersection, offsetVertices[offsetVertices.Count - 2], offsetVertices[offsetVertices.Count - 1], offsetVertices[0], offsetVertices[1]);
        if (foundFinalIntersection)
        {
            intersectionPoints.Add(finalIntersection);
        }

        return intersectionPoints;
    }

    private bool LineLineIntersection(out Vector2 intersection, Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        intersection = new Vector2();

        float d = (point1.x - point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x - point4.x);
        if (d == 0) return false; // 平行或共线，无交点

        float pre = (point1.x * point2.y - point1.y * point2.x), post = (point3.x * point4.y - point3.y * point4.x);
        intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d;
        intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d;

        return true;
    }

    //////////////////////////////////////////

    public Vector2 CheckPoint(Vector2 point, float displacement)
    {
        foreach (var box in BoxController.instance.boxes)
        {
            // 缩放多边形顶点
            List<Vector2> movedVertices = CalculateInwardOffset(box.GetRealPoints(), -displacement);
            lineRenderer.positionCount = movedVertices.Count;
            for (int i = 0; i < movedVertices.Count; i++)
            {
                lineRenderer.SetPosition(i, movedVertices[i]);
            }

            
            foreach (var item in movedVertices)
            {
                DebugLogger.Log(item, DebugLogger.Type.err);
            }
            // 使用移动后的顶点来判断点是否在多边形内
            if (IsPointInPolygon(point, movedVertices))
            {
                // 点在调整后的多边形内，返回原始坐标
                DebugLogger.Log(point, DebugLogger.Type.war, "#FF00FF");
                return point;
            }
        }

        // 点不在任何多边形内，计算最近的垂点
        Vector2 nearestPoint = Vector2.zero;
        Vector2 lineStart = Vector2.zero;
        Vector2 lineEnd = Vector2.zero;
        float nearestDistance = float.MaxValue;
        foreach (var box in BoxController.instance.boxes)
        {
            for (int i = 0, j = box.GetRealPoints().Count - 1; i < box.GetRealPoints().Count; j = i++)
            {
                Vector2 tempNearestPoint = GetNearestPointOnLine(point, box.GetRealPoints()[i], box.GetRealPoints()[j]);
                float tempDistance = Vector2.Distance(point, tempNearestPoint);
                if (tempDistance < nearestDistance)
                {
                    nearestPoint = tempNearestPoint;
                    lineStart = box.GetRealPoints()[i];
                    lineEnd = box.GetRealPoints()[j];
                    nearestDistance = tempDistance;
                }
            }
        }

        // 如果找到最近点，则计算位移后的垂点
        if (nearestDistance < float.MaxValue)
        {
            DebugLogger.Log(CalculateDisplacedPoint(nearestPoint, point, lineStart, lineEnd, -displacement), DebugLogger.Type.war, "#FF0000");

            return CalculateDisplacedPoint(nearestPoint, point, lineStart, lineEnd, -displacement);
        }
        DebugLogger.Log(2, DebugLogger.Type.war);

        return point; // 如果没有找到更近的点，返回原点
    }

}

//杂项
/*
 写蓝心的时候无意中搞出来的弹球式蓝心：

  case BattleControl.PlayerColor.blue:
                int gravity = 10;
                if (rigidBody.gravityScale != gravity)
                rigidBody.gravityScale = gravity;
                switch (playerDir)
                {
                    case PlayerDirEnum.up:
                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        Physics2D.gravity = new Vector2(0, 9.8f);
                        break;

                    case PlayerDirEnum.down:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        Physics2D.gravity = new Vector2(0, -9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);

                        if(MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && !isJump)
                        {
                            rigidBody.gravityScale = 0;
                            moving = new Vector3(moving.x, 2.15f);
                            isJump = true;
                        }
                        if (isJump && !MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && moving.y > 0.55f)
                            moving = new Vector3(moving.x, 0.55f);
                        if (isJump)
                        {
                            Vector2 dirReal = new Vector2();
                            switch (playerDir)
                            {
                                case PlayerDirEnum.up:
                                    dirReal = Vector2.up;
                                    break;

                                case PlayerDirEnum.down:
                                    dirReal = Vector2.down;
                                    break;

                                case PlayerDirEnum.left:
                                    dirReal = Vector2.left;
                                    break;

                                case PlayerDirEnum.right:
                                    dirReal = Vector2.right;
                                    break;
                            }
                            Ray2D ray = new Ray2D(transform.position, dirReal);
                            Debug.DrawRay(ray.origin, ray.direction, Color.blue, collideCollider.radius + 0.05f);
                            RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, collideCollider.radius + 0.05f);//距离为圆碰撞器+0.05f
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame")
                                {
                                    isJump = false;
                                }
                            }

                            moving.y -= Time.deltaTime * (float)Math.Pow(1.85f,jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
                            break;

                    case PlayerDirEnum.left:
                        transform.rotation = Quaternion.Euler(0, 0, 270);
                        Physics2D.gravity = new Vector2(-9.8f, 0);
                        break;

                    case PlayerDirEnum.right:
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        Physics2D.gravity = new Vector2(9.8f, 0);
                        break;
                }
                break;
*/