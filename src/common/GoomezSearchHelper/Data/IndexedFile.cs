namespace GoomezSearchHelper.Data
{
	/// <summary>
	/// Summary description for IndexedFile
	/// </summary>
	//[SerializePropertyNamesAsCamelCase]
	public class IndexedFile
	{
		public IndexedFile() { }

		public string Id { get; set; }
		public string Content { get; set; }
		public long Size { get; set; }
		public string Extension { get; set; }
		public string File { get; set; }
		public string Folder { get; set; }
		public string Full { get; set; }
	}
}