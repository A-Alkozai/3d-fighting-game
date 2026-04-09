using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Panel that displays all rebindable controls and handles the rebinding process
public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject bindingRowPrefab;
    [SerializeField] private UnityEngine.UI.Button resetDefaultsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;

    private InputKeys inputKeys;
    private MainMenuManager menuManager;
    private List<ControlBindingRow> rows = new List<ControlBindingRow>();
    private bool isListening = false; // True while waiting for a key press during rebind

    // Store references and wire up buttons, then build the list of binding rows
    public void Initialise(InputKeys inputKeys, MainMenuManager menuManager)
    {
        this.inputKeys = inputKeys;
        this.menuManager = menuManager;

        resetDefaultsButton.onClick.AddListener(OnResetDefaults);
        backButton.onClick.AddListener(OnBack);

        BuildRows();
    }

    // Destroy old rows, then create one ControlBindingRow per rebindable command
    private void BuildRows()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        rows.Clear();

        foreach (var pair in inputKeys.GetAllBindings())
        {
            // Skip derived commands (Hold/Forward/Backward) - these aren't directly rebindable
            if (IsDirectionalVariant(pair.Key)) continue;

            GameObject rowObj = Instantiate(bindingRowPrefab, contentParent);
            ControlBindingRow row = rowObj.GetComponent<ControlBindingRow>();
            row.Initialise(pair.Key, pair.Value, this);
            rows.Add(row);
        }
    }

    // Returns true for auto-generated directional variants that shouldn't appear in the controls list
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

    // Called by a ControlBindingRow when its rebind button is clicked
    public void OnRebindRequested(ControlBindingRow row, InputCommand command)
    {
        if (isListening) return; // Only one rebind at a time
        isListening = true;
        row.SetListening(true);
        StartCoroutine(ListenForKey(row, command));
    }

    // Coroutine that waits each frame for a valid key press, then applies the rebind
    private System.Collections.IEnumerator ListenForKey(ControlBindingRow row, InputCommand command)
    {
        // Wait one frame so the button click doesn't register as the new key
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
            // Escape cancels the rebind
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                isListening = false;
                row.SetListening(false);
                yield break;
            }

            // Check letter keys A-Z
            for (Key k = Key.A; k <= Key.Z; k++)
            {
                if (keyboard[k].wasPressedThisFrame)
                {
                    pressedKey = k;
                    break;
                }
            }

            // Check number keys 0-9
            if (pressedKey == Key.None)
            {
                for (Key k = Key.Digit0; k <= Key.Digit9; k++)
                {
                    if (keyboard[k].wasPressedThisFrame)
                    {
                        pressedKey = k;
                        break;
                    }
                }
            }

            // Check common modifier and punctuation keys
            if (pressedKey == Key.None)
            {
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

        // Reject the key if it's already bound to another command
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

        // Apply and save the new binding
        inputKeys.SetKey(command, pressedKey);
        inputKeys.Save();
        row.UpdateKey(pressedKey);
        row.SetListening(false);
        isListening = false;
    }

    // Re-read all bindings from InputKeys and update every row's displayed key
    public void Refresh()
    {
        foreach (ControlBindingRow row in rows)
        {
            row.UpdateKey(inputKeys.GetKey(row.GetCommand()));
        }
    }

    // Reset all bindings to defaults and refresh the display
    private void OnResetDefaults()
    {
        inputKeys.ResetDefaults();
        Refresh();
    }

    // Return to main menu (blocked while a rebind is in progress)
    private void OnBack()
    {
        if (isListening) return;
        menuManager.OnBackToMenu();
    }
}