using System;
using System.Collections.Generic;

public class Heap<T>
{
    private List<T> items;
    private IComparer<T> _comparer; // comparator to define heap order

    public Heap()
    {
        items = new List<T>();
        _comparer = Comparer<T>.Default; // default configuration
    }

    public Heap(IComparer<T> comparer)
    {
        items = new List<T>();
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(bool doReverse)
    {
        items = new List<T>();
        if (doReverse)
            _comparer = Comparer<T>.Create((x, y) => Comparer<T>.Default.Compare(y, x));
        else
            _comparer = Comparer<T>.Default;
    }

    public Heap(List<T> _items, bool doReverse)
    {
        items = new List<T>();
        if (doReverse)
            _comparer = Comparer<T>.Create((x, y) => Comparer<T>.Default.Compare(y, x));
        else
            _comparer = Comparer<T>.Default;

        heapify(_items);
    }

    public void insert(T item)
    {
        items.Add(item);

        int index = items.Count - 1;
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;

            if (_comparer.Compare(items[index], items[parentIndex]) >= 0)
                break;

            // Swap
            (items[index], items[parentIndex]) = (items[parentIndex], items[index]);

            index = parentIndex;
        }
    }

    public T remove()
    {
        if (items.Count == 0) throw new InvalidOperationException("Heap is empty");

        T root = items[0];
        int lastIndex = items.Count - 1;

        items[0] = items[lastIndex];
        items.RemoveAt(lastIndex);

        if (items.Count == 0) return root;

        int index = 0;
        while (true)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex < items.Count && _comparer.Compare(items[leftChildIndex], items[smallestIndex]) < 0)
            {
                smallestIndex = leftChildIndex;
            }

            if (rightChildIndex < items.Count && _comparer.Compare(items[rightChildIndex], items[smallestIndex]) < 0)
            {
                smallestIndex = rightChildIndex;
            }

            if (smallestIndex == index)
                break;

            // Swap
            (items[index], items[smallestIndex]) = (items[smallestIndex], items[index]);

            index = smallestIndex;
        }

        return root;
    }

    public void heapify(List<T> _items) // will rebuild the entire list
    {
        // if for whatever reason the item list has values in it already, run an exception saying that needed to be empty
        if (items != null && items.Count > 0) throw new InvalidOperationException("Heap must be empty to heapify a new list.");

        for (int i = 0; i < _items.Count; i++)
        {
            insert(_items[i]);
        }
    }


    public T peek()
    {
        if (items.Count == 0) throw new InvalidOperationException("Heap is empty");
        return items[0];
    }

    public int Count
    {
        get { return items.Count; }
    }

    public bool IsEmpty
    {
        get { return items.Count == 0; }
    }

    public void Clear()
    {
        items.Clear();
    }

    public bool Contains(T item)
    {
        return items.Contains(item);
    }

    public List<T> ToList()
    {
        return new List<T>(items);
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= items.Count)
                throw new IndexOutOfRangeException();
            return items[index];
        }
    }

}
