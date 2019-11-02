using UnityEngine;

namespace Catlike.ObjectManagement
{
    [System.Serializable]
    public struct IntRange
    {
        public int min, max;

        public int RandomValueInRange => Random.Range(min, max + 1);
    }
}