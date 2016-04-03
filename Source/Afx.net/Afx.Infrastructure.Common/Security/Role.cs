using Afx.ObjectModel;
using Afx.ObjectModel.Description.Data;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Infrastructure.Security
{
  [DataContract(Namespace = Constants.WcfNamespace, IsReference = true)]
  [PersistentObject(IsCached = true)]
  public class Role : AfxObject<Role>
  {
    #region string Name

    public const string NameProperty = "Name";
    string mName;
    [DataMember]
    [Mandatory("Name is mandatory")]
    [PersistentProperty(Size = 200)]
    public string Name
    {
      get { return mName; }
      set { SetProperty<string>(ref mName, value); }
    }

    #endregion

    #region ObjectCollection<Role> SubRoles

    public const string SubRolesProperty = "SubRoles";
    Afx.ObjectModel.Collections.ObjectCollection<Role> mSubRoles;
    [PersistentProperty]
    [DataMember]
    public Afx.ObjectModel.Collections.ObjectCollection<Role> SubRoles
    {
      get { return GetObjectCollection<Role>(ref mSubRoles); }
    }

    #endregion
  }
}
