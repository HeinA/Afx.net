using Afx.vsix.ViewModels;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.AfxWizard
{
  public abstract class AfxWizardCommand : ViewModel
  {
    protected AfxWizardCommand(string commandName, AfxWizardContext context)
    {
      Context = context;
      CommandName = commandName;
    }

    public AfxWizardContext Context
    {
      get;
      private set;
    }

    public string CommandName
    {
      get;
      private set;
    }

    #region string ErrorMessage

    string mErrorMessage;
    public string ErrorMessage
    {
      get { return mErrorMessage; }
      set { SetProperty<string>(ref mErrorMessage, value); }
    }

    #endregion

    protected bool AppendErrorMessage(string message)
    {
      ErrorMessage = string.Format("{1}{0}", ErrorMessage == null ? message : string.Format("\n{0}", message), ErrorMessage);
      return false;
    }

    public virtual bool Validate()
    {
      ErrorMessage = null;
      return true;
    }

    public abstract void Apply();

    protected void Reformat(CodeElement ce)
    {
      var objMovePt = ce.EndPoint.CreateEditPoint();
      var objEditPt = ce.StartPoint.CreateEditPoint();
      objEditPt.StartOfDocument();
      objMovePt.EndOfDocument();
      objMovePt.SmartFormat(objEditPt);
    }
  }
}
