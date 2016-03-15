using Afx.vsix.Utilities;
using EnvDTE;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.AfxWizard.Items
{
  public class CreateServiceImplementation : AfxWizardCommand
  {
    const string BusinessService = "Afx.ServiceModel.BusinessService";
    const string BusinessServiceAttribute = "Afx.ServiceModel.Description.BusinessServiceBehaviorAttribute";

    #region Constructors

    public CreateServiceImplementation(AfxWizardContext context) 
      : base("Create Service Implementation", context)      
    {
    }

    #endregion

    #region string ClassName

    string mClassName;
    public string ClassName
    {
      get { return mClassName; }
      set { SetProperty<string>(ref mClassName, value); }
    }

    #endregion

    Collection<string> mImplementableInterfaces;
    public IEnumerable<string> ImplementableInterfaces
    {
      get
      {
        if (mImplementableInterfaces != null) return mImplementableInterfaces;
        mImplementableInterfaces = new Collection<string>();
        mImplementableInterfaces.Add(string.Empty);
        foreach (Project p in Context.AllProjects)
        {
          foreach (CodeInterface i in Utilities.TypeHelper.GetCodeServiceInterfaces(p, true))
          {
            if (Utilities.TypeHelper.GetServiceImplementation(i.FullName) == null) mImplementableInterfaces.Add(i.FullName);
          }
        }
        return mImplementableInterfaces;
      }
    }

    #region string SelectedInterface

    public const string SelectedInterfaceProperty = "SelectedInterface";
    string mSelectedInterface;
    public string SelectedInterface
    {
      get { return mSelectedInterface; }
      set
      {
        if (SetProperty<string>(ref mSelectedInterface, value))
        {
          if (!string.IsNullOrWhiteSpace(SelectedInterface))
          {
            string s = value.Substring(value.LastIndexOf('.') + 1);
            if (s.Substring(0, 1).Equals("I")) ClassName = s.Substring(1);
            else ClassName = s;
          }
          else
          {
            ClassName = string.Empty;
          }
        }
      }
    }

    #endregion

    public override bool Validate()
    {
      bool isValid = base.Validate();

      if (string.IsNullOrWhiteSpace(ClassName))
      {
        isValid = AppendErrorMessage("Service Class Name is mandatory.");
      }

      if (string.IsNullOrWhiteSpace(SelectedInterface))
      {
        isValid = AppendErrorMessage("Service Interface is mandatory.");
      }

      return isValid;
    }

    public override void Apply()
    {
      Dictionary<string, string> map = new Dictionary<string, string>();
      map.Add(BusinessService, BusinessServiceAttribute);

      string serviceType = null;
      CodeInterface ci = TypeHelper.GetCodeServiceInterfaces(Context.Project, false).FirstOrDefault(si1 => si1.FullName.Equals(SelectedInterface));
      if (ci != null)
      {
        if (ci.Attributes.Cast<CodeElement>().FirstOrDefault(ca => ca.FullName.Equals(BusinessServiceAttribute)) != null) serviceType = BusinessService;
      }

      Type t = TypeHelper.GetReferenceServiceInterfaces(Context.Project).FirstOrDefault(t1 => t1.FullName.Equals(SelectedInterface));
      if (t != null)
      {
        if (t.GetCustomAttributes().FirstOrDefault(ca => ca.GetType().FullName.Equals(BusinessServiceAttribute)) != null) serviceType = BusinessService;
      }

      CodeGeneration.AfxServiceImplementation si = new CodeGeneration.AfxServiceImplementation();
      si.Session = new Dictionary<string, object>();
      si.Session["ns"] = Context.Namespace;
      si.Session["name"] = ClassName;
      si.Session["serviceType"] = serviceType;
      si.Session["interfaceName"] = SelectedInterface;
      si.Initialize();
      string ss = si.TransformText();
      string fileName = string.Format("{0}{1}.cs", Context.Folder, ClassName);
      File.WriteAllText(fileName, ss);
      ProjectItem newItem = Context.NodeItems.AddFromFile(fileName);
      Window w = newItem.Open(EnvDTE.Constants.vsViewKindCode);
      w.Visible = true;
    }
  }
}
