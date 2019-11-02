using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class SatelliteShapeBehaviour : ShapeBehaviour
    {
        public override ShapeBehaviourType BehaviorType => ShapeBehaviourType.Satellite;

        ShapeInstance focalShape;

        float frequency;

        Vector3 cosOffset, sinOffset;

        public void Initialize (
            Shape shape, Shape focalShape, float radius, float frequency
        ) {
            this.focalShape = focalShape;
            this.frequency = frequency;
            Vector3 orbitAxis = Random.onUnitSphere;
            do
            {
                cosOffset = Vector3.Cross(orbitAxis, Random.onUnitSphere).normalized;
            } 
            while (cosOffset.sqrMagnitude < 0.1f);

            sinOffset = Vector3.Cross(cosOffset, orbitAxis);
            cosOffset *= radius;
            sinOffset *= radius;

            shape.AddBehaviour<RotationShapeBehaviour>().AngularVelocity =
                -360f * frequency * shape.transform.InverseTransformDirection(orbitAxis);
        }

        public override void GameUpdate(Shape shape)
        {
            if (!focalShape.IsValid) return;
            
            var t = 2f * Mathf.PI * frequency * shape.Age;
            shape.transform.localPosition =
                focalShape.Shape.transform.localPosition +
                cosOffset * Mathf.Cos(t) + sinOffset * Mathf.Sin(t);
        }

        public override void Save(GameDataWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public override void Load(GameDataReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override void Recycle()
        {
            ShapeBehaviourPool<SatelliteShapeBehaviour>.Reclaim(this);
        }
    }
}