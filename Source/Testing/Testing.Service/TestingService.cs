using Afx.ServiceModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Testing.Services;

namespace Testing.Service
{
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class TestingService : BusinessService, ITestingService
  {
    //[PrincipalPermission(SecurityAction.Demand, Role = "AAA")]
    public Collection<Invoice> Test(Collection<Invoice> invoices)
    {
      return invoices;
    }
  }
}
