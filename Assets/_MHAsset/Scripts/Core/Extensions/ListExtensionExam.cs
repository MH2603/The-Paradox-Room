using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public class ListExtensionExam : MonoBehaviour
    {
        public List<Vector3> list01 = new();
        public List<Vector3> list02 = new();
        public List<int> removeIndices = new();

        public void UpdateLists()
        {
            list01 = ListExtensions.RemoveDuplicates(list01, removeIndices);

            foreach (var index in removeIndices)
            {
                list02.RemoveAt(index);
            }
        }
    }
}