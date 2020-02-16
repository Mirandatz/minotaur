namespace Minotaur.Output {
	using System;
	using CsvHelper;
	using Minotaur.Classification;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public static class CsvSerializationHelper {

		public static void Write(CsvWriter csvWriter, ContinuousFeatureTest continuousFeatureTest) {
			csvWriter.WriteField($"" +
				$"{continuousFeatureTest.LowerBound} " +
				$"<= x[{continuousFeatureTest.FeatureIndex}] < " +
				$"{continuousFeatureTest.UpperBound}");
		}

		public static void Write(CsvWriter csvWriter, SingleLabel singleLabel) {
			csvWriter.WriteField(singleLabel.Value);
		}

		public static void Write(CsvWriter csvWriter, MultiLabel multiLabel) {
			foreach (var label in multiLabel.Values) {

				switch (label) {
				case false:
				csvWriter.WriteField(0);
				break;

				case true:
				csvWriter.WriteField(1);
				break;

				default:
				throw new InvalidOperationException("Jesus Christ!");
				}
			}
		}
	}
}
