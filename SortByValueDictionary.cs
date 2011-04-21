using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace UtilityCodeAsset.Collections
{
    /// <summary>
    /// A Hashtable that returns the list to you in the order sorted by Value.
    /// The design aims at small memory footprint as this is intended to be used by limited resource device.
    /// 
    /// </summary>
    public class SortByValueDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class, IComparable<TValue>  where TKey : class
    {
        private Dictionary<TKey, int> _map = new Dictionary<TKey, int>();
        private TKey[] _keys;
        private TValue[] _values;
        private bool _reSortRequired;
        private int _size=100;
        private int _increment = 100;
        private string _notSupportedMsg = @"List of Keys and Values is not accessible in SortByValueDictionary. 
                                            Please use GetEnumerator to loop throw the list of KeyValuePair.
                                            It will be sorted by value";
        private IComparer<TValue> _comparer;

        public SortByValueDictionary(IComparer<TValue> c, int size, int increment)
        {
            _comparer = c;
            if (size <= 0)
                size = 100;
            _size = size;
            _keys = new TKey[_size];
            _values = new TValue[_size];
        }

        public SortByValueDictionary(int size, int increment)
            : this(new NullsLastComparer<TValue>(), size, increment)
        {
        }

        public SortByValueDictionary()
            : this(100, 100)
        {
        }

        protected virtual bool ReSortRequired
        {
            get { return _reSortRequired; }
            set { _reSortRequired = value; }
        }

        public virtual TKey Previous( TKey current)
        {
            if (ReSortRequired)
                Sort();
            try
            {
                return _keys[(int)_map[current]-1];
            }
            catch
            {
            }
            return default(TKey);

        }

        public virtual TKey Next(TKey current)
        {
            if (ReSortRequired)
                Sort();
            try
            {
                return _keys[(int)_map[current] + 1];
            }
            catch
            {
            }
            return default(TKey);
        }



        private void IncreaseArraySize()
        {
            lock (this)
            {
                _size += _increment;
                Array.Resize<TKey>(ref _keys, _size);
                Array.Resize<TValue>(ref _values, _size);
            }
        }

        protected virtual void Sort()
        {
            lock (this)
            {
                Array.Sort(_values, _keys, _comparer);
                _map.Clear();
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (_keys[i] != null && _values[i] != null)
                        _map.Add(_keys[i], i);
                }
                ReSortRequired = false;
            }
        }

        #region IEnumerator


        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (ReSortRequired)
                Sort();
            Enumerator elist = new Enumerator(this);
            return elist;
            //if (ReSortRequired)
            //    Sort();
            //int i = 0;
            //while (i < _count)
            //{
            //    yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            //    i++;
            //}
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            if (ReSortRequired)
                Sort(); 
            Enumerator elist = new Enumerator(this);
            return elist;
            //if (ReSortRequired)
            //    Sort();
            //int i = 0;
            //while (i < _count)
            //{
            //    yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            //    i++;
            //}
        }
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            //private IComparable<TValue>[] _values;
            //private TKey[] _keys;
            //private KeyValuePair<TKey, TValue>[] _kvp;
            private int _index;
            private bool _maxreached;
            private SortByValueDictionary<TKey, TValue> _s;

            public Enumerator(SortByValueDictionary<TKey, TValue> s)
            {
                //s.Values.CopyTo(_values, 0);
                //s.Keys.CopyTo(_keys, 0);
                //_kvp = new KeyValuePair<TKey, TValue>[s.Count];
                //s.CopyTo(_kvp, 0);
                _s = s;
                _index = -1;
                _maxreached = false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_index > -1)
                        return new KeyValuePair<TKey,TValue>(_s._keys[_index], _s._values[_index]);
                    else
                        throw new IndexOutOfRangeException("MoveNext should be invoked before invoking Current");
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return null;// System.Collections.IEnumerator<KeyValuePair<TKey, IComparable<TValue>>>.Current;
                }
            }

            public bool MoveNext()
            {
                if (_maxreached)
                    return false;
                _index++;
                if (_index >= (_s.Count-1))
                    _maxreached = true;
                return (_s.Count == 0) ? false : true;
            }

            public void Reset()
            {
                _index = -1;
                _maxreached = false;
            }


            public void Dispose()
            {
                //for (int i = 0; i < _kvp.Length; i++)
                //{
                //    _kvp[i] = default(KeyValuePair<TKey, TValue>);
                //    //_values[i] = null;
                //}
                //_kvp = null;
                //_values = null;
                Reset();
            }
        }


        public virtual IEnumerable<KeyValuePair<TKey, TValue>> GetEnumerable()
        {
            // if _sortiedEntries == null then sort (and set _sortiedEntries)
            // take a local copy of _sortiedEntries
            // do a foreach on the local sortiedEntries (doingyield reutrn)
            if (ReSortRequired)
                Sort();
            int i = 0;
            while (i < Count)
            {
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                i++;
            }
        }

#endregion

       
#region IDictionary
        public virtual ICollection<TKey> Keys
        {
            get
            {
                return _map.Keys;
            }
        }
        public virtual ICollection<TKey> KeysSortedByValue
        {
            get
            {
                //lock (this)
                //{
                //    //LockList();
                //    //try
                //    //{
                //    List<TKey> kList = new List<TKey>(_map.Keys);

                //    kList.Sort(delegate(TKey firstkey, TKey secondkey)
                //    {
                //        IComparable<TValue> firstvalue = _map[firstkey];
                //        IComparable<TValue> secondvalue = _map[secondkey];
                //        return firstvalue.CompareTo((TValue)secondvalue);
                //    });
                //    return new ReadOnlyCollection<TKey>(kList);
                //    //}
                //    //finally
                //    //{
                //    //    UnlockList();
                //    //}
                //}
                throw new NotSupportedException(_notSupportedMsg);
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            { 
                //if (_sortedList == null || ReSortRequired)
                //{
                //    //LockList();
                //    lock (this)
                //    {
                //        if (_sortedList == null || ReSortRequired)
                //            //try
                //            //{
                //            _sortedList = new List<IComparable<TValue>>(_map.Values);
                //        _sortedList.Sort();
                //        // comparison is done in the TValue object since it is comparable.
                //        //delegate(IComparable<TValue> firstPair,
                //        //IComparable<TValue> nextPair)
                //        //{
                //        //    return firstPair.Value.CompareTo(nextPair.Value);
                //        //});
                //        ReSortRequired = false;
                //    }
                //    //finally
                //    //{
                //    //    UnlockList();
                //    //}
                //}
                //return _sortedList;
                throw new NotSupportedException(_notSupportedMsg);
            }
        }


        public virtual void Add(TKey key, TValue value)
        {
            if (value == null || key == null)
            {
                throw new ArgumentException("SortByValueDictionary - Add: Cannot null key or value");
            }
            lock (this)
            {
                int index;
                if (_map.ContainsKey(key))
                {
                    index = (int)_map[key];
                    _map.Remove(key);
                }
                else
                {
                    if (Count >= _size)
                    {
                        IncreaseArraySize();
                    }
                    index = Count;
                }
                _keys[index] = key;
                _values[index] = value;
                _map.Add(key, index);
                ReSortRequired = true;
            }

        }

        public virtual bool Remove(TKey key)
        {
            if (_map.ContainsKey(key))
            {
                lock (this)
                {
                    if (_map.ContainsKey(key))
                    {
                        int index = (int)_map[key];
                        _keys[index] = default(TKey);
                        _values[index] = default(TValue);
                        _map.Remove(key);
                        return true;
                    }
                }
                ReSortRequired = true;
                return true;
            }
            return false;
        }

        public virtual bool ContainsKey(TKey k)
        {
            return _map.ContainsKey(k);
        }

        public virtual void Clear()
        {
            lock (this)
            {
                Array.Clear(_keys, 0, Count);
                Array.Clear(_values, 0, Count);
                _map.Clear();
            }
        }


        public virtual bool TryGetValue(TKey k, out TValue v)
        {
            int i;
            bool rc = _map.TryGetValue(k, out i);
            v = default(TValue);
            if (rc)
            {
                v = _values[i];
            }
            return rc;

        }

#endregion


#region ICollection
        public virtual int Count
        {
            get { return _map.Count; }
        }

        public virtual bool IsSynchronized
        {
            get { return true; }
        }

        public virtual object SyncRoot
        {
            get { return this; }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> kv)
        {
            Add(kv.Key, kv.Value);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> kv)
        {
            return Remove(kv.Key);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> kv)
        {
            return _map.ContainsKey((TKey)kv.Key);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] arr, int startindex)
        {
            for (int i = startindex; i < Count; i++)
            {
                arr[i] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            }

        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }


        public virtual TValue this[TKey key]
        {
            get
            {
                return _values[(int)_map[key]];
            }
            set
            {
                if (value is TValue)
                {
                    lock (this)
                    {
                        Remove(key);
                        Add(key, value);
                    }
                }
                else
                {
                    throw new ArgumentException(String.Format("Value cannot be of type {0}", value.GetType()));
                }
            }
        }
#endregion
    }
    /// <summary>
    /// A comparer that put null to the very end
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class NullsLastComparer<T> : Comparer<T>
    {
        public override int Compare(T x, T y)
        {
            if (x == null)
                return (y == null) ? 0 : 1;

            if (y == null)
                return -1;

            return Comparer<T>.Default.Compare(x, y);
        }
    }


}