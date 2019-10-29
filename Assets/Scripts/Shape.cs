using System;
using System.Collections.Generic;
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
        
        private Color[] _colors;
        
        private ShapeFactory _originFactory;

        private List<ShapeBehaviour> _shapeBehaviours;
        
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

        public int ColorCount => _colors.Length;

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
            _shapeBehaviours = new List<ShapeBehaviour>();
        }

        public void GameUpdate()
        {
            foreach (var behaviour in _shapeBehaviours)
            {
                behaviour.GameUpdate(this);
            }
        }

        public T AddBehaviour<T>() where T : ShapeBehaviour
        {
            var component = gameObject.AddComponent<T>();
            _shapeBehaviours.Add(component);
            return component;
        }
        
        ShapeBehaviour AddBehaviour (ShapeBehaviourType type) {
            switch (type) {
                case ShapeBehaviourType.Movement:
                    return AddBehaviour<MovementShapeBehaviour>();
                case ShapeBehaviourType.Rotation:
                    return AddBehaviour<RotationShapeBehaviour>();
            }
            Debug.LogError("Forgot to support " + type);
            return null;
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
            for (int i = 0; i < _shapeBehaviours.Count; i++) {
                Destroy(_shapeBehaviours[i]);
            }
            _shapeBehaviours.Clear();
            OriginFactory.Reclaim(this);
        }
        
        public override void Save (GameDataWriter writer) {
            base.Save(writer);
            writer.Write(_colors.Length);
            for (int i = 0; i < _colors.Length; i++) {
                writer.Write(_colors[i]);
            }
            writer.Write(_shapeBehaviours.Count);
            foreach (var behaviour in _shapeBehaviours)
            {
                writer.Write((int)behaviour.BehaviorType);
                behaviour.Save(writer);
            }
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

            if (reader.Version >= 9) {
                int behaviorCount = reader.ReadInt();
                for (int i = 0; i < behaviorCount; i++) {
                    AddBehaviour((ShapeBehaviourType)reader.ReadInt()).Load(reader);
                }
            }
            else if (reader.Version >= 7) {
                AddBehaviour<RotationShapeBehaviour>().AngularVelocity =
                    reader.ReadVector3();
                AddBehaviour<MovementShapeBehaviour>().Velocity = reader.ReadVector3();
            }
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