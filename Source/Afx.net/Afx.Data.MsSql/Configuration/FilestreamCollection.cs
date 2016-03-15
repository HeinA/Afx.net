using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.Configuration
{
  public class FilestreamCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new Filestream();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((Filestream)element).Name;
    }
  }
}
