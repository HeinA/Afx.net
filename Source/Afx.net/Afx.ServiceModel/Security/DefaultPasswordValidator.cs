//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// The <see cref="DefaultPasswordValidator"/>
  /// class provides a default username password validation implementation.
  /// </summary>
  /// <remarks>
  /// The <see cref="UserNamePasswordValidator"/> ensures that a username value has been supplied. No further validation on the username is done.
  /// Any password will pass this validator.
  /// </remarks>
  public class DefaultPasswordValidator : UserNamePasswordValidator
  {
    /// <summary>
    /// Validates the specified username and password.
    /// </summary>
    /// <param name="userName">The username to validate.</param>
    /// <param name="password">The password to validate.</param>
    /// <remarks>
    /// The <see cref="UserNamePasswordValidator"/> ensures that a username value has been supplied. No further validation on the username is done.
    /// Any password will pass this validator.
    /// </remarks>
    /// <exception cref="SecurityTokenException">No username has been supplied.</exception>
    public override void Validate(String userName, String password)
    {
      IPasswordValidator validator = Afx.ComponentModel.Composition.CompositionHelper.GetExportedValueOrDefault<IPasswordValidator>();
      if (validator != null)
      {
        if (!validator.IsValid(userName, password))
        {
          throw new SecurityTokenException(Properties.Resources.InvalidUserNamePassword);
        }
      }
      else
      {
        if (String.IsNullOrEmpty(userName))
        {
          throw new SecurityTokenException(Properties.Resources.NoUserNameProvided);
        }
      }
    }
  }
}
