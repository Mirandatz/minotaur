namespace Minotaur.Collections.Dataset {
	using System;
	using System.IO;
	using System.Threading.Tasks;

	public static class DatasetLoader {

		private static MutableMatrix<float> LoadData(string filename) {
			var csv = File.ReadAllText(filename);
			return CsvParser.ParseCsv(csv);
		}

		private static MutableMatrix<bool> LoadLabels(string filename) {
			var csv = File.ReadAllText(filename);
			var labelsAsFloats = CsvParser.ParseCsv(csv);
			var labels = labelsAsFloats.CastValues(f => {
				if (f == 0)
					return false;
				if (f == 1)
					return true;
				throw new InvalidOperationException("Label's can only be 0 or 1.");
			});

			return labels;
		}

		private static FeatureType[] LoadFeatureTypes(string filename) {
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

			return sanitizedText switch
			{
				"continuous" => FeatureType.Continuous,

				_ => throw new InvalidOperationException($"Error on line {i}: unable to parse {originalText}"),
			};
		}

		public static (Dataset TrainDataset, Dataset TestDataset) LoadDatasets(
			string trainDataFilename,
			string trainLabelsFilename,
			string testDataFilename,
			string testLabelsFilename,
			string featureTypesFilename
			) {
			Console.Write("Loading datasets... ");

			var trainData = Task.Run(() => DatasetLoader.LoadData(trainDataFilename));
			var trainLabels = Task.Run(() => DatasetLoader.LoadLabels(trainLabelsFilename));

			var testData = Task.Run(() => DatasetLoader.LoadData(testDataFilename));
			var testLabels = Task.Run(() => DatasetLoader.LoadLabels(testLabelsFilename));

			var featureTypes = Task.Run(() => DatasetLoader.LoadFeatureTypes(featureTypesFilename));

			Task.WaitAll(
				trainData,
				trainLabels,
				testData,
				testLabels,
				featureTypes);

			Console.WriteLine("Done.");

			var trainDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes.Result,
				mutableData: trainData.Result,
				mutableLabels: trainLabels.Result,
				isTrainDataset: true);

			var testDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes.Result,
				mutableData: testData.Result,
				mutableLabels: testLabels.Result,
				isTrainDataset: false);

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
