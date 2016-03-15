using Afx.vsix.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.ViewModels
{
  public class CreateViewModelEventArgs<T> : EventArgs
    where T : ViewModel
  {
    public CreateViewModelEventArgs(object model)
    {
      Model = model;
    }

    object mModel;
    public object Model
    {
      get { return mModel; }
      private set { mModel = value; }
    }

    T mViewModel;
    public T ViewModel
    {
      get { return mViewModel; }
      set { mViewModel = value; }
    }
  }
}
