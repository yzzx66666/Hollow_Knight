using System;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 超级冲刺功能
/// </summary>
public class SuperDash : MonoBehaviour
{
    private enum DashState { Idle, Charging, Waiting, Dashing, Stopping };
    private DashState currentState = DashState.Idle;
    [SerializeField] private float chargeTime = 1.0f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashSpeed = 20.0f;
    private float chargeTimer = 0.0f;
    private float dashTimer = 0.0f;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerController playerController;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        switch (currentState)
        {
            case DashState.Idle:
                HandleIdleState();
                break;

            case DashState.Charging:
                HandleChargingState();
                break;

            case DashState.Waiting:
                HandleWaitingState();
                break;

            case DashState.Dashing:
                HandleDashingState();
                break;

            case DashState.Stopping:
                HandleStoppingState();
                break;
        }
    }

    private void ChangeState(DashState newState)
    {
        ExitState(currentState);
        currentState = newState;
        EnterState(newState);
    }

    private void EnterState(DashState newState)
    {
        switch (newState)
        {
            case DashState.Idle:
                playerController.enabled = true;
                break;

            case DashState.Charging:
                playerController.enabled = false;
                chargeTimer = 0.0f;
                animator.SetTrigger("superDash");
                break;

            case DashState.Waiting:
                break;

            case DashState.Dashing:
                dashTimer = 0.0f;
                rb.gravityScale = 0;
                animator.SetTrigger("superDash_sprint");
                break;

            case DashState.Stopping:
                rb.velocity = new Vector2(0, 0);
                rb.gravityScale = 1;
                animator.SetTrigger("superDash_stop");
                break;
        }
    }

    private void ExitState(DashState oldState)
    {
        switch (oldState)
        {
            case DashState.Idle:
                playerController.enabled = false;
                break;

            case DashState.Charging:
                chargeTimer = 0.0f;
                break;

            case DashState.Waiting:
                break;

            case DashState.Dashing:
                dashTimer = 0.0f;
                break;

            case DashState.Stopping:
                break;
        }
    }

    private void HandleIdleState()
    {
        //按下I开始蓄力前摇
        if (Input.GetKeyDown(KeyCode.I))
        {
            ChangeState(DashState.Charging);
        }

    }

    private void HandleChargingState()
    {
        //等待动画播放完毕后开始蓄力等待
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("superDash_charged") && stateInfo.normalizedTime >= 1.0f)
        {
            ChangeState(DashState.Waiting);
        }
        //松开I键则返回Idle状态
        if (Input.GetKeyUp(KeyCode.I))
        {
            animator.SetTrigger("superDash_cancel");
            ChangeState(DashState.Idle);
        }
    }

    private void HandleWaitingState()
    {
        chargeTimer += Time.deltaTime;

        if (chargeTimer >= chargeTime)
        {
            //达到最大蓄力时间后自动进入冲刺状态
            ChangeState(DashState.Dashing);
        }

        //等待松开按键
        if (Input.GetKeyUp(KeyCode.I))
        {
            if (chargeTimer >= chargeTime)
                ChangeState(DashState.Dashing);
            else
            {
                animator.SetTrigger("superDash_cancel");
                ChangeState(DashState.Idle);
            }
                
        }
    }

    private void HandleDashingState()
    {
        dashTimer += Time.deltaTime;
        float dashDir = transform.localScale.x * -1; //根据角色朝向决定冲刺方向
        rb.velocity = new Vector2(dashSpeed * dashDir, 0);

        if (dashTimer >= dashDuration)
        {
            ChangeState(DashState.Stopping);
        }

        if (Input.GetKeyDown(KeyCode.K)) //按下跳跃键停止冲刺
        {
            ChangeState(DashState.Stopping);
        }
    }

    private void HandleStoppingState()
    {
        //等待动画播放完毕后返回Idle状态
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("idle"))
        {
            //恢复玩家控制移动
            playerController.enabled = true;
            ChangeState(DashState.Idle);
        }
    }
}
