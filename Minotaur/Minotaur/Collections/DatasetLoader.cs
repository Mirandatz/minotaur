namespace Minotaur.Collections {
	using System;
	using System.IO;

	public static class DatasetLoader {

		public static MutableMatrix<float> LoadData(string filename) {
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			var csv = File.ReadAllText(filename);
			return CsvParser.ParseCsv(csv);
		}

		public static MutableMatrix<bool> LoadLabels(string filename) {
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			var csv = File.ReadAllText(filename);
			var labelsAsFloats = CsvParser.ParseCsv(csv);
			var labels = labelsAsFloats.CastValues(f => {
				if (f == 0)
					return false;
				if (f == 1)
					return true;
				throw new InvalidOperationException("Label's can only be 0 or 1");
			});

			return labels;
		}

		public static FeatureType[] LoadFeatureTypes(string filename) {
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			var lines = File.ReadAllLines(filename);
			var featureTypes = new FeatureType[lines.Length];

			for (int i = 0; i < lines.Length; i++)
				featureTypes[i] = ParseFeatureType(lines, i);

			return featureTypes;
		}

		private static FeatureType ParseFeatureType(string[] lines, int i) {
			var originalText = lines[i];
			var sanitizedText = originalText
				.Trim()
				.ToLower();

			switch (sanitizedText) {
			case "categorical":
			return FeatureType.Categorical;

			case "continuous":
			return FeatureType.Continuous;

			default:
			throw new InvalidOperationException($"Error on line {i}: unable to parse {originalText}");
			}
		}
	}
}
