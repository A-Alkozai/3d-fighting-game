public class CombatDatabase : BaseDatabase<CombatData>
{
    public CombatDatabase()
    {
        filePath = "Assets/Project/Data/Characters/Player1/combat.json";
    }

    public CombatData GetCombatData(string moveId)
    {
        dict.TryGetValue(moveId, out CombatData data);
        return data;
    }
}