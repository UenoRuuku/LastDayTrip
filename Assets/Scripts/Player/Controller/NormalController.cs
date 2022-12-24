using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NormalController : MonoBehaviour
{
    KeyCode D;
    KeyCode A;
    KeyCode W;
    KeyCode S;
    KeyCode J;
    KeyCode K;
    KeyCode Space;

    Vector3 ls;
    Rigidbody2D rb;
    Animator am;
    SpriteRenderer spr;
    bool isRunning;
    [SerializeField]
    bool isJumping;
    bool isFalling;
    bool isOnWall;
    bool isDashing;
    bool isRiding;

    [SerializeField]
    GroundCheck gc;
    [SerializeField]
    GroundCheck wc;
    Cinemachine.CinemachineImpulseSource imp;
    [SerializeField]
    ParticleSystem jumpP;
    [SerializeField]
    ParticleSystem slideP;
    [SerializeField]
    ParticleSystem wallJumpP;
    [SerializeField]
    ParticleSystem dashP;
    [SerializeField]
    DashEffector de;

    Vector2 RideSpeed;

    private void Start()
    {
        D = KeyCodeManager.Instance.Forward;
        A = KeyCodeManager.Instance.Backward;
        W = KeyCodeManager.Instance.Up;
        S = KeyCodeManager.Instance.Down;
        J = KeyCodeManager.Instance.Dash;
        K = KeyCodeManager.Instance.Special;
        Space = KeyCodeManager.Instance.Jump;
        rb = GetComponent<Rigidbody2D>();
        am = GetComponent<Animator>();
        imp = GetComponent<Cinemachine.CinemachineImpulseSource>();
        spr = GetComponent<SpriteRenderer>();
        ls = transform.localScale;
        InertialC.SetAutoKill(false);
    }

    public void UpdateKeys() {
        D = KeyCodeManager.Instance.Forward;
        A = KeyCodeManager.Instance.Backward;
        W = KeyCodeManager.Instance.Up;
        S = KeyCodeManager.Instance.Down;
        J = KeyCodeManager.Instance.Dash;
        K = KeyCodeManager.Instance.Special;
        Space = KeyCodeManager.Instance.Jump;
    }

    [SerializeField]
    float speed = 3;
    [SerializeField]
    int jumpPoint = 1;
    [SerializeField]
    int Max_Jumpoint;

    private void Update()
    {
        isOnGround();
        OnWallCheck();
        OnRideCheck();
        if (!isOnWall) {
            if (!isDashing)
            {
                Move();
                Jump();
            }
            Dash();
        }
        else if(isOnWall && !isDashing) {
            OnWallJump();
        }
        if (!isDashing && !isOnWall) {
            AdjustGravity();
        }
        if (isRiding) {
            RideAnimUpdate();
            RideVelocityAdjust();
        }
        UpdateAnim();
    }
    [SerializeField]
    float InertiaTime;
    Tweener InertialC;
    enum RideType {
        Wall, Ground
    }
    RideType rideType;

    void OnRideCheck() {
        if (gc.isRiding) {
            rideType = RideType.Ground;
            isRiding = true;
        }
        else if (wc.isRiding)
        {
            rideType = RideType.Wall;


            isRiding = true;
        }
        else {
            //切状态
            if (isRiding) {
                InertialC.Pause();
                InertialC = DOVirtual.Vector3(RideSpeed, Vector3.zero, InertiaTime,(Vector3 s)=> { rb.velocity += RideSpeed; });
            }
            if (isOnGround() || isOnWall) {
                InertialC.Pause();
                RideSpeed = Vector2.zero;
            }
            isRiding = false;
        }
    }




    void RideAnimUpdate() {
    }

    void RideVelocityAdjust() {
        if (rideType == RideType.Wall)
        {
            RideSpeed = wc.RidingSpeed;
        
            rb.velocity += RideSpeed;

            //逆向爬墙时略微加速抓的更紧，不修改RideSpeed不影响离开时的惯性
            if (RideSpeed.x * transform.localScale.x > 0) {
                rb.velocity += transform.localScale.x * Vector2.right;
            }
        }
        else if (rideType == RideType.Ground) {

            RideSpeed = gc.RidingSpeed;

            rb.velocity += RideSpeed;
        }
    }

    void Move() {
        if (Input.GetKey(D))
        {
            if (!isWallJumping) {
                rb.velocity = new Vector2(Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y);
            }
            else{
                rb.velocity = Vector2.Lerp(rb.velocity,new Vector2(Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y),2f * Time.deltaTime);
            }
            isRunning = true;
            transform.localScale = new Vector3(ls.x, ls.y);
        }
        if (Input.GetKey(A))
        {
            if (!isWallJumping)
            {
                rb.velocity = new Vector2(-Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y);
            }
            else
            {
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(-Mathf.Abs(transform.localScale.x) * speed, rb.velocity.y), 2f * Time.deltaTime);
            }
            isRunning = true;
            transform.localScale = new Vector3(-ls.x, ls.y);
        }
        if (!Input.GetKey(A) && !Input.GetKey(D) && rb.velocity.x != 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isRunning = false;
        }
    }

    [SerializeField]
    float jumpSpeed = 6;
    [SerializeField]
    float jumpDrag;

    int jumpBuffer = 0;
    [SerializeField]
    int JumpBufferFrame = 6;

    void Jump() {
        if (Input.GetKeyDown(Space))
        {
            jumpBuffer = JumpBufferFrame;
        }
        else if(jumpBuffer > 0){
            jumpBuffer--;
        }
        if (jumpPoint > 0 && jumpBuffer > 0 && !isJumping && !isWallJumping && isOnGround()) {
            jumpBuffer = 0;
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed * transform.localScale.y);
            EndDash();
            RigidbodyDrag(jumpDrag);
            jumpP.Play();
            jumpPoint--;
        }
        if (rb.velocity.y < 0) {
            isJumping = false;
            isWallJumping = false;
            isFalling = true;
            RigidbodyDrag(0);
        }
        if (isOnGround() && isFalling) {
            isFalling = false;
            jumpPoint = Max_Jumpoint;
        }
        //修复吞帧bug
        if (rb.velocity.y == 0 && isJumping && !isFalling && isOnGround()) {
            isJumping = false;
            jumpPoint = Max_Jumpoint;
        }
    }

    Coroutine dashC;
    Tweener DragT;
    [SerializeField]
    float DashRange = 10;
    [SerializeField]
    float DashDuration = 0.8f;
    [SerializeField]
    float DashSpeed = 20;
    enum DashDir {
        None,Up,Down,Left,Right
    }
    DashDir dashDirY;
    DashDir dashDirX;
    int DashPoint = 2;
    int Max_DashPoint = 2;
    int DashBuffer = 0;
    [SerializeField]
    int DashBufferFrame = 6;

    void Dash() {
        if (Input.GetKey(D))
        {
            dashDirX = DashDir.Right;
        }
        else if (Input.GetKey(A))
        {
            dashDirX = DashDir.Left;
        }
        else {
            dashDirX = DashDir.None;
        }
        if (Input.GetKey(W))
        {
            dashDirY = DashDir.Up;
        }
        else if (Input.GetKey(S))
        {
            dashDirY = DashDir.Down;
        }
        else
        {
            dashDirY = DashDir.None;
        }
        if (Input.GetKeyDown(J)){
            DashBuffer = DashBufferFrame;
        }else {
            DashBuffer--;
        }
        if (DashBuffer > 0 && DashPoint > 0 && !isDashing) {
            DashBuffer = 0;
            isDashing = true;
            DashPoint--;
            //冲刺效果
            if (dashDirX != DashDir.None || dashDirY != DashDir.None)
            {
                //屏幕震动
                de.GenerateEffect(transform, DashDuration,spr.sprite);
                imp.GenerateImpulse();
                dashP.Play();
            }
            if (dashC != null)
            {
                StopCoroutine(dashC);
            }
            dashC = StartCoroutine(dash());
        }
        if (isOnGround() || isOnWall && !isDashing) {
            DashPoint = Max_DashPoint;
        }
    }

    IEnumerator dash() {
        am.speed = 0;
        rb.gravityScale = 0;
        Vector2 a = rb.velocity;
        switch (dashDirY) {
            case DashDir.Up:
                rb.velocity = new Vector2(0, DashSpeed * Mathf.Abs(transform.localScale.y));
                break;
            case DashDir.Down:
                rb.velocity = new Vector2(0, -DashSpeed * Mathf.Abs(transform.localScale.y));
                break;
            default:
                rb.velocity = new Vector2(0, 0);
                break;
        }
        switch (dashDirX) { 
            case DashDir.Left:
                rb.velocity = new Vector2(-DashSpeed * Mathf.Abs(transform.localScale.y), rb.velocity.y);
                break;
            case DashDir.Right:
                rb.velocity = new Vector2(DashSpeed * Mathf.Abs(transform.localScale.y), rb.velocity.y);
                break;
            default:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
        if (dashDirX != DashDir.None && dashDirY != DashDir.None) {
            rb.velocity = rb.velocity * 0.7f;
        }
        DragT.Complete();
        DragT = DOVirtual.Float(DashRange, 0, DashDuration, RigidbodyDrag);
        yield return new WaitForSeconds(DashDuration-0.1f);
        dashP.Stop();
        am.speed = 1;
        isDashing = false;
        rb.gravityScale = 1;
        rb.velocity = a;
        yield return null;
    }

    public void RigidbodyDrag(float x) {
        rb.drag = x;
    }

    void EndDash() {
        if (dashC != null)
        {
            StopCoroutine(dashC);
        }
        am.speed = 1;
        isDashing = false;
        rb.gravityScale = 1;
        DragT.Complete();
        RigidbodyDrag(0);
        dashP.Stop();
        DashPoint = Max_DashPoint;

    }


    [SerializeField]
    float JumpMultiplier;
    [SerializeField]
    float FallMultiplier;
    [SerializeField]
    float Max_FallSpeed;
    [SerializeField]
    float Half_GravitySpeed;

    void AdjustGravity() {
        if (rb.velocity.y > 0 && !Input.GetKey(Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * JumpMultiplier * Time.fixedDeltaTime * Mathf.Abs(transform.localScale.y);
            rb.gravityScale = 1;
        }
        else if (rb.velocity.y < 0)
        {
            //print(rb.velocity.y / Mathf.Abs(transform.localScale.y));
            if (rb.velocity.y > -Half_GravitySpeed * Mathf.Abs(transform.localScale.y))
            {
                rb.gravityScale = 0.5f;
            }
            else if (rb.velocity.y > -Max_FallSpeed * Mathf.Abs(transform.localScale.y))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * FallMultiplier * Time.fixedDeltaTime * Mathf.Abs(transform.localScale.y);
                rb.gravityScale = 1;
            }
            else
            {
                rb.gravityScale = 0;
            }
        }
        else {
            rb.gravityScale = 1;
        }
    }

    int OnWallBufferL;
    int OnWallBufferR;
    [SerializeField]
    int OnWallBufferFrame = 6;
    void OnWallCheck() {
        if (isNearWall() && !isOnGroundNoBuffer())
        {
            if (Input.GetKey(D))
            {
                OnWallBufferL = OnWallBufferFrame;
            }
            else
            {
                OnWallBufferL--;
            }
            if (Input.GetKey(A))
            {
                OnWallBufferR = OnWallBufferFrame;
            }
            else
            {
                OnWallBufferR--;
            }
            if ((transform.localScale.x > 0 && OnWallBufferL > 0) || (transform.localScale.x < 0 && OnWallBufferR > 0))
            {
                //向上跳进墙时吞过程
                if (!isOnWall) {
                    isWallJumping = false;
                }
                isOnWall = true;
                slideP.gameObject.SetActive(true);
                EndDash();
            }
            else {
                isOnWall = false;
                slideP.gameObject.SetActive(false);
            }
        }
        else {
            isOnWall = false;
            slideP.gameObject.SetActive(false);
        }
    }
    [SerializeField]
    float WallSpeed;
    [SerializeField]
    float WallJumpSpeedX;
    [SerializeField]
    float WallJumpSpeedY;
    [SerializeField]
    float WallJumpUncontrolableTime = 0.15f;


    bool isWallJumping;

    void OnWallJump() {
        if (!isWallJumping)
        {
            rb.velocity = Vector2.down * Mathf.Abs(transform.localScale.y) * WallSpeed;
            jumpPoint = Max_Jumpoint;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffer = JumpBufferFrame;
        }
        else if (jumpBuffer > 0) {
            jumpBuffer--;
        }
        //蹬墙跳
        if (jumpBuffer > 0 && !isWallJumping) {
            OnWallBufferL = 0;
            OnWallBufferR = 0;
            int i = transform.localScale.x > 0 ? -1 : 1;
            wallJumpP.Play();
            rb.velocity = new Vector2(i * Mathf.Abs(transform.localScale.y) * WallJumpSpeedX, Mathf.Abs(transform.localScale.y) * WallJumpSpeedY);
            isJumping = true;
            isWallJumping = true;
        }
    }


    int OnGroundBuffer = 0;
    [SerializeField]
    int OnGroundBufferFrame = 12;
    bool isOnGround() {
        if (gc.isOnGround)
        {
            OnGroundBuffer = OnGroundBufferFrame;
        }
        else if (OnGroundBuffer > 0) {
            OnGroundBuffer--;
        }
        return OnGroundBuffer > 0 ;
    }

    bool isOnGroundNoBuffer() {
        return gc.isOnGround;
    }

    bool isNearWall() {
        return wc.isOnGround;
    }

    void UpdateAnim() {
        am.SetBool("isRunning", isRunning);
        am.SetBool("isJumping", isJumping);
        am.SetBool("isFalling", isFalling);
        am.SetBool("isOnWall", isOnWall);
    }
}
