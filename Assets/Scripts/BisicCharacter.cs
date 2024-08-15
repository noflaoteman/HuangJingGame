using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BisicCharacter : MonoBehaviour
{

    public AnimancerComponent animancer;
    public ClipTransition idle;
    public ClipTransition shoot;
    public ClipTransition walk;

    /// <summary>
    /// 角色状态
    /// </summary>
    private enum PlayerState
    {
        Move,
        Shoot
    }

    private PlayerState _curState;

    private void OnEnable()
    {
        _curState = PlayerState.Move;
        shoot.Events.OnEnd = OnShootEnd;
    }

    /// <summary>
    /// 射击结束回调
    /// </summary>
    private void OnShootEnd()
    {
        _curState = PlayerState.Move;
    }
    /// <summary>
    /// 移动
    /// </summary>
    private void Move()
    {
        _curState = PlayerState.Move;
        float y = Input.GetAxis("Vertical");
        animancer.Play(y > 0.1f ? walk : idle);
    }
    /// <summary>
    /// 射击
    /// </summary>
    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _curState = PlayerState.Shoot;
            animancer.Play(shoot);
        }
    }

    private void Update()
    {
        switch (_curState)
        {
            case PlayerState.Move:
                Move();
                Shoot();
                break;
            case PlayerState.Shoot:
                Shoot();
                break;
        }
    }


}
