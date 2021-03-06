﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Data
{
  [AttributeUsage(AttributeTargets.Class, Inherited = false)]
  public sealed class PersistentObjectAttribute : Attribute
  {
    #region string Catalog

    string mCatalog = null;
    public string Catalog
    {
      get { return mCatalog; }
      set { mCatalog = value; }
    }

    #endregion

    #region string Owner

    string mOwner = null;
    public string Owner
    {
      get { return mOwner; }
      set { mOwner = value; }
    }

    #endregion

    #region bool IsReadOnly

    bool mIsReadOnly = false;
    public bool IsReadOnly
    {
      get { return mIsReadOnly; }
      set { mIsReadOnly = value; }
    }

    #endregion

    #region string Reference

    string mReference = null;
    public string Reference
    {
      get { return mReference; }
      set { mReference = value; }
    }

    #endregion

    #region bool IsCached

    bool mIsCached;
    public bool IsCached
    {
      get { return mIsCached; }
      set { mIsCached = value; }
    }

    #endregion
  }
}
