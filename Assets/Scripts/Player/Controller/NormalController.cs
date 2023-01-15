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

    [Header("---- Outer GameObject ----")]
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
        TimeBackQueue = new Queue<NormalControllerPlayerStatus>();
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
        else if (isOnWall && !isDashing) {
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
        UpdateTimeBack();
        Back();
    }


    #region Ride

    [Header("---- Ride ----")]
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
                InertialC = DOVirtual.Vector3(RideSpeed, Vector3.zero, InertiaTime, (Vector3 s) => { rb.velocity += RideSpeed; });
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

    #endregion

    #region Move
    [Header("---- Move ----")]
    [SerializeField]
    float speed = 15;
    [SerializeField]
    int AccelerateFrameSpeed = 1;
    [SerializeField]
    int DecelerateFrameSpeed = 2;
    [SerializeField]
    int MoveFrame = 6;
    int MoveBuffer = 0;
    int dir = 1;
    void Move() {
        if (Input.GetKey(D))
        {
            if (MoveBuffer < MoveFrame)
            {
                MoveBuffer += AccelerateFrameSpeed;
            }
            dir = 1;
        }
        else if (Input.GetKey(A))
        {
            if (MoveBuffer < MoveFrame)
            {
                MoveBuffer += AccelerateFrameSpeed;
            }
            dir = -1;
        }
        else if (MoveBuffer > 0) {
            MoveBuffer -= DecelerateFrameSpeed;
        }

        if (MoveBuffer > 0)
        {
            transform.localScale = new Vector3(dir * ls.x, ls.y, ls.z);
            isRunning = true;
        }
        else {
            isRunning = false;
        }

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(dir * Mathf.Abs(transform.localScale.x) * speed * MoveBuffer / MoveFrame, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(Mathf.Abs(transform.localScale.x) * speed * dir, rb.velocity.y), 2f * Time.deltaTime);
        }
    }

    #endregion

    #region Jump
    int jumpPoint = 1;
    [Header("---- Jump ----")]
    [SerializeField]
    int Max_Jumpoint;
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
        else if (jumpBuffer > 0) {
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
    #endregion

    #region Dash
    Coroutine dashC;
    Tweener DragT;
    [Header("---- Dash ----")]
    [SerializeField]
    float DashRange = 10;
    [SerializeField]
    float DashDuration = 0.8f;
    [SerializeField]
    float DashSpeed = 20;
    enum DashDir {
        None, Up, Down, Left, Right
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
        if (Input.GetKeyDown(J)) {
            DashBuffer = DashBufferFrame;
        } else {
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
                de.GenerateEffect(transform, DashDuration, spr.sprite);
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
        yield return new WaitForSeconds(DashDuration - 0.1f);
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

    #endregion

    #region Gravity
    [Header("---- Gravity Adjuster ----")]
    [SerializeField]
    float JumpMultiplier;
    [SerializeField]
    float FallMultiplier;
    [SerializeField]
    float Max_FallSpeed;
    [SerializeField]
    float Half_GravitySpeed;

    void AdjustGravity() {
        if (rb.velocity.y > 0 && !Input.GetKey(Space) && !isRiding)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * JumpMultiplier * Time.fixedDeltaTime * Mathf.Abs(transform.localScale.y);
            rb.gravityScale = 1;
        }
        else if (rb.velocity.y < 0 && !isRiding)
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
    #endregion

    #region OnWallCheck
    int OnWallBufferL;
    int OnWallBufferR;
    [Header("---- OnWall Check ----")]
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

    #endregion

    #region OnWall Jump
    [Header("---- OnWall Jump ----")]
    [SerializeField]
    float WallSpeed;
    [SerializeField]
    float WallJumpSpeedX;
    [SerializeField]
    float WallJumpSpeedY;


    bool isWallJumping;

    void OnWallJump() {
        if (!isWallJumping)
        {
            if (Input.GetKey(S))
            {

                rb.velocity = Vector2.down * Mathf.Abs(transform.localScale.y) * WallSpeed;

            }
            else {
                rb.velocity = Vector2.down * 0;
            }
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
        if (jumpBuffer > 0 && !isWallJumping && jumpPoint > 0) {
            OnWallBufferL = 0;
            OnWallBufferR = 0;
            jumpPoint--;
            int i = transform.localScale.x > 0 ? -1 : 1;
            wallJumpP.Play();
            rb.velocity = new Vector2(i * Mathf.Abs(transform.localScale.y) * WallJumpSpeedX, Mathf.Abs(transform.localScale.y) * WallJumpSpeedY);
            isJumping = true;
            isWallJumping = true;
        }
    }


    #endregion

    #region OnGround Check
    int OnGroundBuffer = 0;
    [Header("---- OnGround Check ----")]
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
        return OnGroundBuffer > 0;
    }

    bool isOnGroundNoBuffer() {
        return gc.isOnGround;
    }

    bool isNearWall() {
        return wc.isOnGround;
    }

    #endregion
    void UpdateAnim() {
        am.SetBool("isRunning", isRunning);
        am.SetBool("isJumping", isJumping);
        am.SetBool("isFalling", isFalling);
        am.SetBool("isOnWall", isOnWall);
    }

    public void Stop() {
        EndDash();
        enabled = false;
    }

    #region back
    [SerializeField]
    int TimeBackLength = 3;
    Queue<NormalControllerPlayerStatus> TimeBackQueue;
    void UpdateTimeBack() {
        TimeBackQueue.Enqueue(new NormalControllerPlayerStatus(transform.position, isRunning, isJumping, isOnWall, isFalling, transform.localScale.x > 0?1:-1));
        if (TimeBackQueue.Count > TimeBackLength / Time.deltaTime) {
            TimeBackQueue.Dequeue();
        }
    }

    void Back() {
        if (Input.GetKeyDown(K)) {
            transform.position = GetTimeBackStatus().pos;
            isRunning = GetTimeBackStatus().isRunning;
            isJumping = GetTimeBackStatus().isJumping;
            isFalling = GetTimeBackStatus().isFalling;
            transform.localScale = new Vector3(Mathf.Abs(ls.x) * GetTimeBackStatus().scale, ls.y, ls.z);
            TimeBackQueue.Clear();
        }
    }

    public NormalControllerPlayerStatus GetTimeBackStatus() {
        if (TimeBackQueue.Count > 0)
        {
            return TimeBackQueue.Peek();

        }
        else {
            return null;
        }
    }
    #endregion
}


public class NormalControllerPlayerStatus {
    public NormalControllerPlayerStatus(Vector2 p, bool r, bool j, bool of, bool f, int s) {
        pos = p;
        isRunning = r;
        isJumping = j;
        isOnWall = of;
        isFalling = f;
        scale = s;
    }
    public Vector2 pos {
        get;private set;
    }
    public bool isRunning
    {
        get; private set;
    }
    public bool isJumping
    {
        get; private set;
    }
    public bool isOnWall
    {
        get; private set;
    }
    public bool isFalling
    {
        get; private set;
    }

    public int scale {
        get;private set;
    }
}
