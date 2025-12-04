using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] Player player1;
    [SerializeField] Player player2;

    private IInputProvider inputProvider1;
    private IInputProvider inputProvider2;
    private InputKeys inputKeys;

    private int gameFPS = 120;
    private float logicTimer = 0f;
    private float logicDeltaTime = 1f / 120f;


    void Start()
    {
        Application.targetFrameRate = gameFPS;
        inputKeys = new InputKeys();
        inputProvider1 = new LocalInputProvider(inputKeys);
        inputManager.AddInputToPlayerMap(inputProvider1, player1);
        inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());
        player1.start();
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

        logicTimer += Time.deltaTime; 
    }
}