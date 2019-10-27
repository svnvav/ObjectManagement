using UnityEngine;

namespace DefaultNamespace
{
    [System.Serializable]
    public class FloatRange
    {
        public float min, max;

        public float RandomValueInRange => Random.Range(min, max);
    }
}