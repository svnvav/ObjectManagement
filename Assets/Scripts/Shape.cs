using UnityEngine;

namespace DefaultNamespace
{
    public class Shape : PersistableObject
    {
        private static int colorPropertyId = Shader.PropertyToID("_Color");
        private static MaterialPropertyBlock sharedPropertyBlock;
        
        private int _shapeId = int.MinValue;
        
        private MeshRenderer _meshRenderer;
        
        private Color _color;
        
        public int MaterialId { get; private set; }
        
        public int ShapeId {
            get {
                return _shapeId;
            }
            set {
                if (_shapeId == int.MinValue) {
                    _shapeId = value;
                }
                else {
                    Debug.LogError("Not allowed to change shapeId.");
                }
            }
        }

        void Awake () {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void SetMaterial (Material material, int materialId) {
            _meshRenderer.material = material;
            MaterialId = materialId;
        }
        
        public void SetColor (Color color)
        {
            _color = color;
            
            if (sharedPropertyBlock == null) {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            _meshRenderer.SetPropertyBlock(sharedPropertyBlock);
        }
        
        public override void Save (GameDataWriter writer) {
            base.Save(writer);
            writer.Write(_color);
        }

        public override void Load (GameDataReader reader) {
            base.Load(reader);
            SetColor(reader.Version > 2 ? reader.ReadColor() : Color.white);
        }
    }
}