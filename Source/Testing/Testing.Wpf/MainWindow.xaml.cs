using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Testing.Wpf
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    ViewModel mVM = new ViewModel();

    public MainWindow()
    {
      InitializeComponent();

      this.DataContext = mVM;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      mVM.EditEdit();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      mVM.Invoice.Items.Add(new InvoiceItem());
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
      foreach (var ii in mVM.Invoice.Items)
      {
        ii.ItemNo = "a";
      }
    }
  }
}
