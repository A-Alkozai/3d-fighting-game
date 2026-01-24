public class MovesManager
{
    private MovesDatabase movesDatabase = new MovesDatabase();

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