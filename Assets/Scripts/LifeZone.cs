using System;
using UnityEngine;

namespace Catlike.ObjectManagement
{
    public class LifeZone : MonoBehaviour
    {
        [SerializeField]
        float _dyingDuration;
        
        private void OnTriggerExit(Collider other)
        {
            var shape = other.GetComponent<Shape>();
            if (shape != null)
            {
                if (_dyingDuration <= 0f)
                {
                    shape.Die();
                }
                else if(!shape.IsMarkedAsDying)
                {
                    shape.AddBehaviour<DyingShapeBehaviour>().Initialize(shape, _dyingDuration);
                }
            }
        }
        
        private void OnDrawGizmos () {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            var c = GetComponent<Collider>();
            if (c is BoxCollider b)
            {
                Gizmos.matrix = Matrix4x4.TRS(
                    transform.position, transform.rotation, transform.lossyScale
                );
                Gizmos.DrawWireCube(b.center, b.size);
                return;
            }
            if (c is SphereCollider s){
                Vector3 scale = transform.lossyScale;
                scale = Vector3.one * Mathf.Max(
                            Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)
                        );
                Gizmos.matrix = Matrix4x4.TRS(
                    transform.position, transform.rotation, scale
                );
                Gizmos.DrawWireSphere(s.center, s.radius);
                return;
            }
        }
    }
}