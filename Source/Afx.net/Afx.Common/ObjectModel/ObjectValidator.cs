using Afx.ObjectModel.Description.Metadata;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  class ObjectValidator
  {
    #region Constructors

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Afx.ObjectModel.ObjectPropertyValidator")]
    public ObjectValidator(AfxObject target)
    {
      Target = target;
      Target.Validator = this;

      foreach (var vm in Target.GetType().GetMetadata().ValidationMetadata)
      {
        new ObjectPropertyValidator(this, vm);
      }
    }

    #endregion

    #region Collection<AfxObjectPropertyValidator> PropertyValidators

    Collection<ObjectPropertyValidator> mPropertyValidators = new Collection<ObjectPropertyValidator>();
    internal Collection<ObjectPropertyValidator> PropertyValidators
    {
      get { return mPropertyValidators; }
    }

    #endregion

    #region Collection<CancellationTokenSource> CancellationTokens

    Collection<CancellationTokenSource> mCancellationTokens = new Collection<CancellationTokenSource>();
    Collection<CancellationTokenSource> CancellationTokens
    {
      get { return mCancellationTokens; }
    }

    object mLock = new object();

    public void AddCancellationTokenSource(CancellationTokenSource cts)
    {
      lock(mLock)
      {
        if (!CancellationTokens.Contains(cts))
        {
          CancellationTokens.Add(cts);
        }
      }
    }

    public void RemoveCancellationTokenSource(CancellationTokenSource cts)
    {
      lock (mLock)
      {
        if (cts != null && CancellationTokens.Contains(cts))
        {
          CancellationTokens.Remove(cts);
        }
      }

      RaiseEvents();
    }

    #endregion

    #region void RaiseEvents()

    bool mHasErrorsInternal = false;
    string mErrorMessageInternal = string.Empty;

    public void RaiseEvents()
    {
      if (CancellationTokens.Count == 0)
      {
        bool tempHasErrors = HasErrors;
        if (mHasErrorsInternal != tempHasErrors)
        {
          mHasErrorsInternal = tempHasErrors;
          Target.OnPropertyChanged(AfxObject.HasErrorsProperty);
        }

        string tempErrorMessage = ErrorMessage;
        if (mErrorMessageInternal != tempErrorMessage)
        {
          mErrorMessageInternal = tempErrorMessage;
          Target.OnPropertyChanged(AfxObject.ErrorMessageProperty);
        }
      }
    }

    #endregion

    #region bool IsValidating

    public bool IsValidating
    {
      get
      {
        lock (mLock)
        {
          return CancellationTokens.Count > 0;
        }
      }
    }

    #endregion

    #region AfxObject Target

    AfxObject mTarget;
    public AfxObject Target
    {
      get { return mTarget; }
      private set { mTarget = value; }
    }

    #endregion

    #region IEnumerable<string> GetErrors(...)

    public IEnumerable<string> GetErrors(string propertyName)
    {
      WaitForAsyncValidations();
      IEnumerable<ObjectPropertyValidator> col = null;
      if (propertyName == null)
      {
        col = PropertyValidators.Where(pv1 => !pv1.IsValid);
      }
      else
      {
        col = PropertyValidators.Where(pv1 => !pv1.IsValid && pv1.ValidationMetadata.PropertyInfo.Name == propertyName);
      }

      foreach (var pv in col)
      {
        yield return pv.ValidationMetadata.ValidationAttribute.Message;
      }
    }

    #endregion

    #region bool HasErrors

    public bool HasErrors
    {
      get
      {
        WaitForAsyncValidations();
        return PropertyValidators.Where(pv => !pv.IsValid).Count() > 0;
      }
    }

    #endregion

    #region string ErrorMessage

    public string ErrorMessage
    {
      get { return string.Join("\n", GetErrors(null).Distinct()); }
    }

    #endregion

    #region void WaitForAsyncValidations()

    public void WaitForAsyncValidations()
    {
      SpinWait spin = new SpinWait();
      while (IsValidating) spin.SpinOnce();
    }

    #endregion

    #region void IsValid()

    public bool IsValid()
    {
      foreach (var pv in PropertyValidators)
      {
        if (!pv.ValidateInner()) return false;
      }
      return true;
    }

    #endregion
  }
}
