//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System;
using System.Security.Principal;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// The <see cref="PasswordPrincipal"/>
  /// class provides information about the roles available to the <see cref="PasswordIdentity"/> that it exposes.
  /// </summary>
  public class PasswordPrincipal : GenericPrincipal
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordPrincipal"/> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="roles">The roles.</param>
    public PasswordPrincipal(PasswordIdentity identity, String[] roles)
        : base(identity, roles)
    {
    }
  }
}
