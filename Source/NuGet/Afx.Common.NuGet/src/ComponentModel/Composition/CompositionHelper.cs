using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Afx.ComponentModel.Composition
{
  public static class CompositionHelper
  {
    static CompositionContainer mCompositionContainer;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static CompositionHelper()
    {
      string s = null;
      if (!string.IsNullOrWhiteSpace(HttpRuntime.AppDomainAppVirtualPath))
      {
        s = HttpRuntime.BinDirectory;
      }
      else
      {
        s = AppDomain.CurrentDomain.BaseDirectory;
      }

      DirectoryCatalog dc1 = new DirectoryCatalog(s);
      DirectoryCatalog dc2 = new DirectoryCatalog(s, "*.exe");
      AggregateCatalog ag = new AggregateCatalog(dc1, dc2);

      string folder = string.Format(CultureInfo.InvariantCulture, "{0}{1}", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"\Afx.net");
      DirectoryInfo di = new DirectoryInfo(folder);
      if (di.Exists)
      {
        DirectoryCatalog dc3 = new DirectoryCatalog(folder);
        ag.Catalogs.Add(dc3);
      }

      mCompositionContainer = new CompositionContainer(ag);
    }

    public static T GetExportedValue<T>()
    {
      return mCompositionContainer.GetExportedValue<T>();
    }

    public static T GetExportedValue<T>(string contractName)
    {
      return mCompositionContainer.GetExportedValue<T>(contractName);
    }

    public static T GetExportedValueOrDefault<T>()
    {
      return mCompositionContainer.GetExportedValueOrDefault<T>();
    }

    public static object GetExportedValueOrDefault(Type type)
    {
      MethodInfo mi = mCompositionContainer.GetType().GetMethod("GetExportedValueOrDefault", new Type[] { }).MakeGenericMethod(type);
      object o = mi.Invoke(mCompositionContainer, new object[] { });
      return o;
    }

    public static IEnumerable GetExportedValues(Type type)
    {
      MethodInfo mi = mCompositionContainer.GetType().GetMethod("GetExportedValues", new Type[] { }).MakeGenericMethod(type);
      IEnumerable e = (IEnumerable)mi.Invoke(mCompositionContainer, new object[] { });
      return e;
    }
  }
}
