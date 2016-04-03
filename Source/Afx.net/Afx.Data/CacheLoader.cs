using Afx.Cache;
using Afx.ObjectModel;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  [Export(typeof(ICacheLoader))]
  class CacheLoader : ICacheLoader
  {
    public IEnumerable<AfxObject> LoadCache()
    {
      foreach (var or in Metadata.OrderByDependencies(
        Metadata.KnownAfxTypes.Select(kt => ObjectRepository.GetRepository(kt))
        .Where(or1 => or1 != null && or1.IsCached)
        .Select(or1 => or1.SourceType))
        .Select(kt => ObjectRepository.GetRepository(kt)))
      {
        foreach (var obj in or.LoadObjects())
        {
          yield return obj;
        }
      }
    }
  }
}
