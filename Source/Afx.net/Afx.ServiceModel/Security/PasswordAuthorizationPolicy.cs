//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// The <see cref="PasswordAuthorizationPolicy"/>
  /// class is used to create a <see cref="PasswordPrincipal"/> and <see cref="PasswordIdentity"/> objects
  /// based on the username and password specified.
  /// </summary>
  /// <remarks>
  /// The <see cref="PasswordPrincipal"/> and <see cref="PasswordIdentity"/> created are stored so that they will be available
  /// to the service via <see cref="ServiceSecurityContext.Current">ServiceSecurityContext.Current</see> and (depending on configuration)
  /// <see cref="Thread.CurrentPrincipal">Thread.CurrentPrincipal</see>.
  /// </remarks>
  internal class PasswordAuthorizationPolicy : IAuthorizationPolicy
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordAuthorizationPolicy"/> class.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="password">The password.</param>
    /// <exception cref="ArgumentNullException"><b>null</b> or empty <paramref name="userName"/> was provided</exception>
    public PasswordAuthorizationPolicy(String userName, String password)
    {
      const String UserNameParameterName = "userName";

      if (String.IsNullOrEmpty(userName))
      {
        throw new ArgumentNullException(UserNameParameterName);
      }

      Id = Guid.NewGuid().ToString();
      Issuer = ClaimSet.System;
      UserName = userName;
      Password = password;
    }

    /// <summary>
    /// Evaluates whether a user meets the requirements for this authorization policy.
    /// </summary>
    /// <param name="evaluationContext">An <see cref="T:System.IdentityModel.Policy.EvaluationContext"/> that contains the claim set that the authorization policy evaluates.</param>
    /// <param name="state">A <see cref="T:System.Object"/>, passed by reference that represents the custom state for this authorization policy.</param>
    /// <returns>
    /// false if the <see cref="M:System.IdentityModel.Policy.IAuthorizationPolicy.Evaluate(System.IdentityModel.Policy.EvaluationContext,System.Object@)"/> method for this authorization policy must be called if additional claims are added by other authorization policies to <paramref name="evaluationContext"/>; otherwise, true to state no additional evaluation is required by this authorization policy.
    /// </returns>
    public bool Evaluate(EvaluationContext evaluationContext, ref object state)
    {
      const String IdentitiesKey = "Identities";

      // Check if the properties of the context has the identities list
      if (evaluationContext.Properties.Count == 0
          || evaluationContext.Properties.ContainsKey(IdentitiesKey) == false
          || evaluationContext.Properties[IdentitiesKey] == null)
      {
        return false;
      }

      // Get the identities list
      List<IIdentity> identities = evaluationContext.Properties[IdentitiesKey] as List<IIdentity>;

      // Validate that the identities list is valid
      if (identities == null)
      {
        return false;
      }

      // Get the current identity
      IIdentity currentIdentity =
          identities.Find(
              identityMatch =>
              identityMatch is GenericIdentity
              && String.Equals(identityMatch.Name, UserName, StringComparison.OrdinalIgnoreCase));

      // Check if an identity was found
      if (currentIdentity == null)
      {
        return false;
      }

      PasswordIdentity newIdentity = null;
      PasswordPrincipal newPrincipal = null;

      IPrincipalProvider provider = Afx.ComponentModel.Composition.CompositionHelper.GetExportedValueOrDefault<IPrincipalProvider>();
      if (provider != null)
      {
        newPrincipal = provider.GetPrincipal(UserName, Password);
        newIdentity = newPrincipal.Identity as PasswordIdentity;
      }
      else
      {
        newIdentity = new PasswordIdentity(UserName, Password, currentIdentity.IsAuthenticated, currentIdentity.AuthenticationType);
        newPrincipal = new PasswordPrincipal(newIdentity, null);
      }

      const String PrimaryIdentityKey = "PrimaryIdentity";

      // Update the list and the context with the new identity
      identities.Remove(currentIdentity);
      identities.Add(newIdentity);
      evaluationContext.Properties[PrimaryIdentityKey] = newIdentity;

      const String PrincipalKey = "Principal";

      // Store the new principal in the context
      evaluationContext.Properties[PrincipalKey] = newPrincipal;

      // This policy has successfully been evaluated and doesn't need to be called again
      return true;
    }

    /// <summary>
    /// Gets a string that identifies this authorization component.
    /// </summary>
    /// <value></value>
    /// <returns>A string that identifies this authorization component.</returns>
    public String Id
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets a claim set that represents the issuer of the authorization policy.
    /// </summary>
    /// <value></value>
    /// <returns>A <see cref="T:System.IdentityModel.Claims.ClaimSet"/> that represents the issuer of the authorization policy.</returns>
    public ClaimSet Issuer
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>The password.</value>
    private String Password
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    /// <value>The name of the user.</value>
    private String UserName
    {
      get;
      set;
    }
  }
}
