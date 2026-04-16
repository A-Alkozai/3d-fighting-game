// Simple wrapper that owns the MovesDatabase and handles loading with animation data dependency
public class MovesManager
{
    private MovesDatabase movesDatabase = new MovesDatabase();

    // Load moves JSON - requires animation database to be loaded first for frame counts
    public void LoadMoves(AnimationDatabase animationDatabase)
    {
        movesDatabase.AddAnimationDatabase(animationDatabase);
        movesDatabase.ReadJson();
    }

    public MovesDatabase GetMovesDatabase()
    {
        return movesDatabase;
    }
}