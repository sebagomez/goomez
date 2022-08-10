using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoomezIndexHelper.Helpers
{
	static class StringHelper
	{
		public static bool Contains(this string val, string[] array)
		{
			foreach (string item in array)
				if (val.Contains(item))
					return true;

			return false;
		}
	}
}