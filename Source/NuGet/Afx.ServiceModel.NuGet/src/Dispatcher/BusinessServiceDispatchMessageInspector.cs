using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Dispatcher
{
  public class BusinessServiceDispatchMessageInspector : IDispatchMessageInspector
  {
    public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
    {
      return null;
    }

    public void BeforeSendReply(ref Message reply, object correlationState)
    {
    }
  }
}
