using System;
using Npgsql;

namespace PostgresTest
{
    class Program
    {
        private static NpgsqlConnection? _connection;

        private const string DEFAULT_CONNECTION_STRING = "Host=db.kpersgrugzrgbxynuvoy.supabase.co;Port=5432;Username=postgres;Password=Kiflajefina1!;Database=postgres;SSL Mode=Require;";

        static void Main(string[] args)
        {
            Console.WriteLine("=== PostgreSQL Test Application ===");
            Console.WriteLine("Testing connection to Supabase PostgreSQL database");
            Console.WriteLine();

            try
            {
                string connectionString = GetConnectionString();

                using (_connection = ConnectToDatabase(connectionString))
                {
                    if (_connection == null) return;

                    Console.WriteLine("✅ Connection established successfully!");
                    Console.WriteLine("You can now execute SQL commands or use special commands:");
                    Console.WriteLine("  \\t [table_name] - Show table details");
                    Console.WriteLine("  \\l - List all tables");
                    Console.WriteLine("  \\d - Show database info");
                    Console.WriteLine("  close - Exit application");
                    Console.WriteLine();

                    InitializeSampleTables();

                    RunCommandLoop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error: {ex.Message}");
            }

            Console.WriteLine("Connection has been closed. Goodbye!");
        }

        static string GetConnectionString()
        {
            Console.WriteLine("Choose connection option:");
            Console.WriteLine("1. Use default Supabase connection");
            Console.WriteLine("2. Enter custom connection string");
            Console.Write("Enter choice (1-2): ");

            var choice = Console.ReadLine();

            if (choice == "2")
            {
                Console.Write("Enter your PostgreSQL connection string: ");
                var customConnection = Console.ReadLine();
                return !string.IsNullOrEmpty(customConnection) ? customConnection : DEFAULT_CONNECTION_STRING;
            }

            return DEFAULT_CONNECTION_STRING;
        }

        static NpgsqlConnection? ConnectToDatabase(string connectionString)
        {
            try
            {
                var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                Console.WriteLine($"📊 Connected to: {conn.Database}");
                Console.WriteLine($"🖥️  Server: {conn.Host}:{conn.Port}");
                Console.WriteLine($"👤 User: {conn.UserName}");
                Console.WriteLine();

                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to database: {ex.Message}");
                return null;
            }
        }

        static void InitializeSampleTables()
        {
            try
            {
                Console.WriteLine("🔄 Initializing sample tables...");

                // Create patients table if not exists
                var createTableSQL = @"
                CREATE TABLE IF NOT EXISTS patients (
                    id SERIAL PRIMARY KEY,
                    patient_id VARCHAR(50) UNIQUE NOT NULL,
                    first_name VARCHAR(100) NOT NULL,
                    last_name VARCHAR(100) NOT NULL,
                    date_of_birth DATE NOT NULL,
                    gender CHAR(1) CHECK (gender IN ('M', 'F')),
                    cancer_type VARCHAR(100),
                    diagnosis_date DATE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

                ExecuteNonQuerySQL(createTableSQL);

                // Insert sample data if table is empty
                var countSQL = "SELECT COUNT(*) FROM patients;";
                using (var cmd = new NpgsqlCommand(countSQL, _connection))
                {
                    var count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count == 0)
                    {
                        var insertSQL = @"
                        INSERT INTO patients (patient_id, first_name, last_name, date_of_birth, gender, cancer_type, diagnosis_date) VALUES
                        ('TCGA-OR-A5LC-01', 'John', 'Doe', '1975-03-15', 'M', 'Sarcoma', '2020-01-15'),
                        ('TCGA-OR-A5JJ-01', 'Jane', 'Smith', '1982-07-22', 'F', 'Sarcoma', '2020-02-10'),
                        ('TCGA-OR-A5K3-01', 'Mike', 'Johnson', '1968-11-05', 'M', 'Sarcoma', '2020-03-05'),
                        ('TCGA-PK-A5HA-01', 'Sarah', 'Williams', '1990-09-18', 'F', 'Sarcoma', '2020-04-12'),
                        ('TCGA-OR-A5LN-01', 'David', 'Brown', '1973-12-30', 'M', 'Sarcoma', '2020-05-20');";

                        ExecuteNonQuerySQL(insertSQL);
                        Console.WriteLine("✅ Sample patient data inserted");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Patients table already contains {count} records");
                    }
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Warning: Could not initialize sample tables: {ex.Message}");
                Console.WriteLine();
            }
        }

        static void RunCommandLoop()
        {
            while (true)
            {
                Console.Write("SQL> ");
                string input = Console.ReadLine() ?? "";

                if (IsExitCommand(input))
                    break;

                if (IsListTablesCommand(input))
                {
                    ExecuteListTablesCommand();
                }
                else if (IsDatabaseInfoCommand(input))
                {
                    ExecuteDatabaseInfoCommand();
                }
                else if (IsTableCommand(input))
                {
                    ExecuteTableDetailsCommand(ExtractTableName(input));
                }
                else if (!string.IsNullOrWhiteSpace(input))
                {
                    ExecuteSqlCommand(input);
                }
            }
        }

        static bool IsExitCommand(string input) => input.Trim().ToLower() == "close";
        static bool IsListTablesCommand(string input) => input.Trim().ToLower() == "\\l";
        static bool IsDatabaseInfoCommand(string input) => input.Trim().ToLower() == "\\d";
        static bool IsTableCommand(string input) => input.Trim().StartsWith("\\t", StringComparison.OrdinalIgnoreCase);
        static string ExtractTableName(string input) => input.Trim().Substring(2).Trim();

        static void ExecuteListTablesCommand()
        {
            Console.WriteLine("📋 Tables in database:");
            Console.WriteLine(new string('-', 50));

            var query = @"
                SELECT table_name, table_type 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                ORDER BY table_name;";

            ExecuteSelectQuery(query);
        }

        static void ExecuteDatabaseInfoCommand()
        {
            Console.WriteLine("🗄️  Database Information:");
            Console.WriteLine(new string('-', 50));

            try
            {
                var queries = new[]
                {
                    ("Database Name", "SELECT current_database();"),
                    ("Current User", "SELECT current_user;"),
                    ("Server Version", "SELECT version();"),
                    ("Current Time", "SELECT now();"),
                    ("Database Size", "SELECT pg_size_pretty(pg_database_size(current_database()));")
                };

                foreach (var (label, query) in queries)
                {
                    using (var cmd = new NpgsqlCommand(query, _connection))
                    {
                        var result = cmd.ExecuteScalar();
                        Console.WriteLine($"{label}: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting database info: {ex.Message}");
            }

            Console.WriteLine();
        }

        static void ExecuteTableDetailsCommand(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                Console.WriteLine("❌ Table name is required. Usage: \\t table_name");
                return;
            }

            string query = @"
                SELECT 
                    column_name, 
                    data_type, 
                    character_maximum_length, 
                    is_nullable,
                    column_default
                FROM 
                    information_schema.columns 
                WHERE 
                    table_name = @tableName
                ORDER BY ordinal_position;";

            using (var cmd = new NpgsqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("tableName", tableName);

                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Console.WriteLine($"📋 Details for table: {tableName}");
                            Console.WriteLine(new string('-', 100));
                            Console.WriteLine($"{"Column",-25} {"Type",-20} {"Max Len",-10} {"Nullable",-10} {"Default",-20}");
                            Console.WriteLine(new string('-', 100));

                            while (reader.Read())
                            {
                                var column = reader["column_name"]?.ToString() ?? "";
                                var type = reader["data_type"]?.ToString() ?? "";
                                var maxLen = reader["character_maximum_length"]?.ToString() ?? "-";
                                var nullable = reader["is_nullable"]?.ToString() ?? "";
                                var defaultVal = reader["column_default"]?.ToString() ?? "-";

                                Console.WriteLine($"{column,-25} {type,-20} {maxLen,-10} {nullable,-10} {defaultVal,-20}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ Table '{tableName}' not found or has no columns.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error retrieving table details: {ex.Message}");
                }
            }

            Console.WriteLine();
        }

        static void ExecuteSqlCommand(string commandText)
        {
            using (var cmd = new NpgsqlCommand(commandText, _connection))
            {
                try
                {
                    if (commandText.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                        commandText.Trim().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
                    {
                        ExecuteSelectQuery(commandText);
                    }
                    else
                    {
                        int affectedRows = cmd.ExecuteNonQuery();
                        Console.WriteLine($"✅ Command executed successfully. {affectedRows} rows affected.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error executing command: {ex.Message}");
                }
            }

            Console.WriteLine();
        }

        static void ExecuteSelectQuery(string query)
        {
            using (var cmd = new NpgsqlCommand(query, _connection))
            {
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            PrintHeaders(reader);
                            PrintRows(reader);
                        }
                        else
                        {
                            Console.WriteLine("No rows found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error executing query: {ex.Message}");
                }
            }
        }

        static void ExecuteNonQuerySQL(string sql)
        {
            using (var cmd = new NpgsqlCommand(sql, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        static void PrintHeaders(NpgsqlDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.Write($"{reader.GetName(i),-20}");
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', reader.FieldCount * 20));
        }

        static void PrintRows(NpgsqlDataReader reader)
        {
            var rowCount = 0;
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader[i]?.ToString() ?? "NULL";
                    if (value.Length > 18)
                        value = value.Substring(0, 15) + "...";
                    Console.Write($"{value,-20}");
                }
                Console.WriteLine();
                rowCount++;
            }
            Console.WriteLine(new string('-', reader.FieldCount * 20));
            Console.WriteLine($"({rowCount} rows)");
            Console.WriteLine();
        }
    }
}