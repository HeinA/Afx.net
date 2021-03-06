﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Collections
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")
  , CollectionDataContract(Namespace = Constants.WcfNamespace
    , Name = Constants.WcfAssociativeCollectionName
    , ItemName = Constants.WcfAssociativeCollectionItemName
    , KeyName = Constants.WcfAssociativeCollectionReferenceName
    , ValueName = Constants.WcfAssociativeCollectionAssociativeName
    , IsReference = true)]
  public sealed class AssociativeCollection<TItem, TAssociative> : ObjectCollection<TItem>, IAssociativeCollection
    where TItem : class, IAfxObject
    where TAssociative : class, IAssociativeObject, new()
  {
    #region Constructors

    internal AssociativeCollection(AfxObject owner, string propertyName)
      : base(owner, propertyName)
    {
      IsCompositeReference = ((Description.Metadata.AssociativeMetadata)Description.Metadata.Metadata.GetMetadata(typeof(TAssociative))).IsCompositeReference;
    }

    #endregion

    #region bool IsCompositeReference

    bool mIsCompositeReference;
    bool IsCompositeReference
    {
      get { return mIsCompositeReference; }
      set { mIsCompositeReference = value; }
    }

    #endregion

    #region Items

    #region InsertItemCore(...)

    protected override void InsertItemCore(int index, TItem item)
    {
      TAssociative ass = null;
      if (!mDictionary.Contains(item))
      {
        ass = new TAssociative();
        mDictionary.Insert(index, item, ass);
      }
      else
      {
        ass = (TAssociative)mDictionary[item];
      }

      ass.Reference = item;
      ass.Owner = Owner;
      ass.PropertyChanged += ItemPropertyChanged;
      if (IsCompositeReference) item.PropertyChanged += ItemPropertyChanged;
    }

    #endregion

    #region RemoveItemCore(...)

    protected override void RemoveItemCore(TItem item)
    {
      if (mDictionary.Contains(item))
      {
        TAssociative ass = (TAssociative)mDictionary[item];
        ass.PropertyChanged -= ItemPropertyChanged;
        mDictionary.Remove(item);
      }
      if (IsCompositeReference) item.PropertyChanged -= ItemPropertyChanged;
    }

    #endregion

    #endregion

    #region TAssociative this[TItem item]

    public TAssociative this[TItem item]
    {
      get { return (TAssociative)mDictionary[item]; }
    }

    #endregion

    #region IAssociativeCollection

    Type IAssociativeCollection.AssociativeType
    {
      get { return typeof(TAssociative); }
    }

    OrderedDictionary mDictionary = new OrderedDictionary();

    void IDictionary.Add(object key, object value)
    {
      mDictionary.Add(key, value);
      this.Add((TItem)key);
    }

    void IDictionary.Clear()
    {
      this.Clear();
    }

    bool IDictionary.Contains(object key)
    {
      return mDictionary.Contains(key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return mDictionary.GetEnumerator();
    }

    bool IDictionary.IsFixedSize
    {
      get { return false; }
    }

    bool IDictionary.IsReadOnly
    {
      get { return mDictionary.IsReadOnly; }
    }

    ICollection IDictionary.Keys
    {
      get { return mDictionary.Keys; }
    }

    void IDictionary.Remove(object key)
    {
      Remove((TItem)key);
    }

    ICollection IDictionary.Values
    {
      get { return mDictionary.Values; }
    }

    object IDictionary.this[object key]
    {
      get
      {
        return mDictionary[key];
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      throw new NotImplementedException();
    }

    int ICollection.Count
    {
      get { return this.Count; }
    }

    bool ICollection.IsSynchronized
    {
      get { return ((ICollection)mDictionary).IsSynchronized; }
    }

    object ICollection.SyncRoot
    {
      get { return ((ICollection)mDictionary).SyncRoot; }
    }

    #endregion
  }
}
