using UnityEngine;

namespace Catlike.ObjectManagement
{
    public abstract class ShapeBehaviour
#if UNITY_EDITOR
        : ScriptableObject
#endif
    {
        public abstract ShapeBehaviourType BehaviorType { get; }

        public abstract bool GameUpdate(Shape shape);

        public abstract void Save(GameDataWriter writer);

        public abstract void Load(GameDataReader reader);

        public abstract void Recycle();
        
        public virtual void ResolveShapeInstances () {}

#if UNITY_EDITOR
        public bool IsReclaimed { get; set; }

        private void OnEnable()
        {
            if (IsReclaimed)
            {
                Recycle();
            }
        }
#endif
    }
}