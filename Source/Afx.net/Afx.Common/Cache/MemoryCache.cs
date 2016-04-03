using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Cache
{
  public class MemoryCache
  {
    ICacheLoader Loader { get; set; }
    Dictionary<Guid, AfxObject> ObjectDictionary { get; set; }
    Dictionary<Type, Collection<AfxObject>> TypeCollectionDictionary { get; set; }

    MemoryCache()
    {
      ObjectDictionary = new Dictionary<Guid, AfxObject>();
      TypeCollectionDictionary = new Dictionary<Type, Collection<AfxObject>>();
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
      PopulateTypeCollection(obj, obj.GetType());

      //foreach (PropertyInfo pi in obj.GetType().GetMetadata().OwnedProperties)
      //{
      //  AfxObject obj1 = pi.GetValue(obj) as AfxObject;
      //  if (obj1 != null) LoadObject(obj1);
      //}

      foreach (PropertyInfo pi in obj.GetType().GetMetadata().CollectionProperties)
      {
        IObjectCollection col = pi.GetValue(obj) as IObjectCollection;
        IAssociativeCollection acol = col as IAssociativeCollection;
        //AssociativeMetadata amd = null;
        //if (acol != null) amd = acol.AssociativeType.GetMetadata() as AssociativeMetadata;
        if (col != null && acol == null) // || (amd != null && amd.IsCompositeReference))
        {
          foreach (AfxObject obj1 in col)
          {
            LoadObject(obj1);
          }
        }
      }
    }

    void PopulateTypeCollection(AfxObject obj, Type objectType)
    {
      if (objectType.GetCustomAttribute<AfxBaseTypeAttribute>() != null) return;
      if (!TypeCollectionDictionary.ContainsKey(objectType)) TypeCollectionDictionary.Add(objectType, new Collection<AfxObject>());
      Collection<AfxObject> col = TypeCollectionDictionary[objectType];
      if (!col.Contains(obj)) col.Add(obj);
      PopulateTypeCollection(obj, objectType.BaseType);
    }

    static MemoryCache Instance { get; set; }
    static object mLock = new object();

    public static void Initialize()
    {
      lock(mLock)
      {
        Instance = new MemoryCache();
      }
    }

    public static AfxObject GetObject(Guid id)
    {
      lock(mLock)
      {
        if (Instance == null) return null;
        if (Instance.ObjectDictionary.ContainsKey(id)) return Instance.ObjectDictionary[id];
        return null;
      }
    }
  }
}
