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
  public class BusinessServiceClientMessageInspector : IClientMessageInspector
  {
    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
      return null;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
    }
  }
}
