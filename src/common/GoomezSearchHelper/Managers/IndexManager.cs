using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace GoomezSearchHelper.Managers
{
	public class IndexManager
	{
		private static IndexWriter m_indexWriter = null;

		public static void Initialize(string indexPath)
		{
			FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath));
			m_indexWriter = new IndexWriter(directory, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), true, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);
		}

		public static void CloseIndex()
		{
			if (m_indexWriter != null)
			{
				m_indexWriter.Optimize();
				m_indexWriter.Dispose();
			}
		}

		public static void IndexFile(FileInfo file)
		{
			Document doc = new Document();
			doc.Add(new Field(Constants.Full, file.FullName, Field.Store.YES, Field.Index.NO));
			doc.Add(new Field(Constants.File, file.Name, Field.Store.YES, Field.Index.NO));
			doc.Add(new Field(Constants.Folder, file.Directory.FullName, Field.Store.YES, Field.Index.NO));
			doc.Add(new Field(Constants.Extension, file.Extension.Replace(".",""), Field.Store.YES, Field.Index.NO));
			doc.Add(new Field(Constants.Size, file.Length.ToString(), Field.Store.YES, Field.Index.NO));
			doc.Add(new Field(Constants.Content, GoomezSearchHelper.Tokenizer.TokenizeToIndex(file.FullName), Field.Store.YES, Field.Index.ANALYZED));

			m_indexWriter.AddDocument(doc);

			TraceManager.Debug(file.FullName);
		}

	}
}
