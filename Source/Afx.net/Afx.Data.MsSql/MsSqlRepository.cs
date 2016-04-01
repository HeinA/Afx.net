using Afx.ComponentModel.Composition;
using Afx.Data.Configuration;
using Afx.Data.MsSql.Configuration;
using Afx.Data.MsSql.SchemaValidation;
using Afx.ObjectModel;
using Afx.ObjectModel.Collections;
using Afx.ObjectModel.Description.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Afx.Data.MsSql
{
  public class MsSqlRepository : IDataRepository
  {
    #region string RepositoryName

    string mRepositoryName;
    public string RepositoryName
    {
      get { return mRepositoryName; }
      private set { mRepositoryName = value; }
    }

    #endregion

    #region string ConnectionString

    string mConnectionString;
    public string ConnectionString
    {
      get { return mConnectionString; }
      private set { mConnectionString = value; }
    }

    #endregion

    #region string DatabaseName

    string mDatabaseName;
    public string DatabaseName
    {
      get { return mDatabaseName; }
      private set { mDatabaseName = value; }
    }

    #endregion

    #region string MasterConnectionString

    string mMasterConnectionString;
    public string MasterConnectionString
    {
      get { return mMasterConnectionString; }
      private set { mMasterConnectionString = value; }
    }

    #endregion


    #region void Initialize(...)

    public void Initialize(Repository repositoryConfiguration)
    {
      RepositoryName = repositoryConfiguration.Name;
      ConnectionString = repositoryConfiguration.ConnectionString;

      SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder(ConnectionString);
      DatabaseName = cb.InitialCatalog;
      cb.InitialCatalog = "master";
      MasterConnectionString = cb.ToString();

      if (repositoryConfiguration.ValidateSchema)
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
        {
          ValidateRepository();
          scope.Complete();
        }
      }
    }

    #endregion

    #region AfxObject LoadObject(...)

    public AfxObject LoadObject(Guid id, ObjectRepository objectRepository)
    {
      foreach (var or in objectRepository.ConcreteRepositories)
      {
        DataSet ds = new SqlQuery(or).QueryById(id, ConnectionString);
        if (ds.Tables[0].Rows.Count == 1)
        {
          AfxObject obj = (AfxObject)Activator.CreateInstance(or.SourceType);
          LoadObject(obj, ds.Tables[0].Rows[0], or, new LoadContext());
          return obj;
        }
      }

      throw new InvalidOperationException(string.Format(Properties.Resources.InvalidObject, id));
    }

    #endregion

    #region IEnumerable<AfxObject> LoadObjects(...)

    public IEnumerable<AfxObject> LoadObjects(ObjectRepository objectRepository)
    {
      foreach (var or in objectRepository.ConcreteRepositories)
      {
        DataSet ds = new SqlQuery(or).QueryAll(ConnectionString);
        foreach(DataRow dr in ds.Tables[0].Rows)
        {
          AfxObject obj = (AfxObject)Activator.CreateInstance(or.SourceType);
          LoadObject(obj, dr, or, new LoadContext());
          yield return obj;
        }
      }
    }

    #endregion

    #region void LoadObject(...)

    void LoadObject(AfxObject obj, DataRow dr, ObjectRepository objectRepository, LoadContext context)
    {
      if (objectRepository == null) return;

      #region Load Base Class

      LoadObject(obj, dr, objectRepository.BaseRepository, context);

      #endregion

      #region  Load Simple Properties

      foreach (var p in objectRepository.Properties.OfType<SimpleProperty>().Where(p1 => p1.AllowRead))
      {
        object val = dr[p.Name];
        if (val != DBNull.Value && val != null) p.PropertyInfo.SetValue(obj, val);
      }

      //Register the object in the Load Context after its's simple properties has loaded (id etc)
      context.RegisterObject(obj);

      #endregion

      #region  Load Collections

      foreach (var p in objectRepository.Properties.OfType<CollectionProperty>())
      {
        object col = p.PropertyInfo.GetValue(obj);
        IAssociativeCollection acol = (IAssociativeCollection)col;
        if (acol != null)
        {
          LoadAssociativeObjects(obj, acol, context);
        }
        else
        {
          IObjectCollection ocol = (IObjectCollection)col;
          LoadOwnedObjects(obj, ocol, context);
        }
      }

      #endregion

      #region  Load Complex Properties

      foreach (var p in objectRepository.Properties.OfType<ComplexProperty>().Where(p1 => p1.AllowRead))
      {
        if (p.PropertyInfo.Name == "Owner") continue;

        object referenceId = dr[p.Name];
        if (referenceId != null && referenceId != DBNull.Value)
        {
          Guid referenceGuid = (Guid)referenceId;
          AfxObject referenceObject = null;

          // Set a reference to the object if it has been loaded earlier during this Load.
          if (context.IsRegistered(referenceGuid))
          {
            referenceObject = context.GetObject(referenceGuid);
          }

          // Otherwise set a reference to the object from the Memory Cache.
          if (referenceObject == null)
          {
            //TODO: Load from Memory Cache
          }

          // Otherwise load the object from the Database.
          if (referenceObject == null)
          {
            ObjectRepository or = ObjectRepository.GetRepository(p.PropertyType);
            referenceObject = LoadObject(referenceGuid, or);
          }

          if (referenceObject != null)
          {
            p.PropertyInfo.SetValue(obj, referenceObject);
          }
        }
      }

      #endregion
    }

    #endregion

    #region void LoadAssociativeObjects(...)

    void LoadAssociativeObjects(AfxObject obj, IAssociativeCollection acol, LoadContext context)
    {
      ObjectRepository orAO = ObjectRepository.GetRepository(acol.AssociativeType);

      foreach (var or in orAO.ConcreteRepositories)
      {
        DataSet ds = new SqlQuery(or).QueryByOwner(obj.Id, ConnectionString);
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
          AfxObject obj1 = (AfxObject)Activator.CreateInstance(or.SourceType);
          LoadObject(obj1, dr, or, context);
          IAssociativeObject aobj = (IAssociativeObject)obj1;
          acol.Add(aobj.Reference, aobj);
        }
      }
    }

    #endregion

    #region void LoadOwnedObjects(...)

    void LoadOwnedObjects(AfxObject obj, IObjectCollection col, LoadContext context)
    {
      ObjectRepository orCol = ObjectRepository.GetRepository(col.ItemType);

      foreach (var or in orCol.ConcreteRepositories)
      {
        DataSet ds = new SqlQuery(or).QueryByOwner(obj.Id, ConnectionString);
        foreach (DataRow dr in ds.Tables[0].Rows)
        {
          AfxObject obj1 = (AfxObject)Activator.CreateInstance(or.SourceType);
          LoadObject(obj1, dr, or, context);
          col.Add(obj1);
        }
      }
    }

    #endregion


    #region AfxObject SaveObject(...)

    public AfxObject SaveObject(AfxObject obj, ObjectRepository objectRepository)
    {
      return null;
    }

    #endregion


    #region Database Schema Validation

    #region void ValidateRepository(...)

    void ValidateRepository()
    {
      ValidateDatabase();
      ValidateSchemas();
      ValidateTables();
      ValidateRelationships();
      ValidateTriggers();
      ValidateViews();
      DropRemovedTables();
    }

    #endregion

    #region Collection<string> Schemas

    Collection<string> mSchemas = new Collection<string>();
    Collection<string> Schemas
    {
      get { return mSchemas; }
      set { mSchemas = value; }
    }

    #endregion

    #region void ValidateDatabase(...)

    void ValidateDatabase()
    {
      using (SqlConnection con = new SqlConnection(MasterConnectionString))
      {
        con.Open();

        using (SqlCommand cmd = new SqlCommand("select db_id(@dn)", con))
        {
          cmd.Parameters.AddWithValue("@dn", DatabaseName);
          object o = cmd.ExecuteScalar();
          if (o == DBNull.Value)
          {
            CreateDatabase();
            VerifyDatabase();
          }
          else
          {
            VerifyDatabase();
          }
        }

        con.Close();
      }
    }

    private void VerifyDatabase()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
      {
        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
          con.Open();

          using (SqlCommand cmd = new SqlCommand("EXEC sp_configure filestream_access_level, 2", con))
          {
            cmd.ExecuteNonQuery();
          }

          using (SqlCommand cmd = new SqlCommand("RECONFIGURE", con))
          {
            cmd.ExecuteNonQuery();
          }

          if (FilestreamConfiguration.Default != null)
          {
            foreach (FileGroup fg in FilestreamConfiguration.Default.Groups.OfType<FileGroup>().Where(g => g.Repository == RepositoryName))
            {
              string sql = "SELECT F.name as [FileName], F.physical_name as [FileLocation] FROM sys.filegroups AS FG LEFT OUTER JOIN sys.database_files AS F ON FG.data_space_id = F.data_space_id where FG.type='FD' and FG.name=@fg";
              DataSet ds = null;
              using (SqlCommand cmd = new SqlCommand(sql, con))
              {
                cmd.Parameters.AddWithValue("@fg", fg.Name);
                ds = DbHelper.ExecuteDataSet(cmd);
              }

              if (ds.Tables[0].Rows.Count == 0)
              {
                sql = string.Format("ALTER DATABASE [{0}] ADD FILEGROUP {1} CONTAINS FILESTREAM", DatabaseName, fg.Name);
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                  cmd.ExecuteNonQuery();
                }
              }

              foreach (Filestream fs in fg.Files)
              {
                if (!DoesFileExist(fs.Name, ds))
                {
                  DirectoryInfo di = new DirectoryInfo(fs.Folder);
                  if (di.Parent != null && !di.Parent.Exists) di.Parent.Create();

                  sql = string.Format("ALTER DATABASE [{0}] ADD FILE (NAME = {3}, FILENAME = '{2}') TO FILEGROUP {1}", DatabaseName, fg.Name, fs.Folder, fs.Name);
                  using (SqlCommand cmd = new SqlCommand(sql, con))
                  {
                    cmd.ExecuteNonQuery();
                  }
                }
              }
            }
          }

          con.Close();
        }

        scope.Complete();
      }
    }

    bool DoesFileExist(string fileName, DataSet dsFiles)
    {
      foreach (DataRow dr in dsFiles.Tables[0].Rows)
      {
        if (((string)dr["FileName"]) == fileName) return true;
      }
      return false;
    }

    void CreateDatabase()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
      {
        using (SqlConnection con = new SqlConnection(MasterConnectionString))
        {
          con.Open();

          string sql = string.Format("CREATE DATABASE [{0}]", DatabaseName);
          using (SqlCommand cmd = new SqlCommand(sql, con))
          {
            cmd.ExecuteNonQuery();
          }

          con.Close();
        }

        scope.Complete();
      }
    }

    #endregion

    #region void ValidateSchemas(...)

    void ValidateSchemas()
    {
      using (SqlConnection con = new SqlConnection(ConnectionString))
      {
        con.Open();

        foreach (Type t in Metadata.KnownAfxTypes)
        {
          ObjectRepository or = ObjectRepository.GetRepository(t);
          if (or == null) continue;

          if (!string.IsNullOrWhiteSpace(or.Schema) && !Schemas.Contains(or.Schema))
          {
            Schemas.Add(or.Schema);
            string sqlSchema = string.Format("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}') BEGIN EXEC('CREATE SCHEMA [{0}]') END", or.Schema);
            using (SqlCommand cmd = new SqlCommand(sqlSchema, con))
            {
              cmd.ExecuteNonQuery();
            }
          }
        }

        con.Close();
      }
    }

    #endregion

    #region void ValidateTables(...)

    void ValidateTables()
    {
      using (SqlConnection con = new SqlConnection(ConnectionString))
      {
        con.Open();

        foreach (Type t in Metadata.KnownAfxTypes)
        {
          ObjectRepository or = ObjectRepository.GetRepository(t);
          if (or == null) continue;

          if (!or.IsReadOnly)
          {
            string sqlTableExists = string.Format("SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND  TABLE_NAME = '{1}'", GetSchema(or), or.Catalog);
            using (SqlCommand cmd = new SqlCommand(sqlTableExists, con))
            {
              int i = (int)cmd.ExecuteScalar();
              if (i == 0)
              {
                CreateTable(or);
              }
              else
              {
                ValidateTableColumns(or);
              }
            }
          }
        }

        con.Close();
      }
    }

    #endregion

    #region void CreateTable(...)

    void CreateTable(ObjectRepository or)
    {
      using (SqlConnection con = new SqlConnection(ConnectionString))
      {
        con.Open();

        string sql = null;
        using (StringWriter sw = new StringWriter())
        {
          //sw.Write("CREATE TABLE [{0}].[{1}] ([ix] INT NOT NULL IDENTITY(1,1) PRIMARY KEY, [id] UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL UNIQUE", GetSchema(or), or.Catalog);
          sw.Write("CREATE TABLE [{0}].[{1}] ([id] UNIQUEIDENTIFIER ROWGUIDCOL NOT NULL, [ix] INT NOT NULL IDENTITY(1,1)", GetSchema(or), or.Catalog);
          foreach (var pp in or.Properties)
          {
            SimpleProperty sp = pp as SimpleProperty;
            if (sp != null && sp.IsId) continue;

            sw.Write(", ");
            sw.Write("[{0}] {1}{2}", pp.Name, GetFullSqlDataType(pp), !string.IsNullOrWhiteSpace(or.BinaryStorageName) ? " FILESTREAM" : string.Empty);
            if (!pp.AllowNull) sw.Write(" NOT NULL");
          }
          sw.Write(") ON [PRIMARY]");
          if (!string.IsNullOrWhiteSpace(or.BinaryStorageName))
          {
            sw.Write(" FILESTREAM_ON {0}", or.BinaryStorageName);
          }
          sql = sw.ToString();
        }

        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
          cmd.ExecuteNonQuery();
        }

        sql = string.Format("ALTER TABLE [{0}].[{1}] ADD CONSTRAINT PK_{2} PRIMARY KEY NONCLUSTERED (id)", GetSchema(or), or.Catalog, or.Catalog.Replace(" ", "_"));
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
          cmd.ExecuteNonQuery();
        }

        sql = string.Format("CREATE UNIQUE CLUSTERED INDEX CIX_{0} ON [{1}].[{2}](ix)", or.Catalog.Replace(" ", "_"), GetSchema(or), or.Catalog);
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
          cmd.ExecuteNonQuery();
        }

        sql = string.Format("EXEC sys.sp_addextendedproperty @name = N'Afx Managed', @value = N'Yes', @level0type = N'SCHEMA', @level0name = '{0}', @level1type = N'TABLE', @level1name = '{1}'", GetSchema(or), or.Catalog);
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
          cmd.ExecuteNonQuery();
        }

        con.Close();
      }
    }

    #endregion

    #region void ValidateTableColumns(...)

    void ValidateTableColumns(ObjectRepository or)
    {
      using (SqlConnection con = new SqlConnection(ConnectionString))
      {
        con.Open();

        Type t = typeof(ITableAlterationManager<>).MakeGenericType(or.SourceType);
        ITableManager am = (ITableManager)CompositionHelper.GetExportedValueOrDefault(t);
        bool bTableChangeFlag = false;


        #region Get Database Columns

        DataSet ds = null;
        string sqlColumns = "SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@ts AND TABLE_NAME=@tn";
        using (SqlCommand cmd = new SqlCommand(sqlColumns, con))
        {
          cmd.Parameters.AddWithValue("@ts", GetSchema(or));
          cmd.Parameters.AddWithValue("@tn", or.Catalog);
          ds = DbHelper.ExecuteDataSet(cmd);
        }

        #endregion

        #region Add Missing Columns (Allow nulls)

        Queue<ObjectProperty> added = new Queue<ObjectProperty>();

        foreach (var pp in or.Properties)
        {
          using (StringWriter sw = new StringWriter())
          {
            CollectionProperty colp = pp as CollectionProperty;
            if (colp != null) return;

            SimpleProperty sp = pp as SimpleProperty;
            if (sp != null && sp.IsId) continue;

            if (!HasColumn(or, pp, ds))
            {
              if (am != null && !bTableChangeFlag)
              {
                bTableChangeFlag = true;
                am.BeforeTableChange(this);
              }

              sw.Write("ALTER TABLE [{0}].[{1}]", GetSchema(or), or.Catalog);
              sw.Write(" ADD [{0}] {1}{2} NULL", pp.Name, GetFullSqlDataType(pp), !string.IsNullOrWhiteSpace(or.BinaryStorageName) ? " FILESTREAM" : string.Empty);
              using (SqlCommand cmd = new SqlCommand(sw.ToString(), con))
              {
                cmd.ExecuteNonQuery();
              }
              added.Enqueue(pp);
            }
          }
        }

        while (added.Count > 0)
        {
          ObjectProperty p = added.Dequeue();
          if (am != null) am.AfterColumnAdded(this, p.Name);
        }

        #endregion

        #region Get Database Columns

        using (SqlCommand cmd = new SqlCommand(sqlColumns, con))
        {
          cmd.Parameters.AddWithValue("@ts", GetSchema(or));
          cmd.Parameters.AddWithValue("@tn", or.Catalog);
          ds = DbHelper.ExecuteDataSet(cmd);
        }

        #endregion

        #region Update Column Type & Nullability

        foreach (var pp in or.Properties)
        {
          if (!MustUpdateColumn(or, pp, ds)) continue;

          if (am != null)
          {
            if (!bTableChangeFlag)
            {
              bTableChangeFlag = true;
              am.BeforeTableChange(this);
            }
          }

          using (StringWriter sw = new StringWriter())
          {
            sw.Write("ALTER TABLE [{0}].[{1}] ALTER COLUMN ", GetSchema(or), or.Catalog);
            SimpleProperty sp = pp as SimpleProperty;
            if (sp != null && sp.IsId) continue;

            sw.Write("[{0}] {1}{2}", pp.Name, GetFullSqlDataType(pp), !string.IsNullOrWhiteSpace(or.BinaryStorageName) ? " FILESTREAM" : string.Empty);
            if (!pp.AllowNull) sw.Write(" NOT NULL");
            using (SqlCommand cmd = new SqlCommand(sw.ToString(), con))
            {
              cmd.ExecuteNonQuery();
            }
          }
        }

        #endregion

        #region Drop Removed Columns

        foreach (string droppedColumn in GetDroppedColumns(or, ds))
        {
          if (am != null)
          {
            if (!bTableChangeFlag)
            {
              bTableChangeFlag = true;
              am.BeforeTableChange(this);
            }
            am.BeforeColumnDrop(this, droppedColumn);
          }

          using (StringWriter sw = new StringWriter())
          {
            sw.Write("ALTER TABLE [{0}].[{1}]", GetSchema(or), or.Catalog);
            sw.Write(" DROP COLUMN [{0}]", droppedColumn);
            using (SqlCommand cmd = new SqlCommand(sw.ToString(), con))
            {
              cmd.ExecuteNonQuery();
            }
          }
        }

        #endregion

        if (bTableChangeFlag)
        {
          am.AfterTableChanged(this);
        }

        con.Close();
      }
    }

    #endregion

    #region bool HasColumn(...)

    bool HasColumn(ObjectRepository or, ObjectProperty p, DataSet dsColumns)
    {
      foreach (DataRow dr in dsColumns.Tables[0].Rows)
      {
        if (((string)dr["COLUMN_NAME"]).ToUpperInvariant() == p.Name.ToUpperInvariant()) return true;
      }
      return false;
    }

    #endregion

    #region bool MustUpdateColumn(...)

    bool MustUpdateColumn(ObjectRepository or, ObjectProperty p, DataSet dsColumns)
    {
      foreach (DataRow dr in dsColumns.Tables[0].Rows)
      {
        if (((string)dr["COLUMN_NAME"]).ToUpperInvariant() == p.Name.ToUpperInvariant())
        {
          if (p.AllowNull != ((string)dr["IS_NULLABLE"] != "NO")) return true;
          if (GetFullSqlDataType(p) != ComposeFullSqlDataType(dr))
          {
            return true;
          }
        }
      }
      return false;
    }

    #endregion

    #region IEnumerable<string> GetDroppedColumns(...)

    IEnumerable<string> GetDroppedColumns(ObjectRepository or, DataSet dsColumns)
    {
      Collection<string> dropped = new Collection<string>();
      foreach (DataRow dr in dsColumns.Tables[0].Rows)
      {
        if (((string)dr["COLUMN_NAME"]) == "ix") continue;
        if (or.Properties.FirstOrDefault(p => p.Name.ToUpperInvariant() == ((string)dr["COLUMN_NAME"]).ToUpperInvariant()) == null)
        {
          dropped.Add((string)dr["COLUMN_NAME"]);
        }
      }
      return dropped;
    }

    #endregion

    #region Data Type Conversion

    const string DT_INT = "INT";
    const string DT_BIT = "BIT";
    const string DT_DECIMAL = "DECIMAL";
    const string DT_VARBINARY = "VARBINARY";
    const string DT_NVARCHAR = "NVARCHAR";
    const string DT_UNIQUEIDENTIFIER = "UNIQUEIDENTIFIER";

    #region string GetSqlDataType(...)

    string GetSqlDataType(ObjectProperty p)
    {
      ComplexProperty cp = p as ComplexProperty;
      if (cp != null || p.PropertyType == typeof(Guid)) return DT_UNIQUEIDENTIFIER;

      Type dataType = p.PropertyType;
      Type nt = AssemblyHelper.GetGenericSubClass(typeof(Nullable<>), p.PropertyType);
      if (nt != null)
      {
        dataType = nt.GetGenericArguments()[0];
      }

      if (dataType == typeof(int))
      {
        return DT_INT;
      }
      else if (dataType == typeof(bool))
      {
        return DT_BIT;
      }
      else if (dataType == typeof(decimal))
      {
        return DT_DECIMAL;
      }
      else if (dataType == typeof(string))
      {
        return DT_NVARCHAR;
      }
      else if (dataType == typeof(byte[]))
      {
        return DT_VARBINARY;
      }

      throw new InvalidOperationException(); //TODO: Message
    }

    #endregion

    #region string GetFullSqlDataType(...)

    string GetFullSqlDataType(ObjectProperty p)
    {
      string dataType = GetSqlDataType(p);
      switch (dataType)
      {
        case DT_VARBINARY:
          {
            return string.Format("{0}(MAX)", dataType);
          }

        case DT_NVARCHAR:
          {
            int size = p.Size;
            if (size == 0) size = 50;
            if (size == int.MaxValue) return string.Format("{0}(MAX)", dataType);
            return string.Format("{0}({1})", dataType, size);
          }

        case DT_DECIMAL:
          {
            int size = p.Size;
            if (size == 0) size = 18;
            int decimals = p.Precision;
            if (decimals == 0) decimals = 4;
            return string.Format("{0}({1},{2})", dataType, size, decimals);
          }
      }

      return dataType;
    }

    #endregion

    #region string ComposeFullSqlDataType(...)

    string ComposeFullSqlDataType(DataRow dr)
    {
      string dataType = ((string)dr["DATA_TYPE"]).ToUpperInvariant();
      switch (dataType)
      {
        case DT_VARBINARY:
          {
            return string.Format("{0}(MAX)", dataType);
          }

        case DT_NVARCHAR:
          {
            int size = (int)dr["CHARACTER_MAXIMUM_LENGTH"];
            if (size == -1) return string.Format("{0}(MAX)", dataType);
            return string.Format("{0}({1})", dataType, size);
          }

        case DT_DECIMAL:
          {
            int size = (int)dr["NUMERIC_SCALE"];
            int presision = (int)dr["NUMERIC_PRECISION"];
            return string.Format("{0}({1},{2})", dataType, size, presision);
          }
      }

      return dataType;
    }

    #endregion

    #endregion

    #region string GetSchema(...)

    string GetSchema(ObjectRepository or)
    {
      if (string.IsNullOrWhiteSpace(or.Schema)) return "dbo";
      return or.Schema;
    }

    #endregion

    #region void ValidateRelationships(...)

    void ValidateRelationships()
    {
      foreach (Type t in Metadata.KnownAfxTypes)
      {
        ObjectRepository or = ObjectRepository.GetRepository(t);
        if (or == null) continue;

        if (or.SourceMetadata.BaseType != null) ValidateRelationship(t, AfxObject.IdProperty, or.SourceMetadata.BaseType, AfxObject.IdProperty, true);

        foreach (var cp in or.Properties.OfType<ComplexProperty>())
        {
          if (or.SourceMetadata.OwnerType != null && or.SourceMetadata.OwnerType.Equals(cp.PropertyType))
          {
            if (t.Equals(cp.PropertyType)) ValidateRelationship(t, cp.Name, cp.PropertyType, AfxObject.IdProperty, false);
            else ValidateRelationship(t, cp.Name, cp.PropertyType, AfxObject.IdProperty, true);
          }
          else
          {
            ValidateRelationship(t, cp.Name, cp.PropertyType, AfxObject.IdProperty, false);
          }
        }
      }
    }

    void ValidateRelationship(Type owner, string ownerColumn, Type required, string requiredColumn, bool cascadeDelete)
    {
      ObjectRepository ownerRepo = ObjectRepository.GetRepository(owner);
      ObjectRepository requiredRepo = ObjectRepository.GetRepository(required);

      using (SqlConnection con = new SqlConnection(ConnectionString))
      {
        con.Open();
        string sql = @" select	RC.CONSTRAINT_NAME as ConstraintName
                        ,		CCU1.TABLE_SCHEMA as OwnerSchema
                        ,		CCU1.TABLE_NAME as OwnerTable
                        ,		CCU1.COLUMN_NAME as OwnerColumn
                        ,		CCU2.TABLE_SCHEMA as RequiredSchema
                        ,		CCU2.TABLE_NAME as RequiredTable
                        ,		CCU2.COLUMN_NAME as RequiredColumn
                        ,		RC.UPDATE_RULE as [Update]
                        ,		RC.DELETE_RULE as [Delete]
                        from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
                        inner join INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE CTU on CTU.CONSTRAINT_NAME=RC.CONSTRAINT_NAME
                        inner join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU1 on RC.CONSTRAINT_NAME=CCU1.CONSTRAINT_NAME
                        inner join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU2 on RC.UNIQUE_CONSTRAINT_NAME=CCU2.CONSTRAINT_NAME
                        where CCU1.TABLE_SCHEMA=@osn and CCU1.TABLE_NAME=@otn and CCU1.COLUMN_NAME=@ocn
                        and   CCU2.TABLE_SCHEMA=@rsn and CCU2.TABLE_NAME=@rtn and CCU2.COLUMN_NAME=@rcn";
        DataSet ds = null;
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
          cmd.Parameters.AddWithValue("@osn", ownerRepo.Schema);
          cmd.Parameters.AddWithValue("@otn", ownerRepo.Catalog);
          cmd.Parameters.AddWithValue("@ocn", ownerColumn);
          cmd.Parameters.AddWithValue("@rsn", requiredRepo.Schema);
          cmd.Parameters.AddWithValue("@rtn", requiredRepo.Catalog);
          cmd.Parameters.AddWithValue("@rcn", requiredColumn);
          ds = DbHelper.ExecuteDataSet(cmd);
        }

        string sqlCreateConstraint = string.Format("ALTER TABLE [{0}].[{1}] ADD CONSTRAINT FK_{1}_{2} FOREIGN KEY ([{3}]) REFERENCES [{4}].[{2}]	([{5}]) ON UPDATE NO ACTION ON DELETE {6}", ownerRepo.Schema, ownerRepo.Catalog, requiredRepo.Catalog, ownerColumn, requiredRepo.Schema, requiredColumn, cascadeDelete ? "CASCADE" : "NO ACTION");
        if (ds.Tables[0].Rows.Count == 0)
        {
          using (SqlCommand cmd = new SqlCommand(sqlCreateConstraint, con))
          {
            cmd.ExecuteNonQuery();
          }
        }
        else
        {
          if (ds.Tables[0].Rows.Count != 1) throw new InvalidOperationException(); //TODO: Message

          DataRow dr = ds.Tables[0].Rows[0];
          string sUpdate = (string)dr["Update"];
          string sDelete = (string)dr["Delete"];
          string sDeleteCheck = cascadeDelete ? "CASCADE" : "NO ACTION";
          if (sUpdate != "NO ACTION" || sDelete != sDeleteCheck)
          {
            string sqlDropConstraint = string.Format("ALTER TABLE [{0}].[{1}] DROP CONSTRAINT {2}", ownerRepo.Schema, ownerRepo.Catalog, dr["ConstraintName"]);
            using (SqlCommand cmd = new SqlCommand(sqlDropConstraint, con))
            {
              cmd.ExecuteNonQuery();
            }
            using (SqlCommand cmd = new SqlCommand(sqlCreateConstraint, con))
            {
              cmd.ExecuteNonQuery();
            }
          }
        }
      }
    }

    #endregion





    #region ValidateTriggers(...)

    void ValidateTriggers()
    {
    }

    #endregion

    #region void ValidateViews(...)

    void ValidateViews()
    {
      //using (SqlConnection con = new SqlConnection(ConnectionString))
      //{
      //  con.Open();

      //  foreach (Type t in Metadata.KnownAfxTypes)
      //  {
      //    ObjectRepository or = ObjectRepository.GetRepository(t);

      //    if (or.IsReadOnly)
      //    {
      //      string sqlDropCreateView = string.Format("", GetSchema(or), or.Catalog);
      //      using (SqlCommand cmd = new SqlCommand(sqlDropCreateView, con))
      //      {
      //      }
      //    }
      //  }

      //  con.Close();
      //}
    }

    #endregion

    #region void DropRemovedTables(...)

    private void DropRemovedTables()
    {
    }

    #endregion

    #endregion
  }
}
