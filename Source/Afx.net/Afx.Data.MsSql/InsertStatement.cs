using Afx.ObjectModel;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql
{
  class InsertStatement
  {
    public InsertStatement(ObjectRepository objectRepository)
    {
      mObjectRepository = objectRepository;
      mTableName = string.Format("[{0}].[{1}]", objectRepository.Schema, objectRepository.Catalog);
    }

    string mTableName;
    int mParamCount;
    Collection<string> mColumns = new Collection<string>();
    Collection<string> mValues = new Collection<string>();
    ObjectRepository mObjectRepository;

    void AddProperties(ObjectRepository objectRepository, AfxObject obj, SqlCommand cmd)
    {
      if (objectRepository.BaseRepository != null)
      {
        AddProperty(obj, "id", obj.Id, cmd);
      }

      foreach (var p in objectRepository.Properties)
      {
        if (p is CollectionProperty) continue;
        if (p is ComplexProperty && p.PropertyType.GetMetadata().OwnerType != null && p.PropertyType.GetMetadata().OwnerType.IsAssignableFrom(objectRepository.SourceType)) continue;

        object val = p.PropertyInfo.GetValue(obj);
        if (val == null) continue;
        AddProperty(obj, p.Name, val, cmd);
      }
    }

    void AddProperty(AfxObject obj, string name, object val, SqlCommand cmd)
    {
      AfxObject valAfx = val as AfxObject;
      mColumns.Add(string.Format("[{0}]", name));
      string param = string.Format("@p{0}", ++mParamCount);
      mValues.Add(param);
      cmd.Parameters.AddWithValue(param, valAfx != null ? valAfx.Id : val);
    }

    public void Execute(AfxObject obj, string connectionString)
    {
      using (SqlConnection con = new SqlConnection(connectionString))
      {
        try
        {
          con.Open();

          using (SqlCommand cmd = new SqlCommand(string.Empty, con))
          {
            AddProperties(mObjectRepository, obj, cmd);
            cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", mTableName, string.Join(", ", mColumns), string.Join(", ", mValues));
            cmd.ExecuteNonQuery();
          }
        }
        finally
        {
          con.Close();
        }
      }
    }
  }
}
