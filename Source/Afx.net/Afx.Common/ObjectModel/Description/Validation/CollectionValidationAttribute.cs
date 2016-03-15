using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Validation
{
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class CollectionValidationAttribute : ValidationAttribute
  {
    public CollectionValidationAttribute(string message)
      : base(message, true)
    {
    }

    public override bool Validate(object target, PropertyInfo pi, CancellationToken cancellationToken)
    {
      if (target == null) throw new ArgumentNullException("target");
      if (pi == null) throw new ArgumentNullException("pi");

      //NOTE: Returning gives better performance than throwing an error.
      //cancellationToken.ThrowIfCancellationRequested();
      if (cancellationToken.IsCancellationRequested)
      {
        return false;
      }

      IList col = pi.GetValue(target) as IList;
      if (col == null)
      {
        return false;
        //TODO: Throw exception
      }

      //NOTE: Returning gives better performance than throwing an error.
      //cancellationToken.ThrowIfCancellationRequested();
      if (cancellationToken.IsCancellationRequested)
      {
        return false;
      }

      for (int i = 0; i < col.Count; i++)
      {
        //NOTE: Returning gives better performance than throwing an error.
        //cancellationToken.ThrowIfCancellationRequested();
        if (cancellationToken.IsCancellationRequested)
        {
          return false;
        }

        AfxObject obj1 = col[i] as AfxObject;
        if (obj1 != null)
        {
          if (!obj1.Validator.IsValid())
          {
            return false;
          }
        }
      }
      return true;
    }
  }
}
