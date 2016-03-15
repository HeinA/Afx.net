using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Security
{
  public interface IPrincipalProvider
  {
    PasswordPrincipal GetPrincipal(string userName, string password);
  }
}
