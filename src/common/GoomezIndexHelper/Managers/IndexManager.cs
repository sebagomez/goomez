using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace GoomezIndexHelper.Managers
{
	public class IndexManager
	{
		private static IndexWriter m_indexWriter = null;

		public static void Initialize(string indexPath)
		{
			if (!System.IO.Directory.Exists(indexPath))
				System.IO.Directory.CreateDirectory(indexPath);

			FSDirectory directory = FSDirectory.Open(indexPath);
			IndexWriterConfig config = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48));

			m_indexWriter = new IndexWriter(directory, config);
		}

		public static void CloseIndex()
		{
			if (m_indexWriter != null)
			{
				//m_indexWriter.Optimize();
				m_indexWriter.Dispose();
			}
		}

		public static void IndexFile(FileInfo file)
		{
			Document doc = new Document
			{
				new TextField(Constants.Full, file.FullName, Field.Store.YES),
				new TextField(Constants.File, file.Name, Field.Store.YES),
				new TextField(Constants.Folder, file.Directory.FullName, Field.Store.YES),
				new TextField(Constants.Extension, file.Extension.Replace(".", ""), Field.Store.YES),
				new TextField(Constants.Size, file.Length.ToString(), Field.Store.YES),
				new TextField(Constants.Content, GoomezIndexHelper.Tokenizer.TokenizeToIndex(file.FullName), Field.Store.YES)
			};

			m_indexWriter.AddDocument(doc);
		}
	}
}
