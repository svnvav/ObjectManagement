namespace Catlike.ObjectManagement
{
    [System.Serializable]
    public struct ShapeInstance
    {
        private int _instanceIdOrSaveIndex;

        public Shape Shape { get; private set; }

        public bool IsValid => Shape != null && _instanceIdOrSaveIndex == Shape.InstanceId;

        public ShapeInstance(Shape shape)
        {
            Shape = shape;
            _instanceIdOrSaveIndex = shape.InstanceId;
        }

        public ShapeInstance(int saveIndex)
        {
            Shape = null;
            _instanceIdOrSaveIndex = Shape.InstanceId;
        }
        
        public void Resolve () {
            if (_instanceIdOrSaveIndex >= 0)
            {
                Shape = GameController.Instance.GetShape(_instanceIdOrSaveIndex);
                _instanceIdOrSaveIndex = Shape.InstanceId;
            }
        }

        public static implicit operator ShapeInstance(Shape shape)
        {
            return new ShapeInstance(shape);
        }
    }
}