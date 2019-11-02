using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class GrowingShapeBehaviour : ShapeBehaviour
    {
        public override ShapeBehaviourType BehaviorType => ShapeBehaviourType.Growing;
        
        private Vector3 _originalScale;
        private float _duration;
        
        public void Initialize (Shape shape, float duration) {
            _originalScale = shape.transform.localScale;
            _duration = duration;
            shape.transform.localScale = Vector3.zero;
        }
        
        public override bool GameUpdate(Shape shape)
        {
            if (shape.Age < _duration) {
                float s = shape.Age / _duration;
                s = (3f - 2f * s) * s * s;
                shape.transform.localScale = s * _originalScale;
                return true;
            }
            shape.transform.localScale = _originalScale;
            return false;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_originalScale);
            writer.Write(_duration);
        }

        public override void Load(GameDataReader reader)
        {
            _originalScale = reader.ReadVector3();
            _duration = reader.ReadFloat();
        }

        public override void Recycle()
        {
            ShapeBehaviourPool<GrowingShapeBehaviour>.Reclaim(this);
        }
    }
}