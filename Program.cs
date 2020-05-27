using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace dbtest
{

    public class DBModel
    {
        public int id_num { get; set; }

        public string fname { get; set; }

        public char minit { get; set; }

        public string lname { get; set; }
    }


    class Program
    {

        const string connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = testdb; Integrated Security = True";

        enum InsertType 
        {
                cmdtype,
                dappertype,
                SqlBulkCopy,
        }

      
        static void Main(string[] args)
        {
     
            var flag = InsertType.SqlBulkCopy;

            switch (flag) 
            {
                case InsertType.cmdtype:
                    cmdtype();
                    break;
                case InsertType.dappertype:
                    dappertype();
                    break;
                case InsertType.SqlBulkCopy:
                    SqlBulkCopy();
                    break;
            }

            Console.WriteLine("Hello World!");
        }

        static void SqlBulkCopy()
        {
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
                        "dbo.BulkCopyDemoMatchingColumns";

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

        static void dappertype()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (!conn.State.Equals(ConnectionState.Open))
                {
                    conn.Open();
                }

                List<DBModel> list = new List<DBModel>();
                list.Add(new DBModel() { fname = "1", lname = "2", minit = '3' });


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

    }


}
