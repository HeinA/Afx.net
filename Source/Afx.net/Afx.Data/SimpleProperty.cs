using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public class SimpleProperty : ObjectProperty
  {
    internal SimpleProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo)
      : base(objectRepository, propertyInfo)
    {
    }

    internal SimpleProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo, bool isId)
      : base(objectRepository, propertyInfo)
    {
      IsId = isId;
    }

    #region bool IsId

    bool mIsId;
    public bool IsId
    {
      get { return mIsId; }
      private set { mIsId = value; }
    }

    #endregion
  }
}
