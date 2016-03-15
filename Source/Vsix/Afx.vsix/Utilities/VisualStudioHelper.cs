using Afx.vsix.Extensions;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.Utilities
{
  public static class VisualStudioHelper
  {
    public static IEnumerable<string> GetAssemblyReferences(Project project)
    {
      var vsproject = project.Object as VSLangProj.VSProject;
      // note: you could also try casting to VsWebSite.VSWebSite

      foreach (VSLangProj.Reference reference in vsproject.References)
      {
        if (reference.SourceProject == null)
        {
          yield return reference.Path;
          // This is an assembly reference
          //var fullName = GetFullName(reference);
          //var assemblyName = new AssemblyName(fullName);
          //yield return assemblyName;
        }
      }
    }

    public static IEnumerable<Project> GetProjectReferences(Project project)
    {
      var vsproject = project.Object as VSLangProj.VSProject;
      // note: you could also try casting to VsWebSite.VSWebSite

      foreach (VSLangProj.Reference reference in vsproject.References)
      {
        if (reference.SourceProject != null)
        {
          yield return reference.SourceProject;
        }
      }
    }

    public static string GetFullName(VSLangProj.Reference reference)
    {
      return string.Format("{0}, Version={1}.{2}.{3}.{4}, Culture={5}, PublicKeyToken={6}",
                              reference.Name,
                              reference.MajorVersion, reference.MinorVersion, reference.BuildNumber, reference.RevisionNumber,
                              reference.Culture.Or("neutral"),
                              reference.PublicKeyToken.Or("null"));
    }

    public static string GetProjectTypeGuids(Project proj)
    {
      string projectTypeGuids = "";
      object service = null;
      Microsoft.VisualStudio.Shell.Interop.IVsSolution solution = null;
      Microsoft.VisualStudio.Shell.Interop.IVsHierarchy hierarchy = null;
      Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject aggregatableProject = null;
      int result = 0;
      service = GetService(proj.DTE, typeof(Microsoft.VisualStudio.Shell.Interop.IVsSolution));
      solution = (Microsoft.VisualStudio.Shell.Interop.IVsSolution)service;

      result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

      if (result == 0)
      {
        aggregatableProject = (Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject)hierarchy;
        result = aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
      }

      return projectTypeGuids;
    }

    public static object GetService(object serviceProvider, System.Type type)
    {
      return GetService(serviceProvider, type.GUID);
    }

    public static object GetService(object serviceProviderObject, System.Guid guid)
    {
      object service = null;
      Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider = null;
      IntPtr serviceIntPtr;
      int hr = 0;
      Guid SIDGuid;
      Guid IIDGuid;

      SIDGuid = guid;
      IIDGuid = SIDGuid;
      serviceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)serviceProviderObject;
      hr = serviceProvider.QueryService(ref SIDGuid, ref IIDGuid, out serviceIntPtr);

      if (hr != 0)
      {
        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
      }
      else if (!serviceIntPtr.Equals(IntPtr.Zero))
      {
        service = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(serviceIntPtr);
        System.Runtime.InteropServices.Marshal.Release(serviceIntPtr);
      }

      return service;
    }

    public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
    {
      hierarchy = null;
      itemid = VSConstants.VSITEMID_NIL;
      int hr = VSConstants.S_OK;
      var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
      var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
      if (monitorSelection == null || solution == null)
      {
        return false;
      }
      IVsMultiItemSelect multiItemSelect = null;
      IntPtr hierarchyPtr = IntPtr.Zero;
      IntPtr selectionContainerPtr = IntPtr.Zero;
      try
      {
        hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);
        if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
        {
          // there is no selection
          return false;
        }
        // multiple items are selected
        if (multiItemSelect != null) return false;

        // there is a hierarchy root node selected, thus it is not a single item inside a project
        //if (itemid == VSConstants.VSITEMID_ROOT) return false;

        hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
        if (hierarchy == null) return false;
        Guid guidProjectID = Guid.Empty;
        if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
        {
          return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
        }
        // if we got this far then there is a single project item selected
        return true;
      }
      finally
      {
        if (selectionContainerPtr != IntPtr.Zero)
        {
          Marshal.Release(selectionContainerPtr);
        }
        if (hierarchyPtr != IntPtr.Zero)
        {
          Marshal.Release(hierarchyPtr);
        }
      }
    }
  }
}
