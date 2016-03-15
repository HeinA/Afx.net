using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.Configuration
{
  public class Filestream : ConfigurationElement
  {
    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    [ConfigurationProperty("folder")]
    public string Folder
    {
      get { return (string)this["folder"]; }
      set { this["folder"] = value; }
    }
  }
}
