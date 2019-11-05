using UnityEditor;
using UnityEngine;

namespace Catlike.ObjectManagement
{
    [CustomEditor(typeof(GameLevel))]
    public class GameLevelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var gameLevel = (GameLevel)target;
            if (gameLevel.HasMissingLevelObjects) {
                EditorGUILayout.HelpBox("Missing level objects!", MessageType.Error);
            }
            if (GUILayout.Button("Remove Missing Elements")) {
                Undo.RecordObject(gameLevel, "Remove Missing Level Objects.");
                gameLevel.RemoveMissingLevelObjects();
            }
        }
    }
}