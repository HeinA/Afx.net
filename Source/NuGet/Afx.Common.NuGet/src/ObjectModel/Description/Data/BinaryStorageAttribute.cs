using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Data
{
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class BinaryStorageAttribute : Attribute
  {
    #region Constructors

    public BinaryStorageAttribute(string name)
    {
      Name = name;
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
  }
}
