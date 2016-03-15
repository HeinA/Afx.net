using Afx.ObjectModel.Description.Metadata;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Afx.ObjectModel
{
  class AfxObjectInstancePropertyValidator : AfxObjectPropertyValidator
  {
    public AfxObjectInstancePropertyValidator(AfxObjectValidator owner, ValidationMetadata metadata) : base(owner, metadata)
    {
    }

  }
}
