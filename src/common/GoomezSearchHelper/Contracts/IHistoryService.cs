using System;
using System.Collections.Generic;
using System.ServiceModel;
using GoomezSearchHelper.Data;

namespace GoomezSearchHelper.Contracts
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IHistoryService" in both code and config file together.
	[ServiceContract]
	public interface IHistoryService
	{
		[OperationContract]
		List<TextSearched> GetHistoryByUserDate(DateTime datePicked, string user);

		[OperationContract]
		List<TextSearched> SearchHistoryByUserPattern(string pattern, string user);

		[OperationContract]
		List<TextSearched> GetHistoryBetweenDates(DateTime fromDate, DateTime toDate, string user);
	}
}
