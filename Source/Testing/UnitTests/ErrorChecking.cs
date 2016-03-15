using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Afx.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Testing;

namespace UnitTests
{
  [TestClass]
  public class ErrorChecking
  {
    [TestMethod]
    public void TestInvoiceErrorMessages()
    {
      Invoice i = new Invoice();
      i.PropertyChanged += Invoice_PropertyChanged;
      Assert.IsTrue(i.ErrorMessage == "Document Number is mandatory.\nAt least one item is mandatory.", "Invoice failed to display error message.");
      i.DocumentNumber = "INV001";
      var ii = new InvoiceItem();
      i.Items.Add(ii);
      Assert.IsTrue(i.ErrorMessage == "An item has an error.", "Invoice failed to display collection error message.");
      ii.ItemNo = "11011";
      
      Assert.IsTrue(i.ErrorMessage == string.Empty, "Invoice error message failed to clear previous messages.");

      Thread.Sleep(50);
      Assert.IsTrue(mErrorMessageCount == 4, "Did not receive 4 ErrorMessage changed events");
      Assert.IsTrue(mHasErrorsCount == 4, "Did not receive 4 HasError changed events");
    }

    volatile int mErrorMessageCount = 0;
    volatile int mHasErrorsCount = 0;
    private void Invoice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      Invoice i = sender as Invoice;
      INotifyDataErrorInfo ne = sender as INotifyDataErrorInfo;

      bool hasError = ne.HasErrors;
      string message = i.ErrorMessage;

      switch (e.PropertyName)
      {
        case AfxObject.ErrorMessageProperty:
          mErrorMessageCount++;
          switch (mErrorMessageCount)
          {
            case 1:
              Assert.IsTrue(message == "At least one item is mandatory.", "Property Changed Event failure (ErrorMessage)");
              break;

            case 2:
              Assert.IsTrue(message == string.Empty, "Property Changed Event failure (ErrorMessage)");
              break;

            case 3:
              Assert.IsTrue(message == "An item has an error.", "Property Changed Event failure (ErrorMessage)");
              break;

            case 4:
              Assert.IsTrue(message == string.Empty, "Property Changed Event failure (ErrorMessage)");
              break;
          }
          break;

        case AfxObject.HasErrorsProperty:
          mHasErrorsCount++;
          switch (mHasErrorsCount)
          {
            case 1:
              Assert.IsTrue(hasError == true, "Property Changed Event failure (HasError)");
              break;

            case 2:
              Assert.IsTrue(hasError == false, "Property Changed Event failure (HasError)");
              break;

            case 3:
              Assert.IsTrue(hasError == true, "Property Changed Event failure (HasError)");
              break;

            case 4:
              Assert.IsTrue(hasError == false, "Property Changed Event failure (HasError)");
              break;
          }
          break;
      }
    }
  }
}
