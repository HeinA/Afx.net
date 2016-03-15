using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VSLangProj;

namespace Afx.vsix.AfxWizard
{
  /// <summary>
  /// Interaction logic for AfxWizard.xaml
  /// </summary>
  public partial class AfxWizardUI : System.Windows.Window
  {
    public AfxWizardUI()
    {
      InitializeComponent();
      this.DataContext = new AfxWizardViewModel();
    }

    public AfxWizardViewModel ViewModel
    {
      get { return (AfxWizardViewModel)this.DataContext; }
    }
  }
}
