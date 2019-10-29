using UnityEngine;

namespace Catlike.ObjectManagement
{
    public abstract class ShapeBehaviour : MonoBehaviour
    {
        public abstract ShapeBehaviourType BehaviorType { get; }
        
        public abstract void GameUpdate (Shape shape);
        
        public abstract void Save (GameDataWriter writer);

        public abstract void Load (GameDataReader reader);
    }
}