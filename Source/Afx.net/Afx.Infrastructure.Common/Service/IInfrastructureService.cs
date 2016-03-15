using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Infrastructure.Service
{
  [SecureService]
  [CompressMessage]
  [ServiceContract(Namespace = Afx.Constants.WcfNamespace)]
  [BusinessServiceBehavior]
  public interface IInfrastructureService
  {

    #region User

    ObjectCollection<Afx.Infrastructure.Security.User> LoadUsers();
    ObjectCollection<Afx.Infrastructure.Security.User> SaveUsers(ObjectCollection<Afx.Infrastructure.Security.User> col);


    #endregion
  }
}
