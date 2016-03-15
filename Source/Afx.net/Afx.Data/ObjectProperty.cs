using Afx.ObjectModel.Description.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public abstract class ObjectProperty
  {
    internal ObjectProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo)
    {
      ObjectRepository = objectRepository;
      PropertyInfo = propertyInfo;

      PersistentPropertyAttribute ppa = PropertyInfo.GetCustomAttribute<PersistentPropertyAttribute>();
      if (ppa != null)
      {
        Name = ppa.Name ?? PropertyInfo.Name;
        Size = ppa.Size;
        Precision = ppa.Precision;
        AllowNull = ppa.AllowNull;
      }
      else
      {
        Name = PropertyInfo.Name;
      }

      Type nt = AssemblyHelper.GetGenericSubClass(typeof(Nullable<>), PropertyType);
      if (nt != null)
      {
        AllowNull = true;
      }

      if (PropertyInfo.CanWrite) AllowRead = true;
    }

    internal ObjectProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo, string name)
    {
      ObjectRepository = objectRepository;
      PropertyInfo = propertyInfo;

      Name = name ?? PropertyInfo.Name;

      if (PropertyInfo.CanWrite) AllowRead = true;
    }

    #region ObjectRepository ObjectRepository

    ObjectRepository mObjectRepository;
    public ObjectRepository ObjectRepository
    {
      get { return mObjectRepository; }
      private set { mObjectRepository = value; }
    }

    #endregion

    #region PropertyInfo PropertyInfo

    PropertyInfo mPropertyInfo;
    public PropertyInfo PropertyInfo
    {
      get { return mPropertyInfo; }
      private set { mPropertyInfo = value; }
    }

    #endregion

    #region Type PropertyType

    public Type PropertyType
    {
      get { return PropertyInfo.PropertyType; }
    }

    #endregion

    #region string Name

    string mName;
    public string Name
    {
      get { return mName; }
      private set { mName = value; }
    }

    #endregion

    #region bool AllowRead
    
    bool mAllowRead;
    public bool AllowRead
    {
      get { return mAllowRead; }
      private set { mAllowRead = value; }
    }
    
    #endregion

    #region int Size

    int mSize;
    public int Size
    {
      get { return mSize; }
      private set { mSize = value; }
    }

    #endregion

    #region int Precision

    int mPrecision;
    public int Precision
    {
      get { return mPrecision; }
      private set { mPrecision = value; }
    }

    #endregion

    #region bool AllowNull

    bool mAllowNull;
    public bool AllowNull
    {
      get { return mAllowNull; }
      private set { mAllowNull = value; }
    }

    #endregion

    #region string ToString(...)

    public override string ToString()
    {
      return string.Format("{0}.[{1}]", ObjectRepository, Name);
    }

    #endregion
  }
}
