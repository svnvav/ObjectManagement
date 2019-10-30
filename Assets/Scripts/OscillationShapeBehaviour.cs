using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class OscillationShapeBehaviour : ShapeBehaviour
    {
        public override ShapeBehaviourType BehaviorType => ShapeBehaviourType.Oscillation;

        private float _previousOscillation;
        
        public Vector3 Offset { get; set; }
	
        public float Frequency { get; set; }
        
        public override void GameUpdate(Shape shape)
        {
            float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * Time.time + shape.Age);
            shape.transform.localPosition +=
                (oscillation - _previousOscillation) * Offset;
            _previousOscillation = oscillation;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Frequency);
            writer.Write(_previousOscillation);
        }

        public override void Load(GameDataReader reader)
        {
            Offset = reader.ReadVector3();
            Frequency = reader.ReadFloat();
            _previousOscillation = reader.ReadFloat();
        }

        public override void Recycle()
        {
            _previousOscillation = 0f;
            ShapeBehaviourPool<OscillationShapeBehaviour>.Reclaim(this);
        }
    }
}