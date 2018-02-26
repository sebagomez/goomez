using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;

using GoomezIndexHelper;
using GoomezIndexHelper.Managers;
using Microsoft.Extensions.Configuration;

namespace GoomezCrawler
{
	class Program
	{
		#region Constants

		private static int m_indexed = 0;
		private static int m_scanned = 1;
		private const string K_SERVERS = "server";
		private const string K_EXTENSIONS = "extension";
		private const string K_EXCLUSIONS = "exclusion";
		private const string K_INCLUSIONS = "inclusion";
		private const string K_INDEXNAME = "GoomezIndex";
		private const string K_CONFIGFILE = "GoomezCrawler.XML";
		private static string K_CURRENTPATH = "";
		private static readonly string K_NOINDEX_FILE = "noindex.goomez";

		private const int MAX_PATH = 260;
		private const int MAX_DIR = 248;

		#endregion

		#region Fields

		private static List<string> m_exclusions = null;
		private static List<string> m_extensions = null;
		private static Dictionary<string, string> m_errors = new Dictionary<string, string>();
		private static bool errors = false;
		private static DateTime started;
		private static Stopwatch chrono = new Stopwatch();
		private static bool UseSDS = false;
		private static bool UseParallel = false;

		#endregion

		public static IConfiguration Configuration { get; set; }

		static int Main(string[] args)
		{
			chrono.Start();
			started = DateTime.Now;

			try
			{

				K_CURRENTPATH = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\"));

				//TraceManager.Initialize("GoomezCrawler", Path.Combine(K_CURRENTPATH, "tracing.config"));

				if (args.Length > 1)
				{
					Console.WriteLine("Too many arguments");
					PrintHelp();
					return -1;
				}

				m_exclusions = GetConfigList(K_EXCLUSIONS);
				m_extensions = GetConfigList(K_EXTENSIONS);
				if (m_extensions.Count == 0)
					throw new ApplicationException("No extensions found.");


				var builder = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json");

				Configuration = builder.Build();


				string sdsValue = Configuration["UseSDS"];
				bool.TryParse(sdsValue, out UseSDS);

				string parallelValue = Configuration["UseParallel"];
				bool.TryParse(parallelValue, out UseParallel);

				Console.WriteLine("Indexer started... please wait! (it might take a while)");

				if (args.Length == 1)
				{
					string sharedFolder = args[0];
					if (Directory.Exists(sharedFolder))
						IndexSharedFolder(sharedFolder);
					else
						throw new ApplicationException(sharedFolder + " not found.");

					return 0;
				}

				IndexManager.Initialize(Path.Combine(K_CURRENTPATH, K_INDEXNAME));

				if (UseParallel)
					ParallelIndexFiles();
				else
					IndexFiles();

				return errors ? -1 : 0;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Internal error");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();

			}
			finally
			{
				ShowSummary();

#if DEBUG
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
#endif

			}

			return -1;
		}

		private static void PrintHelp()
		{
			Console.WriteLine("GoomezCrawler [<sharedfolder>]");
			Console.WriteLine("<sharedfolder>    Path to the shared folder to index");
			Console.WriteLine("There must exists 2 files: " + K_SERVERS + " and " + K_EXTENSIONS + ", " + K_EXCLUSIONS + " is optional");
		}

		private static void IndexSharedFolder(string sharedFolder)
		{
			DateTime started = DateTime.Now;
			try
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				IndexFolder(sharedFolder);
			}
			catch (Exception ex)
			{
				ShowAndLogException(FileType.Folder, sharedFolder, ex);
			}
			finally
			{
				ShowSummary();
			}
		}

		private static void IndexFiles()
		{
			try
			{
				List<string> servers = GetConfigList(K_SERVERS);

				foreach (string server in servers)
				{
					foreach (string folder in GetShares(server))
					{
						if (folder.EndsWith("$"))
							continue;

						string folderFullPath = @"\\" + server + @"\" + folder;

						try
						{
							if (UseParallel)
								ParallelIndexFolder(folderFullPath);
							else
								IndexFolder(folderFullPath);
						}
						catch (Exception ex)
						{
							ShowAndLogException(FileType.Folder, folderFullPath, ex);
						}
					}
				}

				List<string> inclusions = GetConfigList(K_INCLUSIONS);
				foreach (string folder in inclusions)
				{
					try
					{
						if (UseParallel)
							ParallelIndexFolder(folder);
						else
							IndexFolder(folder);
					}
					catch (Exception ex)
					{
						ShowAndLogException(FileType.Folder, folder, ex);
					}
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("== ERROR ==");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				IndexManager.CloseIndex();
				//ShowSummary();
			}
		}

		private static void ParallelIndexFiles()
		{
			try
			{
				Parallel.ForEach<string>(GetConfigList(K_SERVERS), server =>
				{
					foreach (string folder in GetShares(server))
					{
						if (folder.EndsWith("$"))
							continue;

						string folderFullPath = @"\\" + server + @"\" + folder;

						try
						{
							ParallelIndexFolder(folderFullPath);
						}
						catch (Exception ex)
						{
							ShowAndLogException(FileType.Folder, folderFullPath, ex);
						}
					}
				});

				Parallel.ForEach<string>(GetConfigList(K_INCLUSIONS), folder =>
				{
					try
					{
						ParallelIndexFolder(folder);
					}
					catch (Exception ex)
					{
						ShowAndLogException(FileType.Folder, folder, ex);
					}
				});
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("== ERROR ==");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				IndexManager.CloseIndex();
				//ShowSummary();
			}
		}

		static bool OkToIndex(string folder)
		{
			return folder.Length <= MAX_DIR && !m_exclusions.Contains(folder) && !File.Exists(Path.Combine(folder, K_NOINDEX_FILE));
		}

		private static void ParallelIndexFolder(string folder)
		{
			try
			{
				if (!OkToIndex(folder))
					return;

				Parallel.ForEach<string>(Directory.GetDirectories(folder), childFolder =>
				{
					ParallelIndexFolder(childFolder);
				});

				Parallel.ForEach<string>(Directory.GetFiles(folder), file =>
				{
					if (file.Length < MAX_PATH)
					{
						FileInfo fi = new FileInfo(file);
						if (m_extensions.Contains(fi.Extension))
						{
							bool ret = IndexFile(fi);
#if DEBUG
							if (ret)
							{
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine(fi.FullName);
							}
#endif
						}
						m_scanned++;
					}
				});
			}
			catch (Exception ex)
			{
				ShowAndLogException(FileType.Folder, folder, ex);
			}
		}

		private static void IndexFolder(string folder)
		{
			try
			{
				if (!OkToIndex(folder))
					return;

				foreach (string file in Directory.GetFiles(folder))
				{
					FileInfo fi = new FileInfo(file);
					if (m_extensions.Contains(fi.Extension))
					{
						bool ret = IndexFile(fi);

#if DEBUG
						if (ret)
						{
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine(fi.FullName);
						}
#endif

					}
					m_scanned++;
				}

				foreach (string childFolder in Directory.GetDirectories(folder))
				{
					IndexFolder(childFolder);
				}
			}
			catch (Exception ex)
			{
				ShowAndLogException(FileType.Folder, folder, ex);
			}
		}

		private static object objLock = new object();
		private static bool IndexFile(FileInfo file)
		{
			try
			{
				lock (objLock)
				{
					IndexManager.IndexFile(file);
					m_indexed++;
				}

				return true;
			}
			catch (Exception ex)
			{
				ShowAndLogException(FileType.File, file.FullName, ex);
				return false;
			}
		}

		private static List<string> GetConfigList(string items)
		{
			List<string> list = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(Path.Combine(K_CURRENTPATH, K_CONFIGFILE));

			XmlNodeList nodes = doc.SelectNodes("//" + items);
			foreach (XmlNode node in nodes)
			{
				list.Add(node.InnerText);
			}

			return list;
		}

		private static void ShowAndLog(string message)
		{
			Console.WriteLine(message);
			Log(message);
		}

		private static void ShowAndLogException(FileType type, string file, Exception ex)
		{
			lock (objLock)
			{
				errors = true;
				string errorMsg = string.Format("Error on {0}:{1}", file, ex.Message);

				Debug.Assert(false, errorMsg);

				m_errors.Add(file, errorMsg);

				ConsoleColor cc = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("");
				Console.WriteLine(errorMsg);
				Console.ForegroundColor = cc;

				//TraceManager.Error(errorMsg);
			}
		}

		private static void ShowSummary()
		{
			ShowAndLog("\r\nIndexing started at " + started.ToShortTimeString() + " and ended at " + DateTime.Now.ToShortTimeString());
			string summary = string.Format("It took '{0:0.#}' seconds to index {1} files. Parallel:{2} Errors:{3}", chrono.Elapsed.TotalSeconds, m_indexed, UseParallel, errors);
			//TraceManager.Debug(summary);
			ShowAndLog(summary);
			if (errors)
			{
				//Console.WriteLine("=== ERRORS ===");
				//foreach (string errorLine in m_errors.Values)
				//{
				//    Console.WriteLine(errorLine);
				//}
				//Console.WriteLine("=== ERRORS ===");
				ShowAndLog(string.Format("There were {0} errors", m_errors.Values.Count));
				AddExclusions();
			}
			else
				Console.WriteLine("No errors found.");
		}

		private static void AddExclusions()
		{
			//TraceManager.Info("== EXCLUSIONS ==");
			//foreach (string dir in m_errors.Keys)
			//	TraceManager.Info(string.Format("<exclusion>{0}</exclusion>", dir));

		}

		private enum FileType
		{
			File,
			Folder
		}

		private static void Log(string tokens)
		{
#if DEBUG
			//TraceManager.Debug(tokens);
#endif
		}

		#region Win32 API


		[StructLayout(LayoutKind.Sequential)]
		public struct SHARE_INFO_0
		{
			[MarshalAs(UnmanagedType.LPWStr)]
			public String shi0_netname;
		}


		[DllImport("Netapi32.dll")]
		public static extern int NetShareEnum([MarshalAs(UnmanagedType.LPWStr)]
			string servername,
			Int32 level,
			out IntPtr bufptr,
			Int32 prefmaxlen,
			[MarshalAs(UnmanagedType.LPArray)] Int32[] entriesread,
			[MarshalAs(UnmanagedType.LPArray)] Int32[] totalentries,
			[MarshalAs(UnmanagedType.LPArray)] Int32[] resume_handle
								);

		[DllImport("Netapi32.dll")]
		public static extern int NetApiBufferFree(long lpBuffer);

		public static string[] GetShares(string server)
		{
			IntPtr buf = new IntPtr(0);
			Int32[] dwEntriesRead = new Int32[1];
			dwEntriesRead[0] = 0;
			Int32[] dwTotalEntries = new Int32[1];
			dwTotalEntries[0] = 0;
			Int32[] dwResumeHandle = new Int32[1];
			dwResumeHandle[0] = 0;
			Int32 success = 0;
			string[] shares = new string[0];


			success = NetShareEnum(server, 0, out buf, -1, dwEntriesRead, dwTotalEntries, dwResumeHandle);
			if (dwEntriesRead[0] > 0)
			{
				SHARE_INFO_0[] s0 = new SHARE_INFO_0[dwEntriesRead[0]];
				shares = new string[dwEntriesRead[0]];
				for (int i = 0; i < dwEntriesRead[0]; i++)
				{
					s0[i] = (SHARE_INFO_0)Marshal.PtrToStructure(buf, typeof(SHARE_INFO_0));
					shares[i] = s0[i].shi0_netname;
					buf = (IntPtr)((long)buf + Marshal.SizeOf(s0[0]));
				}
				//NetApiBufferFree((long)buf);
			}
			return shares;
		}

		#endregion


	}
}
