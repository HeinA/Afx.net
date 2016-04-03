using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  public class MemoryCache
  {
    ICacheLoader Loader { get; set; }
    Dictionary<Guid, AfxObject> ObjectDictionary { get; set; }

    MemoryCache()
    {
      ObjectDictionary = new Dictionary<Guid, AfxObject>();
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
      if (ObjectDictionary.ContainsKey(obj.Id)) ObjectDictionary[obj.Id] = obj;
      else ObjectDictionary.Add(obj.Id, obj);
    }

    static MemoryCache Instance { get; set; }
    
    public static void Initialize()
    {
      Instance = new MemoryCache();
    }

    public static AfxObject GetObject(Guid id)
    {
      if (Instance == null) return null;
      if (Instance.ObjectDictionary.ContainsKey(id)) return Instance.ObjectDictionary[id];
      return null;
    }
  }
}
