// Simple frame counter - tracks how many frames have passed since creation
// NOT an int: always use .GetFrameNumber() to read, .UpdateFrame() to increment
public class FrameCounter
{
    private int frameNumber;

    public FrameCounter(int frameNumber = 0)
    {
        this.frameNumber = frameNumber;
    }

    // Increment by one frame (called once per logic tick)
    public void UpdateFrame()
    {
        frameNumber++;
    }

    public int GetFrameNumber()
    {
        return frameNumber;
    }

    // Set to -1 to signal this counter is disabled (e.g. held input released)
    public void DisableFrame()
    {
        frameNumber = -1;
    }

    // Reset back to frame 0 (e.g. when a looping move restarts)
    public void ResetFrame()
    {
        frameNumber = 0;
    }
}