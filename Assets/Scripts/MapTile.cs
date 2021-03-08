using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace dmdspirit
{
    public class MapTile : MonoBehaviour
    {
        [SerializeField] private TMP_Text coordinateLabel;
        [SerializeField] private Transform[] objectsToRotate;
        [SerializeField] private Renderer floorImage;
        [SerializeField] private Color defaultTileColor;

        public MapPosition Position;

        public ConstructionSite ConstructionSite { get; private set; }
        public Building Building { get; private set; }

        private TileType type;
        private List<ResourceNode> resourceNodes;

        // IMPROVE: Flags?
        private bool[] teamControl;

        // TODO: if tile has resources track when it can become empty.

        private void Start()
        {
            Map.Instance.RegisterMapTile(this);
            resourceNodes = GetComponentsInChildren<ResourceNode>().ToList();
            foreach (var resourceNode in resourceNodes)
            {
                resourceNode.OnResourceDepleted += ResourceDepletedHandler;
            }
        }

        private void ResourceDepletedHandler(ResourceNode node)
        {
            resourceNodes.Remove(node);
            node.OnResourceDepleted -= ResourceDepletedHandler;
        }

        public void Initialize(int x, int y, TileType type, TileDirection direction)
        {
            teamControl = new bool[Enum.GetValues(typeof(TeamTag)).Length - 1];
            this.type = type;
            Position = new MapPosition(x, y);
            coordinateLabel.text = Position.Coordinates;
            name = $"{type.ToString()}({Position.CX}, {Position.y})";
            float rotation = 90 * (int) direction;
            foreach (var objectToRotate in objectsToRotate)
                objectToRotate.Rotate(Vector3.up, rotation);
            // IMPROVE: This is ugly. Rework tile type.
            floorImage.material = Map.Instance.GetTileMaterial(teamControl);
        }

        public void AddTeamControl(Team team)
        {
            teamControl[Team.GetTeamIndexFromTag(team.teamTag)] = true;
            floorImage.material = Map.Instance.GetTileMaterial(teamControl);
        }

        public void RemoveTeamControl(TeamTag teamTag)
        {
            var teamIndex = Team.GetTeamIndexFromTag(teamTag);
            teamControl[teamIndex] = false;
            floorImage.material = Map.Instance.GetTileMaterial(teamControl);
        }

        public bool CheckCanBuild(TeamTag tag, BuildingType buildingType)
        {
            if (teamControl[Team.GetTeamIndexFromTag(tag)] == false || Building != null || resourceNodes.Count != 0) return false;
            if (buildingType == BuildingType.None || ConstructionSite == null) return true;
            return ConstructionSite.Building.type == buildingType;
        }

        public void AddConstructionSite(ConstructionSite constructionSite)
        {
            ConstructionSite = constructionSite;
            constructionSite.OnConstructionSiteFinished += ConstructionSiteFinishedHandler;
        }

        private void ConstructionSiteFinishedHandler(ConstructionSite cs) => ConstructionSite = null;
    }
}