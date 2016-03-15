using Afx.ServiceModel;
using Afx.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Services
{
  [SecureService]
  [CompressMessage]
  [BusinessServiceBehavior]
  [ServiceContract(Namespace = Constants.WcfNamespace)]
  public interface ITestingService
  {
    [OperationContract]
    Collection<Invoice> Test(Collection<Invoice> invoices);
  }
}
