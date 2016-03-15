using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Wpf
{
  public class ViewModel : INotifyPropertyChanged
  {
    public ViewModel()
    {
      Invoice = new Invoice();
      Invoice.DocumentNumber = "INV001";
    }

    #region Invoice Invoice

    Invoice mInvoice;

    public Invoice Invoice
    {
      get { return mInvoice; }
      set
      {
        if (mInvoice != value)
        {
          mInvoice = value;
          OnPropertyChanged("Invoice");
        }
      }
    }

    #endregion

    #region bool UpdateSource

    bool mUpdateSource;
    public bool UpdateSource
    {
      get { return mUpdateSource; }
      set
      {
        if (mUpdateSource != value)
        {
          mUpdateSource = value;
          OnPropertyChanged("UpdateSource");
        }
      }
    }

    public void EditEdit()
    {
      UpdateSource = true;
    }

  #endregion

  public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
