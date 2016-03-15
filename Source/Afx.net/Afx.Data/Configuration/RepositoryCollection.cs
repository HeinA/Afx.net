using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.Configuration
{
  public class RepositoryCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new Repository();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((Repository)element).RepositoryType;
    }
  }
}
