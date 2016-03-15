using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.Configuration
{
  public class FileGroup : ConfigurationElement
  {
    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    [ConfigurationProperty("repository", IsRequired = true)]
    public string Repository
    {
      get { return (string)this["repository"]; }
      set { this["repository"] = value; }
    }

    [ConfigurationProperty("files", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(FilestreamCollection), AddItemName = "file")]
    public FilestreamCollection Files
    {
      get { return (FilestreamCollection)base["files"]; }
    }
  }
}
