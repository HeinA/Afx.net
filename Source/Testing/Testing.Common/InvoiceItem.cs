using Afx.ObjectModel;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
  public class InvoiceItem : AfxObject<Invoice>
  {
    #region string ItemNo

    public const string ItemNoProperty = "ItemNo";
    string mItemNo;
    [Mandatory("Item No is mandatory.")]
    public string ItemNo
    {
      get { return mItemNo; }
      set { SetProperty<string>(ref mItemNo, value); }
    }

    #endregion
  }
}
