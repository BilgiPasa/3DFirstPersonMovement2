using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* HealthText'te Drop Shadow materyalinin Face'inin dilate'sini 0.2 ve Outline'ının thickness'ını 0.2 yap.

    [HideInInspector] public int playerHealth;
    [HideInInspector] public float flatVelocityMagnitude;
    [HideInInspector] public bool walking, running, jumpingUp, sliding;
    const float minimum = 0.1f;
    float relativeFlatVelocityMagnitude;
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
            relativeFlatVelocityMagnitude = !playerMovementManagerScript.playerStandingOnMovingObject ? flatVelocityMagnitude : new Vector2(playerRigidbody.linearVelocity.x - playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn.linearVelocity.x, playerRigidbody.linearVelocity.z - playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn.linearVelocity.z).magnitude;

            if (!playerMovementManagerScript.crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.playerHeight / 2), playerTransform.position.z);
                sliding = false;
                walking = (playerMovementManagerScript.vertical != 0 || playerMovementManagerScript.horizontal != 0) && relativeFlatVelocityMagnitude > minimum;

                if (Input.GetKey(runKey) && playerMovementManagerScript.vertical == 1 && walking)
                {
                    running = true;
                }
                else if (!walking || (playerMovementManagerScript.vertical != 1 && walking) || (playerFrontBumpingManagerScript.frontBumping && !Input.GetKey(runKey)))
                {
                    running = false;
                }

                jumpingUp = playerMovementManagerScript.jumping && playerRigidbody.linearVelocity.y > minimum && playerMovementManagerScript.groundedForAll;
            }
            else
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.crouchHeight / 2), playerTransform.position.z);
                walking = running = jumpingUp = false;
                sliding = flatVelocityMagnitude > playerMovementManagerScript.runSpeed || playerMovementManagerScript.onSlope; // BURAYA, YERE DEĞİYOR MU ŞARTINI EKLEME! Çünkü eklersen; eğimli yüzeyde kayıp yere çarptığında, hızının bir kısmını kaybediyorsun. Bence bunun olmasının sebebi; yere çarptığında, bir anlığına movement scriptinin kaymıyor olduğunu ama sonrasında bu script çalışıp sonraki fixed update tick'ine geçince kayıyor olduğunu anladığı için olabilir. Ve evet; yere değme şartını eklemediğim için, karakter havada hızlı giderken eğilirse kayıyor sayılcak ki böyle olmasında bir sorun yok bence.
            }
        }
        else
        {
            walking = running = jumpingUp = sliding = false;
        }

        if (running && jumpingUp && !runJumpParticles.isPlaying)
        {
            runJumpParticles.Play();
        }

        if ((sliding || running) && relativeFlatVelocityMagnitude > playerMovementManagerScript.runSpeed / 4 && playerMovementManagerScript.groundedForAll && !runAndSlideParticles.isPlaying)
        {
            runAndSlideParticles.Play();
        }
        else if ((!(sliding || running) || relativeFlatVelocityMagnitude <= playerMovementManagerScript.runSpeed / 4 || !playerMovementManagerScript.groundedForAll) && runAndSlideParticles.isPlaying)
        {
            runAndSlideParticles.Stop();
        }
    }
}
