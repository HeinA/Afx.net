using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.ServiceModel.Description
{
  public class ServiceMetadata
  {
    private ServiceMetadata(Type contract)
    {
      IsSecure = contract.GetCustomAttribute<SecureServiceAttribute>() != null;
      IsCompressed = contract.GetCustomAttribute<CompressMessageAttribute>() != null;
    }

    #region bool IsSecure

    bool mIsSecure;
    public bool IsSecure
    {
      get { return mIsSecure; }
      private set { mIsSecure = value; }
    }

    #endregion

    #region bool IsCompressed

    bool mIsCompressed;
    public bool IsCompressed
    {
      get { return mIsCompressed; }
      set { mIsCompressed = value; }
    }

    #endregion

    static Dictionary<Type, ServiceMetadata> mMetadataDictionaty = new Dictionary<Type, ServiceMetadata>();

    public static ServiceMetadata GetMetadata<T>()
      where T : class
    {
      if (!mMetadataDictionaty.ContainsKey(typeof(T)))
      {
        mMetadataDictionaty.Add(typeof(T), new ServiceMetadata(typeof(T)));
      }
      return mMetadataDictionaty[typeof(T)];
    }

    public static ServiceMetadata GetMetadata(Type contractType)
    {
      if (!mMetadataDictionaty.ContainsKey(contractType))
      {
        mMetadataDictionaty.Add(contractType, new ServiceMetadata(contractType));
      }
      return mMetadataDictionaty[contractType];
    }
  }
}
