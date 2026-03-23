using System.Collections;
using System.Collections.Generic;

public class UniqueQueue<T> : IEnumerable<T>, IEnumerable
{
	private readonly HashSet<T> set;

	private readonly Queue<T> queue;

	public int Count => queue.Count;

	public UniqueQueue()
	{
		set = new HashSet<T>();
		queue = new Queue<T>();
	}

	public void Enqueue(T item)
	{
		if (set.Add(item))
		{
			queue.Enqueue(item);
		}
	}

	public T Dequeue()
	{
		T val = queue.Dequeue();
		set.Remove(val);
		return val;
	}

	public bool Contains(T item)
	{
		return set.Contains(item);
	}

	public void Clear()
	{
		set.Clear();
		queue.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return queue.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return queue.GetEnumerator();
	}
}
