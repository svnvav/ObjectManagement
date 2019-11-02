namespace Catlike.ObjectManagement
{
    public struct ShapeInstance
    {
        private int _instanceId;
        
        public Shape Shape { get; private set; }
        
        public bool IsValid => Shape != null && _instanceId == Shape.InstanceId;

        public ShapeInstance(Shape shape)
        {
            Shape = shape;
            _instanceId = shape.InstanceId;
        }

        public static implicit operator ShapeInstance(Shape shape)
        {
            return new ShapeInstance(shape);
        }
    }
}