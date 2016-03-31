using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public class RepositoryScope : IDisposable
  {
    const string RepositoryNameKey = "ConnectionName";
    const string DefaultRepositoryName = "Default";

    public RepositoryScope()
    {
      CallContext.LogicalSetData(RepositoryNameKey, DefaultRepositoryName);
    }

    public RepositoryScope(string repositoryName)
    {
      CallContext.LogicalSetData(RepositoryNameKey, repositoryName);
    }

    public void Dispose()
    {
      CallContext.FreeNamedDataSlot(RepositoryNameKey);
    }

    public static string ConnectionName
    {
      get { return (string)CallContext.LogicalGetData(RepositoryNameKey) ?? DefaultRepositoryName; }
    }
  }
}
