using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Validation
{
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class MandatoryAttribute : InstanceValidationAttribute
  {
    public MandatoryAttribute(string message)
      : base(message)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public override bool Validate(object target, PropertyInfo pi, CancellationToken cancellationToken)
    {
      if (target == null) throw new ArgumentNullException("target");
      if (pi == null) throw new ArgumentNullException("pi");

      object value = pi.GetValue(target);

      if (value == null) return false;
      if (value is string && string.IsNullOrWhiteSpace((string)value)) return false;
      if (value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType()))) return false;
      if (value is AfxObject && ((AfxObject)value).Id == Guid.Empty) return false;
      ICollection col = value as ICollection;
      if (col != null && col.Count == 0) return false;
      return true;
    }
  }
}
