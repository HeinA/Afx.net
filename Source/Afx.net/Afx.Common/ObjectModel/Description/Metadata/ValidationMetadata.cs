using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Metadata
{
  public class ValidationMetadata
  {
    #region Constructors

    internal ValidationMetadata(PropertyInfo propertyInfo, ValidationAttribute validationAttribute)
    {
      PropertyInfo = propertyInfo;
      ValidationAttribute = validationAttribute;
    }

    #endregion

    #region Guid Id

    Guid mId = Guid.NewGuid();
    public Guid Id
    {
      get { return mId; }
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

    #region ValidationAttribute ValidationAttribute

    ValidationAttribute mValidationAttribute;
    public ValidationAttribute ValidationAttribute
    {
      get { return mValidationAttribute; }
      private set { mValidationAttribute = value; }
    }

    #endregion

    #region Equality

    public override bool Equals(object obj)
    {
      ValidationMetadata vm = obj as ValidationMetadata;
      if (vm == null) return false;
      return Id.Equals(vm.Id);
    }

    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }

    #endregion
  }
}
