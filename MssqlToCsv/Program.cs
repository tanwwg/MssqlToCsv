using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using GetPass;

namespace MssqlToCsv
{
    class Program
    {
        static int Main(string server, string user, string password, string catalog, string query, string output)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(server)) throw new ArgumentException("server is required");
                if (string.IsNullOrWhiteSpace(user)) throw new ArgumentException("user is required");
                if (string.IsNullOrWhiteSpace(catalog)) throw new ArgumentException("catalog is required");
                if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("query is required");
                if (string.IsNullOrWhiteSpace(output)) throw new ArgumentException("output is required");

                if (string.IsNullOrWhiteSpace(password))
                {
                    password = ConsolePasswordReader.Read("Password:");
                }

                var conn = new SqlConnection(
                    $"Server=tcp:{server},1433;Initial Catalog={catalog};Persist Security Info=False;User ID={user};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                conn.Open();

                var file = new StreamWriter(output);

                var command = new SqlCommand(query, conn);
                var reader = command.ExecuteReader();

                var coltypes = new List<Type>();
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    file.Write(reader.GetName(i));
                    file.Write("\t");
                    coltypes.Add(reader.GetFieldType(i));
                }
                file.WriteLine();
                
                Console.Out.WriteLine("Header written");

                var linecount = 0;

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!reader.IsDBNull(i))
                        {
                            if (coltypes[i] == typeof(DateTime))
                            {
                                var dt = reader.GetDateTime(i);
                                file.Write(dt.ToString("s", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                file.Write(reader.GetValue(i));
                            }
                        }
                        file.Write("\t");
                    }
                    file.WriteLine();

                    linecount += 1;
                    if (linecount % 1000 == 0)
                    {
                        Console.Out.WriteLine("Written " + linecount);
                    }
                }
                file.Close();

                Console.Out.WriteLine("Total written " + linecount);
                
                conn.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}