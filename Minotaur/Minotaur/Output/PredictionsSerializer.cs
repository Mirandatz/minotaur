namespace Minotaur.Output {
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using CsvHelper;
	using Minotaur.Classification;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class PredictionsSerializer {

		private readonly Dataset _dataset;

		public PredictionsSerializer(Dataset dataset) {
			_dataset = dataset;
		}

		public void SerializePredictions(TextWriter textWriter, Individual individual) {
			using var csvWriter = new CsvWriter(writer: textWriter, cultureInfo: CultureInfo.InvariantCulture);

			var predictions = individual.Predict(_dataset);

			switch (_dataset.ClassificationType) {

			case ClassificationType.SingleLabel:
			var singleLabelPredictions = predictions.Cast<SingleLabel>().ToArray();
			SerializeSingleLabelPredictions(csvWriter, singleLabelPredictions);
			break;

			case ClassificationType.MultiLabel:
			var multiLabelPredictions = predictions.Cast<MultiLabel>().ToArray();
			SerializeMultiLabelPredictions(csvWriter, multiLabelPredictions);
			break;

			default:
			throw CommonExceptions.UnknownClassificationType;
			}
		}

		private static void SerializeSingleLabelPredictions(CsvWriter csvWriter, SingleLabel[] datasetPredictions) {
			foreach (var instancePrediction in datasetPredictions) {
				CsvSerializationHelper.Write(csvWriter, instancePrediction);
				csvWriter.NextRecord();
			}
		}

		private static void SerializeMultiLabelPredictions(CsvWriter csvWriter, MultiLabel[] datasetPredictions) {
			foreach (var instancePrediction in datasetPredictions) {
				CsvSerializationHelper.Write(csvWriter, instancePrediction);
				csvWriter.NextRecord();
			}
		}
	}
}