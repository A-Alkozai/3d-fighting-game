using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] Camera camera;
    [SerializeField] Player player1;
    [SerializeField] Player player2;

    private IInputProvider inputProvider1;
    private IInputProvider inputProvider2;
    private CameraManager cameraManager;
    private InputKeys inputKeys;

    private int gameFPS = 60;
    private float logicTimer = 0f;
    private float logicDeltaTime = 1f / 60f;


    void Start()
    {
        Application.targetFrameRate = gameFPS;
        inputKeys = new InputKeys();
        inputProvider1 = new LocalInputProvider(inputKeys);
        inputManager.AddInputToPlayerMap(inputProvider1, player1);
        inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());
        player1.start();
        cameraManager = new CameraManager(camera, player1.transform, player2.transform);
        cameraManager.SetMode(new CombatCameraMode());
    }

    void Update()
    {
        inputManager.update();

        while (logicTimer >= logicDeltaTime)
        {
            logicTimer -= logicDeltaTime;
            player1.update();
        }

        uiManager.update();
        cameraManager.Update();

        logicTimer += Time.deltaTime; 
    }
}