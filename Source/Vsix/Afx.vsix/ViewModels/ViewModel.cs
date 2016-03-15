using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.ViewModels
{
  public class ViewModel<T> : ViewModel
    where T : class
  {
    public new T Model
    {
      get { return (T)base.Model; }
      set { base.Model = value; }
    }
  }

  public class ViewModel : INotifyPropertyChanged, IDisposable
  {
    #region Constructors & Destructor

    ~ViewModel()
    {
      Dispose(false);
    }

    #endregion

    Collection<WeakReference> mViewModelCollections = new Collection<WeakReference>();

    #region object Model

    public const string ModelProperty = "Model";
    object mModel;
    public object Model
    {
      get { return mModel; }
      set
      {
        if (SetProperty<object>(ref mModel, value, ModelProperty))
        {
          OnModelChanged();
        }
      }
    }

    #endregion

    #region ViewModel Parent

    public const string ParentProperty = "Parent";
    ViewModel mParent;
    public ViewModel Parent
    {
      get { return mParent; }
      internal set { mParent = value; }
    }

    #endregion

    #region void OnModelChanged()

    protected virtual void OnModelChanged()
    {
    }

    #endregion

    #region void OnLoaded()

    protected internal virtual void OnLoaded()
    {
    }

    #endregion

    #region RegisterViewModelCollection(...)

    internal void RegisterViewModelCollection(IDisposable vmc)
    {
      mViewModelCollections.Add(new WeakReference(vmc));
    }

    #endregion

    #region INotifyPropertyChanged 

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
    protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if ((typeof(T).IsValueType || typeof(T) == typeof(string)) && Object.Equals(field, value)) return false;
      else if ((object)field == (object)value) return false;

      field = value;
      this.OnPropertyChanged(propertyName);

      return true;
    }

    #endregion

    #region IDisposable

    #region bool IsDisposed

    bool mIsDisposed;
    public bool IsDisposed
    {
      get { return mIsDisposed; }
      private set { mIsDisposed = value; }
    }

    #endregion

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IsDisposed) return;
      IsDisposed = true;

      if (disposing)
      {
        foreach (var vmc in mViewModelCollections.Where(wr1 => wr1.IsAlive).Select<WeakReference, IDisposable>(wr1 => (IDisposable)wr1.Target))
        {
          vmc.Dispose();
        }
      }
    }

    #endregion
  }
}
