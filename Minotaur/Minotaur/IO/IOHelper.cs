namespace Minotaur.IO {
	using System.Globalization;
	using System.IO;
	using System.Text;
	using CsvHelper;
	using CsvHelper.Configuration;

	public static class IOHelper {

		public static StreamReader CreateStreamReader(string path) {
			return new StreamReader(
				path: path,
				encoding: Encoding.UTF8);
		}

		public static CsvReader CreateCsvReader(StreamReader streamReader) {
			var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) {
				Encoding = Encoding.UTF8,
			};

			return new CsvReader(
				reader: streamReader,
				configuration: cfg);
		}
	}
}
