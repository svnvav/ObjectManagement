namespace Catlike.ObjectManagement
{
    public enum ShapeBehaviourType
    {
        Movement,
        Rotation,
        Oscillation,
        Satellite,
        Growing,
        Dying
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
                case ShapeBehaviourType.Satellite:
                    return ShapeBehaviourPool<SatelliteShapeBehaviour>.Get();
                case ShapeBehaviourType.Growing:
                    return ShapeBehaviourPool<GrowingShapeBehaviour>.Get();
                case ShapeBehaviourType.Dying:
                    return ShapeBehaviourPool<DyingShapeBehaviour>.Get();
            }
            UnityEngine.Debug.Log("Forgot to support " + type);
            return null;
        }
    }
}