using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* HealthText'te Drop Shadow materyalinin Face'inin dilate'sini 0.2 ve Outline'ının thickness'ını 0.2 yap.

    [HideInInspector] public int playerHealth;
    [HideInInspector] public float flatVelocityMagnitude;
    [HideInInspector] public bool idling, walking, running, jumpingUp, jumpingDown, goingUp, goingDown, crouchIdling, crouchWalking, crouchJumpingUp, crouchJumpingDown, crouchGoingUp, crouchGoingDown, sliding, fallDistanceIsBiggerThanMinimum;
    const float minimum = 0.1f;
    KeyCode runKey = KeyCode.R;
    Transform playerTransform;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerMovementManager playerMovementManagerScript;
    [SerializeField] Transform playerGroundParticlesTransform;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] ParticleSystem runJumpParticles, runAndSlideParticles;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;
    [SerializeField] PlayerFrontBumpingManager playerFrontBumpingManagerScript;

    void Start()
    {
        playerTransform = playerRigidbody.transform;
        playerSpawnAndSaveManagerScript = GetComponent<PlayerSpawnAndSaveManager>();
        playerMovementManagerScript = playerRigidbody.GetComponent<PlayerMovementManager>();
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        healthText.text = $"{playerHealth}";
        healthBarSlider.value = playerHealth;

        if (playerMovementManagerScript.playerHealthDecrease > 0)
        {
            playerHealth -= playerMovementManagerScript.playerHealthDecrease;
            playerMovementManagerScript.playerHealthDecrease = 0;
        }

        if (!playerSpawnAndSaveManagerScript.playerDied)
        {
            flatVelocityMagnitude = new Vector2(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z).magnitude;

            if (!playerMovementManagerScript.crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.playerHeight / 2), playerTransform.position.z);
                crouchIdling = false;
                crouchWalking = false;
                crouchJumpingUp = false;
                crouchJumpingDown = false;
                crouchGoingUp = false;
                crouchGoingDown = false;
                sliding = false;
                walking = flatVelocityMagnitude > minimum && (playerMovementManagerScript.vertical != 0 || playerMovementManagerScript.horizontal != 0);

                if (walking && playerMovementManagerScript.vertical == 1 && (Input.GetKeyDown(runKey) || Input.GetKey(runKey)))
                {
                    running = true;
                }
                else if (!walking || (playerFrontBumpingManagerScript.frontBumping && !Input.GetKey(runKey)) || (walking && playerMovementManagerScript.vertical != 1))
                {
                    running = false;
                }

                if (playerMovementManagerScript.groundedForAll)
                {
                    idling = flatVelocityMagnitude <= minimum;
                    jumpingUp = playerMovementManagerScript.jumping && playerRigidbody.linearVelocity.y > minimum;
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
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.crouchHeight / 2), playerTransform.position.z);
                idling = false;
                walking = false;
                running = false;
                jumpingUp = false;
                jumpingDown = false;
                goingUp = false;
                goingDown = false;
                crouchWalking = flatVelocityMagnitude > minimum && (playerMovementManagerScript.vertical != 0 || playerMovementManagerScript.horizontal != 0);

                if (playerMovementManagerScript.groundedForAll)
                {
                    crouchIdling = flatVelocityMagnitude <= minimum;
                    crouchJumpingUp = playerMovementManagerScript.jumping && playerRigidbody.linearVelocity.y > minimum;
                    crouchGoingUp = false;
                    crouchGoingDown = false;
                    sliding = flatVelocityMagnitude > playerMovementManagerScript.runSpeed || playerMovementManagerScript.onSlope;
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

        if (!runAndSlideParticles.isPlaying && playerMovementManagerScript.groundedForAll && !playerSpawnAndSaveManagerScript.playerDied && flatVelocityMagnitude > playerMovementManagerScript.runSpeed / 4 && (sliding || running))
        {
            runAndSlideParticles.Play();
        }
        else if (runAndSlideParticles.isPlaying && (!playerMovementManagerScript.groundedForAll || playerSpawnAndSaveManagerScript.playerDied || flatVelocityMagnitude <= playerMovementManagerScript.runSpeed / 4 || !(sliding || running)))
        {
            runAndSlideParticles.Stop();
        }
    }
}
