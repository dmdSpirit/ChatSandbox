using UnityEditor;
using UnityEngine;

namespace dmdspirit
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator mapGenerator = (MapGenerator) target;
            if (GUILayout.Button("Generate Map"))
                mapGenerator.GenerateMap();
            if (GUILayout.Button("Clear Map"))
                mapGenerator.ClearMap();
            base.OnInspectorGUI();
        }
    }
}