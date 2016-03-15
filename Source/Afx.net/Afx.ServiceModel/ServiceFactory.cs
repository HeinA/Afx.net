using Afx.ServiceModel;
using Afx.ServiceModel.Description;
using Afx.ServiceModel.Security;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ServiceModel
{
  public class ServiceFactory
  {
    public static IDisposableWrapper<T> GetService<T>(string server)
      where T : class //, Afx.ServiceModel.IBusinessService
    {
      if (server.Substring(server.Length - 1) == "/") return GetService<T>(new System.Uri(server));
      return GetService<T>(new System.Uri(string.Format("{0}/", server)));
    }

    public static IDisposableWrapper<T> GetService<T>(System.Uri server)
      where T : class
    {
      string username = null;
      string password = null;

      if (ServiceMetadata.GetMetadata<T>().IsSecure)
      {
        PasswordIdentity pi = Thread.CurrentPrincipal.Identity as PasswordIdentity;
        if (pi != null)
        {
          username = pi.Name;
          password = pi.Password;
        }
      }

      return GetService<T>(server, username, password);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static IDisposableWrapper<T> GetService<T>(System.Uri server, string username, string password)
      where T : class 
    {
      System.Uri service = new System.Uri(server, typeof(T).FullName);
      System.ServiceModel.Channels.CustomBinding binding = null;

      if (ServiceMetadata.GetMetadata<T>().IsSecure)
      {
        var wsB = new System.ServiceModel.WSHttpBinding() { MaxReceivedMessageSize = 5242880 };
        wsB.Security.Mode = System.ServiceModel.SecurityMode.TransportWithMessageCredential;
        wsB.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.UserName;
        binding = new System.ServiceModel.Channels.CustomBinding(wsB);
      }
      else
      {
        var basicB = new System.ServiceModel.BasicHttpBinding() { MaxReceivedMessageSize = 5242880 };
        binding = new System.ServiceModel.Channels.CustomBinding(basicB);
      }

      if (ServiceMetadata.GetMetadata<T>().IsSecure)
      {
        binding.Elements.Remove(binding.Elements.Find<System.ServiceModel.Channels.TextMessageEncodingBindingElement>());
        binding.Elements.Insert(0, new System.ServiceModel.Channels.BinaryMessageEncodingBindingElement() { CompressionFormat = System.ServiceModel.Channels.CompressionFormat.GZip });
      }

      var endpoint = new System.ServiceModel.EndpointAddress(service);
      var factory = new System.ServiceModel.ChannelFactory<T>(binding, endpoint);

      if (ServiceMetadata.GetMetadata<T>().IsSecure)
      {
        var credentialBehaviour = factory.Endpoint.Behaviors.Find<System.ServiceModel.Description.ClientCredentials>();
        credentialBehaviour.UserName.UserName = username;
        credentialBehaviour.UserName.Password = password;
      }

      factory.Open();
      IDisposableWrapper<T> wrapper = WCFExtensions.Wrap<T>(factory);
      return wrapper;
    }
  }

  public interface IDisposableWrapper<T> : System.IDisposable
  {
    T Instance { get; }
  }

  public class DisposableWrapper<T> : IDisposableWrapper<T> where T : class
  {
    public T Instance { get; private set; }
    protected System.ServiceModel.ChannelFactory<T> Factory { get; set; }

    public DisposableWrapper(System.ServiceModel.ChannelFactory<T> factory)
    {
      Factory = factory;
      CreateInstance();
    }

    void CreateInstance()
    {
      Instance = Factory.CreateChannel();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        (Instance as System.IDisposable).Dispose();
      }
    }

    public void Dispose()
    {
      try
      {
        Dispose(true);
        System.GC.SuppressFinalize(this);
      }
      catch { }

      Instance = default(T);
    }
  }

  public class ClientWrapper<TProxy> : DisposableWrapper<TProxy>
      where TProxy : class
  {
    public ClientWrapper(System.ServiceModel.ChannelFactory<TProxy> proxy) : base(proxy) { }
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (this.Instance != null)
        {
          System.ServiceModel.IClientChannel channel = this.Instance as System.ServiceModel.IClientChannel;
          if (channel.State == System.ServiceModel.CommunicationState.Faulted)
          {
            channel.Abort();
          }
          else
          {
            channel.Close();
          }
        }
      }

      base.Dispose(disposing);
    }
  }

  static class WCFExtensions
  {
    public static IDisposableWrapper<TProxy> Wrap<TProxy>(
        this System.ServiceModel.ChannelFactory<TProxy> proxy)
        where TProxy : class
    {
      return new ClientWrapper<TProxy>(proxy);
    }
  }
}
