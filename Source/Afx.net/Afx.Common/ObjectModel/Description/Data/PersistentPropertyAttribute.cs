using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Data
{
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class PersistentPropertyAttribute : Attribute
  {
    #region string Name

    string mName = null;
    public string Name
    {
      get { return mName; }
      set { mName = value; }
    }

    #endregion

    #region int Size

    int mSize;
    public int Size
    {
      get { return mSize; }
      set { mSize = value; }
    }

    #endregion

    #region int Precision

    int mPrecision;
    public int Precision
    {
      get { return mPrecision; }
      set { mPrecision = value; }
    }

    #endregion

    #region bool AllowNull

    bool mAllowNull;
    public bool AllowNull
    {
      get { return mAllowNull; }
      set { mAllowNull = value; }
    }

    #endregion
  }
}
