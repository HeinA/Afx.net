using Afx.ServiceModel.Description;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Activation
{
  //WALKTHROUGH: Server Step 02
  //When a service host is created, apply the default Afx Binding Elements to the endpoints
  public class BusinessServiceHost : System.ServiceModel.ServiceHost
  {
    public BusinessServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, baseAddresses)
    {
    }

    public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
    {
      ReadOnlyCollection<ServiceEndpoint> col = base.AddDefaultEndpoints();

      foreach (var ep in col.Where(ep1 => ep1.Binding is BasicHttpBinding))
      {
        Type ct = ep.Contract.ContractType;
        Binding binding = null;
        Uri uri = null;
        if (ServiceMetadata.GetMetadata(ct).IsSecure)
        {
          var wsB = new WSHttpBinding() { MaxReceivedMessageSize = 5242880 };
          wsB.Security.Mode = SecurityMode.TransportWithMessageCredential;
          wsB.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
          binding = wsB;
          uri = new Uri(ep.ListenUri.ToString().Replace("http://", "https://"));
        }
        else
        {
          var basicB = new BasicHttpBinding() { MaxReceivedMessageSize = 5242880 };
          binding = basicB;
        }

        CustomBinding cb = new CustomBinding(binding);
        if (ServiceMetadata.GetMetadata(ct).IsCompressed)
        {
          cb.Elements.Remove(cb.Elements.Find<TextMessageEncodingBindingElement>());
          cb.Elements.Insert(0, new BinaryMessageEncodingBindingElement() { CompressionFormat = CompressionFormat.GZip });
        }
        ep.Binding = cb;
        if (uri != null)
        {
          ep.ListenUri = uri;
          ep.Address = new EndpointAddress(ep.ListenUri);
        }
      }

      return col;
    }
  }
}
