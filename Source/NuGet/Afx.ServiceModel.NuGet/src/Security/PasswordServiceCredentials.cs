//THANX:  http://www.neovolve.com/2008/04/07/wcf-security-getting-the-password-of-the-user/

using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Threading;

namespace Afx.ServiceModel.Security
{
  /// <summary>
  /// The <see cref="PasswordServiceCredentials"/>
  /// class provides a username password security implementation for WCF services. It creates a custom principal and identity
  /// that exposes both the username and password and makes them available to the service implementation.
  /// </summary>
  /// <remarks>
  /// The custom principal can be obtained (depending on configuration outlined below) via <see cref="Thread.CurrentPrincipal">Thread.CurrentPrincipal</see>.
  /// The identity can also be obtained via <see cref="ServiceSecurityContext.PrimaryIdentity">ServiceSecurityContext.PrimaryIdentity</see>.
  /// </remarks>
  /// <example>
  /// <para>
  /// This example uses netTcpBinding. Given that this implementation leverages username password credentials, the client credential type is set to UserName.
  /// As username and password information is being transferred over the wire in plain text transport security with message credentials 
  /// is required by WCF to secure the information. 
  /// </para> 
  /// <para>
  /// The service behavior defines the custom ServiceCredentials type to use. As this example is a self-hosted service rather than an IIS configuration,
  /// the server x509 certificate is defined so that transport security can be provided. Username password validation is defined as custom, but no validator
  /// type is specified. This results in the <see cref="DefaultPasswordValidator"/> being used. Finally, the service authorization principal permission
  /// mode is defined as custom. This ensures that the <see cref="PasswordPrincipal"/> created is injected into
  /// <see cref="Thread.CurrentPrincipal">Thread.CurrentPrincipal</see> of the service call.
  /// </para>
  /// <code lang="xml" title="Service configuration">
  ///     &lt;?xml version="1.0" encoding="utf-8" ?&gt;
  ///     &lt;configuration&gt;
  ///         &lt;system.serviceModel&gt;
  ///             &lt;bindings&gt;
  ///                 &lt;netTcpBinding&gt;
  ///                     &lt;binding name="netTcpBindingConfig"&gt;
  ///                         &lt;security mode="TransportWithMessageCredential"&gt;
  ///                             &lt;message clientCredentialType="UserName" /&gt;
  ///                         &lt;/security&gt;
  ///                     &lt;/binding&gt;
  ///                 &lt;/netTcpBinding&gt;
  ///             &lt;/bindings&gt;
  ///             &lt;client&gt;
  ///                 &lt;endpoint address="net.tcp://localhost:8792/PasswordSecurityTest"
  ///                     binding="netTcpBinding" bindingConfiguration="netTcpBindingConfig"
  ///                     contract="Neovolve.Framework.Communication.SecurityHost.IService1"
  ///                     name="PasswordSecurityTest"&gt;
  ///                 &lt;/endpoint&gt;
  ///             &lt;/client&gt;
  ///           &lt;services&gt;
  ///             &lt;service behaviorConfiguration="Neovolve.Framework.Communication.SecurityTest.Service1Behavior"
  ///                      name="Neovolve.Framework.Communication.SecurityHost.Service1"&gt;
  ///               &lt;endpoint address="net.tcp://localhost:8792/PasswordSecurityTest"
  ///                         binding="netTcpBinding"
  ///                         bindingConfiguration="netTcpBindingConfig"
  ///                         contract="Neovolve.Framework.Communication.SecurityHost.IService1" /&gt;
  ///             &lt;/service&gt;
  ///           &lt;/services&gt;
  ///           &lt;behaviors&gt;
  ///             &lt;serviceBehaviors&gt;
  ///               &lt;behavior name="Neovolve.Framework.Communication.SecurityTest.Service1Behavior"&gt;
  ///                 &lt;serviceDebug includeExceptionDetailInFaults="true" /&gt;
  ///                 &lt;serviceCredentials type="Neovolve.Framework.Communication.Security.PasswordServiceCredentials, Neovolve.Framework.Communication.Security"&gt;
  ///                   &lt;serviceCertificate findValue="localhost"
  ///                                       x509FindType="FindBySubjectName" /&gt;
  ///                   &lt;userNameAuthentication userNamePasswordValidationMode="Custom" /&gt;
  ///                 &lt;/serviceCredentials&gt;
  ///                 &lt;serviceAuthorization principalPermissionMode="Custom" /&gt;
  ///               &lt;/behavior&gt;
  ///             &lt;/serviceBehaviors&gt;
  ///           &lt;/behaviors&gt;
  ///         &lt;/system.serviceModel&gt;
  ///     &lt;/configuration&gt;
  /// </code>
  /// </example>
  public class PasswordServiceCredentials : ServiceCredentials
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordServiceCredentials"/> class.
    /// </summary>
    public PasswordServiceCredentials()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordServiceCredentials"/> class.
    /// </summary>
    /// <param name="clone">The clone.</param>
    private PasswordServiceCredentials(PasswordServiceCredentials clone)
        : base(clone)
    {
    }

    /// <summary>
    /// Creates a token manager for this service.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.ServiceModel.Security.ServiceCredentialsSecurityTokenManager"/> instance.
    /// </returns>
    public override SecurityTokenManager CreateSecurityTokenManager()
    {
      // Check if the current validation mode is for custom username password validation
      if (UserNameAuthentication.UserNamePasswordValidationMode
          == UserNamePasswordValidationMode.Custom)
      {
        return new PasswordSecurityTokenManager(this);
      }

      Trace.TraceWarning(Properties.Resources.CustomUserNamePasswordValidationNotEnabled);

      return base.CreateSecurityTokenManager();
    }

    /// <summary>
    /// Copies the essential members of the current instance.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.ServiceModel.Description.ServiceCredentials"/> instance.
    /// </returns>
    protected override ServiceCredentials CloneCore()
    {
      return new PasswordServiceCredentials(this);
    }
  }
}
