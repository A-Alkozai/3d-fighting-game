public class MovesManager
{
    private MovesDatabase movesDatabase = new MovesDatabase();

    public void LoadMoves()
    {
        movesDatabase.ReadJson();
    }

    public MovesDatabase GetMovesDatabase()
    {
        return movesDatabase;
    }
}