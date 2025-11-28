public class FrameCounter
{
    private int frameNumber;

    public FrameCounter(int frameNumber = 0)
    {
        this.frameNumber = frameNumber;
    }

    public void UpdateFrame()
    {
        frameNumber++;
    }

    public int GetFrameNumber()
    {
        return frameNumber;
    }

    public void DisableFrame()
    {
        frameNumber = -1;
    }

    public void ResetFrame()
    {
        frameNumber = 0;
    }
}