﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loyc.Collections.Impl;
using Loyc.Math;

namespace Loyc.Collections
{
	/// <summary>Extension methods and helper methods for <see cref="List{T}"/>,
	/// <see cref="IList{T}"/>, <see cref="IListSource<T>"/>, arrays, and for 
	/// related mutable interfaces such as <see cref="IArray{T}"/>. 
	/// </summary>
	/// <remarks>Extension methods that only apply to Loyc's new interfaces will 
	/// go in <see cref="LCExt"/>.</remarks>
	public static class ListExt
	{
		public static T TryGet<T>(this T[] list, int index, T defaultValue)
		{
			if ((uint)index < (uint)list.Length)
				return list[index];
			return defaultValue;
		}
		public static T TryGet<T>(this List<T> list, int index, T defaultValue)
		{
			if ((uint)index < (uint)list.Count)
				return list[index];
			return defaultValue;
		}
		public static T TryGet<T>(this IList<T> list, int index, T defaultValue)
		{
			if ((uint)index < (uint)list.Count)
				return list[index];
			return defaultValue;
		}

		public static void RemoveRange<T>(this IList<T> list, int index, int count)
		{
			if (index < 0)
				throw new IndexOutOfRangeException(index.ToString() + " < 0");
			if (count > list.Count - index)
				throw new IndexOutOfRangeException(index.ToString() + " + " + count.ToString() + " > " + list.Count.ToString());
			if (count > 0) {
				for (int i = index; i < list.Count - count; i++)
					list[i] = list[i + count];
				Resize(list, list.Count - count);
			}
		}
		public static void Resize<T>(this List<T> list, int newSize)
		{
			int dif = newSize - list.Count;
			if (dif > 0) {
				do list.Add(default(T));
				while (--dif != 0);
			} else if (dif < 0) {
				int i = list.Count;
				do list.RemoveAt(--i);
				while (++dif != 0);
			}
		}
		public static void Resize<T>(this IList<T> list, int newSize)
		{
			int dif = newSize - list.Count;
			if (dif > 0) {
				do list.Add(default(T));
				while (--dif != 0);
			} else if (dif < 0) {
				int i = list.Count;
				do list.RemoveAt(--i);
				while (++dif != 0);
			}
		}

		public static IEnumerable<Pair<A, B>> Zip<A, B>(this IEnumerable<A> a, IEnumerable<B> b)
		{
			var ea = a.GetEnumerator();
			var eb = b.GetEnumerator();
			while (ea.MoveNext() && eb.MoveNext())
				yield return Pair.Create(ea.Current, eb.Current);
		}
		public static IEnumerable<Pair<A, B>> ZipLonger<A, B>(this IEnumerable<A> a, IEnumerable<B> b)
		{
			return ZipLonger(a, b, default(A), default(B));
		}
		public static IEnumerable<Pair<A, B>> ZipLonger<A, B>(this IEnumerable<A> a, IEnumerable<B> b, A defaultA, B defaultB)
		{
			var ea = a.GetEnumerator();
			var eb = b.GetEnumerator();
			bool haveA, haveB;
			for (; ; ) {
				haveA = ea.MoveNext();
				haveB = eb.MoveNext();
				if (!haveA && !haveB)
					break;
				yield return Pair.Create(haveA ? ea.Current : defaultA, haveB ? eb.Current : defaultB);
			}
		}

		static int[] RangeArray(int count)
		{
			var n = new int[count];
			for (int i = 0; i < n.Length; i++) n[i] = i;
			return n;
		}

		/// <inheritdoc cref="Sort(IList{T}, int, int, Comparison{T})"/>
		public static void Sort<T>(this IList<T> list)
		{
			Sort(list, Comparer<T>.Default.Compare);
		}
		/// <inheritdoc cref="Sort(IList{T}, int, int, Comparison{T})"/>
		public static void Sort<T>(this IList<T> list, Comparison<T> comp)
		{
			Sort(list, 0, list.Count, comp, null);
		}
		/// <summary>Performs a quicksort using a Comparison function.</summary>
		/// <param name="index">Index at which to begin sorting a portion of the list.</param>
		/// <param name="count">Number of items to sort starting at 'index'.</param>
		/// <remarks>
		/// This method exists because the .NET framework offers no method to
		/// sort <see cref="IList{T}"/>--you can sort arrays and <see cref="List{T}"/>, 
		/// but not IList.
		/// </remarks>
		public static void Sort<T>(this IList<T> list, int index, int count, Comparison<T> comp)
		{
			Sort(list, index, count, comp, null);
		}

		/// <summary>Performs a stable sort, i.e. a sort that preserves the 
		/// relative order of items that compare equal.</summary>
		/// <remarks>
		/// This algorithm uses a quicksort and therefore runs in O(N log N) time,
		/// but it requires O(N) temporary space (specifically, an array of N 
		/// integers) and is slower than a standard quicksort, so you should use
		/// it only if you need a stable sort.
		/// </remarks>
		public static void StableSort<T>(this IList<T> list, Comparison<T> comp)
		{
			Sort(list, 0, list.Count, comp, RangeArray(list.Count));
		}
		public static void StableSort<T>(this IList<T> list)
		{
			StableSort(list, Comparer<T>.Default.Compare);
		}

		private static void Sort<T>(this IList<T> list, int index, int count, Comparison<T> comp, int[] indexes)
		{
			// This code duplicates the code in InternalList.Sort(), except
			// that it also supports stable sorting. This version is slower;
			// Two versions exist so that array sorting can be done faster.
			CheckParam.Range("index", index, 0, list.Count);
			CheckParam.Range("count", count, 0, list.Count - index);

			for (;;) {
				if (count < InternalList.QuickSortThreshold)
				{
					if (count <= 2) {
						if (count == 2)
							SortPair(list, index, index + 1, comp);
					} else {
						InsertionSort(list, index, count, comp);
					}
					return;
				}

				int iPivot = InternalList.PickPivot(list, index, count, comp);

				int iBegin = index;
				// Swap the pivot to the beginning of the range
				T pivot = list[iPivot];
				if (iBegin != iPivot) {
					Swap(list, iBegin, iPivot);
					if (indexes != null)
						MathEx.Swap(ref indexes[iPivot], ref indexes[iBegin]);
				}

				int i = iBegin + 1;
				int iOut = iBegin;
				int iStop = index + count;
				int leftSize = 0; // size of left partition

				// Quick sort pass
				do {
					int order = comp(list[i], pivot);
					if (order > 0)
						continue;
					if (order == 0) {
						if (indexes != null) {
							if (indexes[i] > indexes[iBegin])
								continue;
						} else if (leftSize < (count >> 1))
							continue;
					}
					
					++iOut;
					++leftSize;
					if (i != iOut) {
						Swap(list, i, iOut);
						if (indexes != null)
							MathEx.Swap(ref indexes[i], ref indexes[iOut]);
					}
				} while (++i != iStop);

				// Finally, put the pivot element in the middle (at iOut)
				Swap(list, iBegin, iOut);
				if (indexes != null)
					MathEx.Swap(ref indexes[iBegin], ref indexes[iOut]);

				// Now we need to sort the left and right sub-partitions. Use a 
				// recursive call only to sort the smaller partition, in order to 
				// guarantee O(log N) stack space usage.
				int rightSize = count - 1 - leftSize;
				if (leftSize < rightSize)
				{
					// Recursively sort the left partition; iteratively sort the right
					Sort(list, index, leftSize, comp, indexes);
					index += leftSize + 1;
					count = rightSize;
				}
				else
				{	// Iteratively sort the left partition; recursively sort the right
					count = leftSize;
					Sort(list, index + leftSize + 1, rightSize, comp, indexes);
				}
			}
		}

		/// <summary>Performs an insertion sort.</summary>
		/// <remarks>The insertion sort is a stable sort algorithm that is slow in 
		/// general (O(N^2)). It should be used only when (a) the list to be sorted
		/// is short (less than 10-20 elements) or (b) the list is very nearly
		/// sorted already.</remarks>
		/// <seealso cref="InternalList.InsertionSort"/>
		public static void InsertionSort<T>(this IList<T> array, int index, int count, Comparison<T> comp)
		{
			for (int i = index + 1; i < index + count; i++)
			{
				int j = i;
				do
					if (!SortPair(array, j - 1, j, comp))
						break;
				while (--j > index);
			}
		}

		/// <summary>Sorts two items to ensure that list[i] is less than list[j].</summary>
		/// <returns>True if the array elements were swapped, false if not.</returns>
		public static bool SortPair<T>(this IList<T> list, int i, int j, Comparison<T> comp)
		{
			if (i != j && comp(list[i], list[j]) > 0) {
				Swap(list, i, j);
				return true;
			}
			return false;
		}

		/// <summary>Swaps list[i] with list[j].</summary>
		public static void Swap<T>(this IList<T> list, int i, int j)
		{
			T tmp = list[i];
			list[i] = list[j];
			list[j] = tmp;
		}
		public static void Swap<T>(this IArray<T> list, int i, int j)
		{
			T tmp = list[i];
			list[i] = list[j];
			list[j] = tmp;
		}

		/// <summary>Gets the lowest index at which a condition is true, or -1 if nowhere.</summary>
		public static int IndexWhere<T>(this IList<T> list, Func<T, bool> pred)
		{
			return LCInterfaces.IndexWhere(list.AsListSource(), pred);
		}
		/// <summary>Gets the highest index at which a condition is true, or -1 if nowhere.</summary>
		public static int LastIndexWhere<T>(this IList<T> list, Func<T, bool> pred)
		{
			return LCInterfaces.LastIndexWhere(list.AsListSource(), pred);
		}
		public static ListSlice<T> Slice<T>(this IList<T> list, int start, int length = int.MaxValue)
		{
			return new ListSlice<T>(list, start, length);
		}
		public static ArraySlice<T> Slice<T>(this T[] list, int start, int length = int.MaxValue)
		{
			return new ArraySlice<T>(list, start, int.MaxValue);
		}
		public static Slice_<T> Slice<T>(this IListSource<T> array, int start, int count = int.MaxValue)
		{
			return new Slice_<T>(array, start, count);
		}

		public static int IndexOfMin(this IEnumerable<int> source)
		{
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;
			int i = 0, min_i = 0;
			for (int min = e.Current; e.MoveNext(); i++) {
				if (min > e.Current) {
					min = e.Current;
					min_i = i+1;
				}
			}
			return min_i;
		}
		public static int IndexOfMin<T>(this IEnumerable<T> source, Func<T, int> selector)
		{
			int _;
			return IndexOfMin<T>(source, selector, out _);
		}
		public static int IndexOfMin<T>(this IEnumerable<T> source, Func<T, int> selector, out int min)
		{
			min = 0;
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;
			int i = 0, min_i = 0, cur;
			for (min = selector(e.Current); e.MoveNext(); i++) {
				if (min > (cur = selector(e.Current))) {
					min = cur;
					min_i = i+1;
				}
			}
			return min_i;
		}
		public static int IndexOfMax(this IEnumerable<int> source)
		{
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;
			int i = 0, max_i = 0;
			for (int max = e.Current; e.MoveNext(); i++) {
				if (max < e.Current) {
					max = e.Current;
					max_i = i+1;
				}
			}
			return max_i;
		}
		public static int IndexOfMax<T>(this IEnumerable<T> source, Func<T, int> selector)
		{
			int _;
			return IndexOfMax<T>(source, selector, out _);
		}
		public static int IndexOfMax<T>(this IEnumerable<T> source, Func<T, int> selector, out int max)
		{
			max = 0;
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;
			int i = 0, max_i = 0, cur;
			for (max = selector(e.Current); e.MoveNext(); i++) {
				if (max < (cur = selector(e.Current))) {
					max = cur;
					max_i = i+1;
				}
			}
			return max_i;
		}


		public static int IndexOfMin<T>(this IEnumerable<T> source)
		{
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;

			Comparer<T> comparer = Comparer<T>.Default;
			T min, cur;
			int i = 0, min_i = 0;
			for (min = e.Current; min == null; i++, min = e.Current)
				if (!e.MoveNext())
					return -1;
			while (e.MoveNext()) {
				i++;
				if ((cur = e.Current) != null && comparer.Compare(cur, min) < 0) {
					min = cur;
					min_i = i;
				}
			}
			return min_i;
		}
		public static int IndexOfMin<T, R>(this IEnumerable<T> source, Func<T, R> selector)
		{
			R _;
			return IndexOfMin<T, R>(source, selector, out _);
		}
		public static int IndexOfMin<T, R>(this IEnumerable<T> source, Func<T, R> selector, out R min)
		{
			min = default(R);
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;

			Comparer<R> comparer = Comparer<R>.Default;
			R cur;
			int i = 0, min_i = 0;
			for (min = selector(e.Current); min == null; i++, min = selector(e.Current))
				if (!e.MoveNext())
					return -1;
			while (e.MoveNext()) {
				i++;
				if ((cur = selector(e.Current)) != null && comparer.Compare(cur, min) < 0) {
					min = cur;
					min_i = i;
				}
			}
			return min_i;
		}
		public static int IndexOfMax<T>(this IEnumerable<T> source)
		{
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;

			Comparer<T> comparer = Comparer<T>.Default;
			T max, cur;
			int i = 0, max_i = 0;
			for (max = e.Current; max == null; i++, max = e.Current)
				if (!e.MoveNext())
					return -1;
			while (e.MoveNext()) {
				i++;
				if ((cur = e.Current) != null && comparer.Compare(cur, max) > 0) {
					max = cur;
					max_i = i;
				}
			}
			return max_i;
		}
		public static int IndexOfMax<T, R>(this IEnumerable<T> source, Func<T, R> selector)
		{
			R _;
			return IndexOfMax(source, selector, out _);
		}
		public static int IndexOfMax<T, R>(this IEnumerable<T> source, Func<T, R> selector, out R max)
		{
			max = default(R);
			var e = source.GetEnumerator();
			if (!e.MoveNext())
				return -1;

			Comparer<R> comparer = Comparer<R>.Default;
			R cur;
			int i = 0, max_i = 0;
			for (max = selector(e.Current); max == null; i++, max = selector(e.Current))
				if (!e.MoveNext())
					return -1;
			while (e.MoveNext()) {
				i++;
				if ((cur = selector(e.Current)) != null && comparer.Compare(cur, max) > 0) {
					max = cur;
					max_i = i;
				}
			}
			return max_i;
		}

		static Random _r = new Random();
		public static void Randomize<T>(this IList<T> list)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
				list.Swap(i, _r.Next(count));
		}
		public static void Randomize<T>(this T[] list)
		{
			for (int i = 0; i < list.Length; i++)
				MathEx.Swap(ref list[i], ref list[_r.Next(list.Length)]);
		}
		/// <summary>Quickly makes a copy of a list, as an array, in random order.</summary>
		public static T[] Randomized<T>(this IList<T> list)
		{
			T[] copy = new T[list.Count];
			list.CopyTo(copy, 0);
			Randomize(copy);
			return copy;
		}

		/// <summary>Maps an array to another array of the same length.</summary>
		public static R[] SelectArray<T, R>(this T[] input, Func<T,R> selector)
		{
			if (input == null)
				return null;
			R[] result = new R[input.Length];
			for (int i = 0; i < input.Length; i++)
				result[i] = selector(input[i]);
			return result;
		}

		/// <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
		/// <returns>The number of elements removed from the list</returns>
		public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
		{
			int to = 0, c = list.Count;
			for (int i = 0; i < c; i++) {
				if (!match(list[i])) {
					if (i != to)
						list[to] = list[i];
					to++;
				}
			}
			list.RemoveRange(to, c - to);
			return c - to;
		}

		public static void Reverse<T>(this IList<T> list) 
		{
			int c = list.Count;
			for (int i = 0; i < (c >> 1); i++)
				list.Swap(i, c - i);
		}
		public static void Reverse<T>(this IArray<T> list) 
		{
			int c = list.Count;
			for (int i = 0; i < (c >> 1); i++)
				list.Swap(i, c - i);
		}

		public static void AddRange<T>(this IList<T> list, IEnumerable<T> range)
		{
			foreach (var item in range)
				list.Add(item);
		}
	}
}
