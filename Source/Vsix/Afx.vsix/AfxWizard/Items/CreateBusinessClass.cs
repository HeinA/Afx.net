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
  public class CreateBusinessClass : AfxWizardCommand
  {
    #region Constructors

    public CreateBusinessClass(AfxWizardContext context) 
      : base("Create Business Class", context)
    {
    }

    #endregion


    Collection<string> mBaseTypes;
    public Collection<string> BaseTypes
    {
      get
      {
        if (mBaseTypes != null) return mBaseTypes;
        mBaseTypes = new Collection<string>(TypeHelper.GetAfxTypeNames(Context.Project).Except(TypeHelper.GetAfxTypeNames(Context.Project, TypeHelper.AssociativeObject)).ToList());
        mBaseTypes.Insert(0, string.Empty);
        return mBaseTypes;
      }
    }

    #region string ClassName

    string mClassName;
    public string ClassName
    {
      get { return mClassName; }
      set
      {
        if (SetProperty<string>(ref mClassName, value))
        {
          MethodStub = string.Format("{0}{1}", ClassName, IsCollection ? "Collection" : string.Empty);
        }
      }
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

    #region string BaseClassName

    string mBaseClassName;
    public string BaseClassName
    {
      get { return mBaseClassName; }
      set
      {
        if (SetProperty<string>(ref mBaseClassName, value))
        {
          IsAggregateRoot = false;
          OnPropertyChanged(IsPersistentVisibleProperty);
          OnPropertyChanged(IsAggregateRootVisibleProperty);
        }
      }
    }

    #endregion

    #region bool IsPersistent

    bool mIsPersistent;
    public bool IsPersistent
    {
      get { return mIsPersistent; }
      set
      {
        if (SetProperty<bool>(ref mIsPersistent, value))
        {
          IsAggregateRoot = false;
          OnPropertyChanged(IsAggregateRootVisibleProperty);
          OnPropertyChanged(IsServiceInterfaceVisibleProperty);
        }
      }
    }

    #endregion

    #region bool IsPersistentVisible

    public const string IsPersistentVisibleProperty = "IsPersistentVisible";
    public bool IsPersistentVisible
    {
      get
      {
        if (string.IsNullOrWhiteSpace(BaseClassName)) return true;

        Type baseType = TypeHelper.GetAfxReferenceTypeByName(Context.Project, BaseClassName);
        if (baseType != null)
        {
          if (baseType.GetCustomAttributes(false).FirstOrDefault(t1 => t1.GetType().FullName.Equals(Constants.PersistentObjectAttribute)) != null) return true;
        }

        CodeClass baseClass = TypeHelper.GetAfxCodeTypeByName(Context.Project, BaseClassName);
        if (baseClass != null)
        {
          if (baseClass.Attributes.Cast<CodeElement>().FirstOrDefault(ce => ce.FullName.Equals(Constants.PersistentObjectAttribute)) != null) return true;
        }

        return false;
      }
    }

    #endregion

    #region string SelectedServiceInterfaces

    public const string SelectedServiceInterfacesProperty = "SelectedServiceInterfaces";
    string mSelectedServiceInterface;
    public string SelectedServiceInterface
    {
      get { return mSelectedServiceInterface; }
      set
      {
        if (SetProperty<string>(ref mSelectedServiceInterface, value))
        {
          UpdateMethodNames();
          OnPropertyChanged(IsServiceInterfaceDetailsVisibleProperty);
          ServiceImplementationClass = Utilities.TypeHelper.GetServiceImplementation(SelectedServiceInterface);
        }
      }
    }

    #endregion

    #region bool IsServiceInterfaceVisible

    public const string IsServiceInterfaceVisibleProperty = "IsServiceInterfaceVisible";
    public bool IsServiceInterfaceVisible
    {
      get { return IsAggregateRoot || (GetHasIAggregateRootBase() && IsPersistent); }
    }

    #endregion

    #region bool IsAggregateRoot

    bool mIsAggregateRoot;
    public bool IsAggregateRoot
    {
      get { return mIsAggregateRoot; }
      set
      {
        if (SetProperty<bool>(ref mIsAggregateRoot, value))
        {
          SelectedServiceInterface = null;
          OnPropertyChanged(IsServiceInterfaceVisibleProperty);
        }
      }
    }

    #endregion

    #region bool IsAggregateRootVisible

    public const string IsAggregateRootVisibleProperty = "IsAggregateRootVisible";
    public bool IsAggregateRootVisible
    {
      get { return IsPersistent && !GetHasIAggregateRootBase(); }
    }

    #endregion

    #region bool IsServiceInterfaceDetailsVisible

    public const string IsServiceInterfaceDetailsVisibleProperty = "IsServiceInterfaceDetailsVisible";
    public bool IsServiceInterfaceDetailsVisible
    {
      get { return !string.IsNullOrWhiteSpace(SelectedServiceInterface); }
    }

    #endregion

    #region bool IsCollection

    public const string IsCollectionProperty = "IsCollection";
    bool mIsCollection;
    public bool IsCollection
    {
      get { return mIsCollection; }
      set
      {
        if (SetProperty<bool>(ref mIsCollection, value))
        {
          MethodStub = string.Format("{0}{1}", ClassName, IsCollection ? "Collection" : string.Empty);
        }
      }
    }

    #endregion

    #region string MethodStub

    public const string MethodStubProperty = "MethodStub";
    string mMethodStub;
    public string MethodStub
    {
      get { return mMethodStub; }
      set
      {
        if (SetProperty<string>(ref mMethodStub, value))
        {
          UpdateMethodNames();
        }
      }
    }

    #endregion

    #region string LoadMethodName

    public const string LoadMethodNameProperty = "LoadMethodName";
    string mLoadMethodName;
    public string LoadMethodName
    {
      get { return mLoadMethodName; }
      set { SetProperty<string>(ref mLoadMethodName, value); }
    }

    #endregion

    #region string SaveMethodName

    public const string SaveMethodNameProperty = "SaveMethodName";
    string mSaveMethodName;
    public string SaveMethodName
    {
      get { return mSaveMethodName; }
      set { SetProperty<string>(ref mSaveMethodName, value); }
    }

    #endregion

    #region CodeClass ServiceImplementationClass

    public const string ServiceImplementationClassProperty = "ServiceImplementationClass";
    CodeClass mServiceImplementationClass;
    public CodeClass ServiceImplementationClass
    {
      get { return mServiceImplementationClass; }
      set
      {
        if (SetProperty<CodeClass>(ref mServiceImplementationClass, value))
        {
          OnPropertyChanged(ServiceImplementationClassNameProperty);
        }
      }
    }

    #endregion

    #region string ServiceImplementationClassName

    public const string ServiceImplementationClassNameProperty = "ServiceImplementationClassName";
    public string ServiceImplementationClassName
    {
      get { return ServiceImplementationClass == null ? string.Empty : ServiceImplementationClass.FullName; }
    }

    #endregion


    #region void UpdateMethodNames()

    void UpdateMethodNames()
    {
      SaveMethodName = string.Format("Save{0}", MethodStub);
      LoadMethodName = string.Format("Load{0}", MethodStub);
    }

    #endregion

    #region GetHasIAggregateRootBase()

    bool GetHasIAggregateRootBase()
    {
      if (string.IsNullOrWhiteSpace(BaseClassName)) return false;

      Type baseType = TypeHelper.GetAfxReferenceTypeByName(Context.Project, BaseClassName);
      if (baseType != null)
      {
        return HasIAggregateRootBaseType(baseType);
      }

      CodeClass baseClass = TypeHelper.GetAfxCodeTypeByName(Context.Project, BaseClassName);
      if (baseClass != null)
      {
        return HasIAggregateRootBaseClass(baseClass);
      }

      return false;
    }

    bool HasIAggregateRootBaseType(Type t)
    {
      if (t.Equals(typeof(System.Object))) return false;
      if (t.GetInterfaces().FirstOrDefault(t1 => t1.FullName.Equals(Constants.IAggregateRoot)) != null) return true;
      if (t.BaseType != null) return HasIAggregateRootBaseType(t.BaseType);
      return false;
    }

    bool HasIAggregateRootBaseClass(CodeClass cc)
    {
      if (cc.FullName.Equals("System.Object")) return false;
      if (cc.ImplementedInterfaces.Cast<CodeElement>().FirstOrDefault(ce => ce.FullName.Equals(Constants.IAggregateRoot)) != null) return true;
      foreach (var ce in cc.Bases)
      {
        CodeClass cc1 = ce as CodeClass;
        if (cc1 != null)
        {
          if (HasIAggregateRootBaseClass(cc1)) return true;
        }
      }
      return false;
    }

    #endregion


    #region bool Validate()

    public override bool Validate()
    {
      bool isValid = base.Validate();

      if (string.IsNullOrWhiteSpace(ClassName))
      {
        isValid = AppendErrorMessage("Class Name is mandatory.");
      }

      if (!string.IsNullOrWhiteSpace(SelectedServiceInterface))
      {
        if (string.IsNullOrWhiteSpace(LoadMethodName))
        {
          isValid = AppendErrorMessage("Load Method Name is mandatory.");
        }
        if (string.IsNullOrWhiteSpace(SaveMethodName))
        {
          isValid = AppendErrorMessage("Save Method Name is mandatory.");
        }
      }

      return isValid;
    }

    #endregion

    #region void Apply()

    public override void Apply()
    {
      CodeGeneration.AfxClass ac = new CodeGeneration.AfxClass();
      ac.Session = new Dictionary<string, object>();
      ac.Session["ns"] = Context.Namespace;
      ac.Session["name"] = ClassName;
      ac.Session["baseClass"] = BaseClassName;
      ac.Session["owner"] = OwnerName;
      ac.Session["hasDataContract"] = Context.IsClassLibrary;
      ac.Session["isPersistent"] = IsPersistent;
      ac.Session["isAggregateRoot"] = IsAggregateRoot;
      ac.Initialize();
      string ss = ac.TransformText();
      string fileName = string.Format("{0}{1}.cs", Context.Folder, ClassName);
      File.WriteAllText(fileName, ss);
      ProjectItem newItem = Context.NodeItems.AddFromFile(fileName);
      Window w = newItem.Open(EnvDTE.Constants.vsViewKindCode);
      w.Visible = true;

      if (!string.IsNullOrWhiteSpace(SelectedServiceInterface))
      {
        CodeInterface ci = TypeHelper.GetCodeServiceInterfaces(Context.Project, true).FirstOrDefault(ci1 => ci1.FullName.Equals(SelectedServiceInterface));
        EditPoint ep = ci.EndPoint.CreateEditPoint();
        ep.LineUp();
        ep.EndOfLine();
        ep.Insert(string.Format("\r\n\r\n#region {0}\r\n\r\n", ClassName));
        if (IsCollection)
        {
          ep.Insert(string.Format("ObjectCollection<{2}.{0}> {1}();\r\n", ClassName, LoadMethodName, Context.Namespace));
          ep.Insert(string.Format("ObjectCollection<{2}.{0}> {1}(ObjectCollection<{2}.{0}> col);\r\n", ClassName, SaveMethodName, Context.Namespace));
        }
        else
        {
          ep.Insert(string.Format("{2}.{0} {1}(Guid id);\r\n", ClassName, LoadMethodName, Context.Namespace));
          ep.Insert(string.Format("{2}.{0} {1}({2}.{0} obj);\r\n", ClassName, SaveMethodName, Context.Namespace));
        }
        ep.Insert("\r\n\r\n#endregion");
        Reformat(ci as CodeElement);


        if (ServiceImplementationClass != null)
        {
          ep = ServiceImplementationClass.EndPoint.CreateEditPoint();
          ep.LineUp();
          ep.EndOfLine();
          ep.Insert(string.Format("\r\n\r\n#region {0}\r\n\r\n", ClassName));
          if (IsCollection)
          {
            ep.Insert(string.Format("public ObjectCollection<{2}.{0}> {1}()\r\n{{\r\nthrow new NotImplementedException();\r\n}}\r\n\r\n", ClassName, LoadMethodName, Context.Namespace));
            ep.Insert(string.Format("public ObjectCollection<{2}.{0}> {1}(ObjectCollection<{2}.{0}> col)\r\n{{\r\nthrow new NotImplementedException();\r\n}}\r\n", ClassName, SaveMethodName, Context.Namespace));
          }
          else
          {
            ep.Insert(string.Format("public {2}.{0} {1}(Guid id)\r\n{{\r\nthrow new NotImplementedException();\r\n}}\r\n\r\n", ClassName, LoadMethodName, Context.Namespace));
            ep.Insert(string.Format("public {2}.{0} {1}({2}.{0} obj)\r\n{{\r\nthrow new NotImplementedException();\r\n}}\r\n", ClassName, SaveMethodName, Context.Namespace));
          }
          ep.Insert("\r\n\r\n#endregion");
          Reformat(ServiceImplementationClass as CodeElement);
        }
      }
    }

    #endregion
  }
}
