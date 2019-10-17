namespace Minotaur.Collections.Dataset {
	using System;
	using System.IO;
	using System.Linq;

	public static class CsvParser {
		public const char Delimiter = ',';

		public static MutableMatrix<string> ReadCsv(string filename) {
			var dataAsText = File.ReadAllText(filename);
			var lines = dataAsText.Split(
				separator: Environment.NewLine,
				options: StringSplitOptions.RemoveEmptyEntries);

			if (lines.Length == 0)
				throw new InvalidOperationException($"Parsing error. Empty file: {filename}");

			var width = lines[0].Count(c => c == Delimiter) + 1;
			var height = lines.Length;

			var matrix = new MutableMatrix<string>(
				rowCount: height,
				columnCount: width);

			for (int i = 0; i < height; i++) {
				var line = lines[i];
				var lineValues = line.Split(Delimiter);

				if (lineValues.Length != width) {
					throw new InvalidOperationException($"Parsing error. Line (0-indexed) {i} contains {lineValues.Length} values, expected {width}. " +
						$"File {filename}");
				}

				for (int j = 0; j < width; j++) {
					matrix.Set(
						rowIndex: i,
						columnIndex: j,
						value: lineValues[j]);
				}
			}

			return matrix;
		}
	}
}
