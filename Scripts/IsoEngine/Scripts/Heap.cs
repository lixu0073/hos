using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;
    public int Count { get { return currentItemCount; } }

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        if (currentItemCount >= items.Length)
        {
            T[] temp = new T[items.Length + 10];
            items.CopyTo(temp, 0);
            items = temp;
            Debug.LogError("Finished copying array - new length: " + items.Length);
        }
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public void Clear()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = default(T);
        }
        currentItemCount = 0;
    }

    public T RemoveFirst()
    {
        try
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }
        catch
        {
            return default(T);
        }
    }

    public bool Contains(T item)
    {
        if (item.HeapIndex >= items.Length)
            return false;
        return Equals(items[item.HeapIndex], item);
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    void SortDown(T item)
    {
        int debug = 0;
        while (true)
        {
            debug++;
            if (debug > 25000)
            {
                Debug.LogError("Infinite loop in Heap!");
                return;
            }
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount)
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        swapIndex = childIndexRight;

                if (item.CompareTo(items[swapIndex]) < 0)
                    Swap(item, items[swapIndex]);
                else
                    return;
            }
            else
                return;
        }
    }

    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);
            else
                break;

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAindex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAindex;
    }
}
