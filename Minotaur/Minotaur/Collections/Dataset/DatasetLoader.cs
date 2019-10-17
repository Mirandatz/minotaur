namespace Minotaur.Collections.Dataset {
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;

	public static class DatasetLoader {

		private static MutableMatrix<float> LoadData(string filename) {
			var csv = File.ReadAllText(filename);
			return CsvParser.ParseFloatsCsv(csv);
		}

		private static Array<ILabel> LoadLabels(string filename, ClassificationType classificationType) {
			var csv = File.ReadAllText(filename);
			var labelsAsFloats = CsvParser.ParseFloatsCsv(csv);

			return classificationType switch
			{
				ClassificationType.SingleLabel => ParseSingleLabel(labelsAsFloats),
				ClassificationType.MultiLabel => ParseMultiLabel(labelsAsFloats),
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			static Array<ILabel> ParseSingleLabel(MutableMatrix<float> labelsMatrix) {
				if (labelsMatrix.ColumnCount != 1) {
					throw new InvalidOperationException($"For a classificatio problem with type {ClassificationType.SingleLabel}, " +
						$"the labels file must contain only one column.");
				}

				var instanceCount = labelsMatrix.RowCount;
				var labels = new ILabel[instanceCount];

				for (int i = 0; i < instanceCount; i++) {
					var value = labelsMatrix.Get(rowIndex: i, columnIndex: 0);
					var label = new SingleLabel((int) value);
					labels[i] = label;
				}

				// Sanity check
				var sortedDistinctLabels = labels
					.Select(l => ((SingleLabel) l).Value)
					.Distinct()
					.OrderBy(v => v)
					.ToArray();

				for (int i = 0; i < sortedDistinctLabels.Length; i++) {
					if (sortedDistinctLabels[i] != i)
						throw new InvalidOperationException($"Labels for a {nameof(ClassificationType.SingleLabel)} dataset " +
							"must be a natural range; that is, their values must be between [0, #classes[");
				}

				return labels;
			}

			static Array<ILabel> ParseMultiLabel(MutableMatrix<float> labelsMatrix) {
				var instanceCount = labelsMatrix.RowCount;
				var labels = new ILabel[instanceCount];

				for (int i = 0; i < instanceCount; i++) {
					var values = labelsMatrix.GetRow(i);
					var label = MultiLabelCreator.FromSpanOfBinaryValues(values);
					labels[i] = label;
				}

				return labels;
			}
		}

		private static FeatureType[] LoadFeatureTypes(string filename) {
			var lines = File.ReadAllLines(filename);
			var featureTypes = new FeatureType[lines.Length];

			for (int i = 0; i < lines.Length; i++)
				featureTypes[i] = ParseFeatureType(lines, i);

			return featureTypes;

			static FeatureType ParseFeatureType(string[] lines, int i) {
				var originalText = lines[i];
				var sanitizedText = originalText
					.Trim()
					.ToLower();

				return sanitizedText switch
				{
					"continuous" => FeatureType.Continuous,
					_ => throw new InvalidOperationException($"Error on line {i}: unable to parse {originalText}"),
				};
			}
		}

		public static (Dataset TrainDataset, Dataset TestDataset) LoadDatasets(
			string trainDataFilename,
			string trainLabelsFilename,
			string testDataFilename,
			string testLabelsFilename,
			ClassificationType classificationType
			) {
			Console.Write("Loading datasets... ");

			var trainData = Task.Run(() => DatasetLoader.LoadData(trainDataFilename));
			var trainLabels = Task.Run(() => DatasetLoader.LoadLabels(trainLabelsFilename, classificationType));

			var testData = Task.Run(() => DatasetLoader.LoadData(testDataFilename));
			var testLabels = Task.Run(() => DatasetLoader.LoadLabels(testLabelsFilename, classificationType));

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

		public static MutableMatrix<float> ReadDataFile(string filename) {
			var unparsedMatrix = CsvParser.ReadCsv(filename);

			var parsedData = new MutableMatrix<float>(
				rowCount: unparsedMatrix.RowCount,
				columnCount: unparsedMatrix.ColumnCount);

			throw new NotImplementedException();
		}

		public static SingleLabel[] ReadSingleLabelFile(string filename) {
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
				var labels = new SingleLabel[matrix.ColumnCount];

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

		public static MultiLabel[] ReadMultiLabelFile(string filename) {
			var unparsedMatrix = CsvParser.ReadCsv(filename);
			var labels = new MultiLabel[unparsedMatrix.ColumnCount];

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
					var parsed = bool.TryParse(line[i], out var parsedValue);
					if (!parsed)
						throw new InvalidOperationException($"Parsing error. Can't parse {valueAsString} as bool. Line {i}. File: {filename}.");

					labels[i] = parsedValue;
				}

				return new MultiLabel(labels);
			}
		}
	}
}
