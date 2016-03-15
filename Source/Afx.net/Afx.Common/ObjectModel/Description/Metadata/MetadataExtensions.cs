using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Metadata
{
  public static class MetadataExtensions
  {
    public static Metadata GetMetadata(this Type afxType)
    {
      try
      {
        return Metadata.GetMetadata(afxType);
      }
      catch (System.TypeInitializationException)
      {
        return null;
      }
    }
  }
}
