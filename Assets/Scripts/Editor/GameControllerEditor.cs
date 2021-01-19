using UnityEditor;
using UnityEngine;

namespace dmdspirit
{
    [CustomEditor(typeof(GameController))]
    public class GameControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GameController gameController = (GameController) target;
            if (GUILayout.Button("Start Game"))
                gameController.StartGame();
        }
    }
}