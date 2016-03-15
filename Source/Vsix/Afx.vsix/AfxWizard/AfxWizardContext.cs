using Afx.vsix.Utilities;
using Afx.vsix.ViewModels;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSLangProj;

namespace Afx.vsix.AfxWizard
{
  public class AfxWizardContext
  {
    #region Constructors

    public AfxWizardContext()
    {
    }

    #endregion

    #region Collection<AfxWizardItem> Commands

    Collection<AfxWizardCommand> mCommands;
    public IEnumerable<AfxWizardCommand> Commands
    {
      get
      {
        if (mCommands == null)
        {
          mCommands = new Collection<AfxWizardCommand>();
          mCommands.Add(new Items.CreateBusinessClass(this));
          mCommands.Add(new Items.CreateAssociativeClass(this));
          if (IsClassLibrary) mCommands.Add(new Items.CreateServiceInterface(this));
          if (IsServiceLibrary) mCommands.Add(new Items.CreateServiceImplementation(this));
        }
        return mCommands;
      }
    }

    #endregion


    #region Project Project

    Project mProject;
    public Project Project
    {
      get { return mProject; }
      set
      {
        mProject = value;
        mVSProject = value.Object as VSProject;

        foreach (Property p in Project.Properties)
        {
          try
          {
            Debug.Write(string.Format("{0}: ", p.Name));
            Debug.WriteLine(p.Value.ToString());
          }
          catch { }        
        }
      }
    }

    #endregion

    #region VSProject VSProject

    VSProject mVSProject;
    public VSProject VSProject
    {
      get { return mVSProject; }
      set { mVSProject = value; }
    }

    #endregion

    #region ProjectItem SelectedItem

    public ProjectItem SelectedItem { get; set; }

    #endregion

    #region bool IsServiceLibrary

    public bool IsServiceLibrary
    {
      get
      {
        string guids = VisualStudioHelper.GetProjectTypeGuids(Project);
        if (guids.Contains(ProjectFlavour.ServiceLibrary.ServiceLibraryProjectFactory.ServiceLibraryProjectFactoryGuidString)) return true;
        return false;
      }
    }

    #endregion

    #region bool IsClassLibrary

    public bool IsClassLibrary
    {
      get
      {
        string guids = VisualStudioHelper.GetProjectTypeGuids(Project);
        if (guids.Contains(ProjectFlavour.ClassLibrary.ClassLibraryProjectFactory.ClassLibraryProjectFactoryGuidString)) return true;
        return false;
      }
    }

    #endregion

    #region ProjectItems NodeItems

    ProjectItems mNodeItems;
    public ProjectItems NodeItems
    {
      get { return mNodeItems; }
      set { mNodeItems = value; }
    }

    #endregion

    #region string Namespace

    string mNamespace;
    public string Namespace
    {
      get { return mNamespace; }
      set { mNamespace = value; }
    }

    #endregion

    #region string Folder

    string mFolder;
    public string Folder
    {
      get { return mFolder; }
      set { mFolder = value; }
    }

    #endregion

    //#region Collection<CodeClass> CodeTypes

    //Collection<CodeClass> mCodeTypes = new Collection<CodeClass>();
    //public Collection<CodeClass> CodeTypes
    //{
    //  get { return mCodeTypes; }
    //  set { mCodeTypes = value; }
    //}

    //#endregion

    //#region Collection<Type> ReferenceTypes

    //Collection<Type> mReferenceTypes = new Collection<Type>();
    //public Collection<Type> ReferenceTypes
    //{
    //  get { return mReferenceTypes; }
    //  set { mReferenceTypes = value; }
    //}

    //#endregion

    //#region IEnumerable<string> BaseTypes

    //public IEnumerable<string> BaseTypes
    //{
    //  get
    //  {
    //    yield return string.Empty;
    //    foreach (var name in ReferenceTypes.Select(rt => rt.FullName).Union(CodeTypes.Select(ct => ct.FullName)).OrderBy(s => s))
    //    {
    //      yield return name;
    //    }
    //  }
    //}

    //#endregion

    //#region Type GetAfxReferenceTypeByName(...)

    //public Type GetAfxReferenceTypeByName(string name)
    //{
    //  return ReferenceTypes.FirstOrDefault(rt => rt.FullName.Equals(name));
    //}

    //#endregion

    //#region CodeClass GetAfxCodeTypeByName(...)

    //public CodeClass GetAfxCodeTypeByName(string name)
    //{
    //  return CodeTypes.FirstOrDefault(ct => ct.FullName.Equals(name));
    //}

    //#endregion

    public IEnumerable<Project> AllProjects
    {
      get
      {
        DTE dte = (DTE)AfxPackage.GetGlobalService(typeof(DTE));
        Collection<Project> projects = new Collection<Project>();
        foreach (Project p in dte.Solution.Projects)
        {
          GetAllProjects(p, projects);
        }
        return projects;
      }
    }

    void GetAllProjects(Project target, Collection<Project> projects)
    {
      projects.Add(target);
      foreach (ProjectItem pi in target.ProjectItems)
      {
        Project p = pi.SubProject as Project;
        if (p != null) GetAllProjects(p, projects);
      }
    }

    public IEnumerable<string> GlobalServiceInterfaces
    {
      get
      {
        yield return string.Empty;
        foreach (var name in TypeHelper.GetReferenceServiceInterfaces(Project).Select(rt => rt.FullName)
          .Union(TypeHelper.GetCodeServiceInterfaces(Project, false).Select(ct => ct.FullName)).OrderBy(s => s))
        {
          yield return name;
        }
      }
    }

    public IEnumerable<string> LocalServiceInterfaces
    {
      get
      {
        yield return string.Empty;
        foreach (var ci in TypeHelper.GetCodeServiceInterfaces(Project, true).Select(ci => ci.FullName).OrderBy(s => s))
        {
          yield return ci;
        }
      }
    }
  }
}
