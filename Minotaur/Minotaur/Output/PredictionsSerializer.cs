namespace Minotaur.Output {
	using System.Globalization;
	using System.IO;
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
			SerializeSingleLabelPredictions(csvWriter, (SingleLabel[]) predictions);
			break;

			case ClassificationType.MultiLabel:
			SerializeMultiLabelPredictions(csvWriter, (MultiLabel[]) predictions);
			break;

			default:
			throw CommonExceptions.UnknownClassificationType;
			}
		}

		private static void SerializeSingleLabelPredictions(CsvWriter csvWriter, SingleLabel[] datasetPredictions) {
			foreach (var instancePrediction in datasetPredictions) {
				csvWriter.WriteField(instancePrediction.Value);
				csvWriter.NextRecord();
			}
		}

		private static void SerializeMultiLabelPredictions(CsvWriter csvWriter, MultiLabel[] datasetPredictions) {
			foreach (var instancePrediction in datasetPredictions) {
				foreach (var label in instancePrediction.Values) {
					csvWriter.WriteField(label);
				}

				csvWriter.NextRecord();
			}
		}
	}
}