using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputKeys
{
    public Dictionary<InputCommand, Key> keybinds = new Dictionary<InputCommand, Key>
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
}
