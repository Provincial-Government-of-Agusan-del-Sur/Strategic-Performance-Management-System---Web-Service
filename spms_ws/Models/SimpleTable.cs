using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web; 
using System.Web.Script.Serialization;

namespace IMS.Classess
{

    public class SimpleTable
    {
        private ArrayList _columns;
        private ArrayList[] _rows;


        public SimpleTable(DataTable dtSource)
        {
            setColumns(dtSource.Columns);
            setRows(dtSource.Rows);
        }


        public void setColumns(DataColumnCollection columns)
        {
            this._columns = new ArrayList();
            foreach (DataColumn column in columns)
            {
                this._columns.Add(column.ColumnName);
            }
        }


        public void setRows(DataRowCollection rows)
        {
            int i = 0;
            this._rows = new ArrayList[rows.Count];
            foreach (DataRow row in rows)
            {
                this._rows[i] = new ArrayList(row.ItemArray);
                i++;
            }
        }


        public ArrayList Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }


        public ArrayList[] Rows
        {
            get
            {
                return this._rows;
            }
            set
            {
                this._rows = value;
            }
        }
    }


    public class SerializeDT
    {
        public static string DataTableToJSON(DataTable table)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            var serializer = new JavaScriptSerializer();

            serializer.MaxJsonLength = Int32.MaxValue;

            return serializer.Serialize(list);
        }
    }

}