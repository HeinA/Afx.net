using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.Utilities
{
  public static class TypeHelper
  {
    public const string AfxObject = "Afx.ObjectModel.AfxObject";
    public const string AssociativeObject = "Afx.ObjectModel.AssociativeObject";
    const string ServiceModel = "System.ServiceModel.ServiceContractAttribute";

    #region AfxObjects

    public static IEnumerable<string> GetAfxTypeNames(Project project)
    {
      return GetAfxTypeNames(project, AfxObject);
    }

    public static IEnumerable<string> GetAfxTypeNames(Project project, string afxTypeName)
    {
      foreach (var name in GetReferenceAfxTypes(project, afxTypeName).Select(rt => rt.FullName).Union(GetCodeAfxTypes(project, afxTypeName).Select(ct => ct.FullName)).OrderBy(s => s))
      {
        yield return name;
      }
    }

    public static IEnumerable<CodeClass> GetCodeAfxTypes(Project project)
    {
      return GetCodeAfxTypes(project, AfxObject);
    }

    public static IEnumerable<Type> GetReferenceAfxTypes(Project project)
    {
      return GetReferenceAfxTypes(project, AfxObject);
    }

    public static IEnumerable<CodeClass> GetCodeAfxTypes(Project project, string afxTypeName)
    {
      Collection<CodeClass> types = new Collection<CodeClass>();

      foreach (var pr in VisualStudioHelper.GetProjectReferences(project))
      {
        FindAfxClass(pr.CodeModel.CodeElements, types, afxTypeName);
      }

      FindAfxClass(project.CodeModel.CodeElements, types, afxTypeName);

      return types;
    }

    public static IEnumerable<Type> GetReferenceAfxTypes(Project project, string afxTypeName)
    {
      Collection<Type> types = new Collection<Type>();

      foreach (var name in VisualStudioHelper.GetAssemblyReferences(project))
      {
        if (!string.IsNullOrWhiteSpace(name))
        {
          Assembly a = Assembly.LoadFile(name);
          foreach (Type t in a.GetTypes())
          {
            if (IsAfxObject(t, afxTypeName) && t.GetCustomAttributes().FirstOrDefault(a1 => a1.GetType().FullName == "Afx.ObjectModel.Description.AfxBaseTypeAttribute") == null) types.Add(t);
          }
        }
      }

      return types;
    }

    static void FindAfxClass(CodeElements elements, Collection<CodeClass> types, string afxTypeName)
    {
      foreach (CodeElement ce in elements)
      {
        FindAfxClassInner(ce, types, afxTypeName);
      }
    }

    static bool FindAfxClassInner(CodeElement element, Collection<CodeClass> types, string afxTypeName)
    {
      var cc = element as CodeClass;

      if (cc != null)
      {
        try
        {
          ProjectItem pi = cc.ProjectItem as ProjectItem;
        }
        catch
        {
          return false;
        }

        if (cc.IsDerivedFrom[afxTypeName])
        {
          types.Add(cc);
        }

        foreach (CodeElement cc1 in cc.Members)
        {
          if (!FindAfxClassInner(cc1, types, afxTypeName)) return false;
        }
      }

      var cn = element as CodeNamespace;
      if (cn != null)
      {
        foreach (CodeElement cn1 in cn.Members)
        {
          if (!FindAfxClassInner(cn1, types, afxTypeName)) return false;
        }
      }

      return true;
    }

    static bool IsAfxObject(Type type, string afxTypeName)
    {
      if (type == null) return false;
      if (type.FullName == typeof(System.Object).FullName) return false;
      if (type.FullName == afxTypeName) return true;
      return IsAfxObject(type.BaseType, afxTypeName);
    }

    #endregion

    #region Type GetAfxReferenceTypeByName(...)

    public static Type GetAfxReferenceTypeByName(Project project, string name)
    {
      return GetReferenceAfxTypes(project).FirstOrDefault(rt => rt.FullName.Equals(name));
    }

    #endregion

    #region CodeClass GetAfxCodeTypeByName(...)

    public static CodeClass GetAfxCodeTypeByName(Project project, string name)
    {
      return GetCodeAfxTypes(project).FirstOrDefault(ct => ct.FullName.Equals(name));
    }

    #endregion

    #region ServiceInterfaces

    public static Collection<CodeInterface> GetCodeServiceInterfaces(Project project, bool localOnly)
    {
      Collection<CodeInterface> types = new Collection<CodeInterface>();

      if (!localOnly)
      {
        foreach (var pr in VisualStudioHelper.GetProjectReferences(project))
        {
          FindServiceInterfaces(pr.CodeModel.CodeElements, types);
        }
      }

      if (project.CodeModel != null) FindServiceInterfaces(project.CodeModel.CodeElements, types);

      return types;
    }

    public static Collection<Type> GetReferenceServiceInterfaces(Project project)
    {
      Collection<Type> types = new Collection<Type>();

      foreach (var name in VisualStudioHelper.GetAssemblyReferences(project))
      {
        if (!string.IsNullOrWhiteSpace(name))
        {
          Assembly a = Assembly.LoadFile(name);
          foreach (Type t in a.GetTypes())
          {
            if (t.GetCustomAttributes().FirstOrDefault(a1 => a1.GetType().FullName.Equals(ServiceModel)) != null) types.Add(t);
          }
        }
      }

      return types;
    }

    static void FindServiceInterfaces(CodeElements elements, Collection<CodeInterface> types)
    {
      foreach (CodeElement ce in elements)
      {
        FindServiceInterfacesInner(ce, types);
      }
    }

    static bool FindServiceInterfacesInner(CodeElement element, Collection<CodeInterface> types)
    {
      var ci = element as CodeInterface;

      if (ci != null)
      {
        try
        {
          ProjectItem pi = ci.ProjectItem as ProjectItem;
        }
        catch
        {
          return false;
        }

        if (ci.Attributes.Cast<CodeElement>().FirstOrDefault(ci1 => ci1.FullName.Equals(ServiceModel)) != null)
        {
          types.Add(ci);
        }

        foreach (CodeElement ci1 in ci.Members)
        {
          if (!FindServiceInterfacesInner(ci1, types)) return false;
        }
      }

      var cn = element as CodeNamespace;
      if (cn != null)
      {
        foreach (CodeElement cn1 in cn.Members)
        {
          if (!FindServiceInterfacesInner(cn1, types)) return false;
        }
      }

      return true;
    }

    #endregion

    #region ServiceImplementations

    public static CodeClass GetServiceImplementation(string interfaceName)
    {
      DTE dte = (DTE)AfxPackage.GetGlobalService(typeof(DTE));
      foreach (Project p in dte.Solution.Projects)
      {
        CodeClass cc1 = GetServiceImplementation(interfaceName, p);
        if (cc1 != null) return cc1;
        //foreach (CodeElement ce in p.CodeModel.CodeElements)
        //{
        //  try
        //  {
        //    CodeClass cc1 = FindServiceImplentation(ce, interfaceName);
        //    if (cc1 != null) return cc1;
        //  }
        //  catch
        //  {
        //    break;
        //  }
        //}
      }

      return null;
    }

    static CodeClass GetServiceImplementation(string interfaceName, Project project)
    {
      if (project.CodeModel != null)
      {
        foreach (CodeElement ce in project.CodeModel.CodeElements)
        {
          try
          {
            CodeClass cc1 = FindServiceImplentation(ce, interfaceName);
            if (cc1 != null) return cc1;
          }
          catch
          {
            break;
          }
        }
      }

      foreach (ProjectItem pi in project.ProjectItems)
      {
        Project p = pi.SubProject as Project;
        if (p != null)
        {
          CodeClass cc1 = GetServiceImplementation(interfaceName, p);
          if (cc1 != null) return cc1;
        }
      }

      return null;
    }

    static CodeClass FindServiceImplentation(CodeElement element, string interfaceName)
    {
      var cc = element as CodeClass;

      if (cc != null)
      {
//        try
        //{
          ProjectItem pi = cc.ProjectItem as ProjectItem;
//        }
//        catch
//        {
//          return null;
//        }

        if (cc.ImplementedInterfaces.Cast<CodeElement>().FirstOrDefault(ci1 => ci1.FullName.Equals(interfaceName)) != null)
        {
          return cc;
        }

        foreach (CodeElement ce in cc.Members)
        {
          CodeClass cc1 = FindServiceImplentation(ce, interfaceName);
          if (cc1 != null) return cc1;
        }
      }

      var cn = element as CodeNamespace;
      if (cn != null)
      {
        foreach (CodeElement cn1 in cn.Members)
        {
          CodeClass cc1 = FindServiceImplentation(cn1, interfaceName);
          if (cc1 != null) return cc1;
        }
      }

      return null;
    }

    #endregion
  }
}
