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
            if (GUILayout.Button("Player to Red"))
                gameController.AddPlayer(TeamTag.red);
            if (GUILayout.Button("Player to Green"))
                gameController.AddPlayer(TeamTag.green);
            if (GUILayout.Button("Player Fill"))
                gameController.AddPlayer();
        }
    }
}