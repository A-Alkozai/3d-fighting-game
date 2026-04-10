using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Panel that displays all rebindable controls and handles the rebinding process
// Can be opened from the main menu or the in-game pause menu
public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject bindingRowPrefab;
    [SerializeField] private UnityEngine.UI.Button resetDefaultsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;

    private InputKeys inputKeys;
    private MainMenuManager menuManager;
    private GameManager gameManager;
    private List<ControlBindingRow> rows = new List<ControlBindingRow>();
    private bool isListening = false;      // True while waiting for a key press during rebind
    private bool openedFromPause = false;  // Tracks which menu to return to on back
    private bool initialised = false;      // Prevents adding button listeners more than once

    // Store references and wire up buttons, then build the list of binding rows
    // menuManager can be null if opened from the pause menu
    public void Initialise(InputKeys inputKeys, MainMenuManager menuManager)
    {
        this.inputKeys = inputKeys;
        this.menuManager = menuManager;

        // Only add listeners once to prevent duplicate calls
        if (!initialised)
        {
            resetDefaultsButton.onClick.AddListener(OnResetDefaults);
            backButton.onClick.AddListener(OnBack);
            initialised = true;
        }

        BuildRows();
    }

    // Store a reference to GameManager so we can return to the pause menu
    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    // Set whether this panel was opened from the pause menu
    public void SetOpenedFromPause(bool fromPause)
    {
        openedFromPause = fromPause;
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
            // Skip derived commands (Hold/Forward/Backward) as they are not directly rebindable
            if (IsDirectionalVariant(pair.Key)) continue;

            GameObject rowObj = Instantiate(bindingRowPrefab, contentParent);
            ControlBindingRow row = rowObj.GetComponent<ControlBindingRow>();
            row.Initialise(pair.Key, pair.Value, this);
            rows.Add(row);
        }
    }

    // Returns true for auto-generated directional variants that should not appear in the controls list
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
        // Wait one frame so the button click does not register as the new key
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

        // Reject the key if it is already bound to another command
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

    // Return to whichever menu opened this panel (pause menu or main menu)
    private void OnBack()
    {
        // Block navigation while a rebind is in progress
        if (isListening) return;

        if (openedFromPause && gameManager != null)
        {
            gameManager.ReturnToPauseMenu();
        }
        else if (menuManager != null)
        {
            menuManager.OnBackToMenu();
        }
    }
}