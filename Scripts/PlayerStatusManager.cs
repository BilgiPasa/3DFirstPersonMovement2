using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* "OtherIndicatiors"ta "PlayerHealth"te "HealthText"te "Drop Shadow" materyalinin "Face"inin "Dilate"sini 0.2 ve "Outline"ının "Thickness"ını 0.2 yap.

    int playerHealth;
    const float Minimum = 0.1f;
    float flatVelocityMagnitude, relativeFlatVelocityMagnitude;
    bool running, sliding, walking, jumpingUp;
    Transform playerTransform;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerMovementManager playerMovementManagerScript;
    [SerializeField] Transform playerGroundParticlesTransform;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] ParticleSystem runJumpParticles, runAndSlideParticles;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;
    [SerializeField] PlayerFrontBumpingManager playerFrontBumpingManagerScript;

    public int PlayerHealth
    {
        get => playerHealth;
        set { playerHealth = value; }
    }

    public float FlatVelocityMagnitude
    {
        get => flatVelocityMagnitude;
    }

    public bool Running
    {
        get => running;
    }

    public bool Sliding
    {
        get => sliding;
    }

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

        if (playerMovementManagerScript.PlayerHealthDecrease > 0)
        {
            playerHealth -= playerMovementManagerScript.PlayerHealthDecrease;
            playerMovementManagerScript.PlayerHealthDecrease = 0;
        }

        if (!playerSpawnAndSaveManagerScript.PlayerDied)
        {
            flatVelocityMagnitude = new Vector2(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.z).magnitude;

            if (!(playerMovementManagerScript.StandingOnMovingGround && playerMovementManagerScript.ObjectRigidbodyThatPlayerIsStandingOn))
            {
                relativeFlatVelocityMagnitude = flatVelocityMagnitude;
                playerMovementManagerScript.StandingOnMovingGround = false;
            }
            else
            {
                relativeFlatVelocityMagnitude = new Vector2(playerRigidbody.linearVelocity.x - playerMovementManagerScript.ObjectRigidbodyThatPlayerIsStandingOn.linearVelocity.x, playerRigidbody.linearVelocity.z - playerMovementManagerScript.ObjectRigidbodyThatPlayerIsStandingOn.linearVelocity.z).magnitude; // Oyuncu; hareketli objenin üstündeyken, hareketli objeye göre oyuncunun hızını hesaplamak için
            }

            if (!playerMovementManagerScript.Crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.PlayerHeight / 2), playerTransform.position.z);
                sliding = false;
                walking = playerMovementManagerScript.InputtedVector2.magnitude > Minimum && relativeFlatVelocityMagnitude > Minimum;

                if (playerMovementManagerScript.RunningInput && playerMovementManagerScript.InputtedVector2.y > Minimum && walking)
                {
                    running = true;
                }
                else if (!walking || (playerMovementManagerScript.InputtedVector2.y <= Minimum && walking) || (playerFrontBumpingManagerScript.FrontBumping && !playerMovementManagerScript.RunningInput))
                {
                    running = false;
                }

                jumpingUp = playerMovementManagerScript.Jumping && playerRigidbody.linearVelocity.y > Minimum && playerMovementManagerScript.GroundedForAll;
            }
            else
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementManagerScript.CrouchHeight / 2), playerTransform.position.z);
                walking = running = jumpingUp = false;
                sliding = relativeFlatVelocityMagnitude > playerMovementManagerScript.RunSpeed || playerMovementManagerScript.OnSlope; // BURAYA, "playerMovementManagerScript.groundedForAll" ŞARTINI EKLEME! Çünkü eklersen; eğimli yüzeyden eğilerek kayıp düz zemine (hala eğilirken) çartığında, hızının bir kısmını kaybedebiliyorsun (ama normalde kaybetmemen lazım). Bence bunun olmasının sebebi; yere çarptığında, bir anlığına movement scriptinin, oyuncunun kaymıyor olduğunu zannettiği için olabilir. Ve evet; yere değme şartını eklemediğim için, oyuncu havada hızlı giderken eğilirse kayıyor sayılıyor ki böyle olmasında bence bir sorun yok.
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

        if ((sliding || running) && relativeFlatVelocityMagnitude > playerMovementManagerScript.RunSpeed / 4 && playerMovementManagerScript.GroundedForAll && !runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Play();
        }
        else if ((!(sliding || running) || relativeFlatVelocityMagnitude <= playerMovementManagerScript.RunSpeed / 4 || !playerMovementManagerScript.GroundedForAll) && runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Stop();
        }
    }
}
