public class FrameCounter
{
    private int frameNumber = 0;

    public void UpdateFrame()
    {
        frameNumber++;
    }

    public int GetFrameNumber()
    {
        return frameNumber;
    }
}