using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Data
{
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
  public sealed class SchemaAttribute : Attribute
  {
    #region Constructors

    public SchemaAttribute(string name)
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
