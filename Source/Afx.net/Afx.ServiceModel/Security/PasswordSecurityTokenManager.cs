//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// Represents a System.IdentityModel.Selectors.SecurityTokenManager implementation that provides security token serializers based on the 
  /// System.ServiceModel.Description.ServiceCredentials configured on the service.
  /// </summary>
  internal class PasswordSecurityTokenManager : ServiceCredentialsSecurityTokenManager
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordSecurityTokenManager"/> class.
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    public PasswordSecurityTokenManager(PasswordServiceCredentials credentials)
        : base(credentials)
    {
    }

    /// <summary>
    /// Creates a security token authenticator based on the <see cref="T:System.IdentityModel.Selectors.SecurityTokenRequirement"/>.
    /// </summary>
    /// <param name="tokenRequirement">The <see cref="T:System.IdentityModel.Selectors.SecurityTokenRequirement"/>.</param>
    /// <param name="outOfBandTokenResolver">When this method returns, contains a <see cref="T:System.IdentityModel.Selectors.SecurityTokenResolver"/>. This parameter is passed uninitialized.</param>
    /// <returns>
    /// The <see cref="T:System.IdentityModel.Selectors.SecurityTokenAuthenticator"/>.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="tokenRequirement"/> is null.</exception>
    /// <exception cref="T:System.NotSupportedException">A security token authenticator cannot be created for the<paramref name=" tokenRequirement"/> that was passed in.</exception>
    public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(
        SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
    {
      if (tokenRequirement.TokenType
          == SecurityTokenTypes.UserName)
      {
        outOfBandTokenResolver = null;

        // Get the current validator
        UserNamePasswordValidator validator =
            ServiceCredentials.UserNameAuthentication.CustomUserNamePasswordValidator;

        // Ensure that a validator exists
        if (validator == null)
        {
          Trace.TraceWarning(Properties.Resources.NoCustomUserNamePasswordValidatorConfigured);

          validator = new DefaultPasswordValidator();
        }

        return new PasswordSecurityTokenAuthenticator(validator);
      }

      return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
    }
  }
}
