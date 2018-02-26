using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Core;
using System.IO;

namespace GoomezSearchHelper
{
	public class TraceManager
	{
		private static ILog log;

		static TraceManager()
		{
			if (System.Configuration.ConfigurationManager.GetSection("log4net") != null)
			{
				XmlConfigurator.Configure();
			}
			log = log4net.LogManager.GetLogger("Goomez");
		}

		public static void Initialize(string loggerName, string configFile)
		{
			System.Diagnostics.Debug.Assert(File.Exists(configFile), string.Format("configFile not found '{0}'", configFile));

			ILoggerRepository loggerRep;
			if (((DefaultRepositorySelector)LoggerManager.RepositorySelector).ExistsRepository(loggerName))
				loggerRep = LogManager.GetRepository(loggerName);
			else
				loggerRep = LogManager.CreateRepository(loggerName);

			XmlConfigurator.ConfigureAndWatch(loggerRep, new FileInfo(configFile));
			log = log4net.LogManager.GetLogger(loggerName, loggerName);
		}

		#region Debug
		public static void Debug(string message)
		{
			Debug(message, null);
		}

		public static void Debug(string message, Exception ex)
		{
			if (log.IsDebugEnabled)
				log.Debug(message, ex);
		}
		#endregion

		#region Error
		public static void Error(string message)
		{
			Error(message, null);
		}

		public static void Error(string message, Exception ex)
		{
			if (log.IsErrorEnabled)
				log.Error(message, ex);
		}
		#endregion

		#region Warn
		public static void Warn(string message)
		{
			Warn(message, null);
		}

		public static void Warn(string message, Exception ex)
		{
			if (log.IsWarnEnabled)
				log.Warn(message, ex);
		}
		#endregion

		#region Info
		public static void Info(string message)
		{
			Info(message, null);
		}

		public static void Info(string message, Exception ex)
		{
			if (log.IsInfoEnabled)
				log.Info(message, ex);
		}
		#endregion

		#region Fatal
		public static void Fatal(string message)
		{
			Fatal(message, null);
		}

		public static void Fatal(string message, Exception ex)
		{
			if (log.IsFatalEnabled)
				log.Fatal(message, ex);
		}
		#endregion
	}
}
