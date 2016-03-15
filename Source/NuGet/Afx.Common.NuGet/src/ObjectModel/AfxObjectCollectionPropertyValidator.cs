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
  class AfxObjectCollectionPropertyValidator : AfxObjectPropertyValidator
  {
    public AfxObjectCollectionPropertyValidator(AfxObjectValidator owner, ValidationMetadata metadata) : base(owner, metadata)
    {
    }

    protected override void HookupValidation()
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
      INotifyCollectionChanged cc = ValidationMetadata.PropertyInfo.GetValue(Owner.Target) as INotifyCollectionChanged;
      WeakEventManager<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>.AddHandler(cc, "CollectionChanged", Target_CollectionChanged);
    }

    private void Target_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      ValidateAsync();
    }

    private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ValidationMetadata.PropertyInfo.Name)
      {
        HookCollection();
        ValidateAsync();
      }
    }
  }
}
