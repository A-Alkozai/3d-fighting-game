using System;

[Flags]
public enum PlayerStates
{
    None            = 0,
    Idle            = 1 << 0,
    Crouching       = 1 << 1,
    Lying           = 1 << 2,
    
    Grounded        = 1 << 3,
    Airborne        = 1 << 4,
    
    FaceDown        = 1 << 5,
    FaceUp          = 1 << 6,
    HeadFirst       = 1 << 7,
    FeetFirst       = 1 << 8,

    Walking         = 1 << 9,
    Dashing         = 1 << 10,
    Running         = 1 << 11,
    RunMomentum     = 1 << 12,
    Rising          = 1 << 13,
    Jumping         = 1 << 14,
    Falling         = 1 << 15,
    Sidestepping    = 1 << 16,
    Rolling         = 1 << 17,

    Attacking       = 1 << 18,
    Guarding        = 1 << 19,

    Recovery        = 1 << 20,
    Immunity        = 1 << 21,  
    Stunned         = 1 << 22,
    KO              = 1 << 23
}