using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;

// Stores the mapping of InputCommands to keyboard keys, with save/load via PlayerPrefs
public class InputKeys
{
    // Default key bindings - used on first launch or after reset
    private Dictionary<InputCommand, Key> defaultKeybinds = new Dictionary<InputCommand, Key>
    {
        { InputCommand.Left, Key.A },
        { InputCommand.Right, Key.D },
        { InputCommand.Up, Key.W },
        { InputCommand.Down, Key.S },
        { InputCommand.LeftPunch, Key.I },
        { InputCommand.RightPunch, Key.O },
        { InputCommand.LeftKick, Key.J },
        { InputCommand.RightKick, Key.K },
        { InputCommand.RageArt, Key.R }
    };

    public Dictionary<InputCommand, Key> keybinds;

    // Start with defaults, then overwrite with any saved bindings
    public InputKeys()
    {
        keybinds = new Dictionary<InputCommand, Key>(defaultKeybinds);
        Load();
    }

    public Key GetKey(InputCommand command)
    {
        if (keybinds.TryGetValue(command, out Key key))
            return key;
        return Key.None;
    }

    public void SetKey(InputCommand command, Key newKey)
    {
        if (keybinds.ContainsKey(command))
        {
            keybinds[command] = newKey;
        }
    }

    // Restore all bindings to defaults and save
    public void ResetDefaults()
    {
        keybinds = new Dictionary<InputCommand, Key>(defaultKeybinds);
        Save();
    }

    // Write each binding to PlayerPrefs as a string
    public void Save()
    {
        foreach (var pair in keybinds)
        {
            PlayerPrefs.SetString($"keybind_{pair.Key}", pair.Value.ToString());
        }
        PlayerPrefs.Save();
    }

    // Read saved bindings from PlayerPrefs, falling back to defaults for missing entries
    public void Load()
    {
        Dictionary<InputCommand, Key> loaded = new Dictionary<InputCommand, Key>();

        foreach (var pair in defaultKeybinds)
        {
            string saved = PlayerPrefs.GetString($"keybind_{pair.Key}", "");
            if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse<Key>(saved, out Key loadedKey))
            {
                loaded[pair.Key] = loadedKey;
            }
            else
            {
                loaded[pair.Key] = pair.Value;
            }
        }

        keybinds = loaded;
    }

    public IReadOnlyDictionary<InputCommand, Key> GetAllBindings()
    {
        return keybinds;
    }

    public IReadOnlyDictionary<InputCommand, Key> GetDefaultBindings()
    {
        return defaultKeybinds;
    }
}