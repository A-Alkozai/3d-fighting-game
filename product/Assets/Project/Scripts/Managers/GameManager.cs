using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] Player player1;
    [SerializeField] Player player2;

    IInputProvider inputProvider1;
    IInputProvider inputProvider2;
    InputKeys inputKeys;

    void Start()
    {
        inputKeys = new InputKeys();
        inputProvider1 = new LocalInputProvider(inputKeys);
        inputManager.AddInputToPlayerMap(inputProvider1, player1);
        inputManager.AddRecentInputsUI(uiManager.GetRecentInputsUI());
    }

    void Update()
    {
        inputManager.update();
        player1.update();
        uiManager.update();
    }
}