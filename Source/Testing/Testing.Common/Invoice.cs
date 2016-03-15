using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
  public class Invoice : AfxObject
  {
    #region string DocumentNumber

    public const string DocumentNumberProperty = "DocumentNumber";
    string mDocumentNumber;
    [Mandatory("Document Number is mandatory.")]
    public string DocumentNumber
    {
      get { return mDocumentNumber; }
      set { SetProperty<string>(ref mDocumentNumber, value); }
    }

    #endregion

    ObjectCollection<InvoiceItem> mItems;
    [Mandatory("At least one item is mandatory.")]
    [CollectionValidation("An item has an error.")]
    public ObjectCollection<InvoiceItem> Items
    {
      get { return GetObjectCollection<InvoiceItem>(ref mItems); }
    }
  }
}
