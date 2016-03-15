using Afx.ObjectModel;
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
  public class User : AfxObject, IAggregateRoot
  {
    #region string UserName

    public const string UserNameProperty = "UserName";
    string mUserName;
    [DataMember(EmitDefaultValue = false)]
    [PersistentProperty]
    public string UserName
    {
      get { return mUserName; }
      set { SetProperty<string>(ref mUserName, value); }
    }

    #endregion

    #region AssociativeCollection<Role, UserRole> Roles

    public const string RolesProperty = "Roles";
    Afx.ObjectModel.Collections.AssociativeCollection<Role, UserRole> mRoles;
    [PersistentProperty]
    [DataMember]
    public Afx.ObjectModel.Collections.AssociativeCollection<Role, UserRole> Roles
    {
      get { return GetAssociativeCollection<Role, UserRole>(ref mRoles); }
    }

    #endregion

    #region IAggregateRoot

    public int Revision { get; set; }
    public DateTime? RevisionDate { get; set; }

    #endregion
  }
}
  