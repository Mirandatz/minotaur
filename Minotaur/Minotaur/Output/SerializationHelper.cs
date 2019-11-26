namespace Minotaur.Output {
	using System.Linq;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class SerializationHelper {

		public static string Serialize(Array<Rule> rules) {
			var serializedRules = rules.Select(r => Serialize(r));

			return string.Join(
				separator: " XOR ",
				values: serializedRules);
		}

		public static string Serialize(Rule rule) {
			var antecendent = rule
				.Antecedent
				.Select(test => Serialize(test));

			var combinedAntecedent = string.Join(
				separator: " AND ",
				values: antecendent);

			var consequent = Serialize(rule.Consequent);

			return $"IF {combinedAntecedent} THEN {consequent}";
		}

		public static string Serialize(IFeatureTest test) {
			return test switch
			{
				NullFeatureTest nft => Serialize(nft),
				ContinuousFeatureTest cft => Serialize(cft),
				_ => throw CommonExceptions.UnknownFeatureTestImplementation,
			};
		}

		public static string Serialize(NullFeatureTest nullFeatureTest) {
			return $"{float.NegativeInfinity} <= x[{nullFeatureTest.FeatureIndex}] < {float.PositiveInfinity}";
		}

		public static string Serialize(ContinuousFeatureTest continuousFeatureTest) {
			return $"{continuousFeatureTest.LowerBound} <= x[{continuousFeatureTest.FeatureIndex}] < {continuousFeatureTest.UpperBound}";
		}

		public static string Serialize(ILabel label) {
			return label switch
			{
				SingleLabel sl => Serialize(sl),
				MultiLabel ml => Serialize(ml),

				_ => throw CommonExceptions.UnknownClassificationType,
			};
		}

		public static string Serialize(SingleLabel singleLabel) {
			return singleLabel.Value.ToString();
		}

		public static string Serialize(MultiLabel multiLabel) {
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
}

