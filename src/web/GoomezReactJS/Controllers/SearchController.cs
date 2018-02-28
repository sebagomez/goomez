using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GoomezReactJS.Controllers
{
	[Route("api/[controller]")]
	public class SearchController : Controller
	{
		[HttpGet("[action]")]
		public IEnumerable<string> Search(string pattern)
		{
			GoomezIndexHelper.Managers.SearchManager mgr = new GoomezIndexHelper.Managers.SearchManager();
			//foreach (var item in mgr.Search(pattern, null, 50))
			//	yield return item.Full;

			return mgr.Search(pattern, GoomezIndexHelper.User.Fake(), 50).Select(f => f.Full);
		}
	}
}
