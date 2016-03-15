//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System;
using System.Security.Principal;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// The <see cref="PasswordIdentity"/>
  /// class provides information about an identity defined by a username and password.
  /// </summary>
  public class PasswordIdentity : GenericIdentity
  {
    /// <summary>
    /// Stores the IsAuthenticated value.
    /// </summary>
    private readonly Boolean _isAuthenticated;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordIdentity"/> class.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="password">The password.</param>
    /// <param name="isAuthenticated">if set to <c>true</c> the identity is authenticated.</param>
    /// <param name="authenticationType">Type of the authentication.</param>
    public PasswordIdentity(String userName, String password, Boolean isAuthenticated, String authenticationType)
        : base(userName, authenticationType)
    {
      Password = password;
      _isAuthenticated = isAuthenticated;
    }

    /// <summary>
    /// Gets a value that indicates whether the user has been authenticated.
    /// </summary>
    /// <value></value>
    /// <returns>true if the user was authenticated; otherwise, false.</returns>
    public override Boolean IsAuthenticated
    {
      get
      {
        return (_isAuthenticated && base.IsAuthenticated);
      }
    }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>The password.</value>
    public String Password
    {
      get;
      private set;
    }
  }
}
