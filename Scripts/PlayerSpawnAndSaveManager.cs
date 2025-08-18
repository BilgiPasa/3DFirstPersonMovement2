using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnAndSaveManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    [HideInInspector] public bool playerDied, spawnProtection;
    int savingTheGameDelay = 15, spawnProtectionSeconds = 3;
    float savingTheGameTimer;
    bool thePlayerDiedSaveValueWasZero, respawnButtonPressed;
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
            playerStatusManagerScript.playerHealth = int.Parse(PlayerPrefs.GetString("playerHealth"));
        }
        else
        {
            playerStatusManagerScript.playerHealth = 100;
        }

        playerCameraManagerScript.xRotation = PlayerPrefs.GetFloat("playerRotationX");
        playerCameraManagerScript.yRotation = PlayerPrefs.GetFloat("playerRotationY");
        cameraHolderTransform.rotation = Quaternion.Euler(playerCameraManagerScript.xRotation, playerCameraManagerScript.yRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, playerCameraManagerScript.yRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            if (!thePlayerDiedSaveValueWasZero) // If thePlayerDiedSaveValueWasZero is false, it means that there must be a save.
            {
                playerRigidbody.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ")); // I used playerRigidbody instead of playerTransform because if I use playerTransform, when the player is not dead, the player does not appear at it's last position but appears at Vector3.zero (and I don't know why this happens). So, don't change this.
            }
            else
            {
                playerRigidbody.position = playerInitialPosition;
            }

            playerMovementManagerScript.startOfFall = playerRigidbody.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position at the start of the game.
            cameraHolderTransform.position = cameraPositionTransform.position;
            spawnProtection = true;
            playerRigidbody.linearVelocity = new Vector3(PlayerPrefs.GetFloat("playerLinearVelocityX"), PlayerPrefs.GetFloat("playerLinearVelocityY"), PlayerPrefs.GetFloat("playerLinearVelocityZ"));

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerColliderCapsuleCollider.height = playerMovementManagerScript.playerHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.cameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.playerHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementManagerScript.crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerColliderCapsuleCollider.height = playerMovementManagerScript.crouchHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.cameraPositionLocalPositionWhenCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.frontBumpingDetectorLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.crouchHeight / 2, playerCapsuleModelTransform.localScale.z);
                playerMovementManagerScript.crouching = true;
            }

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
        playerStatusManagerScript.playerHealth = 100;
        playerRigidbody.position = playerInitialPosition;
        playerMovementManagerScript.startOfFall = playerRigidbody.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position at the start of the game.
        spawnProtection = true;
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

        if (playerStatusManagerScript.playerHealth <= 0)
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
            playerStatusManagerScript.playerHealth = 0;
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

        PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
        PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
        PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        PlayerPrefs.SetFloat("playerLinearVelocityX", playerRigidbody.linearVelocity.x);
        PlayerPrefs.SetFloat("playerLinearVelocityY", playerRigidbody.linearVelocity.y);
        PlayerPrefs.SetFloat("playerLinearVelocityZ", playerRigidbody.linearVelocity.z);
        PlayerPrefs.SetFloat("playerRotationX", playerCameraManagerScript.xRotation);
        PlayerPrefs.SetFloat("playerRotationY", playerCameraManagerScript.yRotation);
        PlayerPrefs.SetString("playerHealth", playerStatusManagerScript.playerHealth.ToString());
    }

    void PlayerDeath()
    {
        PlayerDespawning();
        // Making death effects and writing "player died" to chat or something like that.
        SavingTheGame();
    }

    void PlayerDespawning()
    {
        playerDied = true;
        pauseMenuObject.SetActive(false);
        settingsMenuObject.SetActive(false);
        deathMenuObject.SetActive(true);

        if (playerInteractionManagerScript.grabbedObjectRigidbody)
        {
            playerInteractionManagerScript.ReleaseObject();
        }

        crosshairImage.color = Color.black;
        playerStatusManagerScript.playerHealth = 0;
        playerObject.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator PlayerRespawning()
    {
        playerObject.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        playerTransform.position = playerInitialPosition;
        playerRigidbody.linearVelocity = Vector3.zero;
        playerMovementManagerScript.startOfFall = playerTransform.position.y; // Setting the start of fall to player's y position because if not, the PlayerMovementManager script does not know it's initial falling position when respawned.
        playerMovementManagerScript.endOfFall = playerMovementManagerScript.fallDistance = playerCameraManagerScript.xRotation = playerCameraManagerScript.yRotation = 0;
        playerColliderCapsuleCollider.height = playerMovementManagerScript.playerHeight;
        cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, playerMovementManagerScript.cameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
        frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, playerMovementManagerScript.frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
        playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, playerMovementManagerScript.playerHeight / 2, playerCapsuleModelTransform.localScale.z);
        playerMovementManagerScript.crouching = false;
        playerStatusManagerScript.playerHealth = 100;
        playerDied = false;
        deathMenuObject.SetActive(false);
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
        SavingTheGame();
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}
