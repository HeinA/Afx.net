using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public class CollectionProperty : ObjectProperty
  {
    internal CollectionProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo)
      : base(objectRepository, propertyInfo)
    {
    }

    //internal CollectionProperty(ObjectRepository objectRepository, PropertyInfo propertyInfo, string name)
    //  : base(objectRepository, propertyInfo, name)
    //{
    //}
  }
}
