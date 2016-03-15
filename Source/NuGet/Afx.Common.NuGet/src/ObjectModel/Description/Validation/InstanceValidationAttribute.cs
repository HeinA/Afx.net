using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Validation
{
  public abstract class InstanceValidationAttribute : ValidationAttribute
  {
    protected InstanceValidationAttribute(string message)
      : base(message)
    {
    }
  }
}
