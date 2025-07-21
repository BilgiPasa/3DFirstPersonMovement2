using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* HealthText'te Drop Shadow materyalinin Face'inin dilate'sini 0.2 ve Outline'ının thickness'ını 0.2 yap.

    public static int playerHealth;
    public static float flatVelocityMagnitude;
    public static bool idling, walking, running, jumpingUp, jumpingDown, goingUp, goingDown, crouchIdling, crouchWalking, crouchJumpingUp, crouchJumpingDown, crouchGoingUp, crouchGoingDown, sliding, fallDistanceIsBiggerThanMinimum;
    const float minimum = 0.1f;
    KeyCode runKey = KeyCode.R;
    [SerializeField] Transform playerTransform, playerGroundParticlesTransform;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] ParticleSystem runJumpParticles, runAndSlideParticles;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        healthText.text = $"{playerHealth}";
        healthBarSlider.value = playerHealth;

        if (!PlayerSpawnAndSaveManager.playerDied)
        {
            flatVelocityMagnitude = new Vector2(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z).magnitude;

            if (!PlayerMovementManager.crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (PlayerMovementManager.playerHeight / 2), playerTransform.position.z);
                crouchIdling = false;
                crouchWalking = false;
                crouchJumpingUp = false;
                crouchJumpingDown = false;
                crouchGoingUp = false;
                crouchGoingDown = false;
                sliding = false;
                walking = flatVelocityMagnitude > minimum && (PlayerMovementManager.vertical != 0 || PlayerMovementManager.horizontal != 0);

                if (walking && PlayerMovementManager.vertical == 1 && (Input.GetKeyDown(runKey) || Input.GetKey(runKey)))
                {
                    running = true;
                }
                else if (!walking || (PlayerFrontBumpingManager.frontBumping && !Input.GetKey(runKey)) || (walking && PlayerMovementManager.vertical != 1))
                {
                    running = false;
                }

                if (PlayerMovementManager.groundedForAll)
                {
                    idling = flatVelocityMagnitude <= minimum;
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
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (PlayerMovementManager.crouchHeight / 2), playerTransform.position.z);
                idling = false;
                walking = false;
                running = false;
                jumpingUp = false;
                jumpingDown = false;
                goingUp = false;
                goingDown = false;
                crouchWalking = flatVelocityMagnitude > minimum && (PlayerMovementManager.vertical != 0 || PlayerMovementManager.horizontal != 0);

                if (PlayerMovementManager.groundedForAll)
                {
                    crouchIdling = flatVelocityMagnitude <= minimum;
                    crouchJumpingUp = PlayerMovementManager.jumping && playerRigidbody.linearVelocity.y > minimum;
                    crouchGoingUp = false;
                    crouchGoingDown = false;
                    sliding = flatVelocityMagnitude > PlayerMovementManager.runSpeed || PlayerMovementManager.onSlope;
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

        if (!runAndSlideParticles.isPlaying && PlayerMovementManager.groundedForAll && !PlayerSpawnAndSaveManager.playerDied && flatVelocityMagnitude > PlayerMovementManager.runSpeed / 4 && (sliding || running))
        {
            runAndSlideParticles.Play();
        }
        else if (runAndSlideParticles.isPlaying && (!PlayerMovementManager.groundedForAll || PlayerSpawnAndSaveManager.playerDied || flatVelocityMagnitude <= PlayerMovementManager.runSpeed / 4 || !(sliding || running)))
        {
            runAndSlideParticles.Stop();
        }
    }
}
