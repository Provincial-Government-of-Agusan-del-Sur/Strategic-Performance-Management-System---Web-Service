using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Dynamic;
using IMS.Models;
using System.IO;

namespace IMS.Classess
{
    public static class ExecuteProcedure
    {

        public static void NonQuery(this string query)
        {
            OleDbHelper.ExecuteNonQuery(ConfigurationManager.ConnectionStrings["MyConnection"].ToString(), CommandType.Text, query);
        }
       
        public static DataTable DataSet(this string query)
        {
            return OleDbHelper.ExecuteDataset(ConfigurationManager.ConnectionStrings["MyConnection"].ToString(), CommandType.Text, query).Tables[0];
        }
       
        public static int Scalar(this string query)
        {
            return Convert.ToInt32(OleDbHelper.ExecuteScalar(ConfigurationManager.ConnectionStrings["MyConnection"].ToString(), CommandType.Text, query));
        }

        public static string ScalarString(this string query)
        {
            return Convert.ToString(OleDbHelper.ExecuteScalar(ConfigurationManager.ConnectionStrings["MyConnection"].ToString(), CommandType.Text, query));
        }

     

           
        public class DCLASS : DynamicObject
        {
            private IDictionary<string, object> _values;

            public DCLASS(IDictionary<string, object> values)
            {
                _values = values;
            }
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (_values.ContainsKey(binder.Name))
                {
                    result = _values[binder.Name];
                    return true;
                }
                result = null;
                return false;
            }
        }

        //public static class DYNAMICMODEL
        //{
            public static dynamic ToModel(this DataTable dt)
            {
                if (dt.Rows.Count > 0)
                {
                    #region SINGLE ROW

                    if (dt.Rows.Count == 1)
                    {
                        var values = new Dictionary<string, object>();
                        foreach (DataColumn cl in dt.Columns)
                        {
                            values.Add(cl.ColumnName, dt.Rows[0][cl.Ordinal]);
                        }

                        dynamic result = new DCLASS(values);
                        return result;
                    }
                    #endregion

                    #region MULTIPLE ROWS
                    else
                    {
                        List<dynamic> list = new List<dynamic>();
                        foreach (DataRow rw in dt.Rows)
                        {
                            var values = new Dictionary<string, object>();
                            foreach (DataColumn cl in dt.Columns)
                            {
                                values.Add(cl.ColumnName, rw[cl.Ordinal]);
                            }
                            list.Add(new DCLASS(values));
                           
                        }
                        return list;
                    }
                    #endregion
                }

                return null;
            }


        //}
    }
}