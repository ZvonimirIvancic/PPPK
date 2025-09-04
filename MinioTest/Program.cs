using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reactive.Linq; // Add this for ToList() and Wait() methods

namespace MinioTest
{
    class Program
    {
        private const string ENDPOINT = "regoch.net:9000";
        private const string ACCESS_KEY = "minioAdmin";
        private const string SECRET_KEY = "supersecretpassword";
        private const string BUCKET_NAME = "zvonimir-ivancic";
        private static IMinioClient? _minioClient;
        private static string _downloadFolder = "";
        private static IWebDriver? _driver;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== MinIO Test Application ===");
            Console.WriteLine("Testing MinIO connection and TCGA TSV file storage");
            Console.WriteLine();
            try
            {
                await InitializeMinioClient();
                InitializeDownloadFolder();
                await RunMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            finally
            {
                _driver?.Quit();
            }
        }

        static async Task InitializeMinioClient()
        {
            try
            {
                _minioClient = new MinioClient()
                    .WithEndpoint(ENDPOINT)
                    .WithCredentials(ACCESS_KEY, SECRET_KEY)
                    .Build();
                Console.WriteLine("✅ MinIO client initialized successfully!");
                Console.WriteLine($"Endpoint: {ENDPOINT}");
                Console.WriteLine($"Bucket: {BUCKET_NAME}");
                Console.WriteLine();
                await TestMinioConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing MinIO client: {ex.Message}");
                throw;
            }
        }

        static async Task TestMinioConnection()
        {
            try
            {
                bool found = await _minioClient!.BucketExistsAsync(new BucketExistsArgs().WithBucket(BUCKET_NAME));
                if (!found)
                {
                    Console.WriteLine($"🔄 Creating bucket '{BUCKET_NAME}'...");
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BUCKET_NAME));
                    Console.WriteLine($"✅ Bucket '{BUCKET_NAME}' created successfully!");
                }
                else
                {
                    Console.WriteLine($"✅ Bucket '{BUCKET_NAME}' already exists!");
                }

                // List objects in bucket using Option 1: ToList().Subscribe() + Wait()
                var objects = new List<string>();
                var observable = _minioClient.ListObjectsAsync(new ListObjectsArgs().WithBucket(BUCKET_NAME));
                var subscription = observable.ToList().Subscribe(
                    list => objects.AddRange(list.Select(item => item.Key)),
                    ex => Console.WriteLine($"❌ Error: {ex.Message}"),
                    () => Console.WriteLine("✅ Object listing completed")
                );
                observable.Wait(); // Wait for completion
                subscription.Dispose();

                Console.WriteLine($"📊 Objects in bucket: {objects.Count}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing MinIO connection: {ex.Message}");
                throw;
            }
        }

        static void InitializeDownloadFolder()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _downloadFolder = Path.Combine(desktopPath, "MinioTestDownloads");
            Directory.CreateDirectory(_downloadFolder);
            Console.WriteLine($"📁 Download folder: {_downloadFolder}");
            Console.WriteLine();
        }

        static async Task RunMainMenu()
        {
            while (true)
            {
                Console.WriteLine("=== MAIN MENU ===");
                Console.WriteLine("1. Upload mock TSV file to MinIO");
                Console.WriteLine("2. Run TCGA scraper (download real TSV files)");
                Console.WriteLine("3. List files in MinIO bucket");
                Console.WriteLine("4. Download file from MinIO");
                Console.WriteLine("5. Delete file from MinIO");
                Console.WriteLine("6. Exit");
                Console.Write("Choose option (1-6): ");
                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await UploadMockTSVFile();
                        break;
                    case "2":
                        await RunTCGAScraper();
                        break;
                    case "3":
                        await ListMinioFiles();
                        break;
                    case "4":
                        await DownloadFileFromMinio();
                        break;
                    case "5":
                        await DeleteFileFromMinio();
                        break;
                    case "6":
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

        static async Task UploadMockTSVFile()
        {
            Console.WriteLine("🔄 Creating and uploading mock TSV file...");
            try
            {
                // Create mock TSV content (based on your sample)
                var tsvContent = @"sample	TCGA-OR-A5LC-01	TCGA-OR-A5JJ-01	TCGA-OR-A5K3-01	TCGA-PK-A5HA-01	TCGA-OR-A5LN-01
ARHGEF10L	-3.61029246976	-1.21719246976	-1.78669246976	-1.32909246976	-0.944392469762
HIF3A	-0.811626336325	-1.09712633632	-0.336626336325	-3.11902633632	0.294473663675
RNF17	-0.531035005853	-0.531035005853	-0.531035005853	-0.531035005853	-0.531035005853
RNF10	0.562928014046	0.398728014046	1.64932801405	0.525628014046	0.649828014046
RNF11	-0.735278134998	-0.698278134998	-0.527978134998	-0.639778134998	-0.482678134998
C6orf150	2.31429246976	1.45719246976	2.78669246976	1.82909246976	2.944392469762
CCL5	1.811626336325	2.09712633632	1.336626336325	2.11902633632	1.294473663675
CXCL10	0.531035005853	1.531035005853	0.531035005853	1.531035005853	0.531035005853
TMEM173	1.562928014046	0.898728014046	2.64932801405	1.525628014046	1.649828014046
NFKB1	1.31429246976	0.45719246976	1.78669246976	0.82909246976	1.944392469762
IL6	0.478909910261	0.454909910261	0.8423099102614	0.8412099102614	0.427790089739
IL8	0.31429246976	0.25719246976	0.68669246976	0.72909246976	0.844392469762";

                // Save to local file
                var fileName = $"mock_tcga_data_{DateTime.Now:yyyyMMdd_HHmmss}.tsv";
                var filePath = Path.Combine(_downloadFolder, fileName);
                await File.WriteAllTextAsync(filePath, tsvContent);
                Console.WriteLine($"📄 Mock TSV file created: {fileName}");

                // Upload to MinIO
                await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                await _minioClient!.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject($"tcga-data/{fileName}")
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType("text/tab-separated-values"));

                Console.WriteLine($"✅ Successfully uploaded {fileName} to MinIO bucket!");
                Console.WriteLine($"📊 File size: {new FileInfo(filePath).Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error uploading mock TSV file: {ex.Message}");
            }
        }

        static async Task RunTCGAScraper()
        {
            Console.WriteLine("🔄 Starting TCGA scraper...");
            Console.WriteLine("⚠️  This will use Selenium WebDriver - make sure Chrome is installed!");
            Console.Write("Continue? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("❌ Scraper cancelled.");
                return;
            }

            try
            {
                InitializeWebDriver();
                await ProcessXenaDataPage("https://xenabrowser.net/datapages/?host=https%3A%2F%2Ftcga.xenahubs.net&removeHub=https%3A%2F%2Fxena.treehouse.gi.ucsc.edu%3A443");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error running scraper: {ex.Message}");
            }
            finally
            {
                _driver?.Quit();
                _driver = null;
            }
        }

        static void InitializeWebDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Run in background
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
            Console.WriteLine("✅ Chrome WebDriver initialized");
        }

        static async Task ProcessXenaDataPage(string url)
        {
            _driver!.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(drv => drv.FindElement(By.TagName("ul")));

            var cohortLinks = ExtractCohortLinks();
            Console.WriteLine($"Found {cohortLinks.Count} TCGA cohort links.");

            // Process only first 2 cohorts for demo
            var processCount = Math.Min(2, cohortLinks.Count);
            for (int i = 0; i < processCount; i++)
            {
                Console.WriteLine($"Processing cohort {i + 1}/{processCount}...");
                await ProcessCohortPage(cohortLinks[i]);
            }

            Console.WriteLine($"✅ Scraper completed! Processed {processCount} cohorts.");
        }

        static List<string> ExtractCohortLinks()
        {
            System.Threading.Thread.Sleep(2000);
            var links = _driver!.FindElements(By.XPath("//ul/li/a[@href]"));
            var cohortLinks = new List<string>();

            foreach (var link in links)
            {
                string href = link.GetAttribute("href");
                if (href.Contains("TCGA"))
                {
                    cohortLinks.Add(href);
                    Console.WriteLine($"Found cohort link: {href}");
                }
            }
            return cohortLinks;
        }

        static async Task ProcessCohortPage(string cohortUrl)
        {
            Console.WriteLine($"Navigating to: {cohortUrl}");
            _driver!.Navigate().GoToUrl(cohortUrl);
            System.Threading.Thread.Sleep(1000);

            try
            {
                var geneExpressionSection = _driver.FindElement(By.XPath("//div[h3[contains(text(), 'gene expression RNAseq')]]"));
                var illuminaLink = FindIlluminaLink(geneExpressionSection);
                if (!string.IsNullOrEmpty(illuminaLink))
                {
                    await ProcessIlluminaPage(illuminaLink);
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("No 'gene expression RNAseq' section found.");
            }
        }

        static string FindIlluminaLink(IWebElement geneExpressionSection)
        {
            try
            {
                var ulElement = geneExpressionSection.FindElement(By.XPath(".//ul"));
                var illuminaElement = ulElement.FindElement(By.XPath(".//li/a[contains(text(), 'IlluminaHiSeq') and contains(text(), 'pancan')]"));
                string illuminaUrl = illuminaElement?.GetAttribute("href");
                Console.WriteLine($"Found IlluminaHiSeq pancan normalized link: {illuminaUrl}");
                return illuminaUrl;
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("No IlluminaHiSeq pancan normalized link found.");
                return null;
            }
        }

        static async Task ProcessIlluminaPage(string illuminaUrl)
        {
            Console.WriteLine($"Navigating to Illumina link: {illuminaUrl}");
            _driver!.Navigate().GoToUrl(illuminaUrl);
            System.Threading.Thread.Sleep(1000);

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(drv => drv.FindElement(By.TagName("a")));

            var downloadLink = _driver.FindElement(By.XPath("//span/a[contains(text(), 'download')]"));
            if (downloadLink != null)
            {
                string fileUrl = downloadLink.GetAttribute("href");
                string fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
                string destinationPath = Path.Combine(_downloadFolder, fileName);

                Console.WriteLine($"Downloading {fileName}...");
                await DownloadFile(fileUrl, destinationPath);

                // Upload to MinIO
                await UploadFileToMinio(destinationPath, $"tcga-scraped/{fileName}");
            }
            else
            {
                Console.WriteLine("Download link not found on the page.");
            }
        }

        static async Task DownloadFile(string fileUrl, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                var fileData = await client.GetByteArrayAsync(fileUrl);
                await File.WriteAllBytesAsync(destinationPath, fileData);
                Console.WriteLine($"✅ File downloaded to: {destinationPath}");
            }
        }

        static async Task UploadFileToMinio(string localFilePath, string minioObjectName)
        {
            try
            {
                await using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
                await _minioClient!.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(minioObjectName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType("application/gzip"));

                Console.WriteLine($"✅ Uploaded {minioObjectName} to MinIO bucket!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error uploading to MinIO: {ex.Message}");
            }
        }

        static async Task ListMinioFiles()
        {
            Console.WriteLine("📁 Files in MinIO bucket:");
            Console.WriteLine(new string('-', 80));
            try
            {
                // Using Option 1: ToList().Subscribe() + Wait()
                var objects = new List<Minio.DataModel.Item>();
                var observable = _minioClient!.ListObjectsAsync(new ListObjectsArgs().WithBucket(BUCKET_NAME));
                var subscription = observable.ToList().Subscribe(
                    list => objects.AddRange(list),
                    ex => Console.WriteLine($"❌ Error: {ex.Message}"),
                    () => Console.WriteLine("✅ Object listing completed")
                );
                observable.Wait(); // Wait for completion
                subscription.Dispose();

                if (objects.Count == 0)
                {
                    Console.WriteLine("No files found in bucket.");
                }
                else
                {
                    foreach (var item in objects)
                    {
                        Console.WriteLine($"📄 {item.Key}");
                        Console.WriteLine($"   Size: {item.Size} bytes");
                        Console.WriteLine($"   Modified: {item.LastModifiedDateTime}");
                        Console.WriteLine($"   ETag: {item.ETag}");
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Total files: {objects.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error listing files: {ex.Message}");
            }
        }

        static async Task DownloadFileFromMinio()
        {
            Console.Write("Enter object name to download: ");
            var objectName = Console.ReadLine();
            if (string.IsNullOrEmpty(objectName))
            {
                Console.WriteLine("❌ Object name cannot be empty");
                return;
            }

            try
            {
                var downloadPath = Path.Combine(_downloadFolder, Path.GetFileName(objectName));
                await _minioClient!.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(objectName)
                    .WithFile(downloadPath));

                Console.WriteLine($"✅ Downloaded {objectName} to {downloadPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error downloading file: {ex.Message}");
            }
        }

        static async Task DeleteFileFromMinio()
        {
            Console.Write("Enter object name to delete: ");
            var objectName = Console.ReadLine();
            if (string.IsNullOrEmpty(objectName))
            {
                Console.WriteLine("❌ Object name cannot be empty");
                return;
            }

            Console.Write($"Are you sure you want to delete '{objectName}'? (yes/no): ");
            if (Console.ReadLine()?.ToLower() != "yes")
            {
                Console.WriteLine("❌ Operation cancelled");
                return;
            }

            try
            {
                await _minioClient!.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(BUCKET_NAME)
                    .WithObject(objectName));

                Console.WriteLine($"✅ Deleted {objectName} from MinIO bucket");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting file: {ex.Message}");
            }
        }
    }
}