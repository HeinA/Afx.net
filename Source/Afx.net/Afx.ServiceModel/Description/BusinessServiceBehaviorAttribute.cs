using Afx.ObjectModel.Description.Metadata;
using Afx.ServiceModel.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Description
{
  [AttributeUsage(AttributeTargets.Interface)]
  public class BusinessServiceBehaviorAttribute : Attribute, IContractBehavior
  {
    public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
    {
      if (clientRuntime.MessageInspectors.FirstOrDefault(o => o.GetType() == typeof(BusinessServiceClientMessageInspector)) == null)
        clientRuntime.MessageInspectors.Add(new BusinessServiceClientMessageInspector());
    }

    public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
    {
      if (dispatchRuntime.MessageInspectors.FirstOrDefault(o => o.GetType() == typeof(BusinessServiceDispatchMessageInspector)) == null)
        dispatchRuntime.MessageInspectors.Add(new BusinessServiceDispatchMessageInspector());
    }

    public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
    {
      #region Apply Known Types & Fault Contracts

      foreach (OperationDescription opDesc in endpoint.Contract.Operations)
      {
        foreach (Type t in Metadata.KnownAfxTypes)
        {
          if (!opDesc.KnownTypes.Contains(t)) opDesc.KnownTypes.Add(t);
        }

        //foreach (Type t in KnownTypesProvider.Faults)
        //{
        //  FaultDescription faultDescription = new FaultDescription(string.Format(CultureInfo.CurrentCulture, "{0}/{1}/{2}_{3}", opDesc.DeclaringContract.Namespace, opDesc.DeclaringContract.Name, opDesc.Name, t.Name));
        //  faultDescription.Namespace = KnownTypesProvider.GetFaultNamespace(t);
        //  faultDescription.DetailType = t;
        //  faultDescription.Name = t.Name;
        //  if (opDesc.Faults.FirstOrDefault(fd => fd.Name == t.Name) == null) opDesc.Faults.Add(faultDescription);
        //}
      }

      #endregion
    }
  }
}
