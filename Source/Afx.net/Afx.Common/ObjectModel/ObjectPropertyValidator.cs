using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Afx.ObjectModel
{
  class ObjectPropertyValidator
  {
    #region Constructors

    public ObjectPropertyValidator(ObjectValidator owner, ValidationMetadata metadata)
    {
      Owner = owner;
      Owner.PropertyValidators.Add(this);
      ValidationMetadata = metadata;
      IsValid = ValidationMetadata.ValidationAttribute.Validate(Owner.Target, ValidationMetadata.PropertyInfo, CancellationToken.None);
      HookupValidation();
    }

    #endregion

    #region bool IsValid

    bool mIsValid = true;
    public bool IsValid
    {
      get { return mIsValid; }
      private set { mIsValid = value; }
    }

    #endregion

    #region AfxObjectValidator Owner

    ObjectValidator mOwner;
    public ObjectValidator Owner
    {
      get { return mOwner; }
      private set { mOwner = value; }
    }

    #endregion

    #region ValidationMetadata ValidationMetadata

    ValidationMetadata mValidationMetadata;
    public ValidationMetadata ValidationMetadata
    {
      get { return mValidationMetadata; }
      set { mValidationMetadata = value; }
    }

    #endregion

    #region Validation Hooks

    protected void HookupValidation()
    {
      INotifyPropertyChanged pc = Owner.Target as INotifyPropertyChanged;
      if (pc != null)
      {
        pc.PropertyChanged += Target_PropertyChanged;
      }

      HookCollection();
    }

    void HookCollection()
    {
      IEditNotifier en = ValidationMetadata.PropertyInfo.GetValue(Owner.Target) as IEditNotifier;
      if (en != null)
      {
        WeakEventManager<IEditNotifier, EventArgs>.AddHandler(en, "ObjectEdited", Target_CollectionEdited);
      }
    }

    private void Target_CollectionEdited(object sender, EventArgs e)
    {
      Validate();
    }

    private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ValidationMetadata.PropertyInfo.Name)
      {
        Validate();
      }
    }

    #endregion

    #region CancellationTokenSource CancellationTokenSource

    volatile CancellationTokenSource mCancellationTokenSource;
    CancellationTokenSource CancellationTokenSource
    {
      get { return mCancellationTokenSource; }
      set
      {
        var temp = mCancellationTokenSource;
        if (value != null)
        {
          Owner.AddCancellationTokenSource(value);
        }
        if (mCancellationTokenSource != null)
        {
          Owner.RemoveCancellationTokenSource(temp);
        }
        mCancellationTokenSource = value;
      }
    }

    #endregion

    #region void Validate()

    public bool ValidateInner()
    {
      return ValidationMetadata.ValidationAttribute.Validate(Owner.Target, ValidationMetadata.PropertyInfo, CancellationToken.None);
    }

    public bool ValidateInner(CancellationToken token)
    {
      return ValidationMetadata.ValidationAttribute.Validate(Owner.Target, ValidationMetadata.PropertyInfo, token);
    }

    object mLock = new object();

    public async void Validate()
    {
      if (!ValidationMetadata.ValidationAttribute.Concurrent)
      {
        bool isValid = true;
        bool bErrorChanged = false;

        isValid = ValidateInner();
        bErrorChanged = IsValid != isValid;
        IsValid = isValid;

        if (bErrorChanged)
        {
          Owner.Target.OnErrorsChanged(ValidationMetadata.PropertyInfo.Name);
        }

        Owner.RaiseEvents();
      }
      else
      {
        lock (mLock)
        {
          if (CancellationTokenSource != null)
          {
            CancellationTokenSource.Cancel();
            return;
          }
        }

        bool bExit = false;
        while (!bExit)
        {
          try
          {
            lock (mLock)
            {
              CancellationTokenSource = new CancellationTokenSource();
            }

            bool isValid = true;
            bool bErrorChanged = false;

            await Task.Run(() =>
            {
              isValid = ValidateInner(CancellationTokenSource.Token);
              bErrorChanged = IsValid != isValid;
              IsValid = isValid;
            });

            if (CancellationTokenSource.IsCancellationRequested)
            {
              continue;
            }

            lock (mLock)
            {
              CancellationTokenSource = null;
            }

            if (bErrorChanged)
            {
              Owner.Target.OnErrorsChanged(ValidationMetadata.PropertyInfo.Name);
            }

            bExit = true;
          }
          catch (OperationCanceledException)
          {
          }
        }
      }
    }

    #endregion
  }
}
