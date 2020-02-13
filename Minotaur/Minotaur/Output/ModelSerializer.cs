namespace Minotaur.Output {
	using System.Linq;
	using Minotaur.Classification;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class ModelSerializer {

		private readonly CsvSeparators _separators;

		public ModelSerializer(CsvSeparators separators) {
			_separators = separators;
		}

		public string Serialize(Individual individual) {
			var builder = GetCsvBuilder(individual);

			foreach (var rule in individual.Rules) {
				foreach (var ft in rule.Antecedent) {
					builder.AddField(SerializeFeatureTest(ft));
				}

				builder.AddField(SerializeConsequent(rule.Consequent));
				builder.FinishRecord();
			}

			return builder.ToString();
		}

		private string SerializeFeatureTest(IFeatureTest featureTest) {
			return featureTest switch
			{
				ContinuousFeatureTest cft => SerializeContinuousFeatureTest(cft),

				_ => throw CommonExceptions.UnknownFeatureTestImplementation
			};

			static string SerializeContinuousFeatureTest(ContinuousFeatureTest continuousFeatureTest) {
				return $"{continuousFeatureTest.LowerBound} <= x[{continuousFeatureTest.FeatureIndex}] < {continuousFeatureTest.UpperBound}";
			}
		}

		private string SerializeConsequent(ILabel consequent) {
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

		private CsvBuilder GetCsvBuilder(Individual individual) {
			// @Assumption: all rules have the same size and consequent type

			var fieldNames = individual
				.Rules[0]
				.Antecedent
				.Select(ft => $"FeatureTest-{ft.FeatureIndex}")
				.ToList();

			fieldNames.Add("Consequent");

			return new CsvBuilder(
				separators: _separators,
				fieldNames: fieldNames.ToArray());
		}
	}
}
