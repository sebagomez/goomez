using System;
using System.Collections.Generic;
using GoomezIndexHelper.Data;
using Microsoft.AspNetCore.Mvc;

namespace GoomezReactJS.Controllers
{
	[Route("api/[controller]")]
	public class SearchController : Controller
	{
		[HttpGet("[action]")]
		public IEnumerable<IndexedFile> Search(string pattern)
		{
			GoomezIndexHelper.Managers.SearchManager mgr = new GoomezIndexHelper.Managers.SearchManager();
			return mgr.Search(pattern, GoomezIndexHelper.User.Fake(), int.MaxValue);
		}
	}
}
