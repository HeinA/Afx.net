using Afx.vsix.Commands;
using Afx.vsix.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Afx.vsix.AfxWizard
{
  public class AfxWizardViewModel : ViewModel<AfxWizardContext>
  {
    public AfxWizardViewModel()
    {
      this.Model = new AfxWizardContext();
    }

    bool? mDialogResult;
    public bool? DialogResult
    {
      get { return mDialogResult; }
      set { SetProperty<bool?>(ref mDialogResult, value, "DialogResult"); }
    }

    #region AfxWizardCommand SelectedCommand

    AfxWizardCommand mSelectedCommand;
    public AfxWizardCommand SelectedCommand
    {
      get { return mSelectedCommand ?? (mSelectedCommand = Model.Commands.FirstOrDefault()); }
      set { SetProperty<AfxWizardCommand>(ref mSelectedCommand, value); }
    }

    #endregion

    ICommand mOkCommand;
    public ICommand OkCommand
    {
      get { return mOkCommand ?? (mOkCommand = new DelegateCommand(ExecuteOk)); }
    }

    private void ExecuteOk()
    {
      if (SelectedCommand.Validate())
      {
        SelectedCommand.Apply();
        DialogResult = true;
      }
    }

    ICommand mCancelCommand;
    public ICommand CancelCommand
    {
      get { return mCancelCommand ?? (mCancelCommand = new DelegateCommand(ExecuteCancel)); }
    }

    private void ExecuteCancel()
    {
      DialogResult = false;
    }
  }
}
