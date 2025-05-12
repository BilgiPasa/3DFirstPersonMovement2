using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovementManager : MonoBehaviour
{
    //* In Unity Editor, make the gravity "-60".
    //* In Unity Editor, layer 3 should be "Normal Layer".
    //* In Unity Editor, layer 6 should be "Bouncy Layer".
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In the project settings, make the default Physics material a frictionless and not bouncy material.

    [Header("Horizontal and Vertical")]
    public static int vertical, horizontal, runSpeed = 12;
    public static bool onSlope;
    int normalMoveSpeed = 9, crouchSpeed = 6, normalGroundLinearDamping = 10, theMoveSpeed;
    float theMoveMultiplier = 625.005f, airMoveMultiplier = 0.16f, minimum = 0.1f, airLinearDamping = 0.04f, bouncyGroundLinearDamping = 12.5f, flatRotationAngleInAir;
    bool normalizedMoveDirectionRelativeToPlayerInAirYIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirYIsSmallerThanMinusMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsBiggerThanMinimum, normalizedMoveDirectionRelativeToPlayerInAirXIsSmallerThanMinusMinimum;
    Vector2 flatVelocityRelativeToPlayerInAir, normalizedMoveDirectionRelativeToPlayerInAir, normalizedMoveDirectionAsVector2;
    Vector3 normalizedMoveDirection, normalizedSlopeMoveDirection;
    RaycastHit slopeHit;

    [Header("Crouch")]
    public static float playerHeightForOtherScripts, crouchHeightForOtherScripts, playerWidthRadiusForOtherScripts;
    public static bool crouching;
    bool inCrouchingProcess, dontUncrouch;

    [Header("Coyote Time")]
    float coyoteTime = 0.15f, coyoteTimeCounter;

    [Header("Jump And Fall")]
    public static float startOfFall, endOfFall, fallDistance;
    public static bool groundedForAll, wasGrounded, jumping;
    int normalJumpForce = 21, bouncyJumpForce = 56, maxFallWithoutBouncyJumpCalculationByThisScript = 5, maxFallWithoutFallDamage = 15, maxFallWithoutParticles = 5;
    float jumpingCooldown = 0.1f, jumpAgainCooldown = 0.3f, groundedSphereRadius = 0.3f;
    bool readyToJump = true, jumpingInput, falling, wasFalling, groundedForBouncyEnvironment, justBeforeGroundedForNormalEnvironment, justBeforeGroundedForBouncyEnvironment, playerTouchingToAnyGround, playerStandingOnMovableGround;

    [Header("Keybinds")]
    KeyCode forwardKey = KeyCode.W, leftKey = KeyCode.A, backwardKey = KeyCode.S, rightKey = KeyCode.D, jumpKey = KeyCode.Space, crouchKey = KeyCode.LeftShift;

    [Header("Inputs")]
    [SerializeField] float playerHeight = 3;
    [SerializeField] float crouchHeight = 2;
    [SerializeField] float playerWidthRadius = 0.5f;
    [SerializeField] Transform playerModelTransform;
    [SerializeField] CapsuleCollider playerCapsuleCollider;
    [SerializeField] ParticleSystem jumpingDownParticles;
    [SerializeField] LayerMask normalLayer, bouncyLayer, movableNormalLayer;
    Transform playerTransform;
    Rigidbody playerRigidbody;

    void Awake()
    {
        playerHeightForOtherScripts = playerHeight;
        crouchHeightForOtherScripts = crouchHeight;
        playerWidthRadiusForOtherScripts = playerWidthRadius;
        playerTransform = transform;
        playerCapsuleCollider.height = 2;
        playerCapsuleCollider.radius = 0.5f;
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!PauseMenuManager.gamePaused && !PlayerSpawnAndSaveManager.playerDied)
        {
            MovementInputs();
        }
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        if (!PlayerSpawnAndSaveManager.playerDied)
        {// These functions' order are intentional, i wouldn't recommend you to change the order.
            GroundedCheckAndFallingCheckAndBouncyJumpAndCoyoteTimeAndFallDamage();
            Jump();
            Crouch();
            LinearDamping();

            if (!inCrouchingProcess) // For not to gain speed when you crouch
            {
                Movement();
            }

            WasFallingAndWasGroundedCheck();
            GravityAndSpeedControl();
        }
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

    void GroundedCheckAndFallingCheckAndBouncyJumpAndCoyoteTimeAndFallDamage()
    {
        if (!crouching)
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, normalLayer | bouncyLayer | movableNormalLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, bouncyLayer);
            playerStandingOnMovableGround = Physics.CheckSphere(playerTransform.position - new Vector3(0, playerHeight / 2, 0), groundedSphereRadius, movableNormalLayer);
        }
        else
        {
            groundedForAll = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, normalLayer | bouncyLayer);
            groundedForBouncyEnvironment = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, bouncyLayer);
            playerStandingOnMovableGround = Physics.CheckSphere(playerTransform.position - new Vector3(0, crouchHeight / 2, 0), groundedSphereRadius, movableNormalLayer);
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
            PlayerStatusManager.fallDistanceIsBiggerThanMinimum = true;

            if (fallDistance > maxFallWithoutBouncyJumpCalculationByThisScript && groundedForBouncyEnvironment && !crouching && readyToJump && !jumping)
            {
                Jumping(bouncyJumpForce);
            }

            if (fallDistance > maxFallWithoutParticles && !jumpingDownParticles.isPlaying)
            {
                jumpingDownParticles.Play();
            }

            if (fallDistance > maxFallWithoutFallDamage && groundedForAll && !groundedForBouncyEnvironment && !PlayerSpawnAndSaveManager.spawnProtection)
            {
                PlayerStatusManager.playerHealth -= (int)fallDistance - maxFallWithoutFallDamage;
            }

            startOfFall = 0;
            endOfFall = 0;
            fallDistance = 0;
            wasFalling = true;
            wasGrounded = true;
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
        playerRigidbody.AddForce(playerTransform.up * jumpForce, ForceMode.VelocityChange);
        Invoke(nameof(JumpAgainReset), jumpAgainCooldown);
        Invoke(nameof(JumpingReset), jumpingCooldown);
    }

    void JumpingReset() // For jump height consistency
    {
        jumping = false;
    }

    /* For a continuous jump, use JumpAgainReset. If you don't want to use JumpAgainReset, make a jump buffer function
    and use it but don't forget to add "coyoteTimeCounter = 0;" in your jumping function after the jumping force.
    But you don't need to do that in this script if you are using JumpAgainReset. */
    void JumpAgainReset()
    {
        readyToJump = true;
    }

    void Crouch()
    {
        if (!jumping)
        {
            if (!crouching && Input.GetKey(crouchKey))
            {
                StartCoroutine(Crouching());
            }
            else if (crouching)
            {// Bilgi için https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.CheckCapsule.html sitesine bakabilirsin. -0.075f'i de girebildiği ama küçücük bir kısmı CapsuleCollider ile temas ettiği için uncrouch yapamama durumu olmasın diye koydum.
                dontUncrouch = Physics.CheckCapsule(playerTransform.position + new Vector3(0, playerHeight - crouchHeight / 2 - (playerWidthRadius - 0.01f) - 0.075f, 0), playerTransform.position + new Vector3(0, crouchHeight / 2 - (playerWidthRadius - 0.01f), 0), playerWidthRadius - 0.01f, normalLayer | bouncyLayer);

                if (!Input.GetKey(crouchKey) && !dontUncrouch)
                {
                    StartCoroutine(Uncrouching());
                }
            }
        }
    }

    IEnumerator Crouching()
    {
        inCrouchingProcess = true;
        playerTransform.localScale = new Vector3(playerWidthRadius * 2, crouchHeight / 2, playerWidthRadius * 2);

        if (groundedForAll)
        {
            playerRigidbody.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
        }

        crouching = true;
        PlayerPrefs.SetInt("playerCrouching", 1);
        yield return new WaitForFixedUpdate();
        inCrouchingProcess = false;
    }

    IEnumerator Uncrouching()
    {
        inCrouchingProcess = true;

        if (groundedForAll)
        {
            playerRigidbody.position = new Vector3(playerTransform.position.x, playerTransform.position.y + (playerHeight / 2 - crouchHeight / 2), playerTransform.position.z);
        }

        playerTransform.localScale = new Vector3(playerWidthRadius * 2, playerHeight / 2, playerWidthRadius * 2);
        crouching = false;
        PlayerPrefs.SetInt("playerCrouching", -1);
        yield return new WaitForFixedUpdate();
        inCrouchingProcess = false;
    }

    void LinearDamping()
    {
        if (groundedForAll && !jumping && !PlayerStatusManager.sliding)
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
        normalizedMoveDirection = (playerModelTransform.forward * vertical + playerModelTransform.right * horizontal).normalized;
        onSlope = ((!crouching && Physics.Raycast(playerTransform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundedSphereRadius * 2)) || (crouching && Physics.Raycast(playerTransform.position, Vector3.down, out slopeHit, crouchHeight / 2 + groundedSphereRadius * 2))) && slopeHit.normal != Vector3.up; // slopeHit.normal kısmını sona koyman lazım çünkü Raycast'i bilmeden hit olan şeyi hesaplamaya çalışırsan olmaz.

        if (playerRigidbody.linearDamping != airLinearDamping)
        {
            if (!onSlope)
            {
                playerRigidbody.AddForce(normalizedMoveDirection * theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(normalizedSlopeMoveDirection * theMoveSpeed * theMoveMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
        else
        {
            flatRotationAngleInAir = playerModelTransform.rotation.eulerAngles.y;
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
                    normalizedMoveDirectionRelativeToPlayerInAir.y = 0;
                    normalizedMoveDirectionRelativeToPlayerInAir.x = 0;
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
                                playerRigidbody.AddForce(-playerModelTransform.right * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y) / 2, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.x < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(playerModelTransform.right * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.y) / 2, ForceMode.Acceleration);
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
                                playerRigidbody.AddForce(-playerModelTransform.forward * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x) / 2, ForceMode.Acceleration);
                            }
                            else if (flatVelocityRelativeToPlayerInAir.y < -theMoveSpeed)
                            {
                                playerRigidbody.AddForce(playerModelTransform.forward * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime * Mathf.Abs(normalizedMoveDirectionRelativeToPlayerInAir.x) / 2, ForceMode.Acceleration);
                            }
                        }
                    }
                }
            }

            normalizedMoveDirectionAsVector2 = RelativeToPlayerConverter(normalizedMoveDirectionRelativeToPlayerInAir.x, normalizedMoveDirectionRelativeToPlayerInAir.y, -flatRotationAngleInAir);
            normalizedMoveDirection = new Vector3(normalizedMoveDirectionAsVector2.x, 0, normalizedMoveDirectionAsVector2.y);

            if (!onSlope)
            {
                playerRigidbody.AddForce(normalizedMoveDirection * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                normalizedSlopeMoveDirection = Vector3.ProjectOnPlane(normalizedMoveDirection, slopeHit.normal);
                playerRigidbody.AddForce(normalizedSlopeMoveDirection * theMoveSpeed * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }

    Vector2 RelativeToPlayerConverter(float x, float z, float angle)
    {
        // Gets x and z vectors and an angle. Then returs x and y vectors. x means x vector, y means z vector.
        // If you enter the angle negative and the vectors as relative to player, it returns the vectors as relative to world.
        return new Vector2(x * Mathf.Cos(Mathf.Deg2Rad * angle) - z * MathF.Sin(Mathf.Deg2Rad * angle), x * Mathf.Sin(Mathf.Deg2Rad * angle) + z * Mathf.Cos(Mathf.Deg2Rad * angle));
    }

    void WasFallingAndWasGroundedCheck()
    {
        wasFalling = falling;
        wasGrounded = groundedForAll;
    }

    void GravityAndSpeedControl()
    {
        if (groundedForAll && playerTouchingToAnyGround && onSlope)
        {
            playerRigidbody.useGravity = crouching || playerRigidbody.linearVelocity.y > minimum;

            if (!crouching && playerRigidbody.linearVelocity.y > minimum)
            {
                playerRigidbody.AddForce(new Vector3(0, 50, 0), ForceMode.Acceleration);
            }
            else if (PlayerStatusManager.sliding)
            {
                playerRigidbody.AddForce(new Vector3(0, 30, 0), ForceMode.Acceleration);
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
        else if (PlayerStatusManager.running)
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

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
        {
            playerTouchingToAnyGround = true;
        }

        if (collision.gameObject.layer == 7 && playerStandingOnMovableGround)
        {
            if (!crouching && !jumping)
            {
                playerRigidbody.AddForce(collision.gameObject.GetComponent<Rigidbody>().linearVelocity * theMoveMultiplier * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                playerRigidbody.AddForce(collision.gameObject.GetComponent<Rigidbody>().linearVelocity * theMoveMultiplier * airMoveMultiplier * Time.fixedDeltaTime / 49.96f, ForceMode.Acceleration); // Yes, I found the 49.96f number by trying.
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
        {
            playerTouchingToAnyGround = false;
        }
    }
}
