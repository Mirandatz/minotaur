namespace Minotaur.Collections.Dataset {
	using System;
	using System.IO;
	using System.Threading.Tasks;

	public static class DatasetLoader {

		private static MutableMatrix<float> LoadData(string filename) {
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			var csv = File.ReadAllText(filename);
			return CsvParser.ParseCsv(csv);
		}

		private static MutableMatrix<bool> LoadLabels(string filename) {
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

		private static FeatureType[] LoadFeatureTypes(string filename) {
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

		public static (Dataset TrainDataset, Dataset TestDataset) LoadDatasets(
			string trainDataFilename,
			string trainLabelsFilename,
			string testDataFilename,
			string testLabelsFilename,
			string featureTypesFilename
			) {
			if (trainDataFilename is null)
				throw new ArgumentNullException(nameof(trainDataFilename));
			if (trainLabelsFilename is null)
				throw new ArgumentNullException(nameof(trainLabelsFilename));
			if (testDataFilename is null)
				throw new ArgumentNullException(nameof(testDataFilename));
			if (testLabelsFilename is null)
				throw new ArgumentNullException(nameof(testLabelsFilename));
			if (featureTypesFilename is null)
				throw new ArgumentNullException(nameof(featureTypesFilename));

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
				mutableLabels: trainLabels.Result);

			var testDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes.Result,
				mutableData: testData.Result,
				mutableLabels: testLabels.Result);

			Console.Write("Checking if TrainDataset is usable... ");
			{
				var trainFeatureTypes = trainDataset.FeatureTypes;
				for (int i = 0; i < trainFeatureTypes.Length; i++) {
					if (trainFeatureTypes[i] == FeatureType.ContinuousButTriviallyValued) {
						throw new InvalidOperationException(
							"The train dataset cannot contain continuous features" +
							" which all instances have the same value.");
					}
				}
			}
			Console.WriteLine("Yep, it is.");


			Console.Write("Checking if TrainDataset and TestDataset are compatible... ");
			if (trainDataset.FeatureCount != testDataset.FeatureCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same feature count");
			if (trainDataset.ClassCount != testDataset.ClassCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same class count");

			for (int i = 0; i < trainDataset.FeatureCount; i++) {
				var trainFeatureType = trainDataset.GetFeatureType(i);
				var testFeatureType = testDataset.GetFeatureType(i);

				switch (trainFeatureType) {

				case FeatureType.Categorical:
				if (testFeatureType == FeatureType.Categorical ||
					testFeatureType == FeatureType.CategoricalButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.Continuous:
				if (testFeatureType == FeatureType.Continuous ||
					testFeatureType == FeatureType.ContinuousButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.CategoricalButTriviallyValued:
				if (testFeatureType == FeatureType.Categorical ||
					testFeatureType == FeatureType.CategoricalButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.ContinuousButTriviallyValued:
				if (testFeatureType == FeatureType.Continuous ||
					testFeatureType == FeatureType.ContinuousButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				default:
				throw new InvalidOperationException("Unknown / unsupported value for {nameof(FeatureType)}.");
				}
			}

			Console.WriteLine("Yep, they are.");

			return (TrainDataset: trainDataset, TestDataset: testDataset);
		}
	}
}
