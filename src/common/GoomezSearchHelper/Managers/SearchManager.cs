using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GoomezSearchHelper.Data;
using GoomezSearchHelper.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;


namespace GoomezSearchHelper.Managers
{

	/// <summary>
	/// Summary description for IndexSearcher
	/// </summary>
	public class SearchManager
	{
		private string m_SearchPath;
		private string m_HistoryPath;

		public SearchManager(string indexPath)
		{

			if (string.IsNullOrEmpty(indexPath))
				throw new ArgumentNullException("indexPath");

			m_SearchPath = indexPath;
		}


		public SearchManager(string indexPath, string historyPath)
		{

			if (string.IsNullOrEmpty(indexPath) || (string.IsNullOrEmpty(historyPath)))
				throw new ArgumentException("indexPath and historyPath parameters are mandatory");

			m_SearchPath = indexPath;
			m_HistoryPath = historyPath;
		}

		public Lucene.Net.Util.Version LuceneVersion
		{
			get { return Lucene.Net.Util.Version.LUCENE_30; }
		}

		public string SearchPath
		{
			get { return m_SearchPath; }
			set { m_SearchPath = value; }
		}

		Lucene.Net.Store.Directory m_searchDirectory;
		public Lucene.Net.Store.Directory SearchDirectory
		{
			get
			{
				if (m_searchDirectory == null)
				{
					DirectoryInfo dir = new DirectoryInfo(SearchPath);
					m_searchDirectory = Lucene.Net.Store.FSDirectory.Open(dir);
				}
				return m_searchDirectory;
			}
		}


		public string HistoryPath
		{
			get { return m_HistoryPath; }
			set { m_HistoryPath = value; }
		}

		Lucene.Net.Store.Directory m_historyDirectory;
		public Lucene.Net.Store.Directory HistoryDirectory
		{
			get
			{
				if (m_historyDirectory == null)
				{
					DirectoryInfo dir = new DirectoryInfo(HistoryPath);
					m_historyDirectory = Lucene.Net.Store.FSDirectory.Open(dir);
				}
				return m_historyDirectory;
			}
		}

		#region Search

		public List<IndexedFile> Search(string pattern, User user)
		{
			return Search(pattern, user, int.MaxValue);
		}

		string[] UNAUTHORIZED = { "key", "keygen", "keygenerator", "key gen", "key generator", "crack", "trial" };

		public List<IndexedFile> Search(string pattern, User user, int max)
		{
			if (string.IsNullOrEmpty(pattern))
				throw new ArgumentNullException("pattern");

			List<IndexedFile> list = null;
			if (pattern.ToLower().Contains(UNAUTHORIZED))
				return list;

			using (IndexSearcher searcher = new IndexSearcher(SearchDirectory, true))
			{
				list = new List<IndexedFile>();

				string originalPattern = pattern;
				string tokenizedPattern = Tokenizer.Tokenize(pattern, false);
				if (pattern != tokenizedPattern)
					pattern = pattern + " OR \"" + tokenizedPattern + "\"";

				QueryParser parser = new QueryParser(LuceneVersion, Constants.Content, new StandardAnalyzer(LuceneVersion));
				parser.DefaultOperator = QueryParser.Operator.AND;
				Query query = parser.Parse(pattern);

				TopDocs docs = searcher.Search(query, max);
				TraceManager.Debug($"{docs.TotalHits} hits found for '{pattern}'");

				foreach (ScoreDoc scoreDoc in docs.ScoreDocs)
				{
					if (list.Count == max)
						break;

					IndexedFile fi = new IndexedFile();

					Document doc = searcher.Doc(scoreDoc.Doc);
					fi.Full = doc.Get(Constants.Full);
					fi.File = doc.Get(Constants.File);
					fi.Folder = doc.Get(Constants.Folder);
					fi.Extension = doc.Get(Constants.Extension);
					fi.Size = long.Parse(doc.Get(Constants.Size));

					list.Add(fi);
				}

				if (list.Count > 0 &&
					!string.IsNullOrEmpty(m_HistoryPath) &&
					!string.IsNullOrEmpty(user.Name))
				{
					Thread saveThread = new Thread(SaveSearch);
					object[] parms = new object[] { originalPattern, user };
					saveThread.Start(parms);
				}
			}
			return list;

		}

		#endregion

		#region History

		public List<TextSearched> GetHistoryByUserDate(DateTime datePicked, User user)
		{
			if (string.IsNullOrEmpty(m_HistoryPath))
				throw new ApplicationException("HistoryPath not set.");

			if (user == null)
				throw new ArgumentNullException("user");

			List<TextSearched> list = null;
			using (IndexSearcher searcher = new IndexSearcher(HistoryDirectory, true))
			{
				TraceManager.Debug(string.Format("{0} is looking for history from {1}", user, datePicked));

				list = new List<TextSearched>();

				string from = datePicked.ToString("yyyyMMddHHmmss");
				string to = datePicked.AddDays(1).ToString("yyyyMMddHHmmss");

				QueryParser parserDate = new QueryParser(LuceneVersion, Constants.DateTicks, new WhitespaceAnalyzer());
				Query queryDate = parserDate.Parse("[" + from + " TO " + to + "]");

				BooleanQuery query = new BooleanQuery();

				query.Add(queryDate, Occur.MUST);

				QueryParser parserUser = new QueryParser(LuceneVersion, Constants.VisitUser, new WhitespaceAnalyzer());
				Query queryUser = parserUser.Parse(user.Name);
				query.Add(queryUser, Occur.MUST);

				TopDocs docs = searcher.Search(query, int.MaxValue);
				for (int i = 0; i < docs.TotalHits; i++)
				{
					TextSearched st = new TextSearched();

					Document doc = searcher.Doc(i);
					st.DateTicks = doc.Get(Constants.DateTicks);
					st.VisitUser = doc.Get(Constants.VisitUser);
					st.SearchedText = doc.Get(Constants.SearchedText);

					list.Add(st);
				}

				list.Sort();
			}

			return list;

		}

		public List<TextSearched> SearchHistoryByUserPattern(string pattern, User user)
		{
			if (string.IsNullOrEmpty(m_HistoryPath))
				throw new ApplicationException("HistoryPath not set.");

			if (user == null)
				throw new ArgumentNullException("user");

			List<TextSearched> list = null;
			using (IndexSearcher searcher = new IndexSearcher(HistoryDirectory, true))
			{

				TraceManager.Debug(string.Format("{0} is looking for history on {1}", user, pattern));

				list = new List<TextSearched>();

				QueryParser parserUser = new QueryParser(LuceneVersion, Constants.VisitUser, new WhitespaceAnalyzer());
				parserUser.DefaultOperator = QueryParser.Operator.AND;
				QueryParser parserPattern = new QueryParser(LuceneVersion, Constants.SearchedText, new WhitespaceAnalyzer());
				parserUser.DefaultOperator = QueryParser.Operator.AND;

				Query queryUser = parserUser.Parse(user.Name);
				Query queryPattern = parserPattern.Parse(pattern);

				BooleanQuery query = new BooleanQuery();
				query.Add(queryUser, Occur.MUST);
				query.Add(queryPattern, Occur.MUST);

				TopDocs hits = searcher.Search(query, int.MaxValue);
				for (int i = 0; i < hits.TotalHits; i++)
				{
					TextSearched st = new TextSearched();

					Document doc = searcher.Doc(i);
					st.DateTicks = doc.Get(Constants.DateTicks);
					st.VisitUser = doc.Get(Constants.VisitUser);
					st.SearchedText = doc.Get(Constants.SearchedText);

					list.Add(st);
				}

				list.Sort();
			}

			return list;
		}

		public List<TextSearched> GetHistoryBetweenDates(DateTime fromDate, DateTime toDate, User user)
		{
			if (string.IsNullOrEmpty(m_HistoryPath))
				throw new ApplicationException("HistoryPath not set.");

			if (user == null)
				throw new ArgumentNullException("user");

			List<TextSearched> list = null;
			using (IndexSearcher searcher = new IndexSearcher(HistoryDirectory, true))
			{
				TraceManager.Debug(string.Format("{0} is looking for history between {1} and {2}", user, fromDate, toDate));

				list = new List<TextSearched>();

				string from = fromDate.ToString("yyyyMMddHHmmss");
				string to = toDate.ToString("yyyyMMddHHmmss");

				QueryParser parserUser = new QueryParser(LuceneVersion, Constants.VisitUser, new WhitespaceAnalyzer());
				parserUser.DefaultOperator = QueryParser.Operator.AND;
				//parserUser.SetDefaultOperator(QueryParser.AND_OPERATOR);
				QueryParser parserDate = new QueryParser(LuceneVersion, Constants.DateTicks, new WhitespaceAnalyzer());

				Query queryUser = parserUser.Parse(user.Name);
				Query queryDate = parserDate.Parse("[" + from + " TO " + to + "]");

				BooleanQuery query = new BooleanQuery();

				query.Add(queryUser, Occur.MUST);
				query.Add(queryDate, Occur.MUST);

				TopDocs hits = searcher.Search(query, int.MaxValue);
				for (int i = 0; i < hits.TotalHits; i++)
				{
					TextSearched st = new TextSearched();

					Document doc = searcher.Doc(i);
					st.DateTicks = doc.Get(Constants.DateTicks);
					st.VisitUser = doc.Get(Constants.VisitUser);
					st.SearchedText = doc.Get(Constants.SearchedText);

					list.Add(st);
				}
			}
			return list;

		}

		private void SaveSearch(Object data)
		{
			if (data == null)
				return;

			object[] parms = data as object[];

			Debug.Assert(parms != null);

			if (parms == null)
				return;

			string pattern = parms[0] as string;
			User user = parms[1] as User;

			if (string.IsNullOrEmpty(pattern) || user == null)
				return;

			SaveSearchHistory(pattern, user);
		}

		public void SaveSearchHistory(string pattern, User user)
		{
			if (string.IsNullOrEmpty(m_HistoryPath))
				throw new ApplicationException("HistoryPath not set.");

			TraceManager.Debug(string.Format("Saving history. '{0}' for {1}", pattern, user));

			using (IndexWriter index = new IndexWriter(HistoryDirectory, new StandardAnalyzer(LuceneVersion), !Directory.Exists(m_HistoryPath), IndexWriter.MaxFieldLength.UNLIMITED))
			{
				Document doc = new Document();
				doc.Add(new Field(Constants.DateTicks, DateTime.Now.ToString("yyyyMMddHHmmss"), Field.Store.YES, Field.Index.NOT_ANALYZED));
				doc.Add(new Field(Constants.VisitUser, user.Name, Field.Store.YES, Field.Index.NOT_ANALYZED));
				doc.Add(new Field(Constants.SearchedText, pattern, Field.Store.YES, Field.Index.ANALYZED));

				index.AddDocument(doc);
			}
		}

		#endregion

		#region Suggest

		public string DidYouMean(string pattern)
		{
			if (string.IsNullOrEmpty(m_HistoryPath))
				throw new ApplicationException("HistoryPath not set.");

			try
			{
				IndexSearcher searcher = new IndexSearcher(HistoryDirectory, true);

				Term t = new Term(Constants.SearchedText, pattern);
				FuzzyQuery query = new FuzzyQuery(t);

				TopDocs hits = searcher.Search(query, int.MaxValue);

				if (hits.TotalHits != 0)
					return searcher.Doc(0).Get(Constants.SearchedText);
				else
					return "";

			}
			catch (Exception)
			{
				return "";
			}
		}

		#endregion
	}
}