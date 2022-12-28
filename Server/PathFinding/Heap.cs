using System;

namespace AO.PathFinding
{
    public class Heap<T> where T : IHeapItem<T>
    {
        public int Count { get; private set; }

        private readonly T[] items;

        public Heap(int maxHeapSize = 1200)
        {
            items = new T[maxHeapSize];
        }

        public void Clear()
        {
            Count = 0;
        }
        
        public bool Add(T item)
        {
            if (Count >= items.Length)
                return false;
            
            item.HeapIndex = Count;
            items[Count] = item;
            SortUp(item);
            Count++;
            return true;
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            Count--;
            items[0] = items[Count];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }
        
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            var existingItem = items[item.HeapIndex];
            return existingItem is not null && existingItem.Equals(item);
        }

        private void SortDown(T item)
        {
            for(;;)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;

                // Check that there's a left child, if there is non it's done sorting
                if (childIndexLeft >= Count)
                    return;
                
                // Set the swap index to it
                int swapIndex = childIndexLeft;

                // Check if there's a right child
                if (childIndexRight < Count)
                    // If the right child has a lower priority than the left one, set the swap index to it instead
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        swapIndex = childIndexRight;

                // If the item we are adding has a higher priority than the child it's done sorting
                if (item.CompareTo(items[swapIndex]) >= 0)
                    return;
             
                Swap(item, items[swapIndex]);
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            for(;;)
            {
                T parentItem = items[parentIndex];

                // If it's got a higher priority it returns 1, equal 0, lower -1
                if (item.CompareTo(parentItem) <= 0)
                    break;
                
                Swap(item, parentItem);
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
        }
    }

    public interface IHeapItem<in T> : IComparable<T>
    { 
        int HeapIndex { get; set; }
        bool Equals(T other);
    }
}

