using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class MapTile : MonoBehaviour
    {
        [SerializeField] private TMP_Text coordinateLabel;
        [SerializeField] private Transform[] objectsToRotate;

        public MapPosition Position;

        public TileType Type { get; private set; }

        // HACK: rework later.
        public bool isEmpty => transform.childCount <= 1;

        private void Start()
        {
            Map.Instance.RegisterMapTile(this);
        }

        public void Initialize(int x, int y, TileType type, TileDirection direction)
        {
            Type = type;
            Position = new MapPosition(x, y);
            coordinateLabel.text = Position.Coordinates;
            name = $"{type.ToString()}({Position.CX}, {Position.y})";
            float rotation = 90 * (int) direction;
            foreach (var objectToRotate in objectsToRotate)
                objectToRotate.Rotate(Vector3.up, rotation);
        }

        // TODO: if tile has resources track when it can become empty.
    }
}