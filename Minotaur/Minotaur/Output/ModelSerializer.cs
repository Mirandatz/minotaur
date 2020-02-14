namespace Minotaur.Output {
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using CsvHelper;
	using Minotaur.Classification;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public static class ModelSerializer {

		public static void Serialize(TextWriter textWriter, Individual individual) {
			using var csvWriter = new CsvWriter(writer: textWriter, cultureInfo: CultureInfo.InvariantCulture);
			WriteHeader(csvWriter, individual);

			foreach (var rule in individual.Rules) {
				foreach (var ft in rule.Antecedent) {
					csvWriter.WriteField(SerializeFeatureTest(ft));
				}

				csvWriter.WriteField(SerializeConsequent(rule.Consequent));
				csvWriter.NextRecord();
			}
		}

		private static string SerializeFeatureTest(IFeatureTest featureTest) {
			return featureTest switch
			{
				ContinuousFeatureTest cft => SerializeContinuousFeatureTest(cft),

				_ => throw CommonExceptions.UnknownFeatureTestImplementation
			};

			static string SerializeContinuousFeatureTest(ContinuousFeatureTest continuousFeatureTest) {
				return $"{continuousFeatureTest.LowerBound} <= x[{continuousFeatureTest.FeatureIndex}] < {continuousFeatureTest.UpperBound}";
			}
		}

		private static string SerializeConsequent(ILabel consequent) {
			return consequent switch
			{
				SingleLabel sl => SerializeSingleLabel(sl),
				MultiLabel ml => SerializeMultiLabel(ml),

				_ => throw CommonExceptions.UnknownClassificationType,
			};

			static string SerializeSingleLabel(SingleLabel singleLabel) {
				return singleLabel.Value.ToString();
			}

			static string SerializeMultiLabel(MultiLabel multiLabel) {
				var serialized = new char[multiLabel.Length];
				for (int i = 0; i < serialized.Length; i++) {
					serialized[i] = multiLabel[i] switch
					{
						false => '0',
						true => '1',
					};
				}

				return new string(serialized);
			}
		}

		private static void WriteHeader(CsvWriter csvWriter, Individual individual) {
			// @Assumption: all rules have the same size and consequent type

			var fieldNames = individual
				.Rules[0]
				.Antecedent
				.Select(ft => $"Feature Test {ft.FeatureIndex}")
				.ToList();

			fieldNames.Add("Consequent");

			foreach (var f in fieldNames)
				csvWriter.WriteField(f);
			csvWriter.NextRecord();
		}
	}
}
