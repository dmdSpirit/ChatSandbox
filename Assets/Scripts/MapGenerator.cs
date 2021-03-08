using System;
using UnityEngine;

namespace dmdspirit
{
    public enum TileType
    {
        Empty,
        Stone,
        Wood,
        BaseBuilding
    }

    public enum TileDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    [Serializable]
    public struct TilePosition
    {
        public int x;
        public int y;
        public TileType type;
        public TileDirection direction;
        public Team team;
    }

    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private MapTile emptyTilePrefab;
        [SerializeField] private MapTile stoneTilePrefab;
        [SerializeField] private MapTile woodTilePrefab;
        [SerializeField] private MapTile baseBuildingTilePrefab;
        [SerializeField] private int mapWidth = 10;
        [SerializeField] private int mapHeight = 10;

        [SerializeField] private TilePosition[] tilePositions;

        private void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            ClearMap();
            MapTile tilePrefab;
            TileDirection direction;
            Team team;
            TileType tileType;
            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    tileType = TileType.Empty;
                    tilePrefab = emptyTilePrefab;
                    team = null;
                    direction = TileDirection.Up;
                    foreach (var tilePosition in tilePositions)
                    {
                        if (tilePosition.x == x && tilePosition.y == y)
                        {
                            switch (tilePosition.type)
                            {
                                case TileType.Stone:
                                    tilePrefab = stoneTilePrefab;
                                    break;
                                case TileType.Wood:
                                    tilePrefab = woodTilePrefab;
                                    break;
                                case TileType.BaseBuilding:
                                    tilePrefab = baseBuildingTilePrefab;
                                    break;
                            }

                            tileType = tilePosition.type;
                            direction = tilePosition.direction;
                            if (tilePosition.type == TileType.BaseBuilding)
                                team = tilePosition.team;
                            break;
                        }
                    }

                    var mapTile = Instantiate(tilePrefab, new Vector3((x - mapWidth / 2) * 10, 0, (y - mapHeight / 2) * 10), Quaternion.identity, transform);
                    mapTile.Initialize(x,y,tileType, direction);
                    if (team != null)
                    {
                        var baseBuilding = mapTile.GetComponentInChildren<BaseBuilding>();
                        if (baseBuilding != null)
                            team.baseBuilding = baseBuilding;
                    }
                }
            }
        }

        public void ClearMap()
        {
            var mapTiles = FindObjectsOfType<MapTile>();
            foreach (var mapTile in mapTiles)
                DestroyImmediate(mapTile.gameObject);
        }
    }
}