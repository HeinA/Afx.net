//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// Authenticates a System.IdentityModel.Tokens.UserNameSecurityToken security token using a custom authentication scheme.
  /// </summary>
  internal class PasswordSecurityTokenAuthenticator : CustomUserNameSecurityTokenAuthenticator
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordSecurityTokenAuthenticator"/> class.
    /// </summary>
    /// <param name="validator">A <see cref="T:System.IdentityModel.Selectors.UserNamePasswordValidator"/>  that authenticates the username and password using a custom authentication scheme.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="validator"/> is null.</exception>
    public PasswordSecurityTokenAuthenticator(UserNamePasswordValidator validator)
        : base(validator)
    {
    }

    /// <summary>
    /// Authenticates the specified user name and password and returns the set of authorization policies for <see cref="T:System.IdentityModel.Tokens.UserNameSecurityToken"/> security tokens.
    /// </summary>
    /// <param name="userName">The user name associated with the security token.</param>
    /// <param name="password">The password associated with the security token.</param>
    /// <returns>
    /// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> of type <see cref="T:System.IdentityModel.Policy.IAuthorizationPolicy"/> that contains the set of authorization policies in effect for this application.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="userName"/> is null.</exception>
    /// <exception cref="T:System.IdentityModel.Tokens.SecurityTokenValidationException">
    /// 	<paramref name="userName"/> and <paramref name="password"/> combination are not valid.</exception>
    protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(
        String userName, String password)
    {
      // Get the existing policies
      ReadOnlyCollection<IAuthorizationPolicy> currentPolicies = base.ValidateUserNamePasswordCore(
          userName, password);
      List<IAuthorizationPolicy> newPolicies = new List<IAuthorizationPolicy>();

      // Check if there are existing policies to add
      if (currentPolicies != null)
      {
        // Add the existing policies
        newPolicies.AddRange(currentPolicies);
      }

      // Add the custom authorization policy
      newPolicies.Add(new PasswordAuthorizationPolicy(userName, password));

      return newPolicies.AsReadOnly();
    }
  }
}
