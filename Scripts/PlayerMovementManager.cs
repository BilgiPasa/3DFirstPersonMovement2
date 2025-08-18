using System;
using UnityEngine;

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
    [HideInInspector] public int vertical, horizontal, runSpeed = 12;
    [HideInInspector] public bool onSlope;
    const int normalGroundLinearDamping = 10; // Don't change this value if not necessary.
    const float theMoveMultiplier = 625.005f, airMoveMultiplier = 0.16f, airLinearDamping = 0.04f, bouncyGroundLinearDamping = 12.5f, minimum = 0.1f; // Don't change these values if not necessary.
    int normalMoveSpeed = 9, crouchSpeed = 6, theMoveSpeed;
    float flatRotationAngleInAir;
    bool normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum;
    Vector2 flatVelocityRelativeToPlayerInAir, normalizedMoveDirectionRelativeToPlayerInAir, normalizedMoveDirectionAsVector2InAir;
    Vector3 normalizedMoveDirection, normalizedSlopeMoveDirection;
    Transform playerTransform;
    Rigidbody playerRigidbody;
    RaycastHit slopeHit;

    [Header("Crouch")]
    [HideInInspector] public float playerHeight = 3, crouchHeight = 2, cameraPositionLocalPositionWhenNotCrouched, cameraPositionLocalPositionWhenCrouched, frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorLocalScaleWhenCrouched;
    [HideInInspector] public bool crouching;
    const float playerWidthRadius = 0.5f, ifPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe = 0.7f, ifPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe = 1.25f;
    bool dontUncrouch;

    [Header("Coyote Time")]
    const float coyoteTime = 0.15f;
    float coyoteTimeCounter;

    [Header("Jump And Fall")]
    [HideInInspector] public float startOfFall, endOfFall, fallDistance;
    [HideInInspector] public bool jumping, groundedForAll;
    const float groundedSphereRadius = 0.3f, jumpingCooldown = 0.1f, jumpAgainCooldown = 0.3f;
    int normalJumpForce = 21, bouncyJumpForce = 56, maxFallWithoutBouncyJumpCalculationByThisScript = 5, maxFallWithoutFallDamage = 15, maxFallWithoutParticles = 5;
    bool readyToJump = true, jumpingInput, groundedForBouncyEnvironment, playerTouchingToAnyGround, falling, wasFalling, wasGrounded, justBeforeGroundedForNormalEnvironment, justBeforeGroundedForBouncyEnvironment;
    Rigidbody objectRigidbodyThatPlayerIsStandingOn;
    RaycastHit whatMovableObjectIsPlayerStandingOnHit;

    [Header("Keybinds")]
    KeyCode forwardKey = KeyCode.W, leftKey = KeyCode.A, backwardKey = KeyCode.S, rightKey = KeyCode.D, jumpKey = KeyCode.Space, crouchKey = KeyCode.LeftShift;

    [Header("Other Things")]
    [HideInInspector] public int playerHealthDecrease;
    PlayerInteractionManager playerInteractionManagerScript;
    PauseMenuManager pauseMenuManagerScript;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;

    [Header("Inputs")]
    [SerializeField] GameObject userInterfaceObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] CapsuleCollider playerColliderCapsuleCollider;
    [SerializeField] ParticleSystem jumpingDownParticles;
    [SerializeField] LayerMask staticNormalLayer, staticBouncyLayer, movableNormalLayer, movableBouncyLayer;

    void Awake()
    {
        playerTransform = transform;
        playerColliderCapsuleCollider.radius = playerWidthRadius;
        cameraPositionLocalPositionWhenNotCrouched = ifPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe * playerHeight / 2;
        cameraPositionLocalPositionWhenCrouched = ifPlayerHeightWouldBe2AndPlayerTransformWouldBeVector3ZeroThenYLocalPositionOfCameraPositionWouldBe * crouchHeight / 2;
        frontBumpingDetectorLocalScaleWhenNotCrouched = ifPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe * playerHeight / 2;
        frontBumpingDetectorLocalScaleWhenCrouched = ifPlayerHeightWouldBe2ThenYLocalScaleOfFrontBumpingDetectorWouldBe * crouchHeight / 2;
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        playerInteractionManagerScript = GetComponent<PlayerInteractionManager>();
        pauseMenuManagerScript = userInterfaceObject.GetComponent<PauseMenuManager>();
        playerSpawnAndSaveManagerScript = userInterfaceObject.GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = userInterfaceObject.GetComponent<PlayerStatusManager>();
    }

    void Update()
    {
        if (!pauseMenuManagerScript.gamePaused)
        {// I didn't added the if not player died condition because if player dies, this script does not work because it is attached to the player.
            MovementInputs();
        }
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        // And also I didn't added the if not player died condition because if player dies, this script does not work because it is attached to the player.
        // These functions' order are intentional, i wouldn't recommend you to change the order.
        GroundedCheckAndFallingCheckAndBouncyJumpAndFallDamageAndCoyoteTime();
        Jump();
        Crouch();
        LinearDamping();
        Movement();
        GravityAndSpeedControl();
        WasFallingAndWasGroundedCheck();
    }

    void MovementInputs()
    {
        // You can use Input.GetAxis... for inputs. But, I wanted to build horizontal and vertical input myself.
        if (Input.GetKey(forwardKey) && !Input.GetKey(backwardKey))
        {
            vertical = 1;
        }
        else if (!Input.GetKey(forwardKey) && Input.GetKey(backwardKey))
        {
            vertical = -1;
        }
        else
        {
            vertical = 0;
        }

        if (Input.GetKey(rightKey) && !Input.GetKey(leftKey))
        {
            horizontal = 1;
        }
        else if (!Input.GetKey(rightKey) && Input.GetKey(leftKey))
        {
            horizontal = -1;
        }
        else
        {
            horizontal = 0;
        }

        jumpingInput = Input.GetKey(jumpKey);
    }

    void GroundedCheckAndFallingCheckAndBouncyJumpAndFallDamageAndCoyoteTime()
    {
        if (!crouching)
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, staticBouncyLayer | movableBouncyLayer);

            // Üstünde durduğun objeyi algılamak için
            // groundedForAll şartının sebebi, o şartı koymazsam, hold ettiği objeyi grounded etmeden altında tutsan bile elinden bırakıyor. Ben grounded ederse bıraksın istiyorum.
            // Bu arada playerHeight / 2 - 1 yazmamın sebebi, sadece playerHeight / 2 yazarsam çalışmıyor ve eğer ki 2 movable objenin üstünde durarsan sadece alttaki objeyi algılıyor. Ben de -playerTransform.up demek 1 metre aşağı anlamına geldiği için belki playerHeight / 2 - 1 yazarsam (yani 1 metre yukarı kaydırırsam) belki çalışır diye düşündüm. Ve çalıştı da!
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, playerHeight / 2 - 1, 0), groundedSphereRadius, -playerTransform.up, out whatMovableObjectIsPlayerStandingOnHit, movableNormalLayer | movableBouncyLayer))
            {
                objectRigidbodyThatPlayerIsStandingOn = whatMovableObjectIsPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objectRigidbodyThatPlayerIsStandingOn && objectRigidbodyThatPlayerIsStandingOn.Equals(playerInteractionManagerScript.grabbedObjectRigidbody) && playerInteractionManagerScript.canReleaseHoldedObjectWhenTouchedToPlayer)
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
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, staticBouncyLayer | movableBouncyLayer);

            // Üstünde durduğun objeyi algılamak için
            // groundedForAll şartının sebebi, yukarıdakiyle aynı.
            // crouchHeight / 2 - 1 yazmamın sebebi, yukarıda playerHeight / 2 - 1 yazmamın sebebiyle aynı.
            if (groundedForAll && Physics.SphereCast(playerTransform.position - new Vector3(0, crouchHeight / 2 - 1, 0), groundedSphereRadius, -playerTransform.up, out whatMovableObjectIsPlayerStandingOnHit, movableNormalLayer | movableBouncyLayer))
            {
                objectRigidbodyThatPlayerIsStandingOn = whatMovableObjectIsPlayerStandingOnHit.rigidbody;

                // Tuttuğun obje ile havada süzülerek inmeyi engellemek için
                if (objectRigidbodyThatPlayerIsStandingOn && objectRigidbodyThatPlayerIsStandingOn.Equals(playerInteractionManagerScript.grabbedObjectRigidbody) && playerInteractionManagerScript.canReleaseHoldedObjectWhenTouchedToPlayer)
                {
                    playerInteractionManagerScript.ReleaseObject();
                }
            }
            else
            {
                objectRigidbodyThatPlayerIsStandingOn = null;
            }
        }

        falling = !groundedForAll && playerRigidbody.linearVelocity.y < -minimum;

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

        if (!wasGrounded && groundedForAll)
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
            //print(fallDistance); // For testing
        }

        if (fallDistance > minimum)
        {
            if (fallDistance > maxFallWithoutParticles && !jumpingDownParticles.isPlaying)
            {
                jumpingDownParticles.Play();
            }

            if (fallDistance > maxFallWithoutBouncyJumpCalculationByThisScript && groundedForBouncyEnvironment && !crouching && readyToJump && !jumping)
            {
                Jumping(bouncyJumpForce);
            }

            if (fallDistance > maxFallWithoutFallDamage && groundedForAll && !groundedForBouncyEnvironment && !playerSpawnAndSaveManagerScript.spawnProtection)
            {
                playerHealthDecrease += (int)fallDistance - maxFallWithoutFallDamage;
            }

            startOfFall = endOfFall = fallDistance = 0;
            wasFalling = wasGrounded = true;
        }

        if (groundedForAll)
        {
            coyoteTimeCounter = coyoteTime;
            justBeforeGroundedForNormalEnvironment = groundedForAll && !groundedForBouncyEnvironment;
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
        {
            if (justBeforeGroundedForNormalEnvironment && ((!groundedForAll && coyoteTimeCounter > 0) || (groundedForAll && !groundedForBouncyEnvironment && playerTouchingToAnyGround)))
            {
                Jumping(normalJumpForce);
            }
            else if (justBeforeGroundedForBouncyEnvironment && ((!groundedForAll && coyoteTimeCounter > 0) || (groundedForAll && groundedForBouncyEnvironment && playerTouchingToAnyGround)))
            {
                Jumping(bouncyJumpForce);
            }
        }
    }

    void Jumping(int jumpForce)
    {
        readyToJump = false;
        jumping = true;
        playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        playerRigidbody.AddForce(jumpForce * playerTransform.up, ForceMode.VelocityChange);
        Invoke(nameof(JumpAgainReset), jumpAgainCooldown);
        Invoke(nameof(JumpingReset), jumpingCooldown);
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

        if (!crouching && Input.GetKey(crouchKey))
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
            dontUncrouch = Physics.CheckCapsule(playerTransform.position + new Vector3(0, playerHeight - crouchHeight / 2 - (playerWidthRadius - 0.01f) - 0.075f, 0), playerTransform.position + new Vector3(0, crouchHeight / 2 - (playerWidthRadius - 0.01f), 0), playerWidthRadius - 0.01f, staticNormalLayer | staticBouncyLayer | movableNormalLayer | movableBouncyLayer);

            if (!Input.GetKey(crouchKey) && !dontUncrouch)
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
        if (groundedForAll && !jumping && !playerStatusManagerScript.sliding)
        {
            playerRigidbody.linearDamping = !groundedForBouncyEnvironment ? normalGroundLinearDamping : bouncyGroundLinearDamping;
        }
        else
        {
            playerRigidbody.linearDamping = airLinearDamping;
        }
    }

    void Movement()
    {
        normalizedMoveDirection = (playerColliderTransform.forward * vertical + playerColliderTransform.right * horizontal).normalized;
        onSlope = ((!crouching && Physics.Raycast(playerTransform.position, -playerTransform.up, out slopeHit, playerHeight / 2 + groundedSphereRadius * 2)) || (crouching && Physics.Raycast(playerTransform.position, -playerTransform.up, out slopeHit, crouchHeight / 2 + groundedSphereRadius * 2))) && slopeHit.normal != playerTransform.up; // slopeHit.normal kısmını sona koyman lazım çünkü Raycast'i bilmeden hit olan şeyi hesaplamaya çalışırsan olmaz.

        if (playerRigidbody.linearDamping != airLinearDamping)
        {
            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
        else
        {
            flatRotationAngleInAir = playerColliderTransform.rotation.eulerAngles.y;
            flatVelocityRelativeToPlayerInAir = RelativeToPlayerConverter(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z, flatRotationAngleInAir);
            normalizedMoveDirectionRelativeToPlayerInAir = RelativeToPlayerConverter(normalizedMoveDirection.x, normalizedMoveDirection.z, flatRotationAngleInAir);
            normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum = normalizedMoveDirectionRelativeToPlayerInAir.y > minimum;
            normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum = normalizedMoveDirectionRelativeToPlayerInAir.y < -minimum;
            normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum = normalizedMoveDirectionRelativeToPlayerInAir.x > minimum;
            normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum = normalizedMoveDirectionRelativeToPlayerInAir.x < -minimum;

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
                        if (normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum || normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum)
                        {
                            if (flatVelocityRelativeToPlayerInAir.x > theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y / 2) * -playerColliderTransform.right, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y / 2) * playerColliderTransform.right, ForceMode.Acceleration);
                            }
                        }
                    }

                    if ((normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum && flatVelocityRelativeToPlayerInAir.x > theMoveSpeed / 2) || (normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum && flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed / 2))
                    {
                        normalizedMoveDirectionRelativeToPlayerInAir.x = 0;
                    }
                    else
                    {
                        if (normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum || normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum)
                        {
                            if (flatVelocityRelativeToPlayerInAir.y > theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x / 2) * -playerColliderTransform.forward, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x / 2) * playerColliderTransform.forward, ForceMode.Acceleration);
                            }
                        }
                    }
                }
            }

            normalizedMoveDirectionAsVector2InAir = RelativeToPlayerConverter(normalizedMoveDirectionRelativeToPlayerInAir.x, normalizedMoveDirectionRelativeToPlayerInAir.y, -flatRotationAngleInAir);
            normalizedMoveDirection = new Vector3(normalizedMoveDirectionAsVector2InAir.x, 0, normalizedMoveDirectionAsVector2InAir.y);

            if (!onSlope)
            {
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * normalizedMoveDirection, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * normalizedSlopeMoveDirection, ForceMode.Acceleration);
            }
        }
    }

    Vector2 RelativeToPlayerConverter(float x, float z, float angle)
    {
        // Gets x and z vectors and an angle. Then returs x and y vectors. x means x vector, y means z vector.
        // If you enter the angle negative and the vectors as relative to player, it returns the vectors as relative to world.
        return new Vector2(x * Mathf.Cos(Mathf.Deg2Rad * angle) - z * MathF.Sin(Mathf.Deg2Rad * angle), x * Mathf.Sin(Mathf.Deg2Rad * angle) + z * Mathf.Cos(Mathf.Deg2Rad * angle));
    }

    void GravityAndSpeedControl()
    {
        if (groundedForAll && playerTouchingToAnyGround && onSlope)
        {
            playerRigidbody.useGravity = crouching || playerRigidbody.linearVelocity.y > minimum;

            if (!crouching && playerRigidbody.linearVelocity.y > minimum)
            {
                playerRigidbody.AddForce(new Vector3(0, 50, 0), ForceMode.Acceleration); // Change this if you change gravity.
            }
            else if (playerStatusManagerScript.sliding)
            {
                playerRigidbody.AddForce(new Vector3(0, 30, 0), ForceMode.Acceleration); // Change this if you change gravity.
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
        else if (playerStatusManagerScript.running)
        {
            theMoveSpeed = runSpeed;
        }
        else
        {
            theMoveSpeed = normalMoveSpeed;
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.z) <= minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.y, 0);
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.x) <= minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, playerRigidbody.linearVelocity.z);
        }

        if (Mathf.Abs(playerRigidbody.linearVelocity.y) <= minimum)
        {
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);
        }
    }

    void WasFallingAndWasGroundedCheck()
    {
        wasFalling = falling;
        wasGrounded = groundedForAll;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            playerTouchingToAnyGround = true;
        }

        if ((collision.gameObject.layer == 7 || collision.gameObject.layer == 8) && collision.rigidbody)
        {
            // Tuttuğun obje ile uçmayı ve sürüklenmeyi engellemek için
            if (collision.rigidbody.Equals(playerInteractionManagerScript.grabbedObjectRigidbody) && playerInteractionManagerScript.canReleaseHoldedObjectWhenTouchedToPlayer)
            {
                playerInteractionManagerScript.ReleaseObject();
            }

            // Üstünde durduğun hareketli yüzeyin hızına göre hareket etmek için
            if (collision.rigidbody.Equals(objectRigidbodyThatPlayerIsStandingOn))
            {
                if (playerRigidbody.linearDamping == normalGroundLinearDamping)
                {
                    playerRigidbody.AddForce(theMoveMultiplier * Time.fixedDeltaTime * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration);
                }
                else if (playerRigidbody.linearDamping == bouncyGroundLinearDamping)
                {
                    playerRigidbody.AddForce(theMoveMultiplier * (Time.fixedDeltaTime * 4 / 3) * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "* 4 / 3"ü deneyerek buldum.
                }
                else
                {
                    playerRigidbody.AddForce(theMoveMultiplier * airMoveMultiplier * (Time.fixedDeltaTime / 49.96f) * objectRigidbodyThatPlayerIsStandingOn.linearVelocity, ForceMode.Acceleration); // Evet, "/ 49.96f"i de deneyerek buldum.
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7 || collision.gameObject.layer == 8)
        {
            playerTouchingToAnyGround = false;
        }
    }
}
