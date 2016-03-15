using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Afx
{
  internal static class AssemblyHelper
  {
    #region void PreLoadDeployedAssemblies()

    static bool mAssembliesLoaded = false;
    public static void PreLoadDeployedAssemblies()
    {
      if (mAssembliesLoaded) return;
      foreach (var path in GetBinFolders())
      {
        PreLoadAssembliesFromPath(path);
      }
      mAssembliesLoaded = true;
    }

    private static IEnumerable<string> GetBinFolders()
    {
      List<string> toReturn = new List<string>();
      if (HttpContext.Current != null)
      {
        toReturn.Add(HttpRuntime.BinDirectory);
      }
      else
      {
        toReturn.Add(AppDomain.CurrentDomain.BaseDirectory);
      }

      return toReturn;
    }

    private static void PreLoadAssembliesFromPath(string p)
    {
      FileInfo[] files = null;
      files = new DirectoryInfo(p).GetFiles("*.dll", SearchOption.AllDirectories);

      AssemblyName a = null;
      string s = null;
      foreach (var fi in files)
      {
        s = fi.FullName;
        a = AssemblyName.GetAssemblyName(s);
        if (!AppDomain.CurrentDomain.GetAssemblies().Any(assembly => AssemblyName.ReferenceMatchesDefinition(a, assembly.GetName())))
        {
          Assembly.Load(a);
        }
      }
    }

    #endregion

    #region Type GetGenericSubClass(...)

    public static Type GetGenericSubClass(Type generic, Type toCheck)
    {
      while (toCheck != null && toCheck != typeof(object))
      {
        var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        if (generic == cur)
        {
          return toCheck;
        }
        toCheck = toCheck.BaseType;
      }
      return null;
    }

    #endregion
  }
}
