﻿using Afx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Cache
{
  public interface ICacheLoader
  {
    IEnumerable<AfxObject> LoadCache();
  }
}
