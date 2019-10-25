namespace Minotaur.Collections.Dataset {
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;

	public static class DatasetLoader {

		public static (Dataset TrainDataset, Dataset TestDataset) LoadDatasets(
			string trainDataFilename,
			string trainLabelsFilename,
			string testDataFilename,
			string testLabelsFilename,
			ClassificationType classificationType
			) {
			Console.Write("Loading datasets... ");

			var trainData = Task.Run(() => DatasetLoader.ReadDataFile(trainDataFilename));
			var trainLabels = Task.Run(() => DatasetLoader.ReadLabelFile(trainLabelsFilename, classificationType));

			var testData = Task.Run(() => DatasetLoader.ReadDataFile(testDataFilename));
			var testLabels = Task.Run(() => DatasetLoader.ReadLabelFile(testLabelsFilename, classificationType));

			Task.WaitAll(
				trainData,
				trainLabels,
				testData,
				testLabels);

			Console.WriteLine("Done.");

			var featureTypes = new FeatureType[trainData.Result.ColumnCount];
			for (int i = 0; i < featureTypes.Length; i++)
				featureTypes[i] = FeatureType.Continuous;

			var trainDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes,
				mutableData: trainData.Result,
				labels: trainLabels.Result,
				isTrainDataset: true,
				classificationType: classificationType);

			var testDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes,
				mutableData: testData.Result,
				labels: testLabels.Result,
				isTrainDataset: false,
				classificationType: classificationType);

			Console.Write("Checking if TrainDataset and TestDataset are compatible... ");
			if (trainDataset.FeatureCount != testDataset.FeatureCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same feature count");
			if (trainDataset.ClassCount != testDataset.ClassCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same class count");

			for (int i = 0; i < trainDataset.FeatureCount; i++) {
				var trainFeatureType = trainDataset.GetFeatureType(i);
				var testFeatureType = testDataset.GetFeatureType(i);

				if (trainFeatureType != testFeatureType)
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " " +
						"must have the same feature types.");
			}

			Console.WriteLine("Yep, they are.");

			return (TrainDataset: trainDataset, TestDataset: testDataset);
		}

		private static MutableMatrix<float> ReadDataFile(string filename) {
			var unparsedMatrix = CsvParser.ReadCsv(filename);

			var parsedData = new MutableMatrix<float>(
				rowCount: unparsedMatrix.RowCount,
				columnCount: unparsedMatrix.ColumnCount);

			var instanceCount = unparsedMatrix.RowCount;
			var featureCount = unparsedMatrix.ColumnCount;

			for (int i = 0; i < instanceCount; i++) {
				for (int j = 0; j < featureCount; j++) {
					var valueAsString = unparsedMatrix.Get(i, j);
					var parsed = float.TryParse(valueAsString, out var parsedValue);

					if (!parsed) {
						throw new InvalidOperationException($"Parsing error. Unable to parse {valueAsString} as float. " +
							$"Line (0-indexed) {i}, column (0-indexed) {j}. " +
							$"File: {filename}");
					}

					if (!float.IsFinite(parsedValue)) {
						throw new InvalidOperationException($"Parsing error. Unable to parse {valueAsString} as finite float. " +
							$"Line (0-indexed) {i}, column (0-indexed) {j}. " +
							$"File: {filename}");
					}


					parsedData.Set(i, j, parsedValue);
				}
			}

			return parsedData;
		}

		private static ILabel[] ReadLabelFile(string filename, ClassificationType classificationType) {
			return classificationType switch
			{
				ClassificationType.SingleLabel => ReadSingleLabelFile(filename),
				ClassificationType.MultiLabel => ReadMultiLabelFile(filename),
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			static SingleLabel[] ReadSingleLabelFile(string filename) {
				var unparsedMatrix = CsvParser.ReadCsv(filename);

				if (unparsedMatrix.ColumnCount != 1) {
					throw new InvalidOperationException($"For a {ClassificationType.SingleLabel} dataset, the labels file must constain a single column. " +
						$"But the file {filename} contains {unparsedMatrix.ColumnCount}.");
				}

				var labels = ParseLabels(unparsedMatrix, filename);

				var labelsAreNaturalRange = CheckIfLabelsAreNaturalRange(labels);
				if (!labelsAreNaturalRange) {
					throw new InvalidOperationException($"Labels must be form a natural range, " +
						$"that is, their values must be between [0, #unique-labels[. " +
						$"That doesn't hold for file: {filename}");
				}

				return labels;

				static SingleLabel[] ParseLabels(MutableMatrix<string> matrix, string filename) {
					var labels = new SingleLabel[matrix.RowCount];

					for (int i = 0; i < labels.Length; i++) {
						var valueAsString = matrix.Get(rowIndex: i, columnIndex: 0);
						var parsed = int.TryParse(valueAsString, out var parsedValue);

						if (!parsed)
							throw new InvalidOperationException($"Parsing error. Can't parse {valueAsString} as int. Line {i}. File: {filename}.");

						labels[i] = new SingleLabel(parsedValue);
					}

					return labels;
				}

				static bool CheckIfLabelsAreNaturalRange(SingleLabel[] labelsAsInts) {
					var sortedUniqueLabelValues = labelsAsInts
						.Select(sl => sl.Value)
						.Distinct()
						.OrderBy(v => v)
						.ToArray();

					for (int i = 0; i < sortedUniqueLabelValues.Length; i++) {
						if (sortedUniqueLabelValues[i] != i)
							return false;
					}

					return true;
				}
			}

			static MultiLabel[] ReadMultiLabelFile(string filename) {
				var unparsedMatrix = CsvParser.ReadCsv(filename);
				var labels = new MultiLabel[unparsedMatrix.RowCount];

				for (int i = 0; i < labels.Length; i++) {
					var line = unparsedMatrix.GetRow(i);
					var label = ParseLine(line, filename);
					labels[i] = label;
				}

				return labels;

				static MultiLabel ParseLine(Span<string> line, string filename) {
					var labels = new bool[line.Length];

					for (int i = 0; i < line.Length; i++) {
						var valueAsString = line[i];

						var parsed = int.TryParse(line[i], out var parsedValue);
						if (!parsed)
							throw new InvalidOperationException($"Parsing error. Can't parse {valueAsString} as bool. Line {i}. File: {filename}.");

						labels[i] = parsedValue switch
						{
							0 => false,
							1 => true,
							_ => throw new InvalidOperationException($"Parsing error. Can't parse {valueAsString} as bool. Line {i}. File: {filename}.")
						};
					}

					return new MultiLabel(labels);
				}
			}
		}
	}
}
