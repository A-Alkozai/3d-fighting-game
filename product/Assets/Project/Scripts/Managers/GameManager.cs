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

    void Start()
    {
        inputKeys = new InputKeys();
        inputProvider1 = new LocalInputProvider(inputKeys);
        inputManager.AddInputToPlayerMap(inputProvider1, player1);
        inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());
        player1.start();
    }

    void Update()
    {
        inputManager.update();
        player1.update();
        uiManager.update();
    }
}