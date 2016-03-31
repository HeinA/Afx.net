using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Metadata
{
  public class AssociativeMetadata : Metadata
  {
    internal AssociativeMetadata(Type type)
      : base(type)
    {
      IsCompositeReference = type.GetCustomAttribute<CompositeReferenceAttribute>() != null;
    }

    #region bool IsCompositeReference

    bool mIsCompositeReference;
    public bool IsCompositeReference
    {
      get { return mIsCompositeReference; }
      private set { mIsCompositeReference = value; }
    }

    #endregion
  }
}
