using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.Configuration
{
  public class RepositoryConfiguration : ConfigurationSection
  {
    public const string SectionName = "repositoryConfiguration";

    public static RepositoryConfiguration Default
    {
      get { return ConfigurationManager.GetSection(RepositoryConfiguration.SectionName) as RepositoryConfiguration; }
    }

    [ConfigurationProperty("repositories", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(RepositoryCollection), AddItemName = "repository")]
    public RepositoryCollection Repositories
    {
      get { return (RepositoryCollection)base["repositories"]; }
    }
  }
}
