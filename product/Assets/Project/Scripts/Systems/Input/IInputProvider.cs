using System.Collections.Generic;

// Interface for anything that can provide player inputs (keyboard, AI, network, etc.)
public interface IInputProvider
{
    List<InputObject> GetInputs();
}