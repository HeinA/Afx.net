using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.Configuration
{
  public class FileGroupCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new FileGroup();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((FileGroup)element).Name;
    }
  }
}
