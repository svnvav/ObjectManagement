namespace Catlike.ObjectManagement
{
    public enum ShapeBehaviourType
    {
        Movement,
        Rotation,
        Oscillation
    }
    
    public static class ShapeBehaviorTypeMethods {

        public static ShapeBehaviour GetInstance (this ShapeBehaviourType type) {
            switch (type) {
                case ShapeBehaviourType.Movement:
                    return ShapeBehaviourPool<MovementShapeBehaviour>.Get();
                case ShapeBehaviourType.Rotation:
                    return ShapeBehaviourPool<RotationShapeBehaviour>.Get();
                case ShapeBehaviourType.Oscillation:
                    return ShapeBehaviourPool<OscillationShapeBehaviour>.Get();
            }
            UnityEngine.Debug.Log("Forgot to support " + type);
            return null;
        }
    }
}