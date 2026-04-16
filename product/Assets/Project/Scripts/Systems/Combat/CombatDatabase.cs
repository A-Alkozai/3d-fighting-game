// Loads combat data from JSON - one entry per move that has hitboxes
public class CombatDatabase : BaseDatabase<CombatData>
{
    public CombatDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/combat.json";
    }

    // Returns null if the move has no combat data (e.g. idle, walking)
    public CombatData GetCombatData(string moveId)
    {
        dict.TryGetValue(moveId, out CombatData data);
        return data;
    }
}