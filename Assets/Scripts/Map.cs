using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dmdspirit
{
    [Serializable]
    public struct MapPosition
    {
        public int x;
        public int y;

        public string Coordinates => string.Concat(CX, y.ToString());
        public char CX => (char) (x + 'A');

        public MapPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool TryParse(string coordinates, out MapPosition position)
        {
            position = new MapPosition();
            coordinates = coordinates.ToUpper();
            char xChar = coordinates[0];
            if (xChar < 'A' || xChar > 'Z') return false;
            if (int.TryParse(coordinates.Substring(1, coordinates.Length - 1), out var yValue) == false) return false;
            position = new MapPosition(xChar - 'A', yValue);
            return true;
        }

        public override string ToString() => $"({CX},{y})";
    }

    public class Map : MonoSingleton<Map>
    {
        public Dictionary<ResourceType, List<ResourceNode>> resources { get; protected set; }

        [SerializeField] private Material empty;
        [SerializeField] private Material red;
        [SerializeField] private Material green;
        [SerializeField] private Material both;

        private MapTile[,] mapTiles;
        private List<MapTile> tiles;
        private int mapWidth;
        private int mapHeight;

        private void Awake()
        {
            resources = Resource.GetResourceTypes().ToDictionary(resource => resource, resource=> new List<ResourceNode>());
            tiles = new List<MapTile>();
        }

        public Material GetTileMaterial(bool[] teamControl)
        {
            // IMPROVE: ??
            var redControl = teamControl[Team.GetTeamIndexFromTag(TeamTag.red)];
            var greenControl = teamControl[Team.GetTeamIndexFromTag(TeamTag.green)];
            if (greenControl && redControl) return both;
            if (greenControl == false && redControl == false) return empty;
            return greenControl ? green : red;
        }

        public void AddTeamControl(Team team, MapTile tile, int radius)
        {
            for (var x = Mathf.Max(0, tile.Position.x - radius); x <= Mathf.Min(mapWidth - 1, tile.Position.x + radius); x++)
            for (var y = Mathf.Max(0, tile.Position.y - radius); y <= Mathf.Min(mapHeight - 1, tile.Position.y + radius); y++)
            {
                if (Mathf.Abs(tile.Position.x - x) + Mathf.Abs(tile.Position.y - y) > radius) continue;
                GetTile(x, y).AddTeamControl(team);
            }
        }

        public void RegisterMapTile(MapTile tile)
        {
            tiles.Add(tile);
            if (mapWidth <= tile.Position.x)
                mapWidth = tile.Position.x + 1;
            if (mapHeight <= tile.Position.y)
                mapHeight = tile.Position.y + 1;
        }

        public void StartGame()
        {
            var resourceObjects = FindObjectsOfType<ResourceNode>();
            foreach (var resource in resourceObjects)
            {
                resources[resource.value.type].Add(resource);
                resource.OnResourceDepleted += ResourceDepletedHandler;
            }

            ProcessTiles();
        }

        public Vector3 GetMapPosition(Vector3 position)
        {
            position.y = 5;
            if (Physics.Raycast(new Ray(position, Vector3.down), out var hit, LayerMask.NameToLayer("Floor")))
            {
                var tile = hit.collider.GetComponent<MapTile>();
                if (tile != null)
                    return tile.transform.position;
            }

            Debug.LogError($"Could not find tile for position {position.ToString()}.");
            return Vector3.zero;
        }

        public bool CheckPosition(MapPosition position) => position.x < mapWidth && position.y < mapHeight && position.x >= 0 && position.y >= 0;


        public MapTile GetTile(int x, int y) => GetTile(new MapPosition(x, y));

        public MapTile GetTile(MapPosition position)
        {
            if (CheckPosition(position) == false)
            {
                Debug.LogError($"Out of bounds when trying to get map tile. {position.ToString()}");
                return null;
            }

            var result = mapTiles[position.x, position.y];
            if (result == null)
                Debug.LogError($"Empty map tile in map bounds. {position.ToString()}");
            return result;
        }

        public List<ResourceNode> GetResourceNodesOfType(ResourceType type) => resources[type].Where(r => r.isAlive).ToList();

        private void ProcessTiles()
        {
            mapTiles = new MapTile[mapWidth, mapHeight];
            foreach (var tile in tiles)
                mapTiles[tile.Position.x, tile.Position.y] = tile;
        }

        private void ResourceDepletedHandler(ResourceNode resourceNode)
        {
            resources[resourceNode.value.type].Remove(resourceNode);
        }
    }
}