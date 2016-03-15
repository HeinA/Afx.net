using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Testing.Wpf
{
  public static class BindingHelper
  {
    public static bool GetUpdateSource(DependencyObject obj)
    {
      return (bool)obj.GetValue(UpdateSourceProperty);
    }

    public static void SetUpdateSource(DependencyObject obj, bool value)
    {
      obj.SetValue(UpdateSourceProperty, value);
    }

    public static readonly DependencyProperty UpdateSourceProperty = DependencyProperty.RegisterAttached("UpdateSource", typeof(bool), typeof(BindingHelper), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnUpdateSourcePropertyChanged));

    private static void OnUpdateSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (GetUpdateSource(d))
      {
        foreach (BindingExpressionBase be in BindingOperations.GetSourceUpdatingBindings(d))
        {
          be.UpdateSource();
        }

        d.Dispatcher.BeginInvoke(new Action(() =>
        {
          SetUpdateSource(d, false);
        }), System.Windows.Threading.DispatcherPriority.Normal);
      }
    }
  }
}
