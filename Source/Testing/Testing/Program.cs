using Afx.Data;
using Afx.ObjectModel;
using Afx.Infrastructure.Security;
using Afx.ServiceModel;
using Afx.ServiceModel.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testing.Services;
using Afx.Cache;

namespace Testing
{
  class Program
  {
    static void Main(string[] args)
    {
      ObjectRepository<User> or = ObjectRepository.GetRepository<User>();
      MemoryCache.Initialize();

      User u = or.LoadObject(new Guid("1f49b30f-3ad0-47e3-a8c1-5b4324f7fdad"));

      User u1 = new User();
      u1.Roles.Add(u.Roles[0]);
      u1.UserName = "aaa";
      User uu = or.SaveObject(u1);

      Console.WriteLine("...");
      Console.ReadKey();

      PasswordIdentity newIdentity = new PasswordIdentity("HeinA", "Dred@123", true, "Afx");
      PasswordPrincipal newPricipal = new PasswordPrincipal(newIdentity, null);

      Thread.CurrentPrincipal = newPricipal;

      Task.Run(() =>
      {
        string ss = Thread.CurrentPrincipal.Identity.Name;
          Invoice i5 = new Invoice();
        //  string em = i.ErrorMessage;
        //  i.DocumentNumber = "INV001";
        //  var ii = new InvoiceItem();
        //  i.Items.Add(ii);
        //  ii.ItemNo = "11011";
          Console.WriteLine("** {0} **", i5.ErrorMessage);
        });

        //Console.ReadKey();


        Stopwatch sw = new Stopwatch();
      sw.Start();

      Invoice i = new Invoice();
      i.PropertyChanged += I_PropertyChanged;

      //Thread.Sleep(25);

      INotifyDataErrorInfo ei = i as INotifyDataErrorInfo;
      ei.ErrorsChanged += Ei_ErrorsChanged;

      i.DocumentNumber = "INV001";

      //Thread.Sleep(500);

      //string s1 = i.ErrorMessage;

      for (int ii = 0; ii <= 100; ii++)
      {
        var iii = new InvoiceItem();
        //iii.ItemNo = "aaa";
        i.Items.Add(iii);
      }

      foreach (var iii in i.Items)
      {
        iii.ItemNo = "asdf";
      }

      i.DocumentNumber = string.Empty;

      Console.WriteLine("{0:#,##0}", sw.ElapsedMilliseconds);
      string s = i.ErrorMessage;
      Console.WriteLine(s);

      Collection<Invoice> col = new Collection<Invoice>();
      using (var svc = ServiceFactory.GetService<ITestingService>("https://dred-dv7.indilinga.local/Testing.ServiceHost"))
      {
        svc.Instance.Test(col);
      }
      Console.ReadKey();
    }

    private static void I_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == AfxObject.ErrorMessageProperty)
      {
        Invoice i = sender as Invoice;
        string s = i.ErrorMessage;
        Console.WriteLine("####\n{0}\n####", s);
      }
    }

    private static void Ei_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
      //Console.WriteLine("*");

      //Invoice i = sender as Invoice;
      //string s = i.ErrorMessage;
      //Console.WriteLine("****\n{0}\n****", s);

    //  Console.WriteLine("****");
    //  INotifyDataErrorInfo ii = sender as INotifyDataErrorInfo;
    //  foreach (string ss in ii.GetErrors(e.PropertyName))
    //  {
    //    Console.WriteLine(ss);
    //  }
    //  Console.WriteLine("****");
    }
  }
}
