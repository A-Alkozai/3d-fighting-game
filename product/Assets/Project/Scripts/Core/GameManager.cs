using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] InputManager inputManager;

    IInputProvider inputProvider;
    InputKeys inputKeys;

    void Start()
    {
        inputKeys = new InputKeys();
        inputProvider = new LocalInputProvider(inputKeys);
        inputManager.AddInputProvider(inputProvider);
    }

    void Update()
    {
        inputManager.update();
    }
}