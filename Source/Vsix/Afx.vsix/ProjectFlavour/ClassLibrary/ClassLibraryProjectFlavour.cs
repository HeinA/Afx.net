﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.ProjectFlavour.ClassLibrary
{
  [Guid(ClassLibraryProjectGuidString)]
  public class ClassLibraryProjectFlavour : FlavoredProjectBase //, IVsProjectFlavorCfgProvider
  {
    public const string ClassLibraryProjectGuidString = "46AB4897-FB54-4F65-839C-C12909CE7753";
    AfxPackage Package { get; set; }

    // The IVsProjectFlavorCfgProvider of the inner project.
    // Because we are flavoring the base project directly, it is always null.
    protected IVsProjectFlavorCfgProvider innerVsProjectFlavorCfgProvider = null;

    public ClassLibraryProjectFlavour(AfxPackage package)
    {
      Package = package;
    }

    #region Overriden Methods

    /// <summary>
    /// This is were all QI for interface on the inner object should happen. 
    /// Then set the inner project wait for InitializeForOuter to be called to do
    /// the real initialization
    /// </summary>
    /// <param name="innerIUnknown"></param>
    protected override void SetInnerProject(IntPtr innerIUnknown)
    {
      object objectForIUnknown = null;
      objectForIUnknown = Marshal.GetObjectForIUnknown(innerIUnknown);
      if (base.serviceProvider == null)
      {
        base.serviceProvider = this.Package;
      }
      base.SetInnerProject(innerIUnknown);
      this.innerVsProjectFlavorCfgProvider = objectForIUnknown as IVsProjectFlavorCfgProvider;
    }

    /// <summary>
    /// Release the innerVsProjectFlavorCfgProvider when closed.
    /// </summary>
    protected override void Close()
    {
      base.Close();
      if (innerVsProjectFlavorCfgProvider != null)
      {
        if (Marshal.IsComObject(innerVsProjectFlavorCfgProvider))
        {
          Marshal.ReleaseComObject(innerVsProjectFlavorCfgProvider);
        }
        innerVsProjectFlavorCfgProvider = null;
      }
    }

    /// <summary>
    ///  By overriding GetProperty method and using propId parameter containing one of 
    ///  the values of the __VSHPROPID2 enumeration, we can filter, add or remove project
    ///  properties. 
    ///  
    ///  For example, to add a page to the configuration-dependent property pages, we
    ///  need to filter configuration-dependent property pages and then add a new page 
    ///  to the existing list. 
    /// </summary>
    //protected override int GetProperty(uint itemId, int propId, out object property)
    //{
    //  if (propId == (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList)
    //  {
    //    // Get a semicolon-delimited list of clsids of the configuration-dependent
    //    // property pages.
    //    ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));

    //    // Add the ClassLibrary property page.
    //    property += ';' + typeof(PropertyPage).GUID.ToString("B");

    //    return VSConstants.S_OK;
    //  }

    //  if (propId == (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList)
    //  {

    //    // Get the list of priority page guids from the base project system.
    //    ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
    //    string pageList = (string)property;

    //    // Remove the Services page from the project designer.
    //    string servicesPageGuidString = "{43E38D2E-43B8-4204-8225-9357316137A4}";

    //    RemoveFromCLSIDList(ref pageList, servicesPageGuidString);
    //    property = pageList;
    //    return VSConstants.S_OK;
    //  }

    //  return base.GetProperty(itemId, propId, out property);
    //}

    //private void RemoveFromCLSIDList(ref string pageList, string pageGuidString)
    //{
    //  // Remove the specified page guid from the string of guids.
    //  int index =
    //      pageList.IndexOf(pageGuidString, StringComparison.OrdinalIgnoreCase);

    //  if (index != -1)
    //  {
    //    // Guids are separated by ';', so we need to ensure we remove the ';' 
    //    // when removing the last guid in the list.
    //    int index2 = index + pageGuidString.Length + 1;
    //    if (index2 >= pageList.Length)
    //    {
    //      pageList = pageList.Substring(0, index).TrimEnd(';');
    //    }
    //    else
    //    {
    //      pageList = pageList.Substring(0, index) + pageList.Substring(index2);
    //    }
    //  }
    //  else
    //  {
    //    throw new ArgumentException(
    //        string.Format("Cannot find the Page {0} in the Page List {1}",
    //        pageGuidString, pageList));
    //  }
    //}

    #endregion

    #region IVsProjectFlavorCfgProvider Members

    /// <summary>
    /// Allows the base project to ask the project subtype to create an 
    /// IVsProjectFlavorCfg object corresponding to each one of its 
    /// (project subtype's) configuration objects.
    /// </summary>
    /// <param name="pBaseProjectCfg">
    /// The IVsCfg object of the base project.
    /// </param>
    /// <param name="ppFlavorCfg">
    /// The IVsProjectFlavorCfg object of the project subtype.
    /// </param>
    /// <returns></returns>
    //public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
    //{
    //  IVsProjectFlavorCfg cfg = null;

    //  if (innerVsProjectFlavorCfgProvider != null)
    //  {
    //    innerVsProjectFlavorCfgProvider.
    //        CreateProjectFlavorCfg(pBaseProjectCfg, out cfg);
    //  }

    //  ClassLibraryProjectFlavorCfg configuration = new ClassLibraryProjectFlavorCfg();

    //  configuration.Initialize(this, pBaseProjectCfg, cfg);
    //  ppFlavorCfg = (IVsProjectFlavorCfg)configuration;

    //  return VSConstants.S_OK;
    //}

    
    #endregion
  }
}
