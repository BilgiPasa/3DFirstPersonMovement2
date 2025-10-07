using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnAndSaveManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    int savingTheGameDelay = 15, spawnProtectionSeconds = 3;
    float savingTheGameTimer;
    bool playerDied, spawnProtection, thePlayerDiedSaveValueWasZero, respawnButtonPressed;
    Transform playerTransform;
    Rigidbody playerRigidbody;
    PlayerStatusManager playerStatusManagerScript;
    PlayerInteractionManager playerInteractionManagerScript;
    PlayerMovementManager playerMovementManagerScript;
    [SerializeField] bool dontUseSaveAtTheStartOfTheGame = false;
    [SerializeField] Vector3 playerInitialPosition = Vector3.zero;
    [SerializeField] GameObject playerObject, deathMenuObject, pauseMenuObject, settingsMenuObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform, cameraHolderTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] CapsuleCollider playerColliderCapsuleCollider;
    [SerializeField] Image crosshairImage;
    [SerializeField] PlayerCameraManager playerCameraManagerScript;

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
        playerTransform = playerObject.transform;
        playerRigidbody = playerObject.GetComponent<Rigidbody>();
        playerStatusManagerScript = GetComponent<PlayerStatusManager>();
        playerInteractionManagerScript = playerObject.GetComponent<PlayerInteractionManager>();
        playerMovementManagerScript = playerObject.GetComponent<PlayerMovementManager>();

        if (!dontUseSaveAtTheStartOfTheGame)
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
            thePlayerDiedSaveValueWasZero = true;
        }

        if (PlayerPrefs.GetInt("playerCrouching") == 0)
        {
            PlayerPrefs.SetInt("playerCrouching", -1);
        }

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("playerHealth")))
        {
            playerStatusManagerScript.PlayerHealth = int.Parse(PlayerPrefs.GetString("playerHealth"));
        }
        else
        {
            playerStatusManagerScript.PlayerHealth = 100;
        }

        playerCameraManagerScript.XRotation = PlayerPrefs.GetFloat("playerRotationX");
        playerCameraManagerScript.YRotation = PlayerPrefs.GetFloat("playerRotationY");
        cameraHolderTransform.rotation = Quaternion.Euler(playerCameraManagerScript.XRotation, playerCameraManagerScript.YRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, playerCameraManagerScript.YRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            spawnProtection = true;

            if (!thePlayerDiedSaveValueWasZero) // If thePlayerDiedSaveValueWasZero is false, it means that there must be a save.
            {
                playerRigidbody.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ")); // I used playerRigidbody instead of playerTransform because if I use playerTransform, when the player is not dead, the player does not appear at it's last position but appears at Vector3.zero (and I don't know why this happens). So, don't change this.
            }
            else
            {
                playerRigidbody.position = playerInitialPosition;
            }

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerColliderCapsuleCollider.height = playerMovementManagerScript.PlayerHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.CameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.FrontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.PlayerHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementManagerScript.Crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerColliderCapsuleCollider.height = playerMovementManagerScript.CrouchHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.CameraPositionLocalPositionWhenCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.FrontBumpingDetectorLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.CrouchHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementManagerScript.Crouching = true;
            }

            cameraHolderTransform.position = cameraPositionTransform.position;
            playerMovementManagerScript.StartOfFall = playerRigidbody.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position at the start of the game.
            playerRigidbody.linearVelocity = new Vector3(PlayerPrefs.GetFloat("playerLinearVelocityX"), PlayerPrefs.GetFloat("playerLinearVelocityY"), PlayerPrefs.GetFloat("playerLinearVelocityZ"));
            yield return new WaitForSeconds(spawnProtectionSeconds);
            spawnProtection = false;
        }
        else if (PlayerPrefs.GetInt("playerDied") == 1)
        {
            playerTransform.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ")); // I used playerTransform instead of playerRigidbody because if I use playerRigidbody, when the player is dead, the player does not appear at it's last position but appears at Vector3.zero. So, don't change this.
            cameraHolderTransform.position = cameraPositionTransform.position;
            PlayerDespawning();
        }
    }

    IEnumerator NotUsingTheSaveAtTheStartOfTheGame()
    {
        playerStatusManagerScript.PlayerHealth = 100;
        spawnProtection = true;
        playerRigidbody.position = playerInitialPosition;
        cameraHolderTransform.position = cameraPositionTransform.position;
        playerMovementManagerScript.StartOfFall = playerRigidbody.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position at the start of the game.
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
    }

    void FixedUpdate()
    {
        // Autosave
        if (savingTheGameTimer > 0)
        {
            savingTheGameTimer -= Time.fixedDeltaTime;
        }
        else
        {
            SavingTheGame();
            savingTheGameTimer = savingTheGameDelay;
        }

        if (playerStatusManagerScript.PlayerHealth <= 0)
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
            playerStatusManagerScript.PlayerHealth = 0;
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

        PlayerPrefs.SetFloat("playerRotationX", playerCameraManagerScript.XRotation);
        PlayerPrefs.SetFloat("playerRotationY", playerCameraManagerScript.YRotation);
        PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
        PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
        PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        PlayerPrefs.SetFloat("playerLinearVelocityX", playerRigidbody.linearVelocity.x);
        PlayerPrefs.SetFloat("playerLinearVelocityY", playerRigidbody.linearVelocity.y);
        PlayerPrefs.SetFloat("playerLinearVelocityZ", playerRigidbody.linearVelocity.z);
        PlayerPrefs.SetString("playerHealth", playerStatusManagerScript.PlayerHealth.ToString());
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
        playerStatusManagerScript.PlayerHealth = 0;
        deathMenuObject.SetActive(true); // Haberin olsun. Eğer ki karakter öldüğünde ekranda gözükmesini istemediğin Menu'ler varsa, bu koddan önce o Menu'lerin SetActive'lerini false yap ve ardından bu kodu çalıştır. Yoksa Menu'ler üst üste olabilir ve biri diğerini engelleyebilir.

        if (playerInteractionManagerScript.GrabbedObjectRigidbody)
        {
            playerInteractionManagerScript.ReleaseObject();
        }

        crosshairImage.color = Color.black;
        playerObject.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator PlayerRespawning()
    {
        playerObject.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        playerCameraManagerScript.XRotation = playerCameraManagerScript.YRotation = 0;
        playerTransform.position = playerInitialPosition;
        playerMovementManagerScript.StartOfFall = playerTransform.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position when respawned.
        playerMovementManagerScript.EndOfFall = playerMovementManagerScript.FallDistance = 0;
        playerColliderCapsuleCollider.height = playerMovementManagerScript.PlayerHeight;
        cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.CameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
        frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.FrontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
        playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.PlayerHeight / 2, playerCapsuleModelTransform.localScale.z);
        playerMovementManagerScript.Crouching = false;
        playerRigidbody.linearVelocity = Vector3.zero;
        playerStatusManagerScript.PlayerHealth = 100;
        playerDied = false;
        deathMenuObject.SetActive(false);
        SavingTheGame();
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}
