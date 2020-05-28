using Dapper;
using Dapper.Bulk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace dbtest
{
    [Table("new_employees")]
    public class DBModel
    {
        [Key]
        public int id_num { get; set; }

        public string fname { get; set; }

        //少欄位是可被接受的，但僅只於符合null
        //public char minit { get; set; }

        public string lname { get; set; }

        //不可以有不存在DB欄位的表
        public string test { get; set; }
    }


    class Program
    {
        const int testcount = 1;
        const string connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = testdb; Integrated Security = True";

        enum InsertType
        {
            cmdtype,
            dappertype,
            SqlBulkCopy_1,
            SqlBulkCopy_2,
            dapperbulk,
        }


        static void Main(string[] args)
        {

            var flag = InsertType.SqlBulkCopy_2;

            DateTime dt = DateTime.UtcNow.AddHours(8);
            switch (flag)
            {
                case InsertType.cmdtype:
                    cmdtype();
                    break;
                case InsertType.dappertype:
                    dappertype(); //1000筆1秒689 , 10000筆4秒227
                    break;
                //case InsertType.SqlBulkCopy_1: //test
                //    SqlBulkCopy();
                //    break;
                case InsertType.SqlBulkCopy_2: //1000筆1秒689,10000筆1秒207
                    SqlBulkCopy2();
                    break;
                case InsertType.dapperbulk:
                    dapperbulk(); //1000筆1秒562,10000筆1秒498
                    break;

            }
            DateTime edt = DateTime.UtcNow.AddHours(8);
            var test = edt.Subtract(dt);
           
            Console.WriteLine($"{test.Minutes}分{test.Seconds}秒{test.Milliseconds}");
        }

        static void dapperbulk()
        {
            using (SqlConnection conn =
               new SqlConnection(connectionString))
            {
                conn.Open();
                List<DBModel> list = new List<DBModel>();
                for(int i = 0; i< testcount; i++)
                {

                    list.Add(new DBModel() { fname = "k", lname = "k" });//, minit = 'k' });
                }
                conn.BulkInsert(list);
            }
           
        }

        static void SqlBulkCopy2()
        {

            using (SqlConnection destinationConnection =
                    new SqlConnection(connectionString))
            {
                destinationConnection.Open();

                //using (SqlBulkCopy bulkCopy = new SqlBulkCopy(
                //     connectionString, SqlBulkCopyOptions.KeepIdentity |
                //     SqlBulkCopyOptions.UseInternalTransaction))

                using (SqlBulkCopy bulkCopy =
       new SqlBulkCopy(destinationConnection))
                {
                    bulkCopy.DestinationTableName =
                        "dbo.new_employees";

                    try
                    {

                        List<DBModel> list = new List<DBModel>();
                        bulkCopy.BulkCopyTimeout = 1000;
                        bulkCopy.BatchSize = 1000;
                   
                        bulkCopy.ColumnMappings.Add("fname", "fname");
                        bulkCopy.ColumnMappings.Add("lname", "lname");


                        for (int i = 0; i < testcount; i++)
                        {

                            list.Add(new DBModel() { fname = "last", lname = "error" });//, minit = 'k' });
                        }
                        DataTable reader = ToDataTable(list);

                        bulkCopy.WriteToServer(reader);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {

                    }
                }

            }

        }

        static void SqlBulkCopy()
        {
            using (SqlConnection sourceConnection =
                               new SqlConnection(connectionString))
            {
                sourceConnection.Open();

                // Perform an initial count on the destination table.
                SqlCommand commandRowCount = new SqlCommand(
                    "SELECT COUNT(*) FROM " +
                    "dbo.new_employees;",
                    sourceConnection);
                long countStart = System.Convert.ToInt32(
                    commandRowCount.ExecuteScalar());
                Console.WriteLine("Starting row count = {0}", countStart);

                // Get data from the source table as a SqlDataReader.
                SqlCommand commandSourceData = new SqlCommand(
                    "SELECT *" +
                    "FROM dbo.new_employees;", sourceConnection);
                SqlDataReader reader =
                    commandSourceData.ExecuteReader();

                // Open the destination connection. In the real world you would
                // not use SqlBulkCopy to move data from one table to the other
                // in the same database. This is for demonstration purposes only.
                using (SqlConnection destinationConnection =
                           new SqlConnection(connectionString))
                {
                    destinationConnection.Open();

                    // Set up the bulk copy object.
                    // Note that the column positions in the source
                    // data reader match the column positions in
                    // the destination table so there is no need to
                    // map columns.
                    using (SqlBulkCopy bulkCopy =
                               new SqlBulkCopy(destinationConnection))
                    {
                        bulkCopy.DestinationTableName =
                            "dbo.new_employees";

                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.WriteToServer(reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            // Close the SqlDataReader. The SqlBulkCopy
                            // object is automatically closed at the end
                            // of the using block.
                            reader.Close();
                        }
                    }

                    // Perform a final count on the destination
                    // table to see how many rows were added.
                    long countEnd = System.Convert.ToInt32(
                        commandRowCount.ExecuteScalar());
                    Console.WriteLine("Ending row count = {0}", countEnd);
                    Console.WriteLine("{0} rows were added.", countEnd - countStart);
                    Console.WriteLine("Press Enter to finish.");
                    Console.ReadLine();
                }
            }
        }

        static void dappertype()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (!conn.State.Equals(ConnectionState.Open))
                {
                    conn.Open();
                }

                List<DBModel> list = new List<DBModel>();
                for (int i = 0; i < testcount; i++)
                {

                   // list.Add(new DBModel() { fname = "k", lname = "k", minit = 'k' });
                }


                var sqlCommand = @"insert into [new_employees] VALUES (@fname,@minit,@lname);";
                conn.Execute(sqlCommand, list);
            }
        }

        static void cmdtype()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (!conn.State.Equals(ConnectionState.Open))
                {
                    conn.Open();
                }

                var sqlCommand = @"insert into [new_employees] VALUES ('bally','F','bally');";
                SqlCommand cmd = new SqlCommand(sqlCommand, conn);
                cmd.ExecuteNonQuery();
            }
        }


        private static DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                if (prop.Name != "test")
                {
                    Type t = GetCoreType(prop.PropertyType);
                    tb.Columns.Add(prop.Name, t);
                }
                else
                {
                    var test = prop.Name;
                }
            }

            foreach (T item in items)
            {
                var values = new object[props.Length-1];

                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i].Name != "test")
                    {
                        values[i] = props[i].GetValue(item, null);
                    }
                    else
                    {
                        var test = props[i].Name;
                    }
                   
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        public static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }

    }

    public static class DataTableExtensions
    {
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            //取得DataTable所有的row data
            foreach (var row in table.Rows)
            {
                var item = MappingItem<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T MappingItem<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    //針對欄位的型態去轉換
                    if (property.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(row[property.Name].ToString(), out dt))
                        {
                            property.SetValue(item, dt, null);
                        }
                        else
                        {
                            property.SetValue(item, null, null);
                        }
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        decimal val = new decimal();
                        decimal.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        double val = new double();
                        double.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        int val = new int();
                        int.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else
                    {
                        if (row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(item, row[property.Name], null);
                        }
                    }
                }
            }
            return item;
        }
    }


}
