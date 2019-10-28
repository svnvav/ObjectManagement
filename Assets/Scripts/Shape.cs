using System;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Catlike.ObjectManagement
{
    public class Shape : PersistableObject
    {
        private static int colorPropertyId = Shader.PropertyToID("_Color");
        private static MaterialPropertyBlock sharedPropertyBlock;

        [SerializeField]
        private MeshRenderer[] _meshRenderers;
        
        private int _shapeId = int.MinValue;
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
        
        public int MaterialId { get; private set; }
        
        private Color[] _colors;
        public int ColorCount => _colors.Length;

        private ShapeFactory _originFactory;

        public ShapeFactory OriginFactory
        {
            get => _originFactory;
            set
            {
                if (_originFactory == null) {
                    _originFactory = value;
                }
                else {
                    Debug.LogError("Not allowed to change origin factory.");
                }
            }
        }

        public Vector3 AngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }

        private void Awake()
        {
            _colors = new Color[_meshRenderers.Length];
        }

        public void GameUpdate()
        {
            transform.Rotate(AngularVelocity * Time.deltaTime);
            transform.localPosition += Velocity * Time.deltaTime;
        }

        public void SetMaterial (Material material, int materialId) {
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.material = material;
            }
            MaterialId = materialId;
        }
        
        public void SetColor (Color color)
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = color;
            }
            
            if (sharedPropertyBlock == null) {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            
            foreach (var meshRenderer in _meshRenderers)
            {
                meshRenderer.SetPropertyBlock(sharedPropertyBlock);
            }
        }
        
        public void SetColor (Color color, int index) {
            if (sharedPropertyBlock == null) {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.SetColor(colorPropertyId, color);
            _colors[index] = color;
            _meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
        }
        
        public void Recycle () {
            OriginFactory.Reclaim(this);
        }
        
        public override void Save (GameDataWriter writer) {
            base.Save(writer);
            writer.Write(_colors.Length);
            for (int i = 0; i < _colors.Length; i++) {
                writer.Write(_colors[i]);
            }
            writer.Write(AngularVelocity);
            writer.Write(Velocity);
        }

        public override void Load (GameDataReader reader) {
            base.Load(reader);
            if (reader.Version >= 8)
            {
                LoadColors(reader);
            }
            else
            {
                SetColor(reader.Version > 2 ? reader.ReadColor() : Color.white);
            }
            
            AngularVelocity = reader.Version >= 7 ? reader.ReadVector3() : Vector3.zero;
            Velocity = reader.Version >= 7 ? reader.ReadVector3() : Vector3.zero;
        }

        private void LoadColors(GameDataReader reader)
        {
            int count = reader.ReadInt();
            int max = count <= _colors.Length ? count : _colors.Length;
            int i = 0;
            for (; i < max; i++) {
                SetColor(reader.ReadColor(), i);
            }
            if (count > _colors.Length) {
                for (; i < count; i++) {
                    reader.ReadColor();
                }
            }
            else if (count < _colors.Length) {
                for (; i < _colors.Length; i++) {
                    SetColor(Color.white, i);
                }
            }
            
        }
    }
}