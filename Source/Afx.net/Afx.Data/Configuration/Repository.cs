using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.Configuration
{
  public class Repository : ConfigurationElement
  {
    [ConfigurationProperty("type", IsRequired = true)]
    [TypeConverter(typeof(TypeNameConverter))]
    public Type RepositoryType
    {
      get { return (Type)this["type"]; } 
      set { this["type"] = value; }
    }

    [ConfigurationProperty("name", IsRequired = true)]
    public string Name
    {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    [ConfigurationProperty("connectionString", IsRequired = true)]
    public string ConnectionString
    {
      get { return (string)this["connectionString"]; }
      set { this["connectionString"] = value; }
    }

    [ConfigurationProperty("validateSchema", IsRequired = false, DefaultValue = false)]
    public bool ValidateSchema
    {
      get { return (bool)this["validateSchema"]; }
      set { this["validateSchema"] = value; }
    }
  }
}
