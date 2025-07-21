using System.Collections;
using UnityEngine;

public class PlayerSpawnAndSaveManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.

    public static bool playerDied, spawnProtection;
    int normalSavingTheGameDelay = 20, pressingAltSavingTheGameDelay = 2, spawnProtectionSeconds = 3;
    float normalSavingTheGameTimer, pressingAltSavingTheGameTimer;
    bool respawnButtonPressed;
    Transform playerTransform;
    [SerializeField] GameObject playerObject, deathMenuObject, pauseMenuObject, settingsMenuObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform, cameraHolderTransform, frontBumpingDetectorTransform, playerCapsuleModelTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] CapsuleCollider playerColliderCapsuleCollider;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] PlayerInteractionManager playerInteractionManagerScript;

    void Start()
    {
        playerTransform = playerObject.transform;
        StartCoroutine(LoadingTheSave());
    }

    IEnumerator LoadingTheSave()
    {
        if (PlayerPrefs.GetInt("playerDied") == 0)
        {
            PlayerPrefs.SetInt("playerDied", -1);
        }

        if (PlayerPrefs.GetInt("playerCrouching") == 0)
        {
            PlayerPrefs.SetInt("playerCrouching", -1);
        }

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("playerHealth")))
        {
            PlayerStatusManager.playerHealth = int.Parse(PlayerPrefs.GetString("playerHealth"));
        }
        else
        {
            PlayerStatusManager.playerHealth = 100;
        }

        PlayerCameraManager.xRotation = PlayerPrefs.GetFloat("playerRotationX");
        PlayerCameraManager.yRotation = PlayerPrefs.GetFloat("playerRotationY");
        playerTransform.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
        cameraHolderTransform.position = cameraPositionTransform.position;
        cameraHolderTransform.rotation = Quaternion.Euler(PlayerCameraManager.xRotation, PlayerCameraManager.yRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, PlayerCameraManager.yRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            spawnProtection = true;
            playerRigidbody.linearVelocity = new Vector3(PlayerPrefs.GetFloat("playerLinearVelocityX"), PlayerPrefs.GetFloat("playerLinearVelocityY"), PlayerPrefs.GetFloat("playerLinearVelocityZ"));

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerColliderCapsuleCollider.height = PlayerMovementManager.playerHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, PlayerMovementManager.cameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, PlayerMovementManager.frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, PlayerMovementManager.playerHeight / 2, playerCapsuleModelTransform.localScale.z);
                PlayerMovementManager.crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerColliderCapsuleCollider.height = PlayerMovementManager.crouchHeight;
                cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, PlayerMovementManager.cameraPositionLocalPositionWhenCrouched, cameraPositionTransform.localPosition.z);
                frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, PlayerMovementManager.frontBumpingDetectorLocalScaleWhenCrouched, frontBumpingDetectorTransform.localScale.z);
                playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, PlayerMovementManager.crouchHeight / 2, playerCapsuleModelTransform.localScale.z);
                PlayerMovementManager.crouching = true;
            }

            yield return new WaitForSeconds(spawnProtectionSeconds);
            spawnProtection = false;
        }
        else if (PlayerPrefs.GetInt("playerDied") == 1)
        {
            PlayerDespawning();
        }
    }

    void FixedUpdate()
    {
        // Autosave
        if (normalSavingTheGameTimer > 0)
        {
            normalSavingTheGameTimer -= Time.fixedDeltaTime;
        }
        else
        {
            SavingTheGame();
            normalSavingTheGameTimer = normalSavingTheGameDelay;
        }

        if (PlayerStatusManager.playerHealth <= 0)
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
            PlayerStatusManager.playerHealth = 0;
        }

        // Preventing not saving the game from Alt + F4
        if (pressingAltSavingTheGameTimer <= 0)
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                SavingTheGame();
                pressingAltSavingTheGameTimer = pressingAltSavingTheGameDelay;
            }
            else
            {
                pressingAltSavingTheGameTimer = 0;
            }
        }
        else
        {
            pressingAltSavingTheGameTimer -= Time.fixedDeltaTime;
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
        PlayerPrefs.SetFloat("playerRotationX", PlayerCameraManager.xRotation);
        PlayerPrefs.SetFloat("playerRotationY", PlayerCameraManager.yRotation);
        PlayerPrefs.SetString("playerHealth", PlayerStatusManager.playerHealth.ToString());
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

        if (PlayerInteractionManager.grabbedObjectRigidbody)
        {
            playerInteractionManagerScript.ReleaseObject();
        }

        PlayerStatusManager.playerHealth = 0;
        playerObject.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator PlayerRespawning()
    {
        playerObject.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        PlayerMovementManager.startOfFall = 0;
        PlayerMovementManager.endOfFall = 0;
        PlayerMovementManager.fallDistance = 0;
        playerColliderCapsuleCollider.height = PlayerMovementManager.playerHeight;
        cameraPositionTransform.localPosition = new Vector3(cameraPositionTransform.localPosition.x, PlayerMovementManager.cameraPositionLocalPositionWhenNotCrouched, cameraPositionTransform.localPosition.z);
        frontBumpingDetectorTransform.localScale = new Vector3(frontBumpingDetectorTransform.localScale.x, PlayerMovementManager.frontBumpingDetectorLocalScaleWhenNotCrouched, frontBumpingDetectorTransform.localScale.z);
        playerCapsuleModelTransform.localScale = new Vector3(playerCapsuleModelTransform.localScale.x, PlayerMovementManager.playerHeight / 2, playerCapsuleModelTransform.localScale.z);
        PlayerMovementManager.crouching = false;
        playerTransform.position = Vector3.zero;
        playerRigidbody.linearVelocity = Vector3.zero;
        PlayerCameraManager.xRotation = 0;
        PlayerCameraManager.yRotation = 0;
        PlayerStatusManager.playerHealth = 100;
        deathMenuObject.SetActive(false);
        playerDied = false;
        yield return new WaitForSeconds(spawnProtectionSeconds);
        spawnProtection = false;
        SavingTheGame();
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}
