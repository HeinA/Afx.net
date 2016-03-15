using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix
{
  [PackageRegistration(UseManagedResourcesOnly = true)]
  //[ProvideObject(typeof(CustomPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
  [ProvideProjectFactory(typeof(ServiceLibraryPropertyPageProjectFactory), "Task Project", null, null, null, @"..\Templates\Projects")]
  [Guid(AfxClassLibraryPackageString)]
  public class AfxServiceLibraryPackage : Package
  {
    public const string AfxClassLibraryPackageString = "2DA43B8C-A78A-4C4A-9A93-251B4998F548";

    public AfxServiceLibraryPackage()
    {
    }
  }
}
