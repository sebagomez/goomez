using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoomezIndexHelper
{
	public class User
	{
		public string Name { get; internal set; }

		public User(string user)
		{
			if (user.Contains("\\"))
				Name = user.Substring(user.LastIndexOf("\\") + 1);
			else
				Name = user;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
