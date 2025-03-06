using System.Collections.Generic;

namespace MH
{
    public static class ListExtensions
    {
        public static List<T> RemoveDuplicates<T>(this List<T> list, List<int> removedIndices = null)
        {
            if (list == null) return null;

            List<T> newList = new List<T>();
            HashSet<T> set = new HashSet<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (set.Add(list[i]))
                {
                    newList.Add(list[i]);
                }
                else
                {
                    removedIndices?.Add(i);
                }
            }

            return newList;
        }
        
        public static void RemoveItemsByIndices<T>(List<T> list, List<int> removeIndices)
        {
            // Sort the removeIndices in descending order
            removeIndices.Sort((a, b) => b.CompareTo(a));

            // Remove items from the list based on the indices
            foreach (int index in removeIndices)
            {
                if (index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                }
            }
        }
    }
}