using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.Data.MsSql
{
  internal class DbHelper
  {
    public static DataSet ExecuteDataSet(SqlCommand cmd)
    {
      System.Data.DataSet ds = new System.Data.DataSet();
      ds.EnforceConstraints = false;
      ds.Locale = CultureInfo.InvariantCulture;
      using (IDataReader r = cmd.ExecuteReader())
      {
        ds.Load(r, LoadOption.OverwriteChanges, string.Empty);
        r.Close();
      }

      return ds;
    }
  }
}
