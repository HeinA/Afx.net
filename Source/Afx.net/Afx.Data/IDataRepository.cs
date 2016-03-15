using Afx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public interface IDataRepository
  {
    AfxObject SaveObject(AfxObject obj, ObjectRepository objectRepository);
  }
}
