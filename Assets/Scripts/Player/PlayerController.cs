using System.Collections;
using UnityEngine;
/// <summary>
/// 玩家控制脚本
/// </summary>
/// 

enum AttackDirection
{
    None = 0,
    LeftRight = 1,
    Up = 2,
    Down = 3
}

enum PlayerState
{
    Movement = 0,
    Dash = 1,
    Attack = 2
}

public class PlayerController : MonoBehaviour
{
    private PlayerState currentState = PlayerState.Movement;
    private AttackDirection currentAttackDirection = AttackDirection.None;

    Vector3 flippedScale = new Vector3(-1, 1, 1);

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private AnimationCurve jumpForceCurve;

    [SerializeField] private float dashForce = 10f;

    private bool canJumpTwice = true; //是否可以二段跳
    private bool canDash = true; //是否可以冲刺
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashDuration = 0.2f;

    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 0.5f;

    private float moveX;
    private float moveY;
    private int moveChanged = 0;
    private bool isOnGround = true;

    private Rigidbody2D rb;
    private Animator anim;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private bool canCombo = false;

    private void HandleMovementInput()
    {
        //处理移动输入逻辑
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");
    }

    private void HandleDashInput()
    {
        //处理冲刺输入逻辑
        if (Input.GetKeyDown(KeyCode.L) && canDash)
        {
            currentState = PlayerState.Dash;
        }
    }

    private void HandleAttackInput()
    {
        //处理攻击输入逻辑
        if (Input.GetKeyDown(KeyCode.J) && canAttack)
        {
            currentState = PlayerState.Attack;
            //根据按键方向确定攻击方向
            if (moveY > 0)
            {
                currentAttackDirection = AttackDirection.Up;
            }
            else if (moveY < 0)
            {
                currentAttackDirection = AttackDirection.Down;
            }
            else
            {
                currentAttackDirection = AttackDirection.LeftRight;
            }

            if (!canAttack) return;

            if (currentAttackDirection == AttackDirection.LeftRight)
            {
                //水平方向有连击
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack_1") &&
                !anim.GetCurrentAnimatorStateInfo(0).IsName("attack_2"))
                {
                    // 不在攻击状态中，开始第一招
                    anim.SetTrigger("attack");
                    anim.SetInteger("attack_dir", (int)currentAttackDirection);
                    rb.velocity = new Vector2(0, rb.velocity.y); //攻击时水平速度为0

                }
                else if (canCombo)
                {
                    // 在连击窗口内，触发第二招
                    anim.SetBool("attack_twice", true);
                    anim.SetInteger("attack_dir", (int)currentAttackDirection);
                    canCombo = false;
                }
            }
            else
            {
                //上下方向无连击
                anim.SetTrigger("attack");
                anim.SetInteger("attack_dir", (int)currentAttackDirection);
                canAttack = false;
            }
        }
    }

    private void OnAttackEnd()
    {
        //攻击结束后恢复移动状态
        currentState = PlayerState.Movement;
        anim.SetBool("attack_twice", false);
        currentAttackDirection = AttackDirection.None;
        anim.SetInteger("attack_dir", (int)AttackDirection.None);
        StartCoroutine(AttackCooldown(attackCooldown));
    }

    IEnumerator AttackCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    private void HandleInput()
    {
        HandleMovementInput();
        HandleDashInput();
        HandleAttackInput();
    }

    void Update()
    {
        HandleInput();
        switch (currentState)
        {
            case PlayerState.Movement:
                //处理移动状态的逻辑
                Movement();
                Direction();
                Jump();
                break;
            case PlayerState.Dash:
                //处理冲刺状态的逻辑
                Dash();
                break;
            case PlayerState.Attack:
                //处理攻击状态的逻辑
                AttackMove();
                break;
        }
    }

    private void AttackMove()
    {
        //攻击移动逻辑
        float x_speed = speed * (isOnGround ? 1f : 0.5f);
        rb.velocity = new Vector2(moveX * x_speed, rb.velocity.y);
    }

    private void Movement()
    {
        float x_speed = speed * (isOnGround ? 1f : 0.5f);
        rb.velocity = new Vector2(moveX * x_speed, rb.velocity.y);

        if (moveX > 0)
        {
            moveChanged = 1;
        }
        else if (moveX < 0)
        {
            moveChanged = -1;
        }
        else
        {
            moveChanged = 0;
        }

        anim.SetInteger("movement", moveChanged);


        //添加下落时间
        if (!isOnGround && rb.velocity.y < 0)
        {
            fall_time += Time.deltaTime;
            if (fall_time > hardLandingThreshold)
            {
                hardLand = true;
                anim.SetBool("hard_land", hardLand);
            }
        }
    }

    private void Direction()
    {
        if (moveX > 0)
        {
            if (transform.localScale != flippedScale)
            {
                transform.localScale = flippedScale;
                anim.SetTrigger("rotate");
            }
        }
        else if (moveX < 0)
        {
            if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
                anim.SetTrigger("rotate");
            }
        }
    }


    private void Dash()
    {
        if (!canDash) return;
        //冲刺逻辑
        canDash = false; //只能冲刺一次，需在地面重置
        rb.velocity = new Vector2(0, 0); //重置当前速度
        float dashForceDir = transform.localScale.x > 0 ? -1 : 1;
        rb.AddForce(new Vector2(dashForce * dashForceDir, 0), ForceMode2D.Impulse);
        //冲刺时忽略重力影响
        rb.gravityScale = 0;
        anim.SetTrigger("dash");
        //使用协程处理冲刺持续时间和结束后的状态恢复
        StartCoroutine(DashCoroutine(dashDuration));
    }

    //冲刺协程
    IEnumerator DashCoroutine(float dashDuration = 0.2f)
    {
        yield return new WaitForSeconds(dashDuration);
        currentState = PlayerState.Movement;
        rb.gravityScale = 1; //恢复重力影响
        rb.velocity = new Vector2(0, 0);//清空所有冲刺时的速度
        StartCoroutine(DashCooldown(dashCooldown));
    }

    IEnumerator DashCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canDash = true;
    }



    private void EnableCombo()
    {
        canCombo = true;
    }

    private void DisableCombo()
    {
        canCombo = false;
    }


    private float jumpPressTime = 0f;
    [SerializeField] private float maxJumpPressTime = 0.8f;
    private bool isJumping = false;

    private float fall_time = 0f; //记录下落时间
    [SerializeField] private float hardLandingThreshold = 0.5f; //硬着陆阈值
    private bool hardLand = false; //是否硬着陆

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isOnGround)
            {
                jumpPressTime = 0f;
                canJumpTwice = true; //在地面时重置二段跳
                isJumping = true;
                hardLand = false; //重置硬着陆状态
                anim.SetBool("hard_land", hardLand);
                anim.SetTrigger("jump");
                SoundManager.instance.PlaySound(SoundIndex.player_jump);
            }
            else if (canJumpTwice)
            {
                jumpPressTime = 0f;
                canJumpTwice = false; //只能二段跳一次
                isJumping = false;
                DoubleJump();
            }
        }

        if (Input.GetKey(KeyCode.K) && isJumping)
        {
            jumpPressTime += Time.deltaTime;
            jumpPressTime = Mathf.Min(jumpPressTime, maxJumpPressTime);
            float jumpForceFactor = jumpForceCurve.Evaluate(jumpPressTime / maxJumpPressTime);
            rb.AddForce(new Vector2(0, jumpForce * jumpForceFactor), ForceMode2D.Force);
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            jumpPressTime = 0f;
            isJumping = false;
            JumpCancel();
        }
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); //重置垂直速度
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        anim.SetTrigger("jumpTwo");
        SoundManager.instance.PlaySound(SoundIndex.player_jump);
    }

    //判断是否在地面
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Grounding(collision, false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Grounding(collision, false);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Grounding(collision, true);
    }

    private void Grounding(Collision2D col, bool exitState)
    {
        if (exitState)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Terrian"))
            {
                isOnGround = false;
                anim.SetBool("isOnGround", isOnGround);
            }
        }
        else
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Terrian")
            && !isOnGround && col.contacts[0].normal == Vector2.up)
            {
                //在地面的一些处理
                TransitionToGround();
            }
            else if (col.gameObject.layer == LayerMask.NameToLayer("Terrian")
            && !isOnGround && col.contacts[0].normal == Vector2.down)
            {
                isOnGround = false;
                JumpCancel();
            }
        }
        anim.SetBool("isOnGround", isOnGround);
    }

    private void TransitionToGround()
    {
        //在地面
        isOnGround = true;
        JumpCancel();
        fall_time = 0f; //重置下落时间
    }

    private void OnLand()
    {
        SoundManager.instance.PlaySound(SoundIndex.player_softLand);
    }

    private void JumpCancel()
    {
        anim.ResetTrigger("jump");
    }
}
