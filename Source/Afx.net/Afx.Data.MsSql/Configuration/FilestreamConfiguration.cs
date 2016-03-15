using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.Configuration
{
  public class FilestreamConfiguration : ConfigurationSection
  {
    public const string SectionName = "filestreamConfiguration";

    public static FilestreamConfiguration Default
    {
      get { return ConfigurationManager.GetSection(FilestreamConfiguration.SectionName) as FilestreamConfiguration; }
    }

    [ConfigurationProperty("groups", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(FileGroupCollection), AddItemName = "group")]
    public FileGroupCollection Groups
    {
      get { return (FileGroupCollection)base["groups"]; }
    }
  }
}
