using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class MovementShapeBehaviour : ShapeBehaviour {

        public override ShapeBehaviourType BehaviorType => ShapeBehaviourType.Movement;

        public Vector3 Velocity { get; set; }

        public override void GameUpdate (Shape shape) {
            shape.transform.localPosition += Velocity * Time.deltaTime;
        }

        public override void Save (GameDataWriter writer) {
            writer.Write(Velocity);
        }

        public override void Load (GameDataReader reader) {
            Velocity = reader.ReadVector3();
        }
    }
}