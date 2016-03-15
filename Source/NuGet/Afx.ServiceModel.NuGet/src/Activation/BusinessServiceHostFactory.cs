using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Afx.ServiceModel.Activation
{
  public class BusinessServiceHostFactory : System.ServiceModel.Activation.ServiceHostFactory
  {
    /// <summary>
    /// Initializes All BusinessService Types, adding HTTP/S routes for them in the application passing BusinessServiceHostFactory as the default factory.
    /// </summary>
    public static void Initialize()
    {
      AssemblyHelper.PreLoadDeployedAssemblies();
      BusinessServiceHostFactory factory = new BusinessServiceHostFactory();

      foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
      {
        foreach (Type t in a.GetTypes().Where(t => t.IsSubclassOf(typeof(BusinessService))))
        {
          Type i = t.GetInterfaces().Where(i1 => i1.GetCustomAttribute<ServiceContractAttribute>() != null).FirstOrDefault();
          RouteTable.Routes.Add(new ServiceRoute(i.FullName, factory, t));
        }
      }
    }

    /// <summary>
    /// Create a BusinessServiceHost for the specified type.
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="baseAddresses"></param>
    /// <returns></returns>
    protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      return new BusinessServiceHost(serviceType, baseAddresses);
    }
  }
}
