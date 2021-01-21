using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class MapTile : MonoBehaviour
    {
        [SerializeField] private TMP_Text coordinateLabel;
        [SerializeField] private Transform[] objectsToRotate;

        public MapPosition Position;
        
        public ConstructionSite ConstructionSite { get; private set; }
        public Building Building { get; private set; }
        
        private TileType type;

        // TODO: if tile has resources track when it can become empty.

        private void Start()
        {
            Map.Instance.RegisterMapTile(this);
        }

        public void Initialize(int x, int y, TileType type, TileDirection direction)
        {
            this.type = type;
            Position = new MapPosition(x, y);
            coordinateLabel.text = Position.Coordinates;
            name = $"{type.ToString()}({Position.CX}, {Position.y})";
            float rotation = 90 * (int) direction;
            foreach (var objectToRotate in objectsToRotate)
                objectToRotate.Rotate(Vector3.up, rotation);
        }

        public bool CheckCanBuild() => ConstructionSite == null && Building == null && type != TileType.Stone && type != TileType.Wood;

        public void AddConstructionSite(ConstructionSite constructionSite)
        {
            if (CheckCanBuild() == false)
            {
                Debug.LogError($"Something is trying to build on tile {Position.ToString()}.");
                return;
            }

            this.ConstructionSite = constructionSite;
            constructionSite.OnConstructionSiteFinished += ConstructionSiteFinishedHandler;
        }

        private void ConstructionSiteFinishedHandler(ConstructionSite cs)
        {
            Building = ConstructionSite.Building;
            Building.transform.SetParent(transform);
            ConstructionSite.OnConstructionSiteFinished -= ConstructionSiteFinishedHandler;
            ConstructionSite = null;
        }
    }
}