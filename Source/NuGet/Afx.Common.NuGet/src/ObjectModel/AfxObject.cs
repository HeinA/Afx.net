using Afx;
using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ObjectModel
{
  [DataContract(Namespace = Constants.WcfNamespace, IsReference = true)]
  [AfxBaseType]
  public abstract class AfxObject : IEditNotifier, INotifyDataErrorInfo, INotifyCollectionChanged, IAfxObject
  {
    #region Constructors

    protected AfxObject()
    {
    }

    protected AfxObject(string id)
      : this(Guid.Parse(id))
    {
    }

    protected AfxObject(Guid id)
    {
      Id = id;
    }

    #endregion

    #region AfxObject Owner

    public const string OwnerProperty = "Owner";
    IAfxObject mOwner;
    [DataMember(EmitDefaultValue = false)]
    internal IAfxObject Owner
    {
      get
      {
        if (IsSerializing) return null;
        return mOwner;
      }
      set { mOwner = ValidateOwner(value); }
    }

    protected virtual IAfxObject ValidateOwner(IAfxObject owner)
    {
      if (owner == null) return null;
      throw new InvalidOperationException(Properties.Resources.TypeNotOwned);
    }

    IAfxObject IAfxObject.Owner
    {
      get { return this.Owner; }
      set { this.Owner = value; }
    }

    #endregion

    #region Guid Id

    public const string IdProperty = "Id";
    Guid mId = Guid.NewGuid();
    [DataMember]
    public Guid Id
    {
      get { return mId; }
      internal set { mId = value; }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected internal virtual void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException(Properties.Resources.PropertyNameNull, "propertyName");

      if (typeof(T).IsValueType && Object.Equals(field, value)) return false;
      else if ((object)field == (object)value) return false;

      field = value;
      OnObjectEdited();
      OnPropertyChanged(propertyName);

      return true;
    }

    #endregion

    #region IEditNotifier

    public event EventHandler ObjectEdited;

    protected virtual void OnObjectEdited()
    {
      if (ObjectEdited != null) ObjectEdited(this, EventArgs.Empty);
      IEditNotifier owner = Owner as IEditNotifier;
      if (owner != null) owner.OnObjectEdited();
    }

    void IEditNotifier.OnObjectEdited()
    {
      OnObjectEdited();
    }

    #endregion

    #region Serialization

    bool mIsSerializing;
    protected internal bool IsSerializing
    {
      get { return mIsSerializing; }
      private set { mIsSerializing = value; }
    }

    bool mIsDeserializing;
    protected internal bool IsDeserializing
    {
      get { return mIsDeserializing; }
      private set { mIsDeserializing = value; }
    }

    [OnSerializing]
    void Serializing(StreamingContext context)
    {
      IsSerializing = true;
    }

    [OnSerialized]
    void Serialized(StreamingContext context)
    {
      IsSerializing = false;
    }

    [OnDeserializing]
    void OnDeserializing(StreamingContext context)
    {
      IsDeserializing = true;
    }

    [OnDeserialized]
    void OnDeserialized(StreamingContext ctx)
    {
      try
      {
        IsDeserializing = false;
      }
      catch
      {
        throw;
      }

      OnDeserialized();
    }

    protected virtual void OnDeserialized()
    {
    }

    #endregion

    #region Validation

    #region AfxObjectValidator Validator

    ObjectValidator mValidator;
    internal ObjectValidator Validator
    {
      get
      {
        EnsureValidator();
        return mValidator;
      }
      set { mValidator = value; }
    }

    #endregion

    #region void EnsureValidator()

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Afx.ObjectModel.ObjectValidator")]
    void EnsureValidator()
    {
      if (mValidator == null)
      {
        new ObjectValidator(this);
      }
    }

    #endregion

    #region INotifyDataErrorInfo

    EventHandler<DataErrorsChangedEventArgs> mErrorsChanged;
    event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
    {
      add
      {
        mErrorsChanged += value;
        EnsureValidator();
      }
      remove { mErrorsChanged -= value; }
    }

    IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
    {
      return Validator.GetErrors(propertyName);
    }

    public const string HasErrorsProperty = "HasErrors";
    public bool HasErrors
    {
      get { return Validator.HasErrors; }
    }

    #endregion

    #region string ErrorMessage

    public const string ErrorMessageProperty = "ErrorMessage";
    public string ErrorMessage
    {
      get { return Validator.ErrorMessage; }
    }

    #endregion

    #region void OnErrorsChanged(...)

    protected internal void OnErrorsChanged(string propertyName)
    {
      if (mErrorsChanged != null) mErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
    }

    #endregion

    #endregion

    #region Owned Collection Generators

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
    protected ObjectCollection<TItem> GetObjectCollection<TItem>(ref ObjectCollection<TItem> field, [CallerMemberName] string propertyName = null)
      where TItem : AfxObject
    {
      return field ?? (field = new ObjectCollection<TItem>(this, propertyName));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
    protected AssociativeCollection<TItem, TAssociative> GetAssociativeCollection<TItem, TAssociative>(ref AssociativeCollection<TItem, TAssociative> field, [CallerMemberName] string propertyName = null)
      where TItem : AfxObject
      where TAssociative : AssociativeObject, new()
    {
      return field ?? (field = new AssociativeCollection<TItem, TAssociative>(this, propertyName));
    }

    #endregion

    #region Equality

    public override bool Equals(object obj)
    {
      AfxObject o = obj as AfxObject;
      if (o == null) return false;
      return Id.Equals(o.Id);
    }

    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }

    #endregion

    #region INotifyCollectionChanged

    /// <summary>
    /// Raised when a contained collection is changed
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    protected internal virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (CollectionChanged != null) CollectionChanged(sender, e);
    }

    #endregion

    #region Explicit Operators

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static explicit operator Guid(AfxObject obj)
    {
      if (obj == null) return Guid.Empty;
      return obj.Id;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
    public static explicit operator string(AfxObject obj)
    {
      if (obj == null) return Guid.Empty.ToString();
      return obj.Id.ToString();
    }

    #endregion
  }

  [DataContract(Namespace = Constants.WcfNamespace, IsReference = true)]
  [AfxBaseType]
  public abstract class AfxObject<TOwner> : AfxObject
    where TOwner : class, IAfxObject
  {
    #region Constructors

    protected AfxObject()
    {
    }

    protected AfxObject(string id)
      : base(id)
    {
    }

    protected AfxObject(Guid id)
      : base(id)
    {
    }

    #endregion

    #region TOwner Owner

    public new TOwner Owner
    {
      get { return (TOwner)base.Owner; }
      set { base.Owner = value; }
    }

    protected override IAfxObject ValidateOwner(IAfxObject owner)
    {
      if (!(owner is TOwner)) throw new InvalidCastException(Properties.Resources.InvalidOwnerType);
      return owner;
    }

    #endregion
  }
}
