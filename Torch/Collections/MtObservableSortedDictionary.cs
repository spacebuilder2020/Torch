using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Torch.Utils;

namespace Torch.Collections
{
    /// <summary>
    ///     Multithread safe observable dictionary
    /// </summary>
    /// <typeparam name="TK">Key type</typeparam>
    /// <typeparam name="TV">Value type</typeparam>
    public class MtObservableSortedDictionary<TK, TV> :
        MtObservableCollectionBase<KeyValuePair<TK, TV>>, IDictionary<TK, TV>
    {
        private readonly List<KeyValuePair<TK, TV>> _backing;
        private readonly KeysCollection _keyCollection;
        private readonly IComparer<TK> _keyComparer;
        private readonly ValueCollection _valueCollection;

        /// <summary>
        ///     Creates an empty observable dictionary
        /// </summary>
        public MtObservableSortedDictionary(IComparer<TK> keyCompare = null)
        {
            _keyComparer = keyCompare ?? Comparer<TK>.Default;
            _backing = new List<KeyValuePair<TK, TV>>();
            _keyCollection = new KeysCollection(this);
            _valueCollection = new ValueCollection(this);
            Lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        protected override ReaderWriterLockSlim Lock { get; }

        /// <inheritdoc cref="IDictionary{TK, TV}.Keys" />
        public MtObservableCollectionBase<TK> Keys => _keyCollection;

        /// <inheritdoc cref="IDictionary{TK, TV}.Values" />
        public MtObservableCollectionBase<TV> Values => _valueCollection;

        /// <inheritdoc />
        public override int Count
        {
            get
            {
                using (Lock.ReadUsing())
                    return _backing.Count;
            }
        }

        /// <inheritdoc />
        public override bool IsReadOnly => false;

        /// <inheritdoc />
        public override void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public override void Clear()
        {
            using (Lock.WriteUsing())
            {
                _backing.Clear();
                RaiseAllChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <inheritdoc />
        public override bool Contains(KeyValuePair<TK, TV> item)
        {
            return TryGetValue(item.Key, out var val) && EqualityComparer<TV>.Default.Equals(val, item.Value);
        }

        /// <inheritdoc />
        public override void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            using (Lock.ReadUsing())
            {
                _backing.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public override bool Remove(KeyValuePair<TK, TV> item)
        {
            return Remove(item.Key);
        }

        /// <inheritdoc />
        public bool ContainsKey(TK key)
        {
            using (Lock.ReadUsing())
                return TryGetValue(key, out _);
        }

        /// <inheritdoc />
        public void Add(TK key, TV value)
        {
            using (Lock.WriteUsing())
            {
                if (TryGetBucket(key, out var firstGtBucket))
                    throw new ArgumentException($"Key {key} already exists", nameof(key));

                var item = new KeyValuePair<TK, TV>(key, value);
                _backing.Insert(firstGtBucket, item);
                RaiseAllChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, firstGtBucket));
            }
        }

        /// <inheritdoc />
        public bool Remove(TK key)
        {
            using (Lock.UpgradableReadUsing())
            {
                if (!TryGetBucket(key, out var bucket))
                    return false;

                using (Lock.WriteUsing())
                {
                    var old = _backing[bucket];
                    _backing.RemoveAt(bucket);
                    RaiseAllChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, bucket));
                    return true;
                }
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(TK key, out TV value)
        {
            using (Lock.ReadUsing())
            {
                if (!TryGetBucket(key, out var bucket))
                {
                    value = default(TV);
                    return false;
                }

                value = _backing[bucket].Value;
                return true;
            }
        }

        /// <inheritdoc />
        public TV this[TK key]
        {
            get
            {
                if (TryGetValue(key, out var result))
                    return result;

                throw new KeyNotFoundException($"Key {key} not found");
            }
            set
            {
                using (Lock.WriteUsing())
                {
                    var item = new KeyValuePair<TK, TV>(key, value);
                    if (TryGetBucket(key, out var firstGtBucket))
                    {
                        var old = _backing[firstGtBucket].Value;
                        _backing[firstGtBucket] = item;
                        _valueCollection.ParentChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, old,
                                value, firstGtBucket));
                    }
                    else
                    {
                        _backing.Insert(firstGtBucket, item);
                        RaiseAllChanged(
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item,
                                firstGtBucket));
                    }
                }
            }
        }

        ICollection<TK> IDictionary<TK, TV>.Keys => _keyCollection;
        ICollection<TV> IDictionary<TK, TV>.Values => _valueCollection;

        /// <inheritdoc />
        protected override List<KeyValuePair<TK, TV>> Snapshot(List<KeyValuePair<TK, TV>> old)
        {
            if (old == null)
                return new List<KeyValuePair<TK, TV>>(_backing);

            old.Clear();
            old.AddRange(_backing);
            return old;
        }

        /// <inheritdoc />
        public override void CopyTo(Array array, int index)
        {
            using (Lock.ReadUsing())
                foreach (var k in _backing)
                    array.SetValue(k, index++);
        }

        private void RaiseAllChanged(NotifyCollectionChangedEventArgs evt)
        {
            MarkSnapshotsDirty();
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(evt);
            _keyCollection.ParentChanged(evt);
            _valueCollection.ParentChanged(evt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetBucket(TK key, out int res)
        {
            int min = 0, max = _backing.Count;
            while (min != max)
            {
                var mid = (min + max) / 2;
                if (_keyComparer.Compare(_backing[mid].Key, key) < 0)
                    min = mid + 1;
                else
                    max = mid;
            }

            res = min;
            return res < _backing.Count && _keyComparer.Compare(_backing[res].Key, key) == 0;
        }

        private abstract class ProxyCollection<TT> : MtObservableCollectionBase<TT>
        {
            private readonly Func<KeyValuePair<TK, TV>, TT> _selector;
            protected readonly MtObservableSortedDictionary<TK, TV> Owner;

            protected ProxyCollection(MtObservableSortedDictionary<TK, TV> owner,
                                      Func<KeyValuePair<TK, TV>, TT> selector)
            {
                Owner = owner;
                _selector = selector;
            }

            protected override ReaderWriterLockSlim Lock => Owner.Lock;

            public override int Count => Owner.Count;
            public override bool IsReadOnly => Owner.IsReadOnly;

            protected override List<TT> Snapshot(List<TT> old)
            {
                if (old == null)
                    old = new List<TT>(Owner._backing.Count);
                else
                    old.Clear();
                foreach (var kv in Owner._backing)
                    old.Add(_selector(kv));
                return old;
            }

            public override void CopyTo(Array array, int index)
            {
                using (Lock.ReadUsing())
                {
                    foreach (var entry in Owner._backing)
                        array.SetValue(_selector(entry), index++);
                }
            }

            public override void Clear()
            {
                Owner.Clear();
            }

            public override void CopyTo(TT[] array, int arrayIndex)
            {
                using (Lock.ReadUsing())
                {
                    foreach (var entry in Owner._backing)
                        array[arrayIndex++] = _selector(entry);
                }
            }

            private IList TransformList(IEnumerable list)
            {
                if (list == null) return null;

                var res = new ArrayList();
                foreach (var k in list)
                    res.Add(_selector((KeyValuePair<TK, TV>)k));
                return res;
            }

            public void ParentChanged(NotifyCollectionChangedEventArgs args)
            {
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
                if (args.Action == NotifyCollectionChangedAction.Reset)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                else if (args.OldItems == null && args.OldStartingIndex == -1)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, TransformList(args.NewItems),
                        args.NewStartingIndex));
                else if (args.NewItems == null && args.NewStartingIndex == -1)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, TransformList(args.OldItems),
                        args.OldStartingIndex));
                else if (ReferenceEquals(args.NewItems, args.OldItems))
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, TransformList(args.NewItems),
                        args.NewStartingIndex, args.OldStartingIndex));
                else if (args.NewStartingIndex == args.OldStartingIndex && args.NewItems != null &&
                         args.OldItems != null)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(args.Action, TransformList(args.NewItems),
                        TransformList(args.OldItems),
                        args.NewStartingIndex));
                else
                    Debugger.Break();
            }
        }

        private class KeysCollection : ProxyCollection<TK>
        {
            public KeysCollection(MtObservableSortedDictionary<TK, TV> owner) : base(owner, x => x.Key) { }

            // ReSharper disable once AssignNullToNotNullAttribute
            public override void Add(TK item) => Owner.Add(item, default(TV));

            // ReSharper disable once AssignNullToNotNullAttribute
            public override bool Contains(TK item) => Owner.ContainsKey(item);

            // ReSharper disable once AssignNullToNotNullAttribute
            public override bool Remove(TK item) => Owner.Remove(item);
        }

        private class ValueCollection : ProxyCollection<TV>
        {
            public ValueCollection(MtObservableSortedDictionary<TK, TV> owner) : base(owner, x => x.Value) { }

            public override void Add(TV item)
            {
                throw new NotImplementedException();
            }

            public override bool Contains(TV item)
            {
                var cmp = EqualityComparer<TV>.Default;
                using (Lock.ReadUsing())
                    foreach (var kv in Owner._backing)
                        if (cmp.Equals(kv.Value, item))
                            return true;

                return false;
            }

            public override bool Remove(TV item)
            {
                var cmp = EqualityComparer<TV>.Default;
                var hasKey = false;
                var key = default(TK);
                using (Lock.ReadUsing())
                    foreach (var kv in Owner._backing)
                        if (cmp.Equals(kv.Value, item))
                        {
                            hasKey = true;
                            key = kv.Key;
                            break;
                        }

                return hasKey && Owner.Remove(key);
            }
        }
    }
}