using System.Collections;
using UnityEngine;

public class PlayerSpawnAndSaveManager : MonoBehaviour
{
    public static bool playerDied, spawnProtection;
    int normalSavingTheGameDelay = 20, pressingAltSavingTheGameDelay = 2;
    float normalSavingTheGameTimer, pressingAltSavingTheGameTimer, playerWidthRadiusForOtherScriptsFromPlayerMovementManager;
    bool respawnButtonPressed;
    [SerializeField] GameObject player, deathMenu, pauseMenu, settingsMenu;
    [SerializeField] Transform playerModelTransform, cameraPositionTransform, cameraHolderTransform;
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] Camera mainCamera;
    Transform playerTransform;

    void Start()
    {
        playerTransform = player.transform;
        playerWidthRadiusForOtherScriptsFromPlayerMovementManager = PlayerMovementManager.playerWidthRadiusForOtherScripts;
        StartCoroutine(LoadingTheSave());
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
                StartCoroutine(Respawning());
            }
        }

        if (playerRigidbody.position.y < -100 && !playerDied)
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
            PlayerPrefs.SetFloat("playerPositionX", playerRigidbody.position.x);
            PlayerPrefs.SetFloat("playerPositionY", playerRigidbody.position.y);
            PlayerPrefs.SetFloat("playerPositionZ", playerRigidbody.position.z);
        }
        else
        {
            PlayerPrefs.SetInt("playerDied", 1);
            PlayerPrefs.SetFloat("playerPositionX", playerTransform.position.x);
            PlayerPrefs.SetFloat("playerPositionY", playerTransform.position.y);
            PlayerPrefs.SetFloat("playerPositionZ", playerTransform.position.z);
        }

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
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        deathMenu.SetActive(true);
        PlayerStatusManager.playerHealth = 0;
        player.SetActive(false);
        mainCamera.fieldOfView = PlayerPrefs.GetInt("FOV");
    }

    IEnumerator Respawning()
    {
        player.SetActive(true);
        respawnButtonPressed = false;
        spawnProtection = true;
        PlayerMovementManager.startOfFall = 0;
        PlayerMovementManager.endOfFall = 0;
        PlayerMovementManager.fallDistance = 0;
        playerTransform.localScale = new Vector3(playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2, PlayerMovementManager.playerHeightForOtherScripts / 2, playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2);
        PlayerMovementManager.crouching = false;
        playerRigidbody.position = new Vector3(0, 0, 0);
        playerRigidbody.linearVelocity = new Vector3(0, 0, 0);
        PlayerCameraManager.xRotation = 0;
        PlayerCameraManager.yRotation = 0;
        PlayerStatusManager.playerHealth = 100;
        deathMenu.SetActive(false);
        playerDied = false;
        yield return new WaitForSeconds(3);
        spawnProtection = false;
        SavingTheGame();
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
        cameraHolderTransform.position = cameraPositionTransform.position;
        cameraHolderTransform.rotation = Quaternion.Euler(PlayerCameraManager.xRotation, PlayerCameraManager.yRotation, 0);
        playerModelTransform.rotation = Quaternion.Euler(0, PlayerCameraManager.yRotation, 0);

        if (PlayerPrefs.GetInt("playerDied") == -1)
        {
            spawnProtection = true;
            playerRigidbody.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
            playerRigidbody.linearVelocity = new Vector3(PlayerPrefs.GetFloat("playerLinearVelocityX"), PlayerPrefs.GetFloat("playerLinearVelocityY"), PlayerPrefs.GetFloat("playerLinearVelocityZ"));

            if (PlayerPrefs.GetInt("playerCrouching") == -1)
            {
                playerTransform.localScale = new Vector3(playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2, PlayerMovementManager.playerHeightForOtherScripts / 2, playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2);
                PlayerMovementManager.crouching = false;
            }
            else if (PlayerPrefs.GetInt("playerCrouching") == 1)
            {
                playerTransform.localScale = new Vector3(playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2, PlayerMovementManager.crouchHeightForOtherScripts / 2, playerWidthRadiusForOtherScriptsFromPlayerMovementManager * 2);
                PlayerMovementManager.crouching = true;
            }

            yield return new WaitForSeconds(3);
            spawnProtection = false;
        }
        else if (PlayerPrefs.GetInt("playerDied") == 1)
        {
            PlayerDespawning();
            playerTransform.position = new Vector3(PlayerPrefs.GetFloat("playerPositionX"), PlayerPrefs.GetFloat("playerPositionY"), PlayerPrefs.GetFloat("playerPositionZ"));
        }
    }

    public void RespawnButtonPressed()
    {
        respawnButtonPressed = true;
    }
}
