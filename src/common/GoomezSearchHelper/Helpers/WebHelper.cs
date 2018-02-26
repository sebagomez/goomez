using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GoomezSearchHelper.Helpers
{
	public class WebHelper
	{
		public static string WebStartupPath(Assembly ass)
		{
			string catalog = ass.CodeBase;

			catalog = catalog.Replace("file:///", "");

			int slash = catalog.LastIndexOf("/");
			catalog = catalog.Substring(0, slash - 1);
			slash = catalog.LastIndexOf("/");
			catalog = catalog.Substring(0, slash);

			return catalog.Replace("/", "\\");
		}
	}
}
