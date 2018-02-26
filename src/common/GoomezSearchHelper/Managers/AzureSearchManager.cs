
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GoomezSearchHelper.Data;
//using Microsoft.Azure.Search;
//using Microsoft.Azure.Search.Models;

namespace GoomezSearchHelper.Managers
{
	public class AzureSearchManager
	{
		const string INDEX_NAME = "files";
		const string searchServiceName = "goomez";
		const string apiKey = "741B8860A6F1E225A69DE3F8E02C5952";

		//static SearchServiceClient s_serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));

		public static void CreateIndex()
		{
			if (s_serviceClient.Indexes.Exists(INDEX_NAME))
				s_serviceClient.Indexes.Delete(INDEX_NAME);

			Index index = new Index
			{
				Name = INDEX_NAME,
				Fields = new[]
				{
					new Field(Constants.Id, DataType.String) { IsKey = true },
					new Field(Constants.Full, DataType.String),
					new Field(Constants.File, DataType.String),
					new Field(Constants.Folder, DataType.String),
					new Field(Constants.Extension, DataType.String) {IsFilterable = true },
					new Field(Constants.Size, DataType.Int64),
					new Field(Constants.Content, DataType.String) { IsSearchable = true, IsRetrievable = false }
				}
			};

			s_serviceClient.Indexes.Create(index);
			s_files = new List<IndexedFile>();
		}

		static List<IndexedFile> s_files;

		public static void IndexFile(FileInfo file)
		{
			lock (s_files)
			{
				if (s_files == null)
					throw new Exception("Index not initialized");

				IndexedFile idxFile = new IndexedFile
				{
					Id = Guid.NewGuid().ToString(),
					Full = file.FullName,
					File = file.Name,
					Folder = file.Directory.FullName,
					Extension = file.Extension.Replace(".", ""),
					Size = file.Length,
					Content = Tokenizer.TokenizeToIndex(file.FullName)
				};

				s_files.Add(idxFile);
			}
		}

		static string SanitizeKey(string filename)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var c in filename)
			{
				int ascii = (int)c;
				if (ascii >= 65 && ascii <= 90 || ascii >= 97 && ascii <= 122)
					sb.Append(c);
				else
					sb.Append("_");
			}

			return sb.ToString();
		}

		public static int UpdateIndex()
		{
			ISearchIndexClient indexClient = s_serviceClient.Indexes.GetClient(INDEX_NAME);
			int count = 1000;
			int skip = 0;

			IEnumerable<IndexedFile> aux = s_files.Skip(skip).Take(count);
			while (aux.Count() > 0)
			{
				var batch = IndexBatch.Upload<IndexedFile>(aux);
				indexClient.Documents.Index(batch);
				skip++;
				aux = s_files.Skip(skip * count).Take(count);
			}

			return s_files.Count;
		}

		public static List<IndexedFile> Search(string pattern)
		{
			List<IndexedFile> result = new List<IndexedFile>();
			ISearchIndexClient indexClient = s_serviceClient.Indexes.GetClient(INDEX_NAME);

			DocumentSearchResult<IndexedFile> response = indexClient.Documents.Search<IndexedFile>(pattern);
			foreach (SearchResult<IndexedFile> sr in response.Results)
				result.Add(sr.Document);

			return result;
		}
	}
}
