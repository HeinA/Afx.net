using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.AfxWizard.Items
{
  public class CreateServiceInterface : AfxWizardCommand
  {
    const string BusinessService = "Business Service";
    const string BusinessServiceAttribute = "[BusinessServiceBehavior]";

    #region Constructors

    public CreateServiceInterface(AfxWizardContext context) 
      : base("Create Service Interface", context)      
    {
    }

    #endregion


    #region <string> ServiceTypes

    public IEnumerable<string> ServiceTypes
    {
      get
      {
        yield return BusinessService;
      }
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

    #region string SelectedBusinessService

    public const string SelectedServiceTypeProperty = "SelectedServiceType";
    string mSelectedServiceType = BusinessService;
    public string SelectedServiceType
    {
      get { return mSelectedServiceType; }
      set
      {
        if (SetProperty<string>(ref mSelectedServiceType, value))
        {
          OnPropertyChanged(IsMessageCompressionVisibleProperty);
        }
      }
    }

    #endregion

    #region bool IsMessageCompressionVisible

    public const string IsMessageCompressionVisibleProperty = "IsMessageCompressionVisible";
    public bool IsMessageCompressionVisible
    {
      get { return SelectedServiceType == BusinessService; }
    }

    #endregion

    #region bool UseMessageCompression

    public const string UseMessageCompressionProperty = "UseMessageCompression";
    bool mUseMessageCompression;
    public bool UseMessageCompression
    {
      get { return mUseMessageCompression; }
      set { SetProperty<bool>(ref mUseMessageCompression, value); }
    }

    #endregion


    #region bool Validate()

    public override bool Validate()
    {
      bool isValid = base.Validate();

      if (string.IsNullOrWhiteSpace(ClassName))
      {
        isValid = AppendErrorMessage("Service Interface Name is mandatory.");
      }

      return isValid;
    }

    #endregion

    #region void Apply()

    public override void Apply()
    {
      Dictionary<string, string> map = new Dictionary<string, string>();
      map.Add(BusinessService, BusinessServiceAttribute);

      CodeGeneration.AfxServiceInterface si = new CodeGeneration.AfxServiceInterface();
      si.Session = new Dictionary<string, object>();
      si.Session["ns"] = Context.Namespace;
      si.Session["name"] = ClassName;
      si.Session["serviceType"] = map[SelectedServiceType];
      si.Session["isSecure"] = SelectedServiceType == BusinessService ? true : false;
      si.Session["isCompressed"] = UseMessageCompression;
      si.Initialize();
      string ss = si.TransformText();
      string fileName = string.Format("{0}{1}.cs", Context.Folder, ClassName);
      File.WriteAllText(fileName, ss);
      ProjectItem newItem = Context.NodeItems.AddFromFile(fileName);
      Window w = newItem.Open(EnvDTE.Constants.vsViewKindCode);
      w.Visible = true;
    }

    #endregion
  }
}
