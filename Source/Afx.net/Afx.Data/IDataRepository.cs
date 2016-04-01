using Afx.Data.Configuration;
using Afx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public interface IDataRepository
  {
    void Initialize(Repository repositoryConfiguration);
    AfxObject LoadObject(Guid id, ObjectRepository objectRepository);
    IEnumerable<AfxObject> LoadObjects(ObjectRepository objectRepository);
    AfxObject SaveObject(AfxObject obj, ObjectRepository objectRepository);
  }
}
