using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description;
using Afx.ObjectModel.Description.Validation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Metadata
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class Metadata
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Metadata()
    {
      AssemblyHelper.PreLoadDeployedAssemblies();
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        foreach (Type type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(AfxObject)) && t.GetCustomAttribute<AfxBaseTypeAttribute>() == null))
        {
          ParseAfxType(type);
        }
      }
    }

    internal Metadata(Type type)
    {
      mMetadataDictionary.Add(type, this);

      SourceType = type;
      IsAbstract = SourceType.IsAbstract;

      Type gsc = AssemblyHelper.GetGenericSubClass(typeof(AfxObject<>), SourceType);
      if (gsc != null)
      {
        var piOwner = SourceType.GetProperty(AfxObject.OwnerProperty);
        OwnerType = piOwner.PropertyType;
      }

      gsc = AssemblyHelper.GetGenericSubClass(typeof(AssociativeObject<,>), SourceType);
      if (gsc != null)
      {
        var piOwner = SourceType.GetProperty(AfxObject.OwnerProperty);
        var piReference = SourceType.GetProperty(AssociativeObject.ReferenceProperty);
        OwnerType = piOwner.PropertyType;
        if (!mDirectDependencies.Contains(piReference.PropertyType)) mDirectDependencies.Add(piReference.PropertyType);
      }

      Metadata baseMetadata = Metadata.GetMetadata(SourceType.BaseType);
      if (baseMetadata != null)
      {
        BaseType = SourceType.BaseType;
        baseMetadata.mSubtypes.Add(SourceType);
      }

      foreach (var pi in SourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
      {
        foreach (var va in pi.GetCustomAttributes<ValidationAttribute>())
        {
          mValidationMetadata.Add(new ValidationMetadata(pi, va));
        }

        if (pi.PropertyType.IsSubclassOf(typeof(AfxObject)))
        {
          if (!mDirectDependencies.Contains(pi.PropertyType)) mDirectDependencies.Add(pi.PropertyType); 
        }

        Type cgsc = AssemblyHelper.GetGenericSubClass(typeof(ObjectCollection<>), pi.PropertyType);
        if (cgsc != null)
        {
          Type t = cgsc.GetGenericArguments()[0];
          if (!mDirectDependencies.Contains(t)) mDirectDependencies.Add(t);
        }

        cgsc = AssemblyHelper.GetGenericSubClass(typeof(AssociativeCollection<,>), pi.PropertyType);
        if (cgsc != null)
        {
          Type t = cgsc.GetGenericArguments()[1];
          if (!mDirectDependencies.Contains(t)) mDirectDependencies.Add(t);
        }
      }
    }

    static Collection<Type> mKnownAfxTypes = new Collection<Type>();
    public static IEnumerable<Type> KnownAfxTypes
    {
      get { return mKnownAfxTypes; }
    }

    public static Metadata GetMetadata(Type type)
    {
      try
      {
        if (mMetadataDictionary.ContainsKey(type)) return mMetadataDictionary[type];
        return ParseAfxType(type);
      }
      catch (System.TypeInitializationException)
      {
        return null;
      }
    }

    static Dictionary<Type, Metadata> mMetadataDictionary = new Dictionary<Type, Metadata>();

    static Metadata ParseAfxType(Type type)
    {
      if (type.GetCustomAttribute<AfxBaseTypeAttribute>() != null) return null;
      if (mKnownAfxTypes.Contains(type)) return mMetadataDictionary[type];
      mKnownAfxTypes.Add(type);

      if (type.IsSubclassOf(typeof(AssociativeObject)))
        return new AssociativeMetadata(type);
      else
        return new Metadata(type);
    }

    #region Type SourceType

    Type mSourceType;
    public Type SourceType
    {
      get { return mSourceType; }
      private set { mSourceType = value; }
    }

    #endregion

    #region Type OwnerType

    Type mOwnerType;
    public Type OwnerType
    {
      get { return mOwnerType; }
      private set { mOwnerType = value; }
    }

    #endregion

    #region Type BaseType

    Type mBaseType;
    public Type BaseType
    {
      get { return mBaseType; }
      private set { mBaseType = value; }
    }

    #endregion

    #region Collection<Type> Subtypes

    Collection<Type> mSubtypes = new Collection<Type>();
    public IEnumerable<Type> Subtypes
    {
      get { return mSubtypes; }
    }

    #endregion

    #region bool IsAbstract

    bool mIsAbstract;
    public bool IsAbstract
    {
      get { return mIsAbstract; }
      private set { mIsAbstract = value; }
    }

    #endregion

    #region Collection<Type> DirectDependencies

    Collection<Type> mDirectDependencies = new Collection<Type>();
    IEnumerable<Type> DirectDependencies
    {
      get { return mDirectDependencies; }
    }

    #endregion

    #region IEnumerable<Type> Dependencies

    Collection<Type> mDependencies;
    public IEnumerable<Type> Dependencies
    {
      get
      {
        if (mDependencies == null)
        {
          mDependencies = new Collection<Type>();
          AppendDependencies(mDependencies);
        }
        return mDependencies;
      }
    }

    void AppendDependencies(Collection<Type> dependencies)
    {
      foreach (Type t in DirectDependencies)
      {
        if (!dependencies.Contains(t))
        {
          dependencies.Add(t);
          t.GetMetadata().AppendDependencies(dependencies);
        }
      }

      if (BaseType != null)
      {
        foreach (Type t in BaseType.GetMetadata().Dependencies) 
        {
          if (!dependencies.Contains(t))
          {
            dependencies.Add(t);
            t.GetMetadata().AppendDependencies(dependencies);
          }
        }
      }
    }

    #endregion

    #region Collection<ValidationMetadata> ValidationAttributes

    Collection<ValidationMetadata> mValidationMetadata = new Collection<ValidationMetadata>();
    internal IEnumerable<ValidationMetadata> ValidationMetadata
    {
      get { return mValidationMetadata; }
    }

    #endregion

    public static IEnumerable<Type> OrderByDependencies(IEnumerable<Type> types)
    {
      Queue<Type> q = new Queue<Type>(types);
      Collection<Type> col = new Collection<Type>();
      Type last = null;
      while (q.Count > 0)
      {
        Type t = q.Dequeue();
        foreach (Type td in GetMetadata(t).Dependencies)
        {
          if (!col.Contains(td))
          {
            q.Enqueue(t);
            break;
          }
        }
        if (q.Contains(t))
        {
          if (last == null) last = t;
          else if (last.Equals(t)) throw new InvalidOperationException("Circular Dependencies"); //TODO: Message
          continue;
        }
        last = null;
        col.Add(t);
      }
      return col;
    }
  }
}
