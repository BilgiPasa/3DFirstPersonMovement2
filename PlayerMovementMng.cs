using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementMng : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, make the gravity "-60".
    //* In Unity Editor, layer 3 should be "Static Normal Layer".
    //* In Unity Editor, layer 6 should be "Static Bouncy Layer".
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* In the project settings, make the default Physics material a frictionless and not bouncy material.
    //* Make sure that movable objects have a Rigidbody.

    [Header("Horizontal and Vertical")]
    const int NormalGroundLinearDamping = 10; // DO NOT change this value if not necessary.
    int normalMoveSpeed; // If you want to change the value, change it from the PauseMenuManager script.
    const float TheMoveMult = 625.005f, AirMoveMult = 0.16f, AirLinearDamping = 0.04f, BouncyGroundLinearDamping = 12.5f, Min = 0.1f; // DO NOT change these values if not necessary.
    float crouchSpeed, runSpeed, theMoveSpeed, flatRotAngleInAir;
    bool standingOnMovingGround, runningInput, onSlope;
    bool nMDRTPInAirYBiggerThanMin, nMDRTPInAirYSmallerThanNegMin, nMDRTPInAirXBiggerThanMin, nMDRTPInAirXSmallerThanNegMin; // Neg = Negative
    Vector2 inputtedVector2, flatVelRelativeToPlayerInAir;
    Vector2 normdMoveDirRelToPlayerInAir, normdMoveDirAsVector2InAir; // Dir = Direction, Rel = Relative
    Vector3 normdMoveDirection, normdSlopeMoveDirection;
    Transform playerTransform;
    Rigidbody objRbThatPlayerStandingOn, playerRb;
    RaycastHit slopeHit, movableObjPlayerStandingOnHit;

    public int NormalMoveSpeed
    {
        get => normalMoveSpeed;
        set { normalMoveSpeed = value; }
    }

    public float CrouchSpeed
    {
        get => crouchSpeed;
        set { crouchSpeed = value; }
    }

    public float RunSpeed
    {
        get => runSpeed;
        set { runSpeed = value; }
    }

    public bool StandingOnMovingGround
    {
        get => standingOnMovingGround;
        set { standingOnMovingGround = value; }
    }

    public bool RunningInput
    {
        get => runningInput;
    }

    public bool OnSlope
    {
        get => onSlope;
    }

    public Vector2 InputtedVector2
    {
        get => inputtedVector2;
    }

    public Rigidbody ObjRbThatPlayerStandingOn
    {
        get => objRbThatPlayerStandingOn;
    }

    [Header("Crouch")]
    const float PlayerWidthRadius = 0.5f, LocalPosYOfCamPosWhenHeightIs2 = 0.7f, LocalScaleYOfFBDWhenHeightIs2 = 1.25f; // FBD = FrontBumpingDetector
    float playerHeight = 3, crouchHeight = 2;
    float camPosLocalPosWhenNotCrouched, camPosLocalPosWhenCrouched, fBDLocalScaleWhenNotCrouched, fBDLocalScaleWhenCrouched; // fBD = frontBumpingDetector
    bool crouching, crouchingInput, dontUncrouch;

    public float PlayerHeight
    {
        get => playerHeight;
    }

    public float CrouchHeight
    {
        get => crouchHeight;
    }

    public float CamPosLocalPosWhenNotCrouched
    {
        get => camPosLocalPosWhenNotCrouched;
    }

    public float CamPosLocalPosWhenCrouched
    {
        get => camPosLocalPosWhenCrouched;
    }

    public float FBDLocalScaleWhenNotCrouched
    {
        get => fBDLocalScaleWhenNotCrouched;
    }

    public float FBDLocalScaleWhenCrouched
    {
        get => fBDLocalScaleWhenCrouched;
    }

    public bool Crouching
    {
        get => crouching;
        set { crouching = value; }
    }

    [Header("Coyote Time")]
    const float CoyoteTimeSeconds = 0.15f;
    float coyoteTimeCounter;

    [Header("Jump And Fall")]
    const int MaxFallWithoutBouncyJump = 5, MaxFallWithoutParticles = 5;
    int normalJumpForce, bouncyJumpForce; // If you want to change the values, change them from the PauseMenuManager script.
    const float GroundedSphereRadius = 0.3f, JumpingCooldown = 0.1f, JumpAgainCooldown = 0.3f;
    float maxFallWithoutFallDamage = 15, startOfFall, endOfFall, fallDistance;
    bool noFallDamage, jumping, groundedForAll, readyToJump = true, jumpingInput, groundedForBouncyEnvironment, touchingAnyGround, falling, wasFalling, wasTouchingAnyGround;
    bool tAGAndGFA; // tAG = touchingAnyGround, GFA = GroundedForAll
    bool jBGroundedForNormalEnvironment, jBGroundedForBouncyEnvironment; // jB = justBefore

    public int NormalJumpForce
    {
        get => normalJumpForce;
        set { normalJumpForce = value; }
    }

    public int BouncyJumpForce
    {
        get => bouncyJumpForce;
        set { bouncyJumpForce = value; }
    }

    public float MaxFallWithoutFallDamage
    {
        get => maxFallWithoutFallDamage;
    }

    public float StartOfFall
    {
        get => startOfFall;
        set { startOfFall = value; }
    }

    public float EndOfFall
    {
        get => endOfFall;
        set { endOfFall = value; }
    }

    public float FallDistance
    {
        get => fallDistance;
        set { fallDistance = value; }
    }

    public bool NoFallDamage
    {
        get => noFallDamage;
        set { noFallDamage = value; }
    }

    public bool Jumping
    {
        get => jumping;
    }

    public bool GroundedForAll
    {
        get => groundedForAll;
    }

    [Header("Other Things")]
    int playerHealthDecrease;
    PlayerInteractionMng playerInteractionMng;
    PauseMenuMng pauseMenuMng;
    PlayerSpawnAndSaveMng playerSpawnAndSaveMng;
    PlayerStatusMng playerStatusMng;
    InputSystem_Actions inputActions;

    public int PlayerHealthDecrease
    {
        get => playerHealthDecrease;
        set { playerHealthDecrease = value; }
    }

    [Header("Inputs")]
    [SerializeField] GameObject userInterfaceObject;
    [SerializeField] Transform playerCollTransform, camPosTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] CapsuleCollider playerCollCapsuleColl;
    [SerializeField] ParticleSystem jumpingDownParticles;
    [SerializeField] LayerMask staticNormalLayer, staticBouncyLayer, movableNormalLayer, movableBouncyLayer;

    void Awake()
    {
        playerTransform = transform;
        runSpeed = normalMoveSpeed * 4 / 3;
        crouchSpeed = normalMoveSpeed * 2 / 3;
        playerCollCapsuleColl.radius = PlayerWidthRadius;
        camPosLocalPosWhenNotCrouched = LocalPosYOfCamPosWhenHeightIs2 * playerHeight / 2;
        camPosLocalPosWhenCrouched = LocalPosYOfCamPosWhenHeightIs2 * crouchHeight / 2;
        fBDLocalScaleWhenNotCrouched = LocalScaleYOfFBDWhenHeightIs2 * playerHeight / 2;
        fBDLocalScaleWhenCrouched = LocalScaleYOfFBDWhenHeightIs2 * crouchHeight / 2;
        playerRb = GetComponent<Rigidbody>();
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        playerInteractionMng = GetComponent<PlayerInteractionMng>();
        pauseMenuMng = userInterfaceObject.GetComponent<PauseMenuMng>();
        playerSpawnAndSaveMng = userInterfaceObject.GetComponent<PlayerSpawnAndSaveMng>();
        playerStatusMng = userInterfaceObject.GetComponent<PlayerStatusMng>();
    }

    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += JumpInputPerformed;
        inputActions.Player.Jump.canceled += JumpInputCancelled;
        inputActions.Player.Crouch.performed += CrouchInputPerformed;
        inputActions.Player.Crouch.canceled += CrouchInputCancelled;
        inputActions.Player.Run.performed += RunInputPerformed;
        inputActions.Player.Run.canceled += RunInputCancelled;
    }

    void JumpInputPerformed(InputAction.CallbackContext context)
    {
        jumpingInput = true;
    }

    void JumpInputCancelled(InputAction.CallbackContext context)
    {
        jumpingInput = false;
    }

    void CrouchInputPerformed(InputAction.CallbackContext context)
    {
        crouchingInput = true;
    }

    void CrouchInputCancelled(InputAction.CallbackContext context)
    {
        crouchingInput = false;
    }

    void RunInputPerformed(InputAction.CallbackContext context)
    {
        runningInput = true;
    }

    void RunInputCancelled(InputAction.CallbackContext context)
    {
        runningInput = false;
    }

    void Update()
    {
        if (!pauseMenuMng.GamePaused)
        {
            inputtedVector2 = inputActions.Player.Walk.ReadValue<Vector2>();
        }
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        // And also I didn't added the if not player died condition because if player dies, this script does not work because it is attached to the player.
        // These functions' order are intentional, DO NOT change the order if not necessary.
        GroundedCheck();
        FallingCheckAndBouncyJumpAndFallDamage();
        CoyoteTime();
        Jump();
        Crouch();
        LinearDamping();
        Movement();
        GravityAndSpeedControl();
        WasFallingAndWasTouchingToAnyGroundCheck();
    }

    void GroundedCheck()
    {
        if (!crouching)
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), GroundedSphereRadius, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), GroundedSphereRadius, staticBouncyLayer | movableBouncyLayer);

            // Üstünde durduğun objeyi algılamak için
            // groundedForAll şartının sebebi, o şartı koymazsam, hold ettiği objeyi grounded etmeden altında tutsan bile elinden bırakıyor. Ben grounded ederse bıraksın istiyorum.
            // Bu arada playerHeight / 2 - 1 yazmamın sebebi, sadece playerHeight / 2 yazarsam çalışmıyor ve eğer ki 2 movable objenin üstünde durarsan sadece alttaki objeyi algılıyor. Ben de -playerTransform.up demek 1 metre aşağı anlamına geldiği için belki playerHeight / 2 - 1 yazarsam (yani 1 metre yukarı kaydırırsam) belki çalışır diye düşündüm. Ve çalıştı da!
            // SphereCast'in maxDistance'ı olarak da 1 yazdım çünkü test ettiğimde tam istediğim gibi çalışıyor.
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, playerHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out movableObjPlayerStandingOnHit, 1, movableNormalLayer | movableBouncyLayer))
            {
                objRbThatPlayerStandingOn = movableObjPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objRbThatPlayerStandingOn && objRbThatPlayerStandingOn.Equals(playerInteractionMng.GrabbedObjRb) && playerInteractionMng.CanReleaseTouchedObj)
                {
                    playerInteractionMng.ReleaseObj();
                }
            }
            else
            {
                objRbThatPlayerStandingOn = null;
            }
        }
        else
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), GroundedSphereRadius, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), GroundedSphereRadius, staticBouncyLayer | movableBouncyLayer);

            // Üstünde durduğun objeyi algılamak için
            // groundedForAll şartının sebebi, yukarıdakiyle aynı.
            // crouchHeight / 2 - 1 yazmamın sebebi, yukarıda playerHeight / 2 - 1 yazmamın sebebiyle aynı.
            // SphereCast'in maxDistance'ı olarak 1 yazmamın sebebi de yukarıdakiyke aynı.
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, crouchHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out movableObjPlayerStandingOnHit, 1, movableNormalLayer | movableBouncyLayer))
            {
                objRbThatPlayerStandingOn = movableObjPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objRbThatPlayerStandingOn && objRbThatPlayerStandingOn.Equals(playerInteractionMng.GrabbedObjRb) && playerInteractionMng.CanReleaseTouchedObj)
                {
                    playerInteractionMng.ReleaseObj();
                }
            }
            else
            {
                objRbThatPlayerStandingOn = null;
            }
        }

        tAGAndGFA = touchingAnyGround && groundedForAll;
    }

    void FallingCheckAndBouncyJumpAndFallDamage()
    {
        falling = !tAGAndGFA && playerRb.linearVelocity.y < -Min;

        if (!wasFalling && falling)
        {
            if (!crouching)
            {
                startOfFall = playerTransform.position.y - playerHeight / 2;
            }
            else
            {
                startOfFall = playerTransform.position.y - crouchHeight / 2;
            }
        }

        if (!wasTouchingAnyGround && tAGAndGFA)
        {
            if (!crouching)
            {
                endOfFall = playerTransform.position.y - playerHeight / 2;
            }
            else
            {
                endOfFall = playerTransform.position.y - crouchHeight / 2;
            }

            fallDistance = startOfFall - endOfFall;
            //print($"Calculated fall distance is: {fallDistance}"); // For testing
        }

        if (fallDistance > Min)
        {
            if (fallDistance > MaxFallWithoutParticles && !jumpingDownParticles.isEmitting)
            {
                jumpingDownParticles.Play();
            }

            if (fallDistance > MaxFallWithoutBouncyJump && groundedForBouncyEnvironment && !crouching && readyToJump && !jumping)
            {
                ExecutingJump(bouncyJumpForce);
            }

            if (fallDistance > maxFallWithoutFallDamage && !groundedForBouncyEnvironment && !playerSpawnAndSaveMng.SpawnProtection && !noFallDamage)
            {
                playerHealthDecrease += (int)(fallDistance - maxFallWithoutFallDamage);
            }

            startOfFall = endOfFall = fallDistance = 0;
            wasFalling = wasTouchingAnyGround = true;
        }
    }

    void CoyoteTime()
    {
        if (groundedForAll)
        {
            coyoteTimeCounter = CoyoteTimeSeconds;
            jBGroundedForNormalEnvironment = !groundedForBouncyEnvironment;
            jBGroundedForBouncyEnvironment = groundedForBouncyEnvironment;
        }
        else if (coyoteTimeCounter <= 0)
        {
            coyoteTimeCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }

    void Jump()
    {
        if (jumpingInput && readyToJump && !jumping)
        {// Hatıladığım kadarıyla; jBGrounded (justBeforeGrounded) şeylerini eklememin sebebi bir bug'ı engellemek içindi. O bug ise hatırladığım kadarıyla eğer ki bouncy bir yüzeyden zıplayıp sonrasında bir duvara değerek normal zemine düşersen ve düşerken de zıplama tuşuna basılı tutarsan, normal zeminde zıpladığında sanki bouncy zeminde zıplıyormuşsun gibi çok zıplıyorsun.
            if (jBGroundedForNormalEnvironment && ((tAGAndGFA && !groundedForBouncyEnvironment) || (!groundedForAll && coyoteTimeCounter > 0)))
            {
                ExecutingJump(normalJumpForce);
            }
            else if (jBGroundedForBouncyEnvironment && ((tAGAndGFA && groundedForBouncyEnvironment) || (!groundedForAll && coyoteTimeCounter > 0)))
            {
                ExecutingJump(bouncyJumpForce);
            }
        }
    }

    void ExecutingJump(int jumpForce)
    {
        //print($"Jump started at y: {playerTransform.position.y}"); // For testing
        readyToJump = false;
        jumping = true;
        playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        playerRb.AddForce(jumpForce * playerTransform.up, ForceMode.VelocityChange);
        Invoke(nameof(JumpAgainReset), JumpAgainCooldown);
        Invoke(nameof(JumpingReset), JumpingCooldown);
    }

    /* For a continuous jump, use JumpAgainReset. If you don't want to use JumpAgainReset, make a jump buffer
    function and use it but don't forget to add "coyoteTimeCounter = 0;" in your jumping function after the
    jumping force. But you don't need to do that in this script if you are using JumpAgainReset. */
    void JumpAgainReset()
    {
        readyToJump = true;
    }

    void JumpingReset() // For jump height consistency
    {
        jumping = false;
    }

    void Crouch()
    {
        if (jumping)
        {
            return;
        }

        if (!crouching && crouchingInput)
        {
            playerCollCapsuleColl.height = crouchHeight;
            camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, camPosLocalPosWhenCrouched, camPosTransform.localPosition.z);
            frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, fBDLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
            playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, crouchHeight / 2, playerCapsuleModelTransform.localScale.z);

            if (groundedForAll)
            {
                playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
            }

            crouching = true;
            PlayerPrefs.SetInt("playerCrouching", 1);
        }
        else if (crouching)
        {// Bilgi için https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.CheckCapsule.html sitesine bakabilirsin. -0.075f'i de girebildiği ama küçücük bir kısmı CapsuleCollider ile temas ettiği için uncrouch yapamama durumu olmasın diye koydum.
            dontUncrouch = Physics.CheckCapsule(playerTransform.position + new Vector3(0, playerHeight - crouchHeight / 2 - (PlayerWidthRadius - 0.01f) - 0.075f, 0), playerTransform.position + new Vector3(0, crouchHeight / 2 - (PlayerWidthRadius - 0.01f), 0), PlayerWidthRadius - 0.01f, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);

            if (!crouchingInput && !dontUncrouch)
            {
                if (groundedForAll)
                {
                    playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
                }

                playerCollCapsuleColl.height = playerHeight;
                camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, camPosLocalPosWhenNotCrouched, camPosTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, fBDLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerHeight / 2, playerCapsuleModelTransform.localScale.z);
                crouching = false;
                PlayerPrefs.SetInt("playerCrouching", -1);
            }
        }
    }

    void LinearDamping()
    {
        if (groundedForAll && !jumping && !playerStatusMng.Sliding)
        {
            playerRb.linearDamping = !groundedForBouncyEnvironment ? NormalGroundLinearDamping : BouncyGroundLinearDamping;
        }
        else
        {
            playerRb.linearDamping = AirLinearDamping;
        }
    }

    void Movement()
    {
        // Burada fazladan ".normalized" yazmadım çünkü inputtedVector2 zaten normalized olmuş halde.
        normdMoveDirection = inputtedVector2.y * playerCollTransform.forward + inputtedVector2.x * playerCollTransform.right;
        // Burada playerHeight / 2 - 1, crouchHeight / 2 - 1 ve SphereCast'in maxDistance'ı olarak 1 yazmamın sebebini GroundedCheck fonksiyonunun orada anlatmıştım.
        onSlope = ((!crouching && Physics.SphereCast(playerTransform.position - new Vector3(0, playerHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out slopeHit, 1, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer)) || (crouching && Physics.SphereCast(playerTransform.position - new Vector3(0, crouchHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out slopeHit, 1, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer))) && slopeHit.normal != playerTransform.up; // slopeHit.normal kısmını sona koyman lazım çünkü RaycastHit'i bilmeden hit olan şeyi hesaplamaya çalışırsan olmaz.

        if (playerRb.linearDamping != AirLinearDamping)
        {
            if (!onSlope)
            {
                playerRb.AddForce(theMoveSpeed * TheMoveMult * Time.fixedDeltaTime * normdMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normdSlopeMoveDirection = Vector3.ProjectOnPlane(normdMoveDirection, slopeHit.normal);
                playerRb.AddForce(theMoveSpeed * TheMoveMult * Time.fixedDeltaTime * normdSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
        else
        {
            flatRotAngleInAir = playerCollTransform.rotation.eulerAngles.y;
            flatVelRelativeToPlayerInAir = RelativeToPlayerConverter(playerRb.linearVelocity.x, playerRb.linearVelocity.z, flatRotAngleInAir);
            normdMoveDirRelToPlayerInAir = RelativeToPlayerConverter(normdMoveDirection.x, normdMoveDirection.z, flatRotAngleInAir);
            // Aşağıdaki değişkenleri kullanmamın sebebi, daha az işlem yaparak performansı arttırmak için.
            nMDRTPInAirYBiggerThanMin = normdMoveDirRelToPlayerInAir.y > Min;
            nMDRTPInAirYSmallerThanNegMin = normdMoveDirRelToPlayerInAir.y < -Min;
            nMDRTPInAirXBiggerThanMin = normdMoveDirRelToPlayerInAir.x > Min;
            nMDRTPInAirXSmallerThanNegMin = normdMoveDirRelToPlayerInAir.x < -Min;

            if ((nMDRTPInAirYBiggerThanMin && flatVelRelativeToPlayerInAir.y > theMoveSpeed) || (nMDRTPInAirYSmallerThanNegMin && flatVelRelativeToPlayerInAir.y < -theMoveSpeed))
            {
                normdMoveDirRelToPlayerInAir.y = 0;
            }

            if ((nMDRTPInAirXBiggerThanMin && flatVelRelativeToPlayerInAir.x > theMoveSpeed) || (nMDRTPInAirXSmallerThanNegMin && flatVelRelativeToPlayerInAir.x < -theMoveSpeed))
            {
                normdMoveDirRelToPlayerInAir.x = 0;
            }

            if (flatVelRelativeToPlayerInAir.magnitude > theMoveSpeed)
            {
                if ((nMDRTPInAirYBiggerThanMin && flatVelRelativeToPlayerInAir.y < theMoveSpeed && flatVelRelativeToPlayerInAir.y > theMoveSpeed * 0.4f && nMDRTPInAirXBiggerThanMin && flatVelRelativeToPlayerInAir.x < theMoveSpeed && flatVelRelativeToPlayerInAir.x > theMoveSpeed * 0.4f) || (nMDRTPInAirYBiggerThanMin && flatVelRelativeToPlayerInAir.y < theMoveSpeed && flatVelRelativeToPlayerInAir.y > theMoveSpeed * 0.4f && nMDRTPInAirXSmallerThanNegMin && flatVelRelativeToPlayerInAir.x > -theMoveSpeed && flatVelRelativeToPlayerInAir.x < -theMoveSpeed * 0.4f) || (nMDRTPInAirYSmallerThanNegMin && flatVelRelativeToPlayerInAir.y > -theMoveSpeed && flatVelRelativeToPlayerInAir.y < -theMoveSpeed * 0.4f && nMDRTPInAirXBiggerThanMin && flatVelRelativeToPlayerInAir.x < theMoveSpeed && flatVelRelativeToPlayerInAir.x > theMoveSpeed * 0.4f) || (nMDRTPInAirYSmallerThanNegMin && flatVelRelativeToPlayerInAir.y > -theMoveSpeed && flatVelRelativeToPlayerInAir.y < -theMoveSpeed * 0.4f && nMDRTPInAirXSmallerThanNegMin && flatVelRelativeToPlayerInAir.x > -theMoveSpeed && flatVelRelativeToPlayerInAir.x < -theMoveSpeed * 0.4f))
                {
                    normdMoveDirRelToPlayerInAir.y = normdMoveDirRelToPlayerInAir.x = 0;
                }
                else
                {
                    if ((nMDRTPInAirYBiggerThanMin && flatVelRelativeToPlayerInAir.y > theMoveSpeed / 2) || (nMDRTPInAirYSmallerThanNegMin && flatVelRelativeToPlayerInAir.y < -theMoveSpeed / 2))
                    {
                        normdMoveDirRelToPlayerInAir.y = 0;
                    }
                    else
                    {
                        if ((nMDRTPInAirYBiggerThanMin || nMDRTPInAirYSmallerThanNegMin) && !(nMDRTPInAirYBiggerThanMin && flatVelRelativeToPlayerInAir.y < -Min) && !(nMDRTPInAirYSmallerThanNegMin && flatVelRelativeToPlayerInAir.y > Min))
                        {
                            if (flatVelRelativeToPlayerInAir.x > theMoveSpeed)
                            {
                                playerRb.AddForce(theMoveSpeed / 2 * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * Mathf.Abs(normdMoveDirRelToPlayerInAir.y) * -playerCollTransform.right, ForceMode.Acceleration);
                            }
                            else if (flatVelRelativeToPlayerInAir.x < -theMoveSpeed)
                            {
                                playerRb.AddForce(theMoveSpeed / 2 * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * Mathf.Abs(normdMoveDirRelToPlayerInAir.y) * playerCollTransform.right, ForceMode.Acceleration);
                            }
                        }
                    }

                    if ((nMDRTPInAirXBiggerThanMin && flatVelRelativeToPlayerInAir.x > theMoveSpeed / 2) || (nMDRTPInAirXSmallerThanNegMin && flatVelRelativeToPlayerInAir.x < -theMoveSpeed / 2))
                    {
                        normdMoveDirRelToPlayerInAir.x = 0;
                    }
                    else
                    {
                        if ((nMDRTPInAirXBiggerThanMin || nMDRTPInAirXSmallerThanNegMin) && !(nMDRTPInAirXBiggerThanMin && flatVelRelativeToPlayerInAir.x < -Min) && !(nMDRTPInAirXSmallerThanNegMin && flatVelRelativeToPlayerInAir.x > Min))
                        {
                            if (flatVelRelativeToPlayerInAir.y > theMoveSpeed)
                            {
                                playerRb.AddForce(theMoveSpeed / 2 * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * Mathf.Abs(normdMoveDirRelToPlayerInAir.x) * -playerCollTransform.forward, ForceMode.Acceleration);
                            }
                            else if (flatVelRelativeToPlayerInAir.y < -theMoveSpeed)
                            {
                                playerRb.AddForce(theMoveSpeed / 2 * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * Mathf.Abs(normdMoveDirRelToPlayerInAir.x) * playerCollTransform.forward, ForceMode.Acceleration);
                            }
                        }
                    }
                }
            }

            normdMoveDirAsVector2InAir = RelativeToPlayerConverter(normdMoveDirRelToPlayerInAir.x, normdMoveDirRelToPlayerInAir.y, -flatRotAngleInAir);
            normdMoveDirection = new Vector3(normdMoveDirAsVector2InAir.x, 0, normdMoveDirAsVector2InAir.y);

            if (!onSlope)
            {
                playerRb.AddForce(theMoveSpeed * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * normdMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normdSlopeMoveDirection = Vector3.ProjectOnPlane(normdMoveDirection, slopeHit.normal);
                playerRb.AddForce(theMoveSpeed * TheMoveMult * AirMoveMult * Time.fixedDeltaTime * normdSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
    }

    Vector2 RelativeToPlayerConverter(float x, float z, float angle)
    {
        // Gets x and z vectors and an angle. Then returs x and y vectors. x means x vector, y means z vector.
        // If you enter the angle negative and the vectors as relative to player, it returns the vectors as relative to world.
        return new Vector2(x * Mathf.Cos(Mathf.Deg2Rad * angle) - z * Mathf.Sin(Mathf.Deg2Rad * angle), x * Mathf.Sin(Mathf.Deg2Rad * angle) + z * Mathf.Cos(Mathf.Deg2Rad * angle));
    }

    void GravityAndSpeedControl()
    {
        if (tAGAndGFA)
        {
            playerRb.useGravity = onSlope && (playerRb.linearVelocity.y > Min || crouching);

            if (!crouching && playerRb.linearVelocity.y > Min)
            {
                playerRb.AddForce(50 * playerTransform.up, ForceMode.Acceleration); // Change this if you change the gravity. (60 - 10 = 50)
            }
        }
        else
        {
            playerRb.useGravity = true;
        }

        if (crouching)
        {
            theMoveSpeed = crouchSpeed;
        }
        else if (playerStatusMng.Running)
        {
            theMoveSpeed = runSpeed;
        }
        else
        {
            theMoveSpeed = normalMoveSpeed;
        }

        if (Mathf.Abs(playerRb.linearVelocity.z) <= Min)
        {
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, playerRb.linearVelocity.y, 0);
        }

        if (Mathf.Abs(playerRb.linearVelocity.x) <= Min)
        {
            playerRb.linearVelocity = new Vector3(0, playerRb.linearVelocity.y, playerRb.linearVelocity.z);
        }

        if (Mathf.Abs(playerRb.linearVelocity.y) <= Min)
        {
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        }
    }

    void WasFallingAndWasTouchingToAnyGroundCheck()
    {
        wasFalling = falling;
        wasTouchingAnyGround = touchingAnyGround;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tuttuğun obje ile uçmayı ve sürüklenmeyi engellemek için
        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody && collision.rigidbody.Equals(playerInteractionMng.GrabbedObjRb) && playerInteractionMng.CanReleaseTouchedObj)
        {
            playerInteractionMng.ReleaseObjWithVelReset();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            touchingAnyGround = true;
        }
        else
        {
            touchingAnyGround = false;
        }

        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody)
        {
            // Tuttuğun obje ile uçmayı ve sürüklenmeyi engellemek için
            if (collision.rigidbody.Equals(playerInteractionMng.GrabbedObjRb) && playerInteractionMng.CanReleaseTouchedObj)
            {
                playerInteractionMng.ReleaseObjWithVelReset();
            }

            // Üstünde durduğun hareketli yüzeyin hızına göre hareket etmek için
            if (collision.rigidbody.Equals(objRbThatPlayerStandingOn) && collision.rigidbody.linearVelocity.magnitude > Min)
            {
                standingOnMovingGround = true;

                if (playerRb.linearDamping == NormalGroundLinearDamping)
                {
                    playerRb.AddForce(TheMoveMult * Time.fixedDeltaTime * objRbThatPlayerStandingOn.linearVelocity, ForceMode.Acceleration);
                }
                else if (playerRb.linearDamping == BouncyGroundLinearDamping)
                {
                    playerRb.AddForce(TheMoveMult * 4 / 3 * Time.fixedDeltaTime * objRbThatPlayerStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "* 4 / 3"ü deneyerek buldum.
                }
                else
                {
                    playerRb.AddForce(TheMoveMult / 49.96f * AirMoveMult * Time.fixedDeltaTime * objRbThatPlayerStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "/ 49.96f"i de deneyerek buldum.
                }
            }
            else
            {
                standingOnMovingGround = false;
            }
        }
        else
        {
            standingOnMovingGround = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            touchingAnyGround = false;
        }

        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody)
        {
            standingOnMovingGround = false;
        }
    }
}
