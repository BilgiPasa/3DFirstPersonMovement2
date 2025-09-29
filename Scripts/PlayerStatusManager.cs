using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* "OtherIndicatiors"ta "PlayerHealth"te "HealthText"te "Drop Shadow" materyalinin "Face"inin "Dilate"sini 0.2 ve "Outline"ının "Thickness"ını 0.2 yap.

    [NonSerialized] public int playerHealth;
    [NonSerialized] public float flatVelocityMagnitude;
    [NonSerialized] public bool walking, running, jumpingUp, sliding;
    const float Minimum = 0.1f;
    float relativeFlatVelocityMagnitude;
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

            if (!(playerMovementManagerScript.standingOnMovingGround && playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn))
            {
                relativeFlatVelocityMagnitude = flatVelocityMagnitude;
                playerMovementManagerScript.standingOnMovingGround = false;
            }
            else
            {
                relativeFlatVelocityMagnitude = new Vector2(playerRigidbody.linearVelocity.x - playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn.linearVelocity.x, playerRigidbody.linearVelocity.z - playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn.linearVelocity.z).magnitude; // Oyuncu; hareketli objenin üstündeyken, hareketli objeye göre oyuncunun hızını hesaplamak için
            }

            if (!playerMovementManagerScript.crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.playerHeight / 2), playerTransform.position.z);
                sliding = false;
                walking = playerMovementManagerScript.inputtedVector2.magnitude > Minimum && relativeFlatVelocityMagnitude > Minimum;

                if (playerMovementManagerScript.runningInput && playerMovementManagerScript.inputtedVector2.y > Minimum && walking)
                {
                    running = true;
                }
                else if (!walking || (playerMovementManagerScript.inputtedVector2.y <= Minimum && walking) || (playerFrontBumpingManagerScript.frontBumping && !playerMovementManagerScript.runningInput))
                {
                    running = false;
                }

                jumpingUp = playerMovementManagerScript.jumping && playerRigidbody.linearVelocity.y > Minimum && playerMovementManagerScript.groundedForAll;
            }
            else
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.crouchHeight / 2), playerTransform.position.z);
                walking = running = jumpingUp = false;
                sliding = relativeFlatVelocityMagnitude > playerMovementManagerScript.runSpeed || playerMovementManagerScript.onSlope; // BURAYA, "playerMovementManagerScript.groundedForAll" ŞARTINI EKLEME! Çünkü eklersen; eğimli yüzeyden eğilerek kayıp düz zemine (hala eğilirken) çartığında, hızının bir kısmını kaybedebiliyorsun (ama normalde kaybetmemen lazım). Bence bunun olmasının sebebi; yere çarptığında, bir anlığına movement scriptinin, oyuncunun kaymıyor olduğunu zannettiği için olabilir. Ve evet; yere değme şartını eklemediğim için, oyuncu havada hızlı giderken eğilirse kayıyor sayılıyor ki böyle olmasında bence bir sorun yok.
            }
        }
        else
        {
            walking = running = jumpingUp = sliding = false;
        }

        if (running && jumpingUp && !runJumpParticles.isEmitting)
        {
            runJumpParticles.Play();
        }

        if ((sliding || running) && relativeFlatVelocityMagnitude > playerMovementManagerScript.runSpeed / 4 && playerMovementManagerScript.groundedForAll && !runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Play();
        }
        else if ((!(sliding || running) || relativeFlatVelocityMagnitude <= playerMovementManagerScript.runSpeed / 4 || !playerMovementManagerScript.groundedForAll) && runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Stop();
        }
    }
}
