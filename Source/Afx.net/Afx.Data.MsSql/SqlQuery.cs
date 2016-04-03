using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql
{
  class SqlQuery
  {
    public SqlQuery(ObjectRepository objectRepository)
    {
      mTableName = string.Format("[{0}].[{1}] AS [T]", objectRepository.Schema, objectRepository.Catalog);
      AddProperties(objectRepository, "T");
      BaseJoin(objectRepository.BaseRepository);
      if (objectRepository.SourceMetadata.OwnerType != null) mIsCyclic = objectRepository.SourceMetadata.OwnerType.Equals(objectRepository.SourceType);
    }

    void AddProperties(ObjectRepository objectRepository, string alias)
    {
      foreach (var p in objectRepository.Properties.Where(p1 => p1.AllowRead))
      {
        if (p is CollectionProperty) continue;
        mColumns.Add(string.Format("[{0}].[{1}]", alias, p.Name));
        if (p.PropertyInfo.Name == "Owner")
        {
          mOwnerColumnName = p.Name;
          mOwnerAlias = alias;
        }
      }
    }

    bool mIsCyclic = false;
    string mTableName;
    string mOwnerColumnName;
    string mOwnerAlias;
    Collection<string> mColumns = new Collection<string>();

    int mJoinCount;
    Collection<string> mJoins = new Collection<string>();

    DataSet ExecuteDataSet(SqlCommand cmd)
    {
      System.Data.DataSet ds = new System.Data.DataSet();
      ds.Locale = CultureInfo.InvariantCulture;
      using (IDataReader r = cmd.ExecuteReader())
      {
        ds.Load(r, LoadOption.OverwriteChanges, string.Empty);
        r.Close();
      }
      return ds;
    }

    public void BaseJoin(ObjectRepository objectRepository)
    {
      if (objectRepository == null) return;
      BaseJoin(objectRepository.BaseRepository);
      string alias = string.Format("J{0}", ++mJoinCount);
      AddProperties(objectRepository, alias);
      mJoins.Add(string.Format("INNER JOIN [{0}].[{1}] AS [{2}] ON [T].[id]=[{2}].[id]", objectRepository.Schema, objectRepository.Catalog, alias));
    }

    public DataSet QueryById(Guid id, string connectionString)
    {
      using (SqlConnection con = new SqlConnection(connectionString))
      {
        try
        {
          con.Open();

          using (SqlCommand cmd = new SqlCommand(string.Format("SELECT {0} FROM {1} {2} WHERE [T].[id]=@id", string.Join(", ", mColumns), mTableName, string.Join(" AND ", mJoins)), con))
          {
            cmd.Parameters.AddWithValue("@id", id);
            return ExecuteDataSet(cmd);
          }
        }
        finally
        {
          con.Close();
        }
      }
    }

    public DataSet QueryByOwner(Guid owner, string connectionString)
    {
      using (SqlConnection con = new SqlConnection(connectionString))
      {
        try
        {
          con.Open();

          using (SqlCommand cmd = new SqlCommand(string.Format("SELECT {0} FROM {1} {2} WHERE [{3}].[{4}]=@owner", string.Join(", ", mColumns), mTableName, string.Join(" AND ", mJoins), mOwnerAlias, mOwnerColumnName), con))
          {
            cmd.Parameters.AddWithValue("@owner", owner);
            return ExecuteDataSet(cmd);
          }
        }
        finally
        {
          con.Close();
        }
      }
    }

    public DataSet QueryAll(string connectionString)
    {
      using (SqlConnection con = new SqlConnection(connectionString))
      {
        try
        {
          con.Open();

          using (SqlCommand cmd = new SqlCommand(string.Format("SELECT {0} FROM {1} {2}{3}", string.Join(", ", mColumns), mTableName, string.Join(" AND ", mJoins), mIsCyclic ? string.Format(" WHERE [{0}].[{1}] IS NULL", mOwnerAlias, mOwnerColumnName) : string.Empty), con))
          {
            return ExecuteDataSet(cmd);
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
