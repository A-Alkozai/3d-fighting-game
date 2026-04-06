using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ControlBindingRow : MonoBehaviour
{
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private Button rebindButton;

    private InputCommand command;
    private ControlsPanel controlsPanel;

    public void Initialise(InputCommand command, Key currentKey, ControlsPanel panel)
    {
        this.command = command;
        this.controlsPanel = panel;

        actionLabel.text = FormatCommandName(command);
        keyLabel.text = FormatKeyName(currentKey);
        rebindButton.onClick.AddListener(OnRebindClicked);
    }

    private void OnRebindClicked()
    {
        controlsPanel.OnRebindRequested(this, command);
    }

    public void SetListening(bool listening)
    {
        if (listening)
        {
            keyLabel.text = "Press any key...";
            keyLabel.color = Color.yellow;
        }
        else
        {
            keyLabel.color = Color.white;
        }
    }

    public void UpdateKey(Key newKey)
    {
        keyLabel.text = FormatKeyName(newKey);
    }

    public InputCommand GetCommand()
    {
        return command;
    }

    private string FormatCommandName(InputCommand cmd)
    {
        switch (cmd)
        {
            case InputCommand.Left: return "Move Left";
            case InputCommand.Right: return "Move Right";
            case InputCommand.Up: return "Move Up";
            case InputCommand.Down: return "Move Down";
            case InputCommand.LeftPunch: return "Left Punch";
            case InputCommand.RightPunch: return "Right Punch";
            case InputCommand.LeftKick: return "Left Kick";
            case InputCommand.RightKick: return "Right Kick";
            case InputCommand.RageArt: return "Rage Art";
            default: return cmd.ToString();
        }
    }

    private string FormatKeyName(Key key)
    {
        return key.ToString().Replace("Key.", "");
    }
}