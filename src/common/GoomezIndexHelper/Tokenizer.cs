using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GoomezIndexHelper
{
	public class Tokenizer
	{
		public static string Tokenize(string path, bool manageExtension)
		{
			path = path.Replace(Path.DirectorySeparatorChar, ' ');
			path = path.Replace('_', ' ');
			path = path.Replace('-', ' ');
			
			if (manageExtension)
			{
				int dot = path.LastIndexOf(".");
				if (dot != -1)
					path = path.Insert(dot, " ");
			}

			string output = "";

			bool lower = true;
			bool upper = true;
			bool number = true;
			bool space = true;
			bool other = true;

			foreach (char c in path.Trim())
			{
				if (char.IsNumber(c))
				{
					if (!number && !space)
						output += " ";

					output += c;

					lower = false;
					upper = false;
					number = true;
					other = false;
					space = false;
					continue;
				}
				else if (char.IsLetter(c))
				{
					if (char.IsLower(c))
					{
						if (!lower && !upper && !space)
							output += " ";

						output += c;

						lower = true;
						upper = false;
						number = false;
						other = false;
						space = false;
						continue;
					}
					else if (char.IsUpper(c))
					{
						if (!upper && !space)
							output += " ";

						output += c;

						lower = false;
						upper = true;
						number = false;
						other = false;
						space = false;
						continue;
					}
				}
				else if (char.IsWhiteSpace(c))
				{
					if (space)
						continue;

					output += c;

					lower = false;
					upper = false;
					number = false;
					other = false;
					space = true;
					continue;
				}
				else
				{
					if (!other && !space)
						output += " ";

					output += c;

					lower = false;
					upper = false;
					number = false;
					other = true;
					space = false;
					continue;
				}
			}

			return output;
		}

		private static string Spaceize(string fullPath)
		{
			fullPath = fullPath.Replace(@"\", " ");
			fullPath = fullPath.Replace(@"_", " ");
			fullPath = fullPath.Replace(@"-", " ");
			fullPath = fullPath.Replace(@".", " ");

			return fullPath;
		}

		public static string TokenizeToIndex(string fullPath)
		{
			string strRet = Tokenize(fullPath, true);
			strRet = strRet + " " + Spaceize(fullPath);

			return strRet;
		}
	}
}
