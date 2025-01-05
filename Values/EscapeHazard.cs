namespace SawTapes.Values
{
    public class EscapeHazard(string hazardName, bool spawnFacingAwayFromWall, bool spawnFacingWall, bool spawnWithBackToWall, bool spawnWithBackFlushAgainstWall)
    {
        public string HazardName { get; internal set; } = hazardName;
        public bool SpawnFacingAwayFromWall { get; internal set; } = spawnFacingAwayFromWall;
        public bool SpawnFacingWall { get; internal set; } = spawnFacingWall;
        public bool SpawnWithBackToWall { get; internal set; } = spawnWithBackToWall;
        public bool SpawnWithBackFlushAgainstWall { get; internal set; } = spawnWithBackFlushAgainstWall;
    }
}
