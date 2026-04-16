// All possible input commands - includes raw directional (Left/Right), normalised (Forward/Backward),
// held variants (LeftHold, ForwardHold), and action buttons
public enum InputCommand
{
    Left,
    LeftHold,
    Right,
    RightHold,

    Forward,       // Normalised: towards opponent
    ForwardHold,
    Backward,      // Normalised: away from opponent
    BackwardHold,
    Up,
    UpHold,
    Down,
    DownHold,
    LeftPunch,
    RightPunch,
    LeftKick,
    RightKick,
    RageArt
}