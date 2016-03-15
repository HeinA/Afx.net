using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  public interface IAssociativeObject : IAfxObject
  {
    IAfxObject Reference { get; set; }
  }
}
