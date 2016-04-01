using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  public class Cache
  {
    ICacheLoader Loader { get; set; }

    Cache()
    {
      Loader = ComponentModel.Composition.CompositionHelper.GetExportedValueOrDefault<ICacheLoader>();
      if (Loader != null)
      {
        foreach (var obj in Loader.LoadCache())
        {
          LoadObject(obj);
        }
      }
    }

    void LoadObject(AfxObject obj)
    {
    }

    static Cache mInstance;
    public static Cache Instance
    {
      get { return mInstance ?? (mInstance = new Cache()); }
    }
  }
}
