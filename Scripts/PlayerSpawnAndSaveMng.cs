using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnAndSaveMng : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    int savingGameDelay = 15, spawnProtectionSeconds = 3;
    float savingGameTimer;
    bool playerDied, spawnProtection, playerDiedSaveValueWasZero, respawnButtonPressed;
    Transform playerTransform;
    Rigidbody playerRb;
    PlayerStatusMng playerStatusMng;
    PlayerInteractionMng playerInteractionMng;
    PlayerMovementMng playerMovementMng;
    [SerializeField] bool dontUseSaveAtStart = false;
    [SerializeField] Vector3 playerInitialPos = Vector3.zero;
    [SerializeField] GameObject playerObj, deathMenuObj, pauseMenuObj, settingsMenuObj;
    [SerializeField] Transform playerCollTransform, camPosTransform, camHolderTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] CapsuleCollider playerCollCapsuleColl;
    [SerializeField] Image crosshairImg;
    [SerializeField] PlayerCamMng playerCamMng;

    public bool PlayerDied
    {
        get => playerDied;
    }

    public bool SpawnProtection
    {
        get => spawnProtection;
    }

    void Start()
    {
        playerTransform = playerObj.transform;
        playerRb = playerObj.GetComponent<Rigidbody>();
        playerStatusMng = GetComponent<PlayerStatusMng>();
        playerInteractionMng = playerObj.GetComponent<PlayerInteractionMng>();
        playerMovementMng = playerObj.GetComponent<PlayerMovementMng>();

        if (!dontUseSaveAtStart)
        {
            StartCoroutine(LoadingTheSave());
        }
        else
        {
            StartCoroutine(NotUsingTheSaveAtTheStartOfTheGame());
        }
    }

    IEnumerator LoadingTheSave()
    {
        if (PlayerPrefs.GetInt("playerDied") == 0)
        {
            PlayerPrefs.SetInt("playerDied", -1);
            playerDiedSaveValueWasZero = true;
        }

        if (PlayerPrefs.GetInt("playerCrouching") == 0)
        {
            PlayerPrefs.SetInt("playerCrouching", -1);
        }

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("playerHealth")))
        {
            playerStatusMng.PlayerHealth = int.Parse(PlayerPrefs.GetString("playerHealth"));
        }
        else
        {
            playerStatusMng.PlayerHealth = 100;
        }

        playerCamMng.XRotation = PlayerPrefs.GetFloat("playerRotationX");
        playerCamMng.YRotation = PlayerPrefs.GetFloat("playerRotationY");
        camHolderTransform.rotation = Quaternion.Euler(playerCamMng.XRotation, playerCamMng.YRotation, 0);
        playerCollTransform.rotation = Quaternion.Euler(0, playerCamMng.YRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            spawnProtection = true;

            if (!playerDiedSaveValueWasZero) // If playerDiedSaveValueWasZero is false, it means that there must be a save.
            {
                playerRb.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ")); // I used playerRigidbody instead of playerTransform because if I use playerTransform, when the player is not dead, the player does not appear at it's last position but appears at Vector3.zero (and I don't know why this happens). So, don't change this.
            }
            else
            {
                playerRb.position = playerInitialPos;
            }

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerCollCapsuleColl.height = playerMovementMng.PlayerHeight;
                camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, playerMovementMng.CamPosLocalPosWhenNotCrouched, camPosTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementMng.FBDLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementMng.PlayerHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementMng.Crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerCollCapsuleColl.height = playerMovementMng.CrouchHeight;
                camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, playerMovementMng.CamPosLocalPosWhenCrouched, camPosTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementMng.FBDLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementMng.CrouchHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementMng.Crouching = true;
            }

            camHolderTransform.position = camPosTransform.position;
            playerMovementMng.StartOfFall = playerRb.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementMng  does not know it's initial falling position at the start of the game.
            playerRb.linearVelocity = new Vector3(PlayerPrefs.GetFloat("playerLinearVelocityX"), PlayerPrefs.GetFloat("playerLinearVelocityY"), PlayerPrefs.GetFloat("playerLinearVelocityZ"));
            yield return new WaitForSeconds(spawnProtectionSeconds);
            spawnProtection = false;
        }
        else if (PlayerPrefs.GetInt("playerDied") == 1)
        {
            playerTransform.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ")); // I used playerTransform instead of playerRigidbody because if I use playerRigidbody, when the player is dead, the player does not appear at it's last position but appears at Vector3.zero. So, don't change this.
            camHolderTransform.position = camPosTransform.position;
            PlayerDespawning();
        }
    }

    IEnumerator NotUsingTheSaveAtTheStartOfTheGame()
    {
        playerStatusMng.PlayerHealth = 100;
        spawnProtection = true;
        playerRb.position = playerInitialPos;
        camHolderTransform.position = camPosTransform.position;
        playerMovementMng.StartOfFall = playerRb.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementMng  does not know it's initial falling position at the start of the game.
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
    }

    void FixedUpdate()
    {
        // Autosave
        if (savingGameTimer > 0)
        {
            savingGameTimer -= Time.fixedDeltaTime;
        }
        else
        {
            SavingTheGame();
            savingGameTimer = savingGameDelay;
        }

        if (playerStatusMng.PlayerHealth <= 0)
        {
            if (!playerDied)
            {
                PlayerDeath();
            }

            if (respawnButtonPressed)
            {
                StartCoroutine(PlayerRespawning());
            }
        }

        if (playerTransform.position.y < -100 && !playerDied)
        {
            playerStatusMng.PlayerHealth = 0;
        }
    }

    public void SavingTheGame()
    {
        if (!playerDied)
        {
            PlayerPrefs.SetInt("playerDied", -1);
        }
        else
        {
            PlayerPrefs.SetInt("playerDied", 1);
        }

        PlayerPrefs.SetFloat("playerRotationX", playerCamMng.XRotation);
        PlayerPrefs.SetFloat("playerRotationY", playerCamMng.YRotation);
        PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
        PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
        PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        PlayerPrefs.SetFloat("playerLinearVelocityX", playerRb.linearVelocity.x);
        PlayerPrefs.SetFloat("playerLinearVelocityY", playerRb.linearVelocity.y);
        PlayerPrefs.SetFloat("playerLinearVelocityZ", playerRb.linearVelocity.z);
        PlayerPrefs.SetString("playerHealth", playerStatusMng.PlayerHealth.ToString());
    }

    void PlayerDeath()
    {
        PlayerDespawning();
        // Instantiating a player death effect and writing "player died" to chat or something like that.
        SavingTheGame();
    }

    void PlayerDespawning()
    {
        playerDied = true;
        playerStatusMng.PlayerHealth = 0;
        deathMenuObj.SetActive(true); // Haberin olsun. Eğer ki karakter öldüğünde ekranda gözükmesini istemediğin Menu'ler varsa, bu koddan önce o Menu'lerin SetActive'lerini false yap ve ardından bu kodu çalıştır. Yoksa Menu'ler üst üste olabilir ve biri diğerini engelleyebilir.

        if (playerInteractionMng.GrabbedObjRb)
        {
            playerInteractionMng.ReleaseObj();
        }

        crosshairImg.color = Color.black;
        playerObj.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator PlayerRespawning()
    {
        playerObj.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        playerCamMng.XRotation = playerCamMng.YRotation = 0;
        playerTransform.position = playerInitialPos;
        playerMovementMng.StartOfFall = playerTransform.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementMng  does not know it's initial falling position when respawned.
        playerMovementMng.EndOfFall = playerMovementMng.FallDistance = 0;
        playerCollCapsuleColl.height = playerMovementMng.PlayerHeight;
        camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, playerMovementMng.CamPosLocalPosWhenNotCrouched, camPosTransform.localPosition.z);
        frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementMng.FBDLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
        playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementMng.PlayerHeight / 2, playerCapsuleModelTransform.localScale.z);
        playerMovementMng.Crouching = false;
        playerRb.linearVelocity = Vector3.zero;
        playerStatusMng.PlayerHealth = 100;
        playerDied = false;
        deathMenuObj.SetActive(false);
        SavingTheGame();
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}
