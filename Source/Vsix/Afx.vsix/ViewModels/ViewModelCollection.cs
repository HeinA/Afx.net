using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.ViewModels
{
  public class ViewModelCollection<T> : IEnumerable<T>, INotifyCollectionChanged, IDisposable
    where T : ViewModel
  {
    List<T> mViewModels = new List<T>();
    ViewModel mOwner = null;

    #region Constructors & Destructor

    public ViewModelCollection(ViewModel owner)
    {
      if (owner == null) throw new ArgumentNullException("owner");

      mOwner = owner;
      mOwner.RegisterViewModelCollection(this);
    }

    ~ViewModelCollection()
    {
      Dispose(false);
    }

    #endregion

    #region T GetViewModel(...)

    /// <summary>
    /// Gets the first ViewModel where the Model's instances match
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public T GetViewModel(object model)
    {
      return mViewModels.FirstOrDefault(vm1 => vm1.Model == model);
    }

    #endregion

    #region IEnumerable ItemsSource

    IEnumerable mItemsSource;
    public IEnumerable ItemsSource
    {
      get { return mItemsSource; }
      set
      {
        if (mItemsSource == value) return;
        Reset(true, value);
        if (CollectionChanged != null) CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }

    #region void ItemSource_CollectionChanged(...)

    void ItemSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      // Only really meant to be used with ObservableCollection<T>, which always only has one item in it's e.NewItems & e.OldItems

      NotifyCollectionChangedEventArgs args = null;
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          Add(e.NewStartingIndex, e.NewItems[0]);
          args = new NotifyCollectionChangedEventArgs(e.Action, GetViewModel(e.NewItems[0]), e.NewStartingIndex);
          break;

        case NotifyCollectionChangedAction.Remove:
          T removedVM = GetViewModel(e.OldItems[0]);
          RemoveAt(e.OldStartingIndex);
          args = new NotifyCollectionChangedEventArgs(e.Action, removedVM, e.OldStartingIndex);
          break;

        case NotifyCollectionChangedAction.Reset:
          Reset();
          args = new NotifyCollectionChangedEventArgs(e.Action);
          break;

        case NotifyCollectionChangedAction.Replace:
          T replacedVM = GetViewModel(e.OldItems[0]);
          RemoveAt(e.OldStartingIndex);
          Add(e.NewStartingIndex, e.NewItems[0]);
          args = new NotifyCollectionChangedEventArgs(e.Action, GetViewModel(e.NewItems[0]), replacedVM, e.NewStartingIndex);
          break;

        case NotifyCollectionChangedAction.Move:
          T vm = mViewModels[e.OldStartingIndex];
          mViewModels.RemoveAt(e.OldStartingIndex);
          mViewModels.Insert(e.NewStartingIndex, vm);
          args = new NotifyCollectionChangedEventArgs(e.Action, GetViewModel(e.NewItems[0]), e.NewStartingIndex, e.OldStartingIndex);
          break;
      }

      if (CollectionChanged != null) CollectionChanged.Invoke(this, args);
    }

    #region ViewModel Add(...)

    ViewModel Add(int startIndex, object item)
    {
      T vm = CreateViewModel(item);
      vm.Parent = mOwner;
      vm.Model = item;
      if (startIndex >= 0) mViewModels.Insert(startIndex, vm);
      else mViewModels.Add(vm);
      return vm;
    }

    public virtual T CreateViewModel(object model)
    {
      CreateViewModelEventArgs<T> args = new CreateViewModelEventArgs<T>(model);
      if (OnCreateViewModel != null) OnCreateViewModel(this, args);
      return args.ViewModel;
    }

    public event EventHandler<CreateViewModelEventArgs<T>> OnCreateViewModel;

    #endregion

    #region ViewModel RemoveAt(...)

    ViewModel RemoveAt(int index)
    {
      T vm = mViewModels[index];
      vm.Dispose();
      mViewModels.Remove(vm);
      return vm;
    }

    #endregion

    #region void Reset(...)

    void Reset(bool updateSource = false, IEnumerable newSource = null)
    {
      foreach (var vm in mViewModels)
      {
        vm.Dispose();
      }
      mViewModels.Clear();

      if (updateSource)
      {
        INotifyCollectionChanged ncc = mItemsSource as INotifyCollectionChanged;
        if (ncc != null) ncc.CollectionChanged -= ItemSource_CollectionChanged;

        mItemsSource = newSource;

        ncc = mItemsSource as INotifyCollectionChanged;
        if (ncc != null) ncc.CollectionChanged += ItemSource_CollectionChanged;
      }

      foreach (var o in mItemsSource)
      {
        T vm = CreateViewModel(o);
        vm.Parent = mOwner;
        vm.Model = o;
        mViewModels.Add(vm);
      }
    }

    #endregion

    #endregion

    #endregion

    #region bool IsDisposed

    bool mIsDisposed;
    public bool IsDisposed
    {
      get { return mIsDisposed; }
      private set { mIsDisposed = value; }
    }

    #endregion


    #region INotifyCollectionChanged

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    #region IEnumerable<T>

    public IEnumerator<T> GetEnumerator()
    {
      return mViewModels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return mViewModels.GetEnumerator();
    }

    #endregion

    #region IDisposable

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
        foreach (var vm in mViewModels)
        {
          vm.Dispose();
        }
      }
    }

    #endregion
  }
}
