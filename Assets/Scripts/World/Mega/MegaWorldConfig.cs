using UnityEngine;

namespace NeonApocalypse.World.Mega
{
    [CreateAssetMenu(menuName = "Neon Apocalypse/Mega World Config", fileName = "MegaWorldConfig")]
    public sealed class MegaWorldConfig : ScriptableObject
    {
        [Header("World Footprint")]
        [Min(4)] public int cellsX = 30;
        [Min(4)] public int cellsZ = 24;
        [Min(100f)] public float cellSize = 700f;
        public int seed = 917401;

        [Header("Streaming")]
        [Range(1, 4)] public int activeRadius = 1;
        [Range(1, 5)] public int visualRadius = 2;
        [Min(0.05f)] public float rebuildBudgetSeconds = 0.004f;
        [Min(1)] public int buildsPerFrame = 1;

        [Header("Cell Density")]
        [Range(0, 100)] public int buildingsPerUrbanCell = 28;
        [Range(0, 60)] public int propsPerCell = 18;
        [Range(0, 20)] public int landmarkChancePercent = 8;

        public float WidthMeters => cellsX * cellSize;
        public float DepthMeters => cellsZ * cellSize;
        public Vector2 WorldMin => new Vector2(-WidthMeters * 0.5f, -DepthMeters * 0.5f);

        public Vector2Int WorldToCell(Vector3 world)
        {
            Vector2 min = WorldMin;
            return new Vector2Int(
                Mathf.FloorToInt((world.x - min.x) / cellSize),
                Mathf.FloorToInt((world.z - min.y) / cellSize));
        }

        public bool IsValidCell(Vector2Int cell) => cell.x >= 0 && cell.x < cellsX && cell.y >= 0 && cell.y < cellsZ;

        public Vector3 CellCenter(Vector2Int cell)
        {
            Vector2 min = WorldMin;
            return new Vector3(min.x + (cell.x + 0.5f) * cellSize, 0f, min.y + (cell.y + 0.5f) * cellSize);
        }
    }
}
