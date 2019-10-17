namespace Minotaur.Collections.Dataset {
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;

	public static class DatasetLoader {

		private static MutableMatrix<float> LoadData(string filename) {
			var csv = File.ReadAllText(filename);
			return CsvParser.ParseCsv(csv);
		}

		private static Array<ILabel> LoadLabels(string filename, ClassificationType classificationType) {
			var csv = File.ReadAllText(filename);
			var labelsAsFloats = CsvParser.ParseCsv(csv);

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
	}
}
