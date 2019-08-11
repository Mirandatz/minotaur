namespace Minotaur.Collections {
	using System;
	using System.Linq;

	public static class CsvParser {
		public const char DefaultDelimiter = ',';

		public static MutableMatrix<float> ParseCsv(string csvData, char delimiter = DefaultDelimiter) {
			if (string.IsNullOrEmpty(csvData))
				throw new ArgumentException(nameof(csvData) + " can't be null nor empty.");

			var lines = GetAndCheckLines(csvData);
			var width = GetAndCheckMatrixWidth(lines, delimiter);
			var height = lines.Length;

			var matrix = new MutableMatrix<float>(rowCount: height, columnCount: width);
			for (int i = 0; i < height; i++)
				ParseLine(lineNumber: i, delimiter: delimiter, lines: lines, matrix: matrix);

			return matrix;
		}

		private static String[] GetAndCheckLines(string csvData) {
			var lines = csvData.Split(
				separator: Environment.NewLine,
				options: StringSplitOptions.RemoveEmptyEntries);

			if (lines.Length == 0)
				throw new InvalidOperationException("The file is empty.");

			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Length == 0)
					throw new InvalidOperationException($"Line {i} is empty.");
			}

			return lines;
		}

		private static int GetAndCheckMatrixWidth(string[] lines, char delimiter) {
			var firstLineDelimiterCount = lines
				.First()
				.Count(c => c == delimiter);

			for (int i = 0; i < lines.Length; i++) {
				var delimiterCount = lines[i].Count(c => c == delimiter);

				if (delimiterCount != firstLineDelimiterCount)
					throw new InvalidOperationException($"Line {i} contains {delimiterCount} delimiters, but should contain {firstLineDelimiterCount}.");
			}

			// +1 coz if we have N delimiters, we have N+1 values
			return firstLineDelimiterCount + 1;
		}

		private static void ParseLine(
			int lineNumber,
			char delimiter,
			string[] lines,
			MutableMatrix<float> matrix) {

			var line = lines[lineNumber];
			var splits = line.Split(delimiter);

			for (int i = 0; i < splits.Length; i++) {
				var parsed = float.TryParse(splits[i], out var value);

				if (!parsed) {
					throw new InvalidOperationException($"Unable to parse element. Line: {lineNumber},  Column: {i}, Value: {splits[i]}.");
				}

				matrix.Set(
					rowIndex: lineNumber,
					columnIndex: i,
					value: value);
			}
		}
	}
}
