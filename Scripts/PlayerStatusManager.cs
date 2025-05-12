using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusManager : MonoBehaviour
{
    public static int playerHealth;
    public static bool idling, walking, running, jumpingUp, jumpingDown, goingUp, goingDown, crouchIdling, crouchWalking, crouchJumpingUp, crouchJumpingDown, crouchGoingUp, crouchGoingDown, sliding, fallDistanceIsBiggerThanMinimum;
    public static Vector2 flatVelocity;
    int runSpeedFromPlayerMovementManager, verticalFromPlayerMovementManager;
    float minimum = 0.1f;
    bool playerDiedFromPlayerSpawnAndSaveManager, groundedForAllFromPlayerMovementManager;
    KeyCode runKey = KeyCode.R;
    [SerializeField] Transform playerTransform, playerGroundParticles;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] ParticleSystem runJumpParticles, runAndSlideParticles;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBar;

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        healthText.text = $"{playerHealth}";
        healthBar.value = playerHealth;
        playerDiedFromPlayerSpawnAndSaveManager = PlayerSpawnAndSaveManager.playerDied;
        groundedForAllFromPlayerMovementManager = PlayerMovementManager.groundedForAll;
        runSpeedFromPlayerMovementManager = PlayerMovementManager.runSpeed;

        if (!playerDiedFromPlayerSpawnAndSaveManager)
        {
            flatVelocity = new Vector2(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z);
            verticalFromPlayerMovementManager = PlayerMovementManager.vertical;

            if (!PlayerMovementManager.crouching)
            {
                playerGroundParticles.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (PlayerMovementManager.playerHeightForOtherScripts / 2), playerTransform.position.z);
                crouchIdling = false;
                crouchWalking = false;
                crouchJumpingUp = false;
                crouchJumpingDown = false;
                crouchGoingUp = false;
                crouchGoingDown = false;
                sliding = false;
                walking = flatVelocity.magnitude > minimum && (verticalFromPlayerMovementManager != 0 || PlayerMovementManager.horizontal != 0);

                if (walking && verticalFromPlayerMovementManager == 1 && (Input.GetKeyDown(runKey) || Input.GetKey(runKey)))
                {
                    running = true;
                }
                else if (!walking || (PlayerFrontBumpingManager.frontBumping && !Input.GetKey(runKey)) || (walking && verticalFromPlayerMovementManager != 1))
                {
                    running = false;
                }

                if (groundedForAllFromPlayerMovementManager)
                {
                    idling = flatVelocity.magnitude <= minimum;
                    jumpingUp = PlayerMovementManager.jumping && playerRigidbody.linearVelocity.y > minimum;
                    goingUp = false;
                    goingDown = false;
                }
                else
                {
                    idling = false;
                    jumpingUp = false;
                    goingUp = playerRigidbody.linearVelocity.y > minimum;
                    goingDown = playerRigidbody.linearVelocity.y < -minimum;
                }

                if (fallDistanceIsBiggerThanMinimum) // This variable is controlled by the movement script.
                {
                    jumpingDown = true;
                    fallDistanceIsBiggerThanMinimum = false;
                }
                else
                {
                    jumpingDown = false;
                }
            }
            else
            {
                playerGroundParticles.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (PlayerMovementManager.crouchHeightForOtherScripts / 2), playerTransform.position.z);
                idling = false;
                walking = false;
                running = false;
                jumpingUp = false;
                jumpingDown = false;
                goingUp = false;
                goingDown = false;
                crouchWalking = flatVelocity.magnitude > minimum && (verticalFromPlayerMovementManager != 0 || PlayerMovementManager.horizontal != 0);

                if (groundedForAllFromPlayerMovementManager)
                {
                    crouchIdling = flatVelocity.magnitude <= minimum;
                    crouchJumpingUp = PlayerMovementManager.jumping && playerRigidbody.linearVelocity.y > minimum;
                    crouchGoingUp = false;
                    crouchGoingDown = false;
                    sliding = flatVelocity.magnitude > runSpeedFromPlayerMovementManager || PlayerMovementManager.onSlope;
                }
                else
                {
                    crouchIdling = false;
                    crouchJumpingUp = false;
                    crouchGoingUp = playerRigidbody.linearVelocity.y > minimum;
                    crouchGoingDown = playerRigidbody.linearVelocity.y < -minimum;
                }

                if (fallDistanceIsBiggerThanMinimum) // This variable is controlled by the movement script.
                {
                    crouchJumpingDown = true;
                    fallDistanceIsBiggerThanMinimum = false;
                }
                else
                {
                    crouchJumpingDown = false;
                }
            }
        }
        else
        {
            idling = false;
            walking = false;
            running = false;
            jumpingUp = false;
            jumpingDown = false;
            goingUp = false;
            goingDown = false;
            crouchIdling = false;
            crouchWalking = false;
            crouchJumpingUp = false;
            crouchJumpingDown = false;
            crouchGoingUp = false;
            crouchGoingDown = false;
            sliding = false;
        }

        if (running && jumpingUp && !runJumpParticles.isPlaying)
        {
            runJumpParticles.Play();
        }

        if (!runAndSlideParticles.isPlaying && groundedForAllFromPlayerMovementManager && !playerDiedFromPlayerSpawnAndSaveManager && (sliding || (running && flatVelocity.magnitude > runSpeedFromPlayerMovementManager / 4)))
        {
            runAndSlideParticles.Play();
        }
        else if (runAndSlideParticles.isPlaying && (!groundedForAllFromPlayerMovementManager || playerDiedFromPlayerSpawnAndSaveManager || (!sliding && (!running || flatVelocity.magnitude <= runSpeedFromPlayerMovementManager / 4))))
        {
            runAndSlideParticles.Stop();
        }
    }
}
