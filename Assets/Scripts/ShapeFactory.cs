using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField]
        private Shape[] _shapes;
        
        [SerializeField]
        private Material[] _materials;
        
        public Shape Get (int shapeId = 0, int materialId = 0) {
            Shape instance = Instantiate(_shapes[shapeId]);
            instance.ShapeId = shapeId;
            instance.SetMaterial(_materials[materialId], materialId);
            return instance;
        }
        
        public Shape GetRandom () {
            return Get(
                Random.Range(0, _shapes.Length), 
                Random.Range(0, _materials.Length)
                );
        }
    }
}