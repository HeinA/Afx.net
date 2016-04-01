using Afx.Data.Configuration;
using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Data;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data
{
  public abstract class ObjectRepository
  {
    #region Constructors

    static ObjectRepository()
    {
      foreach (Type type in Metadata.KnownAfxTypes.Where(t => t.GetCustomAttribute<PersistentObjectAttribute>() != null))
      {
        if (!VerifyPersistanceChain(type)) throw new InvalidOperationException("Persistance Chain"); //TODO: Message
        ParsePersistantType(type);
      }
    }

    internal ObjectRepository(Type type)
    {
      SourceType = type;
      SourceMetadata = SourceType.GetMetadata();

      mObjectRepositoryDictionary.Add(type, this);

      var poa = SourceType.GetCustomAttribute<PersistentObjectAttribute>();
      if (poa == null) throw new InvalidOperationException(); //TODO: Message

      var bsa = SourceType.GetCustomAttribute<BinaryStorageAttribute>();
      if (bsa != null)
      {
        BinaryStorageName = bsa.Name;
      }

      var sa = SourceType.Assembly.GetCustomAttribute<SchemaAttribute>();
      Schema = sa == null ? null : sa.Name;

      Catalog = poa.Catalog ?? type.Name;
      IsReadOnly = poa.IsReadOnly;
      IsCached = poa.IsCached;

      foreach (Type t in SourceMetadata.Subtypes)
      {
        var or1 = GetRepository(t);
        or1.BaseRepository = this;
        mSubRepositories.Add(or1);
      }

      PropertyInfo piId = SourceType.GetProperty(AfxObject.IdProperty);
      AddIdProperty(piId);

      Type gsc = AssemblyHelper.GetGenericSubClass(typeof(AfxObject<>), SourceType);
      if (gsc != null)
      {
        PropertyInfo piOwner = SourceType.GetProperty(AfxObject.OwnerProperty);
        AddProperty(piOwner, poa.Owner ?? piOwner.PropertyType.Name);
      }

      gsc = AssemblyHelper.GetGenericSubClass(typeof(AssociativeObject<,>), SourceType);
      if (gsc != null)
      {
        PropertyInfo piOwner = SourceType.GetProperty(AfxObject.OwnerProperty);
        PropertyInfo piReference = SourceType.GetProperty(AssociativeObject.ReferenceProperty);
        AddProperty(piOwner, poa.Owner ?? piOwner.PropertyType.Name);
        AddProperty(piReference, poa.Reference ?? piReference.PropertyType.Name);
      }

      foreach (var pi in SourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(pi1 => pi1.GetCustomAttribute<PersistentPropertyAttribute>() != null))
      {
        AddProperty(pi);
      }
    }

    #endregion

    #region void AddProperty(...)

    void AddIdProperty(PropertyInfo pi)
    {
      if (pi.PropertyType.IsSubclassOf(typeof(AfxObject))) throw new InvalidOperationException(); //TODO: message
      else if (typeof(IObjectCollection).IsAssignableFrom(pi.PropertyType)) throw new InvalidOperationException(); //TODO: message
      else mProperties.Add(new SimpleProperty(this, pi, true));
    }

    void AddProperty(PropertyInfo pi)
    {
      if (pi.PropertyType.IsSubclassOf(typeof(AfxObject))) mProperties.Add(new ComplexProperty(this, pi));
      else if (typeof(IObjectCollection).IsAssignableFrom(pi.PropertyType)) mProperties.Add(new CollectionProperty(this, pi));
      else mProperties.Add(new SimpleProperty(this, pi));
    }

    void AddProperty(PropertyInfo pi, string name)
    {
      if (pi.PropertyType.IsSubclassOf(typeof(AfxObject))) mProperties.Add(new ComplexProperty(this, pi, name));
      else if (typeof(IObjectCollection).IsAssignableFrom(pi.PropertyType)) throw new InvalidOperationException(); //TODO: message
      else throw new InvalidOperationException(); //TODO: message
    }

    #endregion

    #region ObjectRepository ParsePersistantType(...)

    static ObjectRepository ParsePersistantType(Type type)
    {
      if (type.GetCustomAttribute<AfxBaseTypeAttribute>() != null) return null;
      if (type.GetCustomAttribute<PersistentObjectAttribute>() == null) return null;
      if (type.BaseType != null) ParsePersistantType(type.BaseType);

      if (mObjectRepositoryDictionary.ContainsKey(type)) return mObjectRepositoryDictionary[type];

      Type ort = typeof(ObjectRepository<>);
      ort = ort.MakeGenericType(type);
      return (ObjectRepository)Activator.CreateInstance(ort, true); //new ObjectRepository(type);
    }

    #endregion

    #region ObjectRepository GetRepository(...)

    public static ObjectRepository GetRepository(Type type)
    {
      if (mObjectRepositoryDictionary.ContainsKey(type)) return mObjectRepositoryDictionary[type];
      return ParsePersistantType(type);
    }

    public static ObjectRepository<T> GetRepository<T>()
      where T : AfxObject
    {
      if (mObjectRepositoryDictionary.ContainsKey(typeof(T))) return (ObjectRepository<T>)mObjectRepositoryDictionary[typeof(T)];
      return (ObjectRepository<T>)ParsePersistantType(typeof(T));
    }

    #endregion

    #region IDataRepository GetDataRepository(...)

    protected IDataRepository GetDataRepository(string repositoryName)
    {
      if (!mDataRepositoryDictionary.ContainsKey(repositoryName))
      {
        Repository rc = RepositoryConfiguration.Default.Repositories.OfType<Repository>().Where(r => r.Name == repositoryName).FirstOrDefault();
        if (rc == null) throw new InvalidOperationException(); //TODO: message
        IDataRepository idr = (IDataRepository)Activator.CreateInstance(rc.RepositoryType);
        idr.Initialize(rc);
        mDataRepositoryDictionary.Add(repositoryName, idr);
      }

      return mDataRepositoryDictionary[repositoryName];
    }

    #endregion

    #region ObjectRepository BaseRepository

    public ObjectRepository BaseRepository { get; private set; }

    #endregion

    #region Type SourceType

    Type mSourceType;
    public Type SourceType
    {
      get { return mSourceType; }
      private set { mSourceType = value; }
    }

    #endregion

    #region Metadata SourceMetadata

    Metadata mSourceMetadata;
    public Metadata SourceMetadata
    {
      get { return mSourceMetadata; }
      private set { mSourceMetadata = value; }
    }

    #endregion

    #region string Schema

    string mSchema;
    public string Schema
    {
      get { return mSchema ?? "dbo"; }
      private set { mSchema = value; }
    }

    #endregion

    #region string Catalog

    string mCatalog;
    public string Catalog
    {
      get { return mCatalog; }
      private set { mCatalog = value; }
    }

    #endregion

    #region string BinaryStorageName

    string mBinaryStorageName;
    public string BinaryStorageName
    {
      get { return mBinaryStorageName; }
      private set { mBinaryStorageName = value; }
    }

    #endregion

    #region bool IsReadOnly

    bool mIsReadOnly;
    public bool IsReadOnly
    {
      get { return mIsReadOnly; }
      private set { mIsReadOnly = value; }
    }

    #endregion

    #region bool IsCached

    bool mIsCached;
    public bool IsCached
    {
      get { return mIsCached; }
      private set { mIsCached = value; }
    }

    #endregion

    #region bool VerifyPersistanceChain(...)

    static bool VerifyPersistanceChain(Type type)
    {
      if (type.GetCustomAttribute<PersistentObjectAttribute>() == null) return false;
      Type baseType = type.GetMetadata().BaseType;
      if (baseType != null) return VerifyPersistanceChain(baseType);
      return true;
    }

    #endregion

    #region Collection<ObjectRepository> SubRepositories

    Collection<ObjectRepository> mSubRepositories = new Collection<ObjectRepository>();
    public IEnumerable<ObjectRepository> SubRepositories
    {
      get { return mSubRepositories; }
    }

    #endregion

    #region Collection<ObjectProperty> Properties

    Collection<ObjectProperty> mProperties = new Collection<ObjectProperty>();
    public IEnumerable<ObjectProperty> Properties
    {
      get { return mProperties; }
    }

    #endregion

    #region IEnumerable<ObjectRepository> ConcreteRepositories

    public IEnumerable<ObjectRepository> ConcreteRepositories
    {
      get
      {
        if (!SourceMetadata.IsAbstract) yield return this;
        foreach (var or in this.SubRepositories)
        {
          foreach (var or1 in or.ConcreteRepositories)
          {
            yield return or1;
          }
        }
      }
    }

    #endregion

    #region string ToString(...)

    public override string ToString()
    {
      if (string.IsNullOrWhiteSpace(Schema)) return string.Format("[{0}]", Catalog);
      return string.Format("[{0}].[{1}]", Schema, Catalog);
    }

    #endregion


    #region AfxObject LoadObject(...)

    public AfxObject LoadObject(Guid id)
    {
      return this.LoadObject(id, RepositoryScope.ConnectionName);
    }

    public AfxObject LoadObject(Guid id, string repositoryName)
    {
      IDataRepository idr = GetDataRepository(repositoryName);
      return idr.LoadObject(id, this);
    }

    #endregion

    #region AfxObject LoadObjects(...)

    public ObjectCollection<AfxObject> LoadObjects()
    {
      return LoadObjects(RepositoryScope.ConnectionName);
    }

    public ObjectCollection<AfxObject> LoadObjects(string repositoryName)
    {
      IDataRepository idr = GetDataRepository(repositoryName);
      return new ObjectCollection<AfxObject>(idr.LoadObjects(this));
    }

    #endregion

    #region AfxObject SaveObject(...)

    public AfxObject SaveObject(AfxObject obj)
    {
      return this.SaveObject(obj, RepositoryScope.ConnectionName);
    }

    public AfxObject SaveObject(AfxObject obj, string repositoryName)
    {
      ObjectRepository or = GetRepository(obj.GetType()); //Ensure ObjectRepository is the actual type, not a base type.
      if (or.IsReadOnly) throw new InvalidOperationException(); //TODO: message
      IDataRepository idr = GetDataRepository(repositoryName);
      return idr.SaveObject(obj, or);
    }

    #endregion

    static Dictionary<Type, ObjectRepository> mObjectRepositoryDictionary = new Dictionary<Type, ObjectRepository>();
    static Dictionary<string, IDataRepository> mDataRepositoryDictionary = new Dictionary<string, IDataRepository>();
  }

  public class ObjectRepository<T> : ObjectRepository
    where T : AfxObject
  {
    #region Constructors

    internal ObjectRepository()
      : base(typeof(T))
    {
    }

    #endregion

    #region T LoadObject(...)

    public new T LoadObject(Guid id, string repositoryName)
    {
      return (T)base.LoadObject(id, repositoryName);
    }

    public new T LoadObject(Guid id)
    {
      return (T)base.LoadObject(id);
    }

    #endregion

    #region ObjectCollection<T> LoadObjects(...)

    public new ObjectCollection<T> LoadObjects(string repositoryName)
    {
      IDataRepository idr = GetDataRepository(repositoryName);
      return new ObjectCollection<T>(idr.LoadObjects(this).Cast<T>());
    }

    public new ObjectCollection<T> LoadObjects()
    {
      return this.LoadObjects(RepositoryScope.ConnectionName);
    }

    #endregion

    #region T SaveObject(...)

    public T SaveObject(T obj, string repositoryName)
    {
      return (T)base.SaveObject(obj, repositoryName);
    }

    public T SaveObject(T obj)
    {
      return this.SaveObject(obj, RepositoryScope.ConnectionName);
    }

    #endregion
  }
}
