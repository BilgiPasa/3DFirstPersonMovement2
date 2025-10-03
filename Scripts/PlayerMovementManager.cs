using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementManager : MonoBehaviour
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
    int normalMoveSpeed; // This value has getter and setter. Also; if you want to change the value, change it from the PauseMenuManager script.
    const float TheMoveMultiplier = 625.005f, AirMoveMultiplier = 0.16f, AirLinearDamping = 0.04f, BouncyGroundLinearDamping = 12.5f, Minimum = 0.1f; // DO NOT change these values if not necessary.
    float crouchSpeed, runSpeed; // These values have getters and setters.
    float theMoveSpeed, flatRotationAngleInAir;
    bool runningInput, onSlope, standingOnMovingGround; // These values have getters and setters.
    bool normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum;
    Vector2 inputtedVector2; // This value has getter and setter.
    Vector2 flatVelocityRelativeToPlayerInAir, normalizedMoveDirectionRelativeToPlayerInAir, normalizedMoveDirectionAsVector2InAir;
    Vector3 normalizedMoveDirection, normalizedSlopeMoveDirection;
    Transform playerTransform;
    Rigidbody objectRigidbodyThatPlayerIsStandingOn; // This value has getter and setter.
    Rigidbody playerRigidbody;
    RaycastHit slopeHit, whatMovableObjectIsPlayerStandingOnHit;

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

    public bool RunningInput
    {
        get => runningInput;
        set { runningInput = value; }
    }

    public bool OnSlope
    {
        get => onSlope;
        set { onSlope = value; }
    }

    public bool StandingOnMovingGround
    {
        get => standingOnMovingGround;
        set { standingOnMovingGround = value; }
    }

    public Vector2 InputtedVector2
    {
        get => inputtedVector2;
        set { inputtedVector2 = value; }
    }

    public Rigidbody ObjectRigidbodyThatPlayerIsStandingOn
    {
        get => objectRigidbodyThatPlayerIsStandingOn;
        set { objectRigidbodyThatPlayerIsStandingOn = value; }
    }

    [Header("Crouch")]
    const float PlayerWidthRadius = 0.5f, IfPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe = 0.7f, IfPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe = 1.25f;
    float playerHeight = 3, crouchHeight = 2, cameraPositionLocalPositionWhenNotCrouched, cameraPositionLocalPositionWhenCrouched, frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorLocalScaleWhenCrouched; // These values have getters and setters.
    bool crouching; // This value has getter and setter.
    bool crouchingInput, dontUncrouch;

    public float PlayerHeight
    {
        get => playerHeight;
        set { playerHeight = value; }
    }

    public float CrouchHeight
    {
        get => crouchHeight;
        set { crouchHeight = value; }
    }

    public float CameraPositionLocalPositionWhenNotCrouched
    {
        get => cameraPositionLocalPositionWhenNotCrouched;
        set { cameraPositionLocalPositionWhenNotCrouched = value; }
    }

    public float CameraPositionLocalPositionWhenCrouched
    {
        get => cameraPositionLocalPositionWhenCrouched;
        set { cameraPositionLocalPositionWhenCrouched = value; }
    }

    public float FrontBumpingDetectorLocalScaleWhenNotCrouched
    {
        get => frontBumpingDetectorLocalScaleWhenNotCrouched;
        set { frontBumpingDetectorLocalScaleWhenNotCrouched = value; }
    }

    public float FrontBumpingDetectorLocalScaleWhenCrouched
    {
        get => frontBumpingDetectorLocalScaleWhenCrouched;
        set { frontBumpingDetectorLocalScaleWhenCrouched = value; }
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
    const float GroundedSphereRadius = 0.3f, JumpingCooldown = 0.1f, JumpAgainCooldown = 0.3f;
    int normalJumpForce, bouncyJumpForce; // These values have getters and setters. Also; if you want to change the values, change them from the PauseMenuManager script.
    int maxFallWithoutBouncyJumpCalculationByThisScript = 5, maxFallWithoutParticles = 5;
    float maxFallWithoutFallDamage = 15, startOfFall, endOfFall, fallDistance; // These values have getters and setters.
    bool jumping, groundedForAll, noFallDamage; // These values have getters and setters.
    bool readyToJump = true, jumpingInput, groundedForBouncyEnvironment, touchingToAnyGround, touchingToAnyGroundAndGroundedForAll, falling, wasFalling, wasTouchingToAnyGround, justBeforeGroundedForNormalEnvironment, justBeforeGroundedForBouncyEnvironment;

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
        set { maxFallWithoutFallDamage = value; }
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

    public bool Jumping
    {
        get => jumping;
        set { jumping = value; }
    }

    public bool GroundedForAll
    {
        get => groundedForAll;
        set { groundedForAll = value; }
    }

    public bool NoFallDamage
    {
        get => noFallDamage;
        set { noFallDamage = value; }
    }

    [Header("Other Things")]
    int playerHealthDecrease; // This value has getter and setter.
    PlayerInteractionManager playerInteractionManagerScript;
    PauseMenuManager pauseMenuManagerScript;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;
    InputSystem_Actions inputActions;

    public int PlayerHealthDecrease
    {
        get => playerHealthDecrease;
        set { playerHealthDecrease = value; }
    }

    [Header("Inputs")]
    [SerializeField] GameObject userInterfaceObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] CapsuleCollider playerColliderCapsuleCollider;
    [SerializeField] ParticleSystem jumpingDownParticles;
    [SerializeField] LayerMask staticNormalLayer, staticBouncyLayer, movableNormalLayer, movableBouncyLayer;

    void Awake()
    {
        playerTransform = transform;
        runSpeed = normalMoveSpeed * 4 / 3;
        crouchSpeed = normalMoveSpeed * 2 / 3;
        playerColliderCapsuleCollider.radius = PlayerWidthRadius;
        cameraPositionLocalPositionWhenCrouched = IfPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe * crouchHeight / 2;
        frontBumpingDetectorLocalScaleWhenCrouched = IfPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe * crouchHeight / 2;
        cameraPositionLocalPositionWhenNotCrouched = IfPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe * playerHeight / 2;
        frontBumpingDetectorLocalScaleWhenNotCrouched = IfPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe * playerHeight / 2;
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        playerInteractionManagerScript = GetComponent<PlayerInteractionManager>();
        pauseMenuManagerScript = userInterfaceObject.GetComponent<PauseMenuManager>();
        playerSpawnAndSaveManagerScript = userInterfaceObject.GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = userInterfaceObject.GetComponent<PlayerStatusManager>();
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
        if (!pauseMenuManagerScript.GamePaused)
        {
            jumpingInput = true;
        }
    }

    void JumpInputCancelled(InputAction.CallbackContext context)
    {
        jumpingInput = false;
    }

    void CrouchInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.GamePaused)
        {
            crouchingInput = true;
        }
    }

    void CrouchInputCancelled(InputAction.CallbackContext context)
    {
        crouchingInput = false;
    }

    void RunInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.GamePaused)
        {
            runningInput = true;
        }
    }

    void RunInputCancelled(InputAction.CallbackContext context)
    {
        runningInput = false;
    }

    void Update()
    {
        if (!pauseMenuManagerScript.GamePaused)
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
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, playerHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out whatMovableObjectIsPlayerStandingOnHit, 1, movableNormalLayer | movableBouncyLayer))
            {
                objectRigidbodyThatPlayerIsStandingOn = whatMovableObjectIsPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objectRigidbodyThatPlayerIsStandingOn && objectRigidbodyThatPlayerIsStandingOn.Equals(playerInteractionManagerScript.GrabbedObjectRigidbody) && playerInteractionManagerScript.CanReleaseHoldedObjectWhenTouchedToPlayer)
                {
                    playerInteractionManagerScript.ReleaseObject();
                }
            }
            else
            {
                objectRigidbodyThatPlayerIsStandingOn = null;
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
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, crouchHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out whatMovableObjectIsPlayerStandingOnHit, 1, movableNormalLayer | movableBouncyLayer))
            {
                objectRigidbodyThatPlayerIsStandingOn = whatMovableObjectIsPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objectRigidbodyThatPlayerIsStandingOn && objectRigidbodyThatPlayerIsStandingOn.Equals(playerInteractionManagerScript.GrabbedObjectRigidbody) && playerInteractionManagerScript.CanReleaseHoldedObjectWhenTouchedToPlayer)
                {
                    playerInteractionManagerScript.ReleaseObject();
                }
            }
            else
            {
                objectRigidbodyThatPlayerIsStandingOn = null;
            }
        }

        touchingToAnyGroundAndGroundedForAll = touchingToAnyGround && groundedForAll;
    }

    void FallingCheckAndBouncyJumpAndFallDamage()
    {
        falling = !touchingToAnyGroundAndGroundedForAll && playerRigidbody.linearVelocity.y < -Minimum;

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

        if (!wasTouchingToAnyGround && touchingToAnyGroundAndGroundedForAll)
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

        if (fallDistance > Minimum)
        {
            if (fallDistance > maxFallWithoutParticles && !jumpingDownParticles.isEmitting)
            {
                jumpingDownParticles.Play();
            }

            if (fallDistance > maxFallWithoutBouncyJumpCalculationByThisScript && groundedForBouncyEnvironment && !crouching && readyToJump && !jumping)
            {
                ExecutingJump(bouncyJumpForce);
            }

            if (fallDistance > maxFallWithoutFallDamage && !groundedForBouncyEnvironment && !playerSpawnAndSaveManagerScript.SpawnProtection && !noFallDamage)
            {
                playerHealthDecrease += (int)(fallDistance - maxFallWithoutFallDamage);
            }

            startOfFall = endOfFall = fallDistance = 0;
            wasFalling = wasTouchingToAnyGround = true;
        }
    }

    void CoyoteTime()
    {
        if (groundedForAll)
        {
            coyoteTimeCounter = CoyoteTimeSeconds;
            justBeforeGroundedForNormalEnvironment = !groundedForBouncyEnvironment;
            justBeforeGroundedForBouncyEnvironment = groundedForBouncyEnvironment;
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
        {// Hatıladığım kadarıyla; justBeforeGrounded şeylerini eklememin sebebi bir bug'ı engellemek içindi. O bug ise hatırladığım kadarıyla eğer ki bouncy bir yüzeyden zıplayıp sonrasında bir duvara değerek normal zemine düşersen ve düşerken de zıplama tuşuna basılı tutarsan, normal zeminde zıpladığında sanki bouncy zeminde zıplıyormuşsun gibi çok zıplıyorsun.
            if (justBeforeGroundedForNormalEnvironment && ((touchingToAnyGroundAndGroundedForAll && !groundedForBouncyEnvironment) || (!groundedForAll && coyoteTimeCounter > 0)))
            {
                ExecutingJump(normalJumpForce);
            }
            else if (justBeforeGroundedForBouncyEnvironment && ((touchingToAnyGroundAndGroundedForAll && groundedForBouncyEnvironment) || (!groundedForAll && coyoteTimeCounter > 0)))
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
        playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        playerRigidbody.AddForce(jumpForce * playerTransform.up, ForceMode.VelocityChange);
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
            playerColliderCapsuleCollider.height = crouchHeight;
            cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, cameraPositionLocalPositionWhenCrouched, cameraPositionTransform.localPosition.z);
            frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, frontBumpingDetectorLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
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

                playerColliderCapsuleCollider.height = playerHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, cameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerHeight / 2, playerCapsuleModelTransform.localScale.z);
                crouching = false;
                PlayerPrefs.SetInt("playerCrouching", -1);
            }
        }
    }

    void LinearDamping()
    {
        if (groundedForAll && !jumping && !playerStatusManagerScript.Sliding)
        {
            playerRigidbody.linearDamping = !groundedForBouncyEnvironment ? NormalGroundLinearDamping : BouncyGroundLinearDamping;
        }
        else
        {
            playerRigidbody.linearDamping = AirLinearDamping;
        }
    }

    void Movement()
    {
        // Burada fazladan ".normalized" yazmadım çünkü inputtedVector2 zaten normalized olmuş halde.
        normalizedMoveDirection = playerColliderTransform.forward * inputtedVector2.y + playerColliderTransform.right * inputtedVector2.x;
        // Burada playerHeight / 2 - 1, crouchHeight / 2 - 1 ve SphereCast'in maxDistance'ı olarak 1 yazmamın sebebini GroundedCheck fonksiyonunun orada anlatmıştım.
        onSlope = ((!crouching && Physics.SphereCast(playerTransform.position - new Vector3(0, playerHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out slopeHit, 1, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer)) || (crouching && Physics.SphereCast(playerTransform.position - new Vector3(0, crouchHeight / 2 - 1, 0), GroundedSphereRadius, -playerTransform.up, out slopeHit, 1, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer))) && slopeHit.normal != playerTransform.up; // slopeHit.normal kısmını sona koyman lazım çünkü RaycastHit'i bilmeden hit olan şeyi hesaplamaya çalışırsan olmaz.

        if (playerRigidbody.linearDamping != AirLinearDamping)
        {
            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * TheMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * TheMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
        else
        {
            flatRotationAngleInAir = playerColliderTransform.rotation.eulerAngles.y;
            flatVelocityRelativeToPlayerInAir = RelativeToPlayerConverter(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z, flatRotationAngleInAir);
            normalizedMoveDirectionRelativeToPlayerInAir = RelativeToPlayerConverter(normalizedMoveDirection.x, normalizedMoveDirection.z, flatRotationAngleInAir);
            normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum = normalizedMoveDirectionRelativeToPlayerInAir.y > Minimum;
            normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum = normalizedMoveDirectionRelativeToPlayerInAir.y < -Minimum;
            normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum = normalizedMoveDirectionRelativeToPlayerInAir.x > Minimum;
            normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum = normalizedMoveDirectionRelativeToPlayerInAir.x < -Minimum;

            if ((normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.y > theMoveSpeed) || (normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed))
            {
                normalizedMoveDirectionRelativeToPlayerInAir.y = 0;
            }

            if ((normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x > theMoveSpeed) || (normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed))
            {
                normalizedMoveDirectionRelativeToPlayerInAir.x = 0;
            }

            if (flatVelocityRelativeToPlayerInAir.magnitude > theMoveSpeed)
            {
                if ((normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.y < theMoveSpeed && flatVelocityRelativeToPlayerInAir.y > theMoveSpeed * 0.4f && normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x < theMoveSpeed && flatVelocityRelativeToPlayerInAir.x > theMoveSpeed * 0.4f) || (normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.y < theMoveSpeed && flatVelocityRelativeToPlayerInAir.y > theMoveSpeed * 0.4f && normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x > -theMoveSpeed && flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed * 0.4f) || (normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.y > -theMoveSpeed && flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed * 0.4f && normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x < theMoveSpeed && flatVelocityRelativeToPlayerInAir.x > theMoveSpeed * 0.4f) || (normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.y > -theMoveSpeed && flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed * 0.4f && normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x > -theMoveSpeed && flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed * 0.4f))
                {
                    normalizedMoveDirectionRelativeToPlayerInAir.y = normalizedMoveDirectionRelativeToPlayerInAir.x = 0;
                }
                else
                {
                    if ((normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.y > theMoveSpeed / 2) || (normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed / 2))
                    {
                        normalizedMoveDirectionRelativeToPlayerInAir.y = 0;
                    }
                    else
                    {
                        if ((normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum || normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum) && !(normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.y < -Minimum) && !(normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.y > Minimum))
                        {
                            if (flatVelocityRelativeToPlayerInAir.x > theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed / 2 * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y) * -playerColliderTransform.right, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed / 2 * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y) * playerColliderTransform.right, ForceMode.Acceleration);
                            }
                        }
                    }

                    if ((normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x > theMoveSpeed / 2) || (normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed / 2))
                    {
                        normalizedMoveDirectionRelativeToPlayerInAir.x = 0;
                    }
                    else
                    {
                        if ((normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum || normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum) && !(normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x < -Minimum) && !(normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x > Minimum))
                        {
                            if (flatVelocityRelativeToPlayerInAir.y > theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed / 2 * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x) * -playerColliderTransform.forward, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed / 2 * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x) * playerColliderTransform.forward, ForceMode.Acceleration);
                            }
                        }
                    }
                }
            }

            normalizedMoveDirectionAsVector2InAir = RelativeToPlayerConverter(normalizedMoveDirectionRelativeToPlayerInAir.x, normalizedMoveDirectionRelativeToPlayerInAir.y, -flatRotationAngleInAir);
            normalizedMoveDirection = new Vector3(normalizedMoveDirectionAsVector2InAir.x, 0, normalizedMoveDirectionAsVector2InAir.y);

            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * TheMoveMultiplier * AirMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
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
        if (touchingToAnyGroundAndGroundedForAll)
        {
            playerRigidbody.useGravity = onSlope && (playerRigidbody.linearVelocity.y > Minimum || crouching);

            if (!crouching && playerRigidbody.linearVelocity.y > Minimum)
            {
                playerRigidbody.AddForce(50 * playerTransform.up, ForceMode.Acceleration); // Change this if you change the gravity. (60 - 10 = 50)
            }
        }
        else
        {
            playerRigidbody.useGravity = true;
        }

        if (crouching)
        {
            theMoveSpeed = crouchSpeed;
        }
        else if (playerStatusManagerScript.Running)
        {
            theMoveSpeed = runSpeed;
        }
        else
        {
            theMoveSpeed = normalMoveSpeed;
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.z) <= Minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.y, 0);
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.x) <= Minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, playerRigidbody.linearVelocity.z);
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.y) <= Minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        }
    }

    void WasFallingAndWasTouchingToAnyGroundCheck()
    {
        wasFalling = falling;
        wasTouchingToAnyGround = touchingToAnyGround;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tuttuğun obje ile uçmayı ve sürüklenmeyi engellemek için
        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody && collision.rigidbody.Equals(playerInteractionManagerScript.GrabbedObjectRigidbody) && playerInteractionManagerScript.CanReleaseHoldedObjectWhenTouchedToPlayer)
        {
            playerInteractionManagerScript.ReleaseObjectWithResettingLinearAndAngularVelocity();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            touchingToAnyGround = true;
        }
        else
        {
            touchingToAnyGround = false;
        }

        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody)
        {
            // Tuttuğun obje ile uçmayı ve sürüklenmeyi engellemek için
            if (collision.rigidbody.Equals(playerInteractionManagerScript.GrabbedObjectRigidbody) && playerInteractionManagerScript.CanReleaseHoldedObjectWhenTouchedToPlayer)
            {
                playerInteractionManagerScript.ReleaseObjectWithResettingLinearAndAngularVelocity();
            }

            // Üstünde durduğun hareketli yüzeyin hızına göre hareket etmek için
            if (collision.rigidbody.Equals(objectRigidbodyThatPlayerIsStandingOn) && collision.rigidbody.linearVelocity.magnitude > Minimum)
            {
                standingOnMovingGround = true;

                if (playerRigidbody.linearDamping == NormalGroundLinearDamping)
                {
                    playerRigidbody.AddForce(TheMoveMultiplier * Time.fixedDeltaTime * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration);
                }
                else if (playerRigidbody.linearDamping == BouncyGroundLinearDamping)
                {
                    playerRigidbody.AddForce(TheMoveMultiplier * 4 / 3 * Time.fixedDeltaTime * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "* 4 / 3"ü deneyerek buldum.
                }
                else
                {
                    playerRigidbody.AddForce(TheMoveMultiplier / 49.96f * AirMoveMultiplier * Time.fixedDeltaTime * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "/ 49.96f"i de deneyerek buldum.
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
            touchingToAnyGround = false;
        }

        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody)
        {
            standingOnMovingGround = false;
        }
    }
}
