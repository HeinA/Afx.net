using Afx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql.SchemaValidation
{
  public interface ITableManager
  {
    void BeforeTableChange(MsSqlRepository repository);
    void BeforeColumnDrop(MsSqlRepository repository, string columnName);
    void AfterColumnAdded(MsSqlRepository repository, string columnName);
    void AfterTableChanged(MsSqlRepository repository);
  }

  public interface ITableAlterationManager<T> : ITableManager
    where T : AfxObject
  {
  }
}
