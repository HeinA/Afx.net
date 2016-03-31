using Afx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql
{
  class LoadContext
  {
    Dictionary<Guid, AfxObject> mObjectDictionary = new Dictionary<Guid, AfxObject>();

    public void RegisterObject(AfxObject obj)
    {
      if (!mObjectDictionary.ContainsKey(obj.Id)) mObjectDictionary.Add(obj.Id, obj);
    }

    public bool IsRegistered(Guid id)
    {
      return mObjectDictionary.ContainsKey(id);
    }

    public AfxObject GetObject(Guid id)
    {
      if (!mObjectDictionary.ContainsKey(id)) return null;
      return mObjectDictionary[id];
    }
  }
}
