using Afx.vsix.Utilities;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.AfxWizard.Items
{
  public class CreateAssociativeClass : AfxWizardCommand
  {
    #region Constructors

    public CreateAssociativeClass(AfxWizardContext context) 
      : base("Create Associative Class", context)
    {
    }

    #endregion


    Collection<string> mBaseTypes;
    public Collection<string> BaseTypes
    {
      get
      {
        if (mBaseTypes != null) return mBaseTypes;
        mBaseTypes = new Collection<string>(TypeHelper.GetAfxTypeNames(Context.Project, TypeHelper.AssociativeObject).ToList());
        mBaseTypes.Insert(0, string.Empty);
        return mBaseTypes;
      }
    }

    Collection<string> mTemplateTypes;
    public Collection<string> TemplateTypes
    {
      get
      {
        if (mTemplateTypes != null) return mTemplateTypes;
        mTemplateTypes = new Collection<string>(TypeHelper.GetAfxTypeNames(Context.Project).Except(TypeHelper.GetAfxTypeNames(Context.Project, TypeHelper.AssociativeObject)).ToList());
        mTemplateTypes.Insert(0, string.Empty);
        return mTemplateTypes;
      }
    }

    #region string ClassName

    string mClassName;
    public string ClassName
    {
      get { return mClassName; }
      set { SetProperty<string>(ref mClassName, value); }
    }

    #endregion

    #region string BaseClassName

    string mBaseClassName;
    public string BaseClassName
    {
      get { return mBaseClassName; }
      set
      {
        if (SetProperty<string>(ref mBaseClassName, value))
        {
          OnPropertyChanged(IsPersistentVisibleProperty);
          OnPropertyChanged(IsAssociativeDetailsVisibleProperty);
        }
      }
    }

    #endregion

    #region bool IsAssociativeDetailsVisible

    public const string IsAssociativeDetailsVisibleProperty = "IsAssociativeDetailsVisible";
    public bool IsAssociativeDetailsVisible
    {
      get { return string.IsNullOrWhiteSpace(BaseClassName); }
    }

    #endregion

    #region string OwnerName

    public const string OwnerNameProperty = "OwnerName";
    string mOwnerName;
    public string OwnerName
    {
      get { return mOwnerName; }
      set { SetProperty<string>(ref mOwnerName, value); }
    }

    #endregion

    #region string ReferenceName

    public const string ReferenceNameProperty = "ReferenceName";
    string mReferenceName;
    public string ReferenceName
    {
      get { return mReferenceName; }
      set { mReferenceName = value; }
    }

    #endregion

    #region bool IsPersistent

    bool mIsPersistent;
    public bool IsPersistent
    {
      get { return mIsPersistent; }
      set { SetProperty<bool>(ref mIsPersistent, value); }
    }

    #endregion

    #region bool IsPersistentVisible

    public const string IsPersistentVisibleProperty = "IsPersistentVisible";
    public bool IsPersistentVisible
    {
      get
      {
        if (string.IsNullOrWhiteSpace(BaseClassName)) return true;

        Type baseType = Utilities.TypeHelper.GetAfxReferenceTypeByName(Context.Project, BaseClassName);
        if (baseType != null)
        {
          if (baseType.GetCustomAttributes(false).FirstOrDefault(t1 => t1.GetType().FullName.Equals(Constants.PersistentObjectAttribute)) != null) return true;
        }

        CodeClass baseClass = Utilities.TypeHelper.GetAfxCodeTypeByName(Context.Project, BaseClassName);
        if (baseClass != null)
        {
          if (baseClass.Attributes.Cast<CodeElement>().FirstOrDefault(ce => ce.FullName.Equals(Constants.PersistentObjectAttribute)) != null) return true;
        }

        return false;
      }
    }

    #endregion

    public override bool Validate()
    {
      bool isValid = base.Validate();

      if (string.IsNullOrWhiteSpace(ClassName))
      {
        isValid = AppendErrorMessage("Class Name is mandatory.");
      }

      return isValid;
    }

    public override void Apply()
    {
      CodeGeneration.AfxAssociative ac = new CodeGeneration.AfxAssociative();
      ac.Session = new Dictionary<string, object>();
      ac.Session["ns"] = Context.Namespace;
      ac.Session["name"] = ClassName;
      ac.Session["baseClass"] = BaseClassName;
      ac.Session["owner"] = OwnerName;
      ac.Session["reference"] = ReferenceName;
      ac.Session["hasDataContract"] = Context.IsClassLibrary;
      ac.Session["isPersistent"] = IsPersistent;
      ac.Initialize();
      string ss = ac.TransformText();
      string fileName = string.Format("{0}{1}.cs", Context.Folder, ClassName);
      File.WriteAllText(fileName, ss);
      ProjectItem newItem = Context.NodeItems.AddFromFile(fileName);
      Window w = newItem.Open(EnvDTE.Constants.vsViewKindCode);
      w.Visible = true;
    }
  }
}
