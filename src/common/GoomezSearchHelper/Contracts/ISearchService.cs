using System.Collections.Generic;
using System.ServiceModel;
using GoomezSearchHelper.Data;

namespace GoomezSearchHelper.Contracts
{
	[ServiceContract]
	public interface ISearchService
	{
		[OperationContract]
		List<IndexedFile> Search(string pattern, string user);
	}
}
