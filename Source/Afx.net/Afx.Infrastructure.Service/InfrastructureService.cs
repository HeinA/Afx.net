using Afx.Data;
using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Afx.Infrastructure.Service
{
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class InfrastructureService : Afx.ServiceModel.BusinessService, Afx.Infrastructure.Service.IInfrastructureService
  {
    #region User

    public ObjectCollection<Afx.Infrastructure.Security.User> LoadUsers()
    {
      return null;
    }

    public ObjectCollection<Afx.Infrastructure.Security.User> SaveUsers(ObjectCollection<Afx.Infrastructure.Security.User> col)
    {
      using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
      using (RepositoryScope repository = new RepositoryScope())
      {
        foreach (var u in col)
        {
          ObjectRepository.GetRepository<Afx.Infrastructure.Security.User>().SaveObject(u);
        }

        return null;
      }
    }


    #endregion
  }
}
