using Afx.Data.Configuration;
using Afx.ObjectModel;
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
  public class ObjectRepository
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

      Catalogue = poa.Catalogue ?? type.Name;
      IsReadOnly = poa.IsReadOnly;

      foreach (Type t in SourceMetadata.Subtypes)
      {
        mSubRepositories.Add(GetRepository(t));
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
      else mProperties.Add(new SimpleProperty(this, pi, true));
    }

    void AddProperty(PropertyInfo pi)
    {
      if (pi.PropertyType.IsSubclassOf(typeof(AfxObject))) mProperties.Add(new ComplexProperty(this, pi));
      else mProperties.Add(new SimpleProperty(this, pi));
    }

    void AddProperty(PropertyInfo pi, string name)
    {
      if (pi.PropertyType.IsSubclassOf(typeof(AfxObject))) mProperties.Add(new ComplexProperty(this, pi, name));
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

    #region string Catalogue

    string mCatalogue;
    public string Catalogue
    {
      get { return mCatalogue; }
      private set { mCatalogue = value; }
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

    #region string ToString(...)

    public override string ToString()
    {
      if (string.IsNullOrWhiteSpace(Schema)) return string.Format("[{0}]", Catalogue);
      return string.Format("[{0}].[{1}]", Schema, Catalogue);
    }

    #endregion

    internal AfxObject SaveObject(AfxObject obj, string repositoryName)
    {
      if (IsReadOnly) throw new InvalidOperationException(); //TODO: message

      if (!mDataRepositoryDictionary.ContainsKey(repositoryName))
      {
        Repository dr = RepositoryConfiguration.Default.Repositories.OfType<Repository>().Where(r => r.Name == repositoryName).FirstOrDefault();
        if (dr == null) throw new InvalidOperationException(); //TODO: message
        IDataRepository idr = (IDataRepository)Activator.CreateInstance(dr.RepositoryType, repositoryName);
        mDataRepositoryDictionary.Add(repositoryName, idr);
      }

      return mDataRepositoryDictionary[repositoryName].SaveObject(obj, GetRepository(obj.GetType()));
    }

    static Dictionary<Type, ObjectRepository> mObjectRepositoryDictionary = new Dictionary<Type, ObjectRepository>();
    static Dictionary<string, IDataRepository> mDataRepositoryDictionary = new Dictionary<string, IDataRepository>();
  }

  public class ObjectRepository<T> : ObjectRepository
    where T : AfxObject
  {
    internal ObjectRepository()
      : base(typeof(T))
    {
    }

    public T SaveObject(T obj, string repositoryName)
    {
      return (T)base.SaveObject(obj, repositoryName);
    }

    public T SaveObject(T obj)
    {
      return (T)base.SaveObject(obj, RepositoryScope.ConnectionName);
    }
  }
}
