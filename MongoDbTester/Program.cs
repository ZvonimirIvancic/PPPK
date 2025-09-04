using MongoDB.Driver;
using MongoDbTester.Models;
using System.Globalization;

namespace MongoDbTest
{
    class Program
    {
        private static IMongoDatabase? _database;
        private static IMongoCollection<PatientGeneExpression>? _collection;

        private const string CONNECTION_TEMPLATE = "mongodb+srv://<db_username>:<db_password>@webapp.bdxjt68.mongodb.net/?retryWrites=true&w=majority&appName=webApp";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MongoDB Test Application ===");
            Console.WriteLine("Testing connection to MongoDB Atlas and TCGA gene expression data processing");
            Console.WriteLine();

            try
            {
                string connectionString = GetMongoConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("❌ Connection string cannot be empty. Exiting...");
                    return;
                }

                var client = new MongoClient(connectionString);
                _database = client.GetDatabase("tcga_gene_expression");
                _collection = _database.GetCollection<PatientGeneExpression>("patient_expressions");

                Console.WriteLine("✅ Successfully connected to MongoDB Atlas!");
                Console.WriteLine($"Database: tcga_gene_expression");
                Console.WriteLine($"Collection: patient_expressions");
                Console.WriteLine();

                // Test connection
                await TestConnection();

                // Main menu
                await RunMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error connecting to MongoDB: {ex.Message}");

                if (ex.Message.Contains("authentication"))
                {
                    Console.WriteLine("💡 Hint: Check your username and password");
                }
                else if (ex.Message.Contains("network"))
                {
                    Console.WriteLine("💡 Hint: Check your internet connection and firewall settings");
                }
            }
        }

        static string GetMongoConnectionString()
        {
            Console.WriteLine("🔗 MongoDB Atlas Connection Setup:");
            Console.WriteLine("Choose connection option:");
            Console.WriteLine("1. Use provided Atlas connection (enter credentials)");
            Console.WriteLine("2. Enter complete custom connection string");
            Console.Write("Enter choice (1-2): ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            if (choice == "2")
            {
                Console.Write("Enter your complete MongoDB connection string: ");
                return Console.ReadLine() ?? "";
            }

            // Option 1: Use template with credentials
            Console.WriteLine("📋 Using connection template:");
            Console.WriteLine("mongodb+srv://<username>:<password>@webapp.bdxjt68.mongodb.net/...");
            Console.WriteLine();

            Console.Write("Enter MongoDB username: ");
            var username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("❌ Username cannot be empty");
                return "";
            }

            Console.Write("Enter MongoDB password: ");
            var password = ReadPassword();
            Console.WriteLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("❌ Password cannot be empty");
                return "";
            }

            // Build connection string
            var connectionString = CONNECTION_TEMPLATE
                .Replace("<db_username>", Uri.EscapeDataString(username))
                .Replace("<db_password>", Uri.EscapeDataString(password));

            Console.WriteLine($"🔗 Connection string built successfully");
            Console.WriteLine($"📡 Connecting to: webapp.bdxjt68.mongodb.net");
            Console.WriteLine();

            return connectionString;
        }

        static string ReadPassword()
        {
            var password = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            return password;
        }

        static async Task TestConnection()
        {
            try
            {
                // Test database connectivity
                await _database!.RunCommandAsync<MongoDB.Bson.BsonDocument>("{ping:1}");
                Console.WriteLine("🏓 Database ping successful!");

                var count = await _collection!.CountDocumentsAsync("{}");
                Console.WriteLine($"📊 Current documents in collection: {count}");

                // Show database stats
                var stats = await _database.RunCommandAsync<MongoDB.Bson.BsonDocument>("{dbStats:1}");
                if (stats.Contains("dataSize"))
                {
                    var dataSize = stats["dataSize"].AsInt64;
                    Console.WriteLine($"💾 Database size: {FormatBytes(dataSize)}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing connection: {ex.Message}");
            }
        }

        static async Task RunMainMenu()
        {
            while (true)
            {
                Console.WriteLine("=== MAIN MENU ===");
                Console.WriteLine("1. Load mock TSV data");
                Console.WriteLine("2. View all patients");
                Console.WriteLine("3. View cGAS-STING pathway data");
                Console.WriteLine("4. Search patient by ID");
                Console.WriteLine("5. Show collection statistics");
                Console.WriteLine("6. Delete all data");
                Console.WriteLine("7. Test MongoDB operations");
                Console.WriteLine("8. Exit");
                Console.Write("Choose option (1-8): ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await LoadMockTSVData();
                        break;
                    case "2":
                        await ViewAllPatients();
                        break;
                    case "3":
                        await ViewcGAS_STING_Data();
                        break;
                    case "4":
                        await SearchPatientById();
                        break;
                    case "5":
                        await ShowCollectionStatistics();
                        break;
                    case "6":
                        await DeleteAllData();
                        break;
                    case "7":
                        await TestMongoOperations();
                        break;
                    case "8":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static async Task LoadMockTSVData()
        {
            Console.WriteLine("🔄 Loading mock TSV data (based on your TCGA sample)...");

            // Enhanced mock TSV data based on your sample
            var mockTSVLines = new[]
            {
                "sample\tTCGA-OR-A5LC-01\tTCGA-OR-A5JJ-01\tTCGA-OR-A5K3-01\tTCGA-PK-A5HA-01\tTCGA-OR-A5LN-01\tTCGA-OR-A5JA-01\tTCGA-OR-A5K0-01",
                "ARHGEF10L\t-3.61029246976\t-1.21719246976\t-1.78669246976\t-1.32909246976\t-0.944392469762\t-2.43119246976\t-0.927692469762",
                "HIF3A\t-0.811626336325\t-1.09712633632\t-0.336626336325\t-3.11902633632\t0.294473663675\t-0.000626336324643\t-2.90272633632",
                "RNF17\t-0.531035005853\t-0.531035005853\t-0.531035005853\t-0.531035005853\t-0.531035005853\t-0.531035005853\t-0.531035005853",
                "RNF10\t0.562928014046\t0.398728014046\t1.64932801405\t0.525628014046\t0.649828014046\t0.737528014046\t1.21882801405",
                "RNF11\t-0.735278134998\t-0.698278134998\t-0.527978134998\t-0.639778134998\t-0.482678134998\t-0.101578134998\t-0.736178134998",
                "RNF13\t-0.578909910261\t-0.554909910261\t-0.0423099102614\t-0.0412099102614\t0.527790089739\t-0.566409910261\t-1.44750991026",
                // cGAS-STING pathway genes with realistic expression values
                "C6orf150\t2.31429246976\t1.45719246976\t2.78669246976\t1.82909246976\t2.944392469762\t1.73119246976\t2.127692469762",
                "CCL5\t1.811626336325\t2.09712633632\t1.336626336325\t2.11902633632\t1.294473663675\t2.200626336324643\t1.90272633632",
                "CXCL10\t0.531035005853\t1.531035005853\t0.531035005853\t1.531035005853\t0.531035005853\t1.431035005853\t0.731035005853",
                "TMEM173\t1.562928014046\t0.898728014046\t2.64932801405\t1.525628014046\t1.649828014046\t0.937528014046\t2.21882801405",
                "CXCL9\t0.735278134998\t1.698278134998\t0.527978134998\t1.639778134998\t0.482678134998\t1.101578134998\t0.636178134998",
                "CXCL11\t0.578909910261\t1.554909910261\t1.0423099102614\t1.0412099102614\t1.527790089739\t0.866409910261\t1.44750991026",
                "NFKB1\t1.31429246976\t0.45719246976\t1.78669246976\t0.82909246976\t1.944392469762\t0.63119246976\t1.227692469762",
                "IKBKE\t0.811626336325\t1.09712633632\t0.336626336325\t1.11902633632\t0.294473663675\t1.100626336324643\t0.80272633632",
                "IRF3\t0.431035005853\t0.431035005853\t0.431035005853\t0.431035005853\t0.431035005853\t0.531035005853\t0.631035005853",
                "TREX1\t0.462928014046\t0.298728014046\t1.44932801405\t0.425628014046\t0.549828014046\t0.337528014046\t1.31882801405",
                "ATM\t0.635278134998\t0.598278134998\t0.427978134998\t0.539778134998\t0.382678134998\t0.701578134998\t0.536178134998",
                "IL6\t0.478909910261\t0.454909910261\t0.8423099102614\t0.8412099102614\t0.427790089739\t0.566409910261\t0.74750991026",
                "IL8\t0.31429246976\t0.25719246976\t0.68669246976\t0.72909246976\t0.844392469762\t0.43119246976\t0.627692469762"
            };

            try
            {
                // Parse header
                var header = mockTSVLines[0].Split('\t');
                var patientIds = header.Skip(1).ToArray();

                var patients = new List<PatientGeneExpression>();

                Console.WriteLine($"📋 Processing {patientIds.Length} patients from TSV data...");

                // Process each patient
                foreach (var patientId in patientIds)
                {
                    var patient = new PatientGeneExpression
                    {
                        PatientId = patientId,
                        CancerCohort = "SARC", // Sarcoma cohort
                        GeneExpressions = new Dictionary<string, double>(),
                        cGAS_STING_Genes = new cGAS_STING_GenesExpression()
                    };

                    // Process gene expression data
                    var patientIndex = Array.IndexOf(patientIds, patientId) + 1;

                    for (int i = 1; i < mockTSVLines.Length; i++)
                    {
                        var parts = mockTSVLines[i].Split('\t');
                        var geneName = parts[0];

                        if (parts.Length > patientIndex && double.TryParse(parts[patientIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var expression))
                        {
                            patient.GeneExpressions[geneName] = expression;

                            // Map to cGAS-STING pathway
                            MapTocGAS_STING_Pathway(patient.cGAS_STING_Genes, geneName, expression);
                        }
                    }

                    patients.Add(patient);
                    Console.Write($"✓ Processed {patientId.Substring(0, Math.Min(15, patientId.Length))}... ");
                }

                Console.WriteLine();
                Console.WriteLine($"🔄 Inserting {patients.Count} patients into MongoDB Atlas...");

                // Insert into MongoDB
                await _collection!.InsertManyAsync(patients);

                Console.WriteLine($"✅ Successfully loaded {patients.Count} patients into MongoDB Atlas!");
                Console.WriteLine($"📊 Total genes per patient: ~{patients.First().GeneExpressions.Count}");
                Console.WriteLine($"🧬 cGAS-STING pathway genes extracted: 13 genes per patient");
                Console.WriteLine($"🌍 Data stored in cloud database: webapp.bdxjt68.mongodb.net");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading data: {ex.Message}");

                if (ex.Message.Contains("duplicate"))
                {
                    Console.WriteLine("💡 Hint: Some patients might already exist. Try deleting existing data first.");
                }
            }
        }

        static void MapTocGAS_STING_Pathway(cGAS_STING_GenesExpression cgas, string geneName, double expression)
        {
            switch (geneName.ToUpper())
            {
                case "C6orf150":
                    cgas.C6orf150_cGAS = expression;
                    break;
                case "CCL5":
                    cgas.CCL5 = expression;
                    break;
                case "CXCL10":
                    cgas.CXCL10 = expression;
                    break;
                case "TMEM173":
                    cgas.TMEM173_STING = expression;
                    break;
                case "CXCL9":
                    cgas.CXCL9 = expression;
                    break;
                case "CXCL11":
                    cgas.CXCL11 = expression;
                    break;
                case "NFKB1":
                    cgas.NFKB1 = expression;
                    break;
                case "IKBKE":
                    cgas.IKBKE = expression;
                    break;
                case "IRF3":
                    cgas.IRF3 = expression;
                    break;
                case "TREX1":
                    cgas.TREX1 = expression;
                    break;
                case "ATM":
                    cgas.ATM = expression;
                    break;
                case "IL6":
                    cgas.IL6 = expression;
                    break;
                case "IL8":
                    cgas.IL8_CXCL8 = expression;
                    break;
            }
        }

        static async Task ViewAllPatients()
        {
            Console.WriteLine("👥 All patients in MongoDB Atlas database:");
            Console.WriteLine(new string('-', 80));

            try
            {
                var patients = await _collection!.Find("{}").ToListAsync();

                if (!patients.Any())
                {
                    Console.WriteLine("No patients found in database.");
                    Console.WriteLine("💡 Try loading mock data first (option 1)");
                    return;
                }

                Console.WriteLine($"📊 Found {patients.Count} patients in total");
                Console.WriteLine("📋 Showing first 10 patients:");
                Console.WriteLine();

                foreach (var patient in patients.Take(10))
                {
                    Console.WriteLine($"🆔 Patient ID: {patient.PatientId}");
                    Console.WriteLine($"🩺 Cancer Cohort: {patient.CancerCohort}");
                    Console.WriteLine($"🧬 Total Genes: {patient.GeneExpressions.Count}");
                    Console.WriteLine($"⏰ Created: {patient.CreatedAt:yyyy-MM-dd HH:mm} UTC");
                    Console.WriteLine($"🔗 MongoDB _id: {patient.Id}");
                    Console.WriteLine(new string('-', 40));
                }

                if (patients.Count > 10)
                {
                    Console.WriteLine($"... and {patients.Count - 10} more patients");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving patients: {ex.Message}");
            }
        }

        static async Task ViewcGAS_STING_Data()
        {
            Console.WriteLine("🧬 cGAS-STING Pathway Gene Expression Data:");
            Console.WriteLine("📋 13 target genes: C6orf150, CCL5, CXCL10, TMEM173, CXCL9, CXCL11, NFKB1, IKBKE, IRF3, TREX1, ATM, IL6, IL8");
            Console.WriteLine(new string('=', 80));

            try
            {
                var patients = await _collection!.Find("{}").Limit(5).ToListAsync();

                if (!patients.Any())
                {
                    Console.WriteLine("No patients found in database.");
                    Console.WriteLine("💡 Try loading mock data first (option 1)");
                    return;
                }

                foreach (var patient in patients)
                {
                    Console.WriteLine($"👤 Patient: {patient.PatientId} ({patient.CancerCohort} cohort)");

                    if (patient.cGAS_STING_Genes != null)
                    {
                        var cgas = patient.cGAS_STING_Genes;
                        Console.WriteLine($"  🧬 C6orf150 (cGAS): {cgas.C6orf150_cGAS:F3}");
                        Console.WriteLine($"  🧬 CCL5: {cgas.CCL5:F3}");
                        Console.WriteLine($"  🧬 CXCL10: {cgas.CXCL10:F3}");
                        Console.WriteLine($"  🧬 TMEM173 (STING): {cgas.TMEM173_STING:F3}");
                        Console.WriteLine($"  🧬 CXCL9: {cgas.CXCL9:F3}");
                        Console.WriteLine($"  🧬 CXCL11: {cgas.CXCL11:F3}");
                        Console.WriteLine($"  🧬 NFKB1: {cgas.NFKB1:F3}");
                        Console.WriteLine($"  🧬 IKBKE: {cgas.IKBKE:F3}");
                        Console.WriteLine($"  🧬 IRF3: {cgas.IRF3:F3}");
                        Console.WriteLine($"  🧬 TREX1: {cgas.TREX1:F3}");
                        Console.WriteLine($"  🧬 ATM: {cgas.ATM:F3}");
                        Console.WriteLine($"  🧬 IL6: {cgas.IL6:F3}");
                        Console.WriteLine($"  🧬 IL8 (CXCL8): {cgas.IL8_CXCL8:F3}");

                        // Calculate pathway score (average expression)
                        var pathwayScore = (cgas.C6orf150_cGAS + cgas.CCL5 + cgas.CXCL10 + cgas.TMEM173_STING +
                                          cgas.CXCL9 + cgas.CXCL11 + cgas.NFKB1 + cgas.IKBKE + cgas.IRF3 +
                                          cgas.TREX1 + cgas.ATM + cgas.IL6 + cgas.IL8_CXCL8) / 13.0;
                        Console.WriteLine($"  📊 Pathway Score (avg): {pathwayScore:F3}");
                    }
                    else
                    {
                        Console.WriteLine("  ❌ No cGAS-STING data available");
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error retrieving cGAS-STING data: {ex.Message}");
            }
        }

        static async Task SearchPatientById()
        {
            Console.Write("Enter patient ID to search (e.g., TCGA-OR-A5LC-01): ");
            var patientId = Console.ReadLine();

            if (string.IsNullOrEmpty(patientId))
            {
                Console.WriteLine("❌ Patient ID cannot be empty");
                return;
            }

            try
            {
                var patient = await _collection!.Find(p => p.PatientId == patientId).FirstOrDefaultAsync();

                if (patient == null)
                {
                    Console.WriteLine($"❌ Patient with ID '{patientId}' not found");
                    return;
                }

                Console.WriteLine($"✅ Found patient: {patient.PatientId}");
                Console.WriteLine($"🩺 Cancer Cohort: {patient.CancerCohort}");
                Console.WriteLine($"🧬 Total genes: {patient.GeneExpressions.Count}");
                Console.WriteLine($"⏰ Created: {patient.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"🔗 MongoDB _id: {patient.Id}");
                Console.WriteLine();

                Console.WriteLine("🧬 Gene expressions (first 10):");
                foreach (var gene in patient.GeneExpressions.Take(10))
                {
                    Console.WriteLine($"  {gene.Key}: {gene.Value:F3}");
                }

                if (patient.GeneExpressions.Count > 10)
                {
                    Console.WriteLine($"  ... and {patient.GeneExpressions.Count - 10} more genes");
                }

                if (patient.cGAS_STING_Genes != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("🎯 cGAS-STING pathway genes:");
                    var cgas = patient.cGAS_STING_Genes;
                    Console.WriteLine($"  C6orf150 (cGAS): {cgas.C6orf150_cGAS:F3}");
                    Console.WriteLine($"  TMEM173 (STING): {cgas.TMEM173_STING:F3}");
                    Console.WriteLine($"  IL6: {cgas.IL6:F3}");
                    Console.WriteLine($"  IL8: {cgas.IL8_CXCL8:F3}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error searching patient: {ex.Message}");
            }
        }

        static async Task ShowCollectionStatistics()
        {
            Console.WriteLine("📊 MongoDB Collection Statistics:");
            Console.WriteLine(new string('=', 50));

            try
            {
                // Basic counts
                var totalPatients = await _collection!.CountDocumentsAsync("{}");
                var sarcPatients = await _collection.CountDocumentsAsync(p => p.CancerCohort == "SARC");

                Console.WriteLine($"👥 Total Patients: {totalPatients}");
                Console.WriteLine($"🩺 SARC Cohort Patients: {sarcPatients}");

                if (totalPatients > 0)
                {
                    // Sample patient for gene count
                    var samplePatient = await _collection.Find("{}").FirstOrDefaultAsync();
                    if (samplePatient != null)
                    {
                        Console.WriteLine($"🧬 Genes per Patient: ~{samplePatient.GeneExpressions.Count}");
                        Console.WriteLine($"🎯 cGAS-STING Pathway: {(samplePatient.cGAS_STING_Genes != null ? "Available" : "Not Available")}");
                    }

                    // Recent patients
                    var recentPatients = await _collection.Find("{}")
                        .SortByDescending(p => p.CreatedAt)
                        .Limit(3)
                        .Project(p => new { p.PatientId, p.CreatedAt })
                        .ToListAsync();

                    if (recentPatients.Any())
                    {
                        Console.WriteLine();
                        Console.WriteLine("🕒 Recently Added Patients:");
                        foreach (var patient in recentPatients)
                        {
                            Console.WriteLine($"   {patient.PatientId} - {patient.CreatedAt:yyyy-MM-dd HH:mm} UTC");
                        }
                    }

                    // Database size estimate
                    var stats = await _database!.RunCommandAsync<MongoDB.Bson.BsonDocument>("{collStats: 'patient_expressions'}");
                    if (stats.Contains("size"))
                    {
                        var size = stats["size"].AsInt64;
                        Console.WriteLine();
                        Console.WriteLine($"💾 Collection Size: {FormatBytes(size)}");
                        Console.WriteLine($"📦 Storage Size: {FormatBytes(stats.GetValue("storageSize", 0).AsInt64)}");
                    }
                }
                else
                {
                    Console.WriteLine("📝 Database is empty. Load some mock data first!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting statistics: {ex.Message}");
            }
        }

        static async Task DeleteAllData()
        {
            Console.WriteLine("⚠️  WARNING: This will delete ALL patient data from MongoDB Atlas!");
            Console.Write("Are you sure you want to delete ALL data? Type 'DELETE' to confirm: ");
            var confirmation = Console.ReadLine();

            if (confirmation != "DELETE")
            {
                Console.WriteLine("❌ Operation cancelled");
                return;
            }

            try
            {
                var result = await _collection!.DeleteManyAsync("{}");
                Console.WriteLine($"✅ Deleted {result.DeletedCount} documents from MongoDB Atlas");
                Console.WriteLine("🗑️  All patient gene expression data has been removed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting data: {ex.Message}");
            }
        }

        static async Task TestMongoOperations()
        {
            Console.WriteLine("🧪 Testing MongoDB Operations:");
            Console.WriteLine(new string('-', 40));

            try
            {
                // Test 1: Insert single document
                Console.WriteLine("🔬 Test 1: Insert single test patient...");
                var testPatient = new PatientGeneExpression
                {
                    PatientId = $"TEST-{DateTime.Now:yyyyMMdd-HHmmss}",
                    CancerCohort = "TEST",
                    GeneExpressions = new Dictionary<string, double>
                    {
                        { "TEST_GENE_1", 1.23 },
                        { "TEST_GENE_2", -0.45 },
                        { "TEST_GENE_3", 2.67 }
                    },
                    cGAS_STING_Genes = new cGAS_STING_GenesExpression
                    {
                        C6orf150_cGAS = 1.5,
                        CCL5 = 0.8,
                        IL6 = 2.1
                    }
                };

                await _collection!.InsertOneAsync(testPatient);
                Console.WriteLine($"✅ Inserted test patient: {testPatient.PatientId}");

                // Test 2: Query operations
                Console.WriteLine("🔬 Test 2: Query operations...");
                var count = await _collection.CountDocumentsAsync(p => p.CancerCohort == "TEST");
                Console.WriteLine($"✅ Found {count} test patients");

                // Test 3: Update operation
                Console.WriteLine("🔬 Test 3: Update operation...");
                var updateResult = await _collection.UpdateOneAsync(
                    p => p.PatientId == testPatient.PatientId,
                    MongoDB.Driver.Builders<PatientGeneExpression>.Update.Set(p => p.CancerCohort, "TEST_UPDATED")
                );
                Console.WriteLine($"✅ Updated {updateResult.ModifiedCount} document(s)");

                // Test 4: Delete operation
                Console.WriteLine("🔬 Test 4: Delete operation...");
                var deleteResult = await _collection.DeleteOneAsync(p => p.PatientId == testPatient.PatientId);
                Console.WriteLine($"✅ Deleted {deleteResult.DeletedCount} document(s)");

                Console.WriteLine("🎉 All MongoDB operations completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during MongoDB operations test: {ex.Message}");
            }
        }

        static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}