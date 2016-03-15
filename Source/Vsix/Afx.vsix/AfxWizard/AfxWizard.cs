//------------------------------------------------------------------------------
// <copyright file="CreateBusinessClass.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using EnvDTE;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;
using Afx.vsix.ProjectFlavour.ServiceLibrary;
using Afx.vsix.Utilities;

namespace Afx.vsix.AfxWizard
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class AfxWizard
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0100;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("ffebf002-f5b9-4dd2-b70b-f3edb0cf47a8");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package package;

    /// <summary>
    /// Initializes a new instance of the <see cref="AfxWizard"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private AfxWizard(Package package)
    {
      if (package == null)
      {
        throw new ArgumentNullException("package");
      }

      this.package = package;

      OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
        commandService.AddCommand(menuItem);
      }

    }
    private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
    {
      var menuCommand = sender as OleMenuCommand;
      if (menuCommand != null)
      {
        IVsHierarchy hierarchy = null;
        uint itemid = 0;

        bool visible = VisualStudioHelper.IsSingleProjectItemSelection(out hierarchy, out itemid);
        if (visible)
        {
          object selectedObject = null;
          ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject));
          ProjectItem pi = selectedObject as ProjectItem;
          if (pi != null)
          {
            if (pi.Kind != "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}")
            {
              visible = false;
            }
          }
        }
        menuCommand.Visible = visible;
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static AfxWizard Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package package)
    {
      Instance = new AfxWizard(package);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      IVsHierarchy hierarchy = null;
      uint itemid = VSConstants.VSITEMID_NIL;

      if (VisualStudioHelper.IsSingleProjectItemSelection(out hierarchy, out itemid))
      {
        object selectedObject = null;
        ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject));

        Project project = selectedObject as Project;
        ProjectItem pi = selectedObject as ProjectItem;

        ProjectItems pitems = null;
        EnvDTE.Properties props = null;
        if (project != null)
        {
          props = project.Properties;
          pitems = project.ProjectItems;
        }
        if (pi != null)
        {
          if (pi.Kind != "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") // Is it a folder
          {
            return;
          }
          props = pi.Properties;
          project = pi.ContainingProject;
          pitems = pi.ProjectItems;
        }

        string guids = VisualStudioHelper.GetProjectTypeGuids(project);

        string ns = (string)props.Item("DefaultNamespace").Value;
        string folder = (string)props.Item("FullPath").Value; //LocalPath

        //CodeGeneration.AfxClass ac = new CodeGeneration.AfxClass();
        //ac.Session = new Dictionary<string, object>();
        //ac.Session["ns"] = ns;
        //ac.Session["name"] = "aaa";
        //ac.Session["owner"] = null;
        //ac.Initialize();
        //string ss = ac.TransformText();
//        pitems.AddFromFile();

        AfxWizardUI ui = new AfxWizardUI();
        ui.ViewModel.Model.SelectedItem = pi;
        ui.ViewModel.Model.Namespace = ns;
        ui.ViewModel.Model.Folder = folder;
        ui.ViewModel.Model.Project = project;
        ui.ViewModel.Model.NodeItems = pitems;
        //ui.ViewModel.Model.ReferenceTypes = TypeHelper.GetReferenceAfxTypes(project);
        //ui.ViewModel.Model.CodeTypes = TypeHelper.GetCodeAfxTypes(project);



        AfxPackage.ShowDialog(ui);
      }
    }
  }
}
