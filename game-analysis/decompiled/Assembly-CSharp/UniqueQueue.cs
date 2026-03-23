using System;
using System.Collections;
using System.Collections.Generic;

// Token: 0x020000FE RID: 254
public class UniqueQueue<T> : IEnumerable<T>, IEnumerable
{
	// Token: 0x060006D6 RID: 1750 RVA: 0x000230C2 File Offset: 0x000212C2
	public UniqueQueue()
	{
		this.set = new HashSet<T>();
		this.queue = new Queue<T>();
	}

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x060006D7 RID: 1751 RVA: 0x000230E0 File Offset: 0x000212E0
	public int Count
	{
		get
		{
			return this.queue.Count;
		}
	}

	// Token: 0x060006D8 RID: 1752 RVA: 0x000230ED File Offset: 0x000212ED
	public void Enqueue(T item)
	{
		if (this.set.Add(item))
		{
			this.queue.Enqueue(item);
		}
	}

	// Token: 0x060006D9 RID: 1753 RVA: 0x0002310C File Offset: 0x0002130C
	public T Dequeue()
	{
		T t = this.queue.Dequeue();
		this.set.Remove(t);
		return t;
	}

	// Token: 0x060006DA RID: 1754 RVA: 0x00023133 File Offset: 0x00021333
	public bool Contains(T item)
	{
		return this.set.Contains(item);
	}

	// Token: 0x060006DB RID: 1755 RVA: 0x00023141 File Offset: 0x00021341
	public void Clear()
	{
		this.set.Clear();
		this.queue.Clear();
	}

	// Token: 0x060006DC RID: 1756 RVA: 0x00023159 File Offset: 0x00021359
	public IEnumerator<T> GetEnumerator()
	{
		return this.queue.GetEnumerator();
	}

	// Token: 0x060006DD RID: 1757 RVA: 0x0002316B File Offset: 0x0002136B
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.queue.GetEnumerator();
	}

	// Token: 0x040007E6 RID: 2022
	private readonly HashSet<T> set;

	// Token: 0x040007E7 RID: 2023
	private readonly Queue<T> queue;
}
