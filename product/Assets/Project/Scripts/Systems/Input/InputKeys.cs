using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;

public class InputKeys
{
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

    public void ResetDefaults()
    {
        keybinds = new Dictionary<InputCommand, Key>(defaultKeybinds);
        Save();
    }

    public void Save()
    {
        foreach (var pair in keybinds)
        {
            PlayerPrefs.SetString($"keybind_{pair.Key}", pair.Value.ToString());
        }
        PlayerPrefs.Save();
    }

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