using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  public interface IAfxObject : INotifyPropertyChanged
  {
    Guid Id { get; }
    IAfxObject Owner { get; set; }
  }
}
