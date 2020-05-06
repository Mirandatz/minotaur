namespace Minotaur.Collections.Dataset {
	using System;
	using System.Globalization;
	using System.IO;
	using System.Threading.Tasks;
	using CsvHelper;
	using Minotaur.Classification;

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

			Console.Write("Initializing dataset structs... ");
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

			Console.WriteLine("Done.");

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
			var recordCount = GetNumberOfRecords(filename);
			var fieldCount = GetAndValidateNumberOfFields(filename);

			var matrix = new MutableMatrix<float>(rowCount: recordCount, columnCount: fieldCount);

			using var streamReader = new StreamReader(path: filename);
			using var csvReader = new CsvReader(reader: streamReader, CultureInfo.InvariantCulture);

			for (int i = 0; i < recordCount; i++) {
				csvReader.Read();

				for (int j = 0; j < fieldCount; j++) {
					var fieldValue = csvReader.GetField(j);

					var parsed = float.TryParse(fieldValue, out var parsedValue);
					if (!parsed)
						throw new InvalidOperationException($"Parsing error. Can't parse {fieldValue} as float. Line {i}. File: {filename}.");

					matrix.Set(rowIndex: i, columnIndex: j, value: parsedValue);
				}
			}

			return matrix;
		}

		private static ILabel[] ReadLabelFile(string filename, ClassificationType classificationType) {
			return classificationType switch
			{
				ClassificationType.SingleLabel => throw new NotImplementedException(),
				ClassificationType.MultiLabel => ReadMultiLabelFile(filename),
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			static MultiLabel[] ReadMultiLabelFile(string filename) {
				using var streamReader = new StreamReader(path: filename);
				using var csvReader = new CsvReader(reader: streamReader, CultureInfo.InvariantCulture);

				var recordCount = GetNumberOfRecords(filename);
				var fieldCount = GetAndValidateNumberOfFields(filename);

				var labels = new MultiLabel[recordCount];

				for (int i = 0; i < recordCount; i++) {
					csvReader.Read();
					labels[i] = ParseRecord(csvReader: csvReader, fieldCount: fieldCount, recordNumber: i, filename: filename);
				}

				return labels;

				static MultiLabel ParseRecord(CsvReader csvReader, int fieldCount, int recordNumber, string filename) {
					var labels = new bool[fieldCount];

					for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++) {
						var fieldValue = csvReader.GetField(fieldIndex);
						var parsed = int.TryParse(fieldValue, out var parsedValue);

						labels[fieldIndex] = parsedValue switch
						{
							0 => false,
							1 => true,
							_ => throw new InvalidOperationException($"Parsing error. Can't parse {fieldValue} as bool. Line {recordNumber}. File: {filename}.")
						};
					}

					return new MultiLabel(labels);
				}
			}
		}

		private static int GetNumberOfRecords(string filename) {
			using var streamReader = new StreamReader(path: filename);
			using var csvReader = new CsvReader(reader: streamReader, CultureInfo.InvariantCulture);

			var recordCount = 0;

			while (csvReader.Read())
				recordCount += 1;

			return recordCount;
		}

		private static int GetAndValidateNumberOfFields(string filename) {
			using var streamReader = new StreamReader(path: filename);
			using var csvReader = new CsvReader(reader: streamReader, CultureInfo.InvariantCulture);

			csvReader.Read();

			var firstLineFieldCount = csvReader.Context.Record.Length;

			var lineCount = 0;
			while (csvReader.Read()) {
				var currentLineFieldCount = csvReader.Context.Record.Length;

				if (currentLineFieldCount != firstLineFieldCount)
					throw new InvalidOperationException($"All records must the same number of fields. Line {lineCount} contains {currentLineFieldCount}.");

				lineCount += 1;
			}

			return firstLineFieldCount;
		}
	}
}
