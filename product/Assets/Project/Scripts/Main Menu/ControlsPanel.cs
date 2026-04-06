using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject bindingRowPrefab;
    [SerializeField] private UnityEngine.UI.Button resetDefaultsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;

    private InputKeys inputKeys;
    private MainMenuManager menuManager;
    private List<ControlBindingRow> rows = new List<ControlBindingRow>();
    private bool isListening = false;

    public void Initialise(InputKeys inputKeys, MainMenuManager menuManager)
    {
        this.inputKeys = inputKeys;
        this.menuManager = menuManager;

        resetDefaultsButton.onClick.AddListener(OnResetDefaults);
        backButton.onClick.AddListener(OnBack);

        BuildRows();
    }

    private void BuildRows()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        foreach (var pair in inputKeys.GetAllBindings())
        {
            // Skip derived commands (Hold/Forward/Backward — not rebindable)
            if (IsDirectionalVariant(pair.Key)) continue;

            GameObject rowObj = Instantiate(bindingRowPrefab, contentParent);
            ControlBindingRow row = rowObj.GetComponent<ControlBindingRow>();
            row.Initialise(pair.Key, pair.Value, this);
            rows.Add(row);
        }
    }

    private bool IsDirectionalVariant(InputCommand command)
    {
        return command == InputCommand.LeftHold ||
               command == InputCommand.RightHold ||
               command == InputCommand.UpHold ||
               command == InputCommand.DownHold ||
               command == InputCommand.Forward ||
               command == InputCommand.ForwardHold ||
               command == InputCommand.Backward ||
               command == InputCommand.BackwardHold;
    }

    public void OnRebindRequested(ControlBindingRow row, InputCommand command)
    {
        if (isListening) return;
        isListening = true;
        row.SetListening(true);
        StartCoroutine(ListenForKey(row, command));
    }

    private System.Collections.IEnumerator ListenForKey(ControlBindingRow row, InputCommand command)
    {
        // Wait a frame so the click doesn't register as the new key
        yield return null;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            isListening = false;
            row.SetListening(false);
            yield break;
        }

        Key pressedKey = Key.None;

        while (pressedKey == Key.None)
        {
            // Check escape to cancel
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                isListening = false;
                row.SetListening(false);
                yield break;
            }

            // Iterate all keys to find which was pressed
            for (Key k = Key.A; k <= Key.Z; k++)
            {
                if (keyboard[k].wasPressedThisFrame)
                {
                    pressedKey = k;
                    break;
                }
            }

            if (pressedKey == Key.None)
            {
                // Check number keys
                for (Key k = Key.Digit0; k <= Key.Digit9; k++)
                {
                    if (keyboard[k].wasPressedThisFrame)
                    {
                        pressedKey = k;
                        break;
                    }
                }
            }

            if (pressedKey == Key.None)
            {
                // Check common extra keys
                Key[] extras = {
                    Key.Space, Key.Enter, Key.Tab,
                    Key.LeftShift, Key.RightShift,
                    Key.LeftCtrl, Key.RightCtrl,
                    Key.Comma, Key.Period, Key.Slash,
                    Key.Semicolon, Key.Quote,
                    Key.LeftBracket, Key.RightBracket,
                    Key.Minus, Key.Equals, Key.Backquote
                };

                foreach (Key k in extras)
                {
                    if (keyboard[k].wasPressedThisFrame)
                    {
                        pressedKey = k;
                        break;
                    }
                }
            }

            yield return null;
        }

        // Check for duplicate binding
        foreach (var pair in inputKeys.GetAllBindings())
        {
            if (pair.Value == pressedKey && pair.Key != command && !IsDirectionalVariant(pair.Key))
            {
                Debug.LogWarning($"[Controls] Key {pressedKey} already bound to {pair.Key}");
                isListening = false;
                row.SetListening(false);
                yield break;
            }
        }

        inputKeys.SetKey(command, pressedKey);
        inputKeys.Save();
        row.UpdateKey(pressedKey);
        row.SetListening(false);
        isListening = false;
    }

    public void Refresh()
    {
        foreach (ControlBindingRow row in rows)
        {
            row.UpdateKey(inputKeys.GetKey(row.GetCommand()));
        }
    }

    private void OnResetDefaults()
    {
        inputKeys.ResetDefaults();
        Refresh();
    }

    private void OnBack()
    {
        if (isListening) return;
        menuManager.OnBackToMenu();
    }
}