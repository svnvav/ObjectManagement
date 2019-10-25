using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField] private Shape[] _shapes;

        [SerializeField] private Material[] _materials;

        [SerializeField] private bool _recycle = false;

        [NonSerialized] private List<Shape>[] _pools;

        [NonSerialized] private Scene _poolScene;

        void CreatePools()
        {
            _pools = new List<Shape>[_shapes.Length];
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new List<Shape>();
            }

#if UNITY_EDITOR
            _poolScene = SceneManager.GetSceneByName(name);
            if (_poolScene.isLoaded)
            {
                var inactiveShapes = _poolScene
                    .GetRootGameObjects()
                    .Where(go => !go.activeSelf)
                    .Select(go => go.GetComponent<Shape>());
                foreach (var shape in inactiveShapes)
                {
                    _pools[shape.ShapeId].Add(shape);
                }
                return;
            }
#endif

            _poolScene = SceneManager.CreateScene(name);
        }


        public Shape Get(int shapeId = 0, int materialId = 0)
        {
            Shape instance;

            if (_recycle)
            {
                if (_pools == null)
                {
                    CreatePools();
                }

                List<Shape> pool = _pools[shapeId];
                int lastIndex = pool.Count - 1;
                
                
                if (lastIndex >= 0)
                {
                    instance = pool[lastIndex];
                    pool.RemoveAt(lastIndex);
                }
                else
                {
                    instance = Instantiate(_shapes[shapeId]);
                    instance.ShapeId = shapeId;
                    SceneManager.MoveGameObjectToScene(instance.gameObject, _poolScene);
                }

                instance.gameObject.SetActive(true);
            }
            else
            {
                instance = Instantiate(_shapes[shapeId]);
                instance.ShapeId = shapeId;
            }

            instance.SetMaterial(_materials[materialId], materialId);

            return instance;
        }

        public Shape GetRandom()
        {
            return Get(
                Random.Range(0, _shapes.Length),
                Random.Range(0, _materials.Length)
            );
        }

        public void Reclaim(Shape shapeToRecycle)
        {
            if (_recycle)
            {
                if (_pools == null)
                {
                    CreatePools();
                }

                _pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
                shapeToRecycle.gameObject.SetActive(false);
            }
            else
            {
                Destroy(shapeToRecycle.gameObject);
            }
        }
    }
}