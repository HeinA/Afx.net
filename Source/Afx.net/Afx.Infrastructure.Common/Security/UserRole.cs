using Afx.ObjectModel;
using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Infrastructure.Security
{
	[DataContract(Namespace = Constants.WcfNamespace, IsReference = true)]
	[PersistentObject]  
  public class UserRole : AssociativeObject<Afx.Infrastructure.Security.User, Afx.Infrastructure.Security.Role>
  {
  }
}
  