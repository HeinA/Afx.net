using Testing.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace Testing.ServiceHost.App_Code
{
  public class AppInit
  {
    //WALKTHROUGH: Server Step 01
    //This method is called when the iis service initializes the application.
    //Breakpoints placed here will not be hit, as this code is run before the debugger is attached.
    public static void AppInitialize()
    {
      Afx.ServiceModel.Activation.BusinessServiceHostFactory.Initialize();
    }
  }
}