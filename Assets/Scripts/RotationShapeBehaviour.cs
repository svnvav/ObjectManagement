using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class RotationShapeBehaviour : ShapeBehaviour
    {
        public override ShapeBehaviourType BehaviorType => ShapeBehaviourType.Rotation;
        
        public Vector3 AngularVelocity { get; set; }

        public override void GameUpdate (Shape shape) {
            shape.transform.Rotate(AngularVelocity * Time.deltaTime);
        }

        public override void Save (GameDataWriter writer) {
            writer.Write(AngularVelocity);
        }

        public override void Load (GameDataReader reader) {
            AngularVelocity = reader.ReadVector3();
        }
    }
}