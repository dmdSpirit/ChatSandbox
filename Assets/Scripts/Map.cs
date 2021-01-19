using System;
using System.Collections.Generic;
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

        private MapTile[,] mapTiles;
        private List<MapTile> tiles;
        private int mapWidth;
        private int mapHeight;

        private void Awake()
        {
            resources = new Dictionary<ResourceType, List<ResourceNode>>();
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (resourceType == ResourceType.None) continue;
                resources.Add(resourceType, new List<ResourceNode>());
            }

            tiles = new List<MapTile>();
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