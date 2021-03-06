//
// System.Collections.Generic.List
//
// Authors:
//    Ben Maurer (bmaurer@ximian.com)
//    Martin Baulig (martin@ximian.com)
//    Carlos Alberto Cortez (calberto.cortez@gmail.com)
//    David Waite (mass@akuma.org)
//    Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
// Copyright (C) 2011,2012 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic {
	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	public class List<T> : IList<T>, IList
#if NET_4_5
		, IReadOnlyList<T>
#endif
	{
		T [] _items;
		int _size;
		int _version;
		
		const int DefaultCapacity = 4;
		
		public List ()
		{
			_items = EmptyArray<T>.Value;
		}
		
		public List (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			// initialize to needed size (if determinable)
			ICollection <T> c = collection as ICollection <T>;
			if (c == null) {
				_items = EmptyArray<T>.Value;;
				AddEnumerable (collection);
			} else {
				_size = c.Count;
				_items = new T [_size];
				c.CopyTo (_items, 0);
			}
		}
		
		public List (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			_items = new T [capacity];
		}
		
		internal List (T [] data, int size)
		{
			_items = data;
			_size = size;
		}
		
		public void Add (T item)
		{
			// If we check to see if we need to grow before trying to grow
			// we can speed things up by 25%
			if (_size == _items.Length)
				GrowIfNeeded (1);
			_items [_size++] = item;
			_version++;
		}
		
		void GrowIfNeeded (int newCount)
		{
			int minimumSize = _size + newCount;
			if (minimumSize > _items.Length)
				Capacity = Math.Max (Math.Max (Capacity * 2, DefaultCapacity), minimumSize);
		}
		
		void CheckRange (int idx, int count)
		{
			if (idx < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) idx + (uint) count > (uint) _size)
				throw new ArgumentException ("index and count exceed length of list");
		}
		
		void CheckRangeOutOfRange (int idx, int count)
		{
			if (idx < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) idx + (uint) count > (uint) _size)
				throw new ArgumentOutOfRangeException ("index and count exceed length of list");
		}

		void AddCollection (ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			if (collectionCount == 0)
				return;

			GrowIfNeeded (collectionCount);			 
			collection.CopyTo (_items, _size);
			_size += collectionCount;
		}

		void AddEnumerable (IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
			{
				Add (t);
			}
		}

		public void AddRange (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			ICollection <T> c = collection as ICollection <T>;
			if (c != null)
				AddCollection (c);
			else
				AddEnumerable (collection);
			_version++;
		}
		
				
		public void Clear ()
		{
			Array.Clear (_items, 0, _items.Length);
			_size = 0;
			_version++;
		}
		
		public bool Contains (T item)
		{
            foreach (var itemInList in this) if (itemInList.Equals(item)) return true;

            return false;
		}
				
		public void CopyTo (T [] array)
		{
			Array.Copy (_items, 0, array, 0, _size);
		}
		
		public void CopyTo (T [] array, int arrayIndex)
		{
			Array.Copy (_items, 0, array, arrayIndex, _size);
		}
		
		public void CopyTo (int index, T [] array, int arrayIndex, int count)
		{
			CheckRange (index, count);
			Array.Copy (_items, index, array, arrayIndex, count);
		}

		public bool Exists (Predicate <T> match)
		{
			CheckMatch(match);

			for (int i = 0; i < _size; i++) {
				var item = _items [i];
				if (match (item))
					return true;
			}

			return false;
		}
		
		public T Find (Predicate <T> match)
		{
			CheckMatch(match);

			for (int i = 0; i < _size; i++) {
				var item = _items [i];
				if (match (item))
					return item;
			}

			return default (T);
		}
		
		static void CheckMatch (Predicate <T> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
		}
		
		
		private List <T> FindAllList (Predicate <T> match)
		{
			List <T> results = new List <T> ();
			for (int i = 0; i < this._size; i++)
				if (match (this._items [i]))
					results.Add (this._items [i]);
			
			return results;
		}
				
		public void ForEach (Action <T> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			for(int i=0; i < _size; i++)
				action(_items[i]);
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}
		
		public List <T> GetRange (int index, int count)
		{
			CheckRange (index, count);
			T [] tmpArray = new T [count];
			Array.Copy (_items, index, tmpArray, 0, count);
			return new List <T> (tmpArray, count);
		}
		
		public int IndexOf (T item)
		{
            return _items.IndexOf(item);
		}
		
		
		void Shift (int start, int delta)
		{
			if (delta < 0)
				start -= delta;
			
			if (start < _size)
				Array.Copy (_items, start, _items, start + delta, _size - start);
			
			_size += delta;

			if (delta < 0)
				Array.Clear (_items, _size, -delta);
		}

		void CheckIndex (int index)
		{
			if (index < 0 || (uint) index > (uint) _size)
				throw new ArgumentOutOfRangeException ("index");
		}

		void CheckStartIndex (int index)
		{
			if (index < 0 || (uint) index > (uint) _size)
				throw new ArgumentOutOfRangeException ("startIndex");
		}
		
		public void Insert (int index, T item)
		{
			CheckIndex (index);			
			if (_size == _items.Length)
				GrowIfNeeded (1);
			Shift (index, 1);
			_items[index] = item;
			_version++;
		}
		
		public void InsertRange (int index, IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			CheckIndex (index);
			if (collection == this) {
				T[] buffer = new T[_size];
				CopyTo (buffer, 0);
				GrowIfNeeded (_size);
				Shift (index, buffer.Length);
				Array.Copy (buffer, 0, _items, index, buffer.Length);
			} else {
				ICollection <T> c = collection as ICollection <T>;
				if (c != null)
					InsertCollection (index, c);
				else
					InsertEnumeration (index, collection);
			}
			_version++;
		}

		void InsertCollection (int index, ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			GrowIfNeeded (collectionCount);
			
			Shift (index, collectionCount);
			collection.CopyTo (_items, index);
		}
		
		void InsertEnumeration (int index, IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
				Insert (index++, t);		
		}

		
		public bool Remove (T item)
		{
			int loc = IndexOf (item);
			if (loc != -1)
				RemoveAt (loc);
			
			return loc != -1;
		}
		
		public int RemoveAll (Predicate <T> match)
		{
			CheckMatch(match);
			int i = 0;
			int j = 0;

			// Find the first item to remove
			for (i = 0; i < _size; i++)
				if (match(_items[i]))
					break;

			if (i == _size)
				return 0;

			_version++;

			// Remove any additional items
			for (j = i + 1; j < _size; j++)
			{
				if (!match(_items[j]))
					_items[i++] = _items[j];
			}
			if (j - i > 0)
				Array.Clear (_items, i, j - i);

			_size = i;
			return (j - i);
		}
		
		public void RemoveAt (int index)
		{
			if (index < 0 || (uint)index >= (uint)_size)
				throw new ArgumentOutOfRangeException("index");
			Shift (index, -1);
			Array.Clear (_items, _size, 1);
			_version++;
		}
		
		public void RemoveRange (int index, int count)
		{
			CheckRange (index, count);
			if (count > 0) {
				Shift (index, -count);
				Array.Clear (_items, _size, count);
				_version++;
			}
		}
				
		public T [] ToArray ()
		{
			T [] t = new T [_size];
			Array.Copy (_items, t, _size);
			
			return t;
		}
		
		public void TrimExcess ()
		{
			Capacity = _size;
		}
		
		public bool TrueForAll (Predicate <T> match)
		{
			CheckMatch (match);

			for (int i = 0; i < _size; i++)
				if (!match(_items[i]))
					return false;
				
			return true;
		}
		
		public int Capacity {
			get { 
				return _items.Length;
			}
			set {
				if ((uint) value < (uint) _size)
					throw new ArgumentOutOfRangeException ();

                var newItems = new T[value];
                Array.Copy(_items, newItems, _items.Length);
                _items = newItems;
			}
		}
		
		public int Count {
			get { return _size; }
		}
		
		public T this [int index] {
			[MethodImpl ((MethodImplOptions)256)]
			get {
				if ((uint) index >= (uint) _size)
					throw new ArgumentOutOfRangeException ("index");

                return _items[index];
			}

			[MethodImpl ((MethodImplOptions)256)]
			set {
				if ((uint) index >= (uint) _size)
					throw new ArgumentOutOfRangeException ("index");
				_items [index] = value;
				_version++;
			}
		}
		
#region Interface implementations.
		IEnumerator <T> IEnumerable <T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array"); 
			Array.Copy (_items, 0, array, arrayIndex, _size);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		int IList.Add (object item)
		{
			try {
				Add ((T) item);
				return _size - 1;
			} catch (NullReferenceException) {
			} catch (InvalidCastException) {
			}
			throw new ArgumentException ("item");
		}
		
		bool IList.Contains (object item)
		{
			try {
				return Contains ((T) item);
			} catch (NullReferenceException) {
			} catch (InvalidCastException) {
			}
			return false;
		}
		
		int IList.IndexOf (object item)
		{
			try {
				return IndexOf ((T) item);
			} catch (NullReferenceException) {
			} catch (InvalidCastException) {
			}
			return -1;
		}
		
		void IList.Insert (int index, object item)
		{
			// We need to check this first because, even if the
			// item is null or not the correct type, we need to
			// return an ArgumentOutOfRange exception if the
			// index is out of range
			CheckIndex (index);
			try {
				Insert (index, (T) item);
				return;
			} catch (NullReferenceException) {
			} catch (InvalidCastException) {
			}
			throw new ArgumentException ("item");
		}
		
		void IList.Remove (object item)
		{
			try {
				Remove ((T) item);
				return;
			} catch (NullReferenceException) {
			} catch (InvalidCastException) {
			}
			// Swallow the exception--if we can't cast to the
			// correct type then we've already "succeeded" in
			// removing the item from the List.
		}
		
		bool ICollection <T>.IsReadOnly {
			get { return false; }
		}
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		bool IList.IsReadOnly {
			get { return false; }
		}
		
		object IList.this [int index] {
			get { return this [index]; }
			set {
				try {
					this [index] = (T) value;
					return;
				} catch (NullReferenceException) {
					// can happen when 'value' is null and T is a valuetype
				} catch (InvalidCastException) {
				}
				throw new ArgumentException ("value");
			}
		}
#endregion
				
		[Serializable]
		public struct Enumerator : IEnumerator <T>, IDisposable {
			readonly List<T> l;
			int next;
			readonly int ver;

			T current;

			internal Enumerator (List <T> l)
				: this ()
			{
				this.l = l;
				ver = l._version;
			}
			
			public void Dispose ()
			{
			}

			public bool MoveNext ()
			{
				var list = l;

				if ((uint)next < (uint)list._size && ver == list._version) {
					current = list._items [next++];
					return true;
				}

				if (ver != l._version)
					throw new InvalidOperationException ("Collection was modified; enumeration operation may not execute.");

				next = -1;
				return false;
			}

			public T Current {
				get { return current; }
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != l._version)
					throw new InvalidOperationException ("Collection was modified; enumeration operation may not execute.");

				next = 0;
				current = default (T);
			}
			
			object IEnumerator.Current {
				get {
					if (ver != l._version)
						throw new InvalidOperationException ("Collection was modified; enumeration operation may not execute.");

					if (next <= 0)
						throw new InvalidOperationException ();
					return current;
				}
			}
		}
	}
}
