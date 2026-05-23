using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusMng : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* "OtherIndicatiors"ta "PlayerHealth"te "HealthText"te "Drop Shadow" materyalinin "Face"inin "Dilate"sini 0.2 ve "Outline"ının "Thickness"ını 0.2 yap.
    //* Haberin olsun bu bir state machine değildir ama istersen buraya bir state machine yapabilirsin.

    int playerHealth;
    const float Minimum = 0.1f;
    float flatVelMag, relativeFlatVelMag;
    bool running, sliding, walking, jumpingUp;
    Transform playerTransform;
    PlayerSpawnAndSaveMng playerSpawnAndSaveMng;
    PlayerMovementMng playerMovementMng;
    [SerializeField] Transform playerGroundParticlesTransform;
    [SerializeField] Rigidbody playerRb;
    [SerializeField] ParticleSystem runJumpParticles, runAndSlideParticles;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;
    [SerializeField] PlayerFrontBumpingMng playerFrontBumpingMng;

    public int PlayerHealth
    {
        get => playerHealth;
        set { playerHealth = value; }
    }

    public float FlatVelMag
    {
        get => flatVelMag;
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
        playerTransform = playerRb.transform;
        playerSpawnAndSaveMng = GetComponent<PlayerSpawnAndSaveMng>();
        playerMovementMng = playerRb.GetComponent<PlayerMovementMng>();
    }

    void FixedUpdate()
    {// I didn't added the if not game paused condition because if game pauses, FixedUpdate pauses too.
        healthText.text = $"{playerHealth}";
        healthBarSlider.value = playerHealth;

        if (playerMovementMng.PlayerHealthDecrease > 0)
        {
            playerHealth -= playerMovementMng.PlayerHealthDecrease;
            playerMovementMng.PlayerHealthDecrease = 0;
        }

        if (!playerSpawnAndSaveMng.PlayerDied)
        {
            flatVelMag = new Vector2(playerRb.linearVelocity.x, playerRb.linearVelocity.z).magnitude;

            if (!(playerMovementMng.StandingOnMovingGround && playerMovementMng.ObjRbThatPlayerStandingOn))
            {
                relativeFlatVelMag = flatVelMag;
                playerMovementMng.StandingOnMovingGround = false;
            }
            else
            {
                relativeFlatVelMag = new Vector2(playerRb.linearVelocity.x - playerMovementMng.ObjRbThatPlayerStandingOn.linearVelocity.x, playerRb.linearVelocity.z - playerMovementMng.ObjRbThatPlayerStandingOn.linearVelocity.z).magnitude; // Oyuncu; hareketli objenin üstündeyken, hareketli objeye göre oyuncunun hızını hesaplamak için
            }

            if (!playerMovementMng.Crouching)
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementMng.PlayerHeight / 2), playerTransform.position.z);
                sliding = false;
                walking = playerMovementMng.InputtedVector2.magnitude > Minimum && relativeFlatVelMag > Minimum;

                if (playerMovementMng.RunningInput && playerMovementMng.InputtedVector2.y > Minimum && walking)
                {
                    running = true;
                }
                else if (!walking || (playerMovementMng.InputtedVector2.y <= Minimum && walking) || (playerFrontBumpingMng.FrontBumping && !playerMovementMng.RunningInput))
                {
                    running = false;
                }

                jumpingUp = playerMovementMng.Jumping && playerRb.linearVelocity.y > Minimum && playerMovementMng.GroundedForAll;
            }
            else
            {
                playerGroundParticlesTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y - (playerMovementMng.CrouchHeight / 2), playerTransform.position.z);
                walking = running = jumpingUp = false;
                sliding = relativeFlatVelMag > playerMovementMng.RunSpeed || playerMovementMng.OnSlope; // BURAYA, "playerMovementMng.groundedForAll" ŞARTINI EKLEME! Çünkü eklersen; eğimli yüzeyden eğilerek kayıp düz zemine (hala eğilirken) çartığında, hızının bir kısmını kaybedebiliyorsun (ama normalde kaybetmemen lazım). Bence bunun olmasının sebebi; yere çarptığında, bir anlığına movement scriptinin, oyuncunun kaymıyor olduğunu zannettiği için olabilir. Ve evet; yere değme şartını eklemediğim için, oyuncu havada hızlı giderken eğilirse kayıyor sayılıyor ki böyle olmasında bence bir sorun yok.
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

        if ((sliding || running) && relativeFlatVelMag > playerMovementMng.RunSpeed / 4 && playerMovementMng.GroundedForAll && !runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Play();
        }
        else if ((!(sliding || running) || relativeFlatVelMag <= playerMovementMng.RunSpeed / 4 || !playerMovementMng.GroundedForAll) && runAndSlideParticles.isEmitting)
        {
            runAndSlideParticles.Stop();
        }
    }
}
