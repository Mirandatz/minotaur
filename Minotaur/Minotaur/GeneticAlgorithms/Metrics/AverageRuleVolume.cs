namespace Minotaur.GeneticAlgorithms.Metrics {
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class AverageRuleVolume: IMetric {

		public readonly Dataset Dataset;

		public AverageRuleVolume(Dataset dataset) {
			Dataset = dataset;
		}

		public string Name => nameof(AverageRuleVolume);

		public float EvaluateAsMaximizationTask(Individual individual) {
			var rules = individual.Rules;
			double sumOfVolumes = 1;

			for (int i = 0; i < rules.Length; i++)
				sumOfVolumes += ComputeRuleVolume(rules[i]);

			return (float) (sumOfVolumes / rules.Length);
		}

		private double ComputeRuleVolume(Rule rule) {
			double volume = 1;

			var tests = rule.Antecedent;
			for (int i = 0; i < tests.Length; i++)
				volume *= ComputeTestVolume(rule, i);

			return volume;
		}

		private double ComputeTestVolume(Rule rule, int testIndex) {
			var test = rule.Antecedent[testIndex];
			var featureType = Dataset.GetFeatureType(featureIndex: testIndex);

			return featureType switch
			{
				FeatureType.Continuous => ComputeContinuousFeatureTestVolume(test, testIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private double ComputeContinuousFeatureTestVolume(IFeatureTest test, int testIndex) {
			if (test is ContinuousFeatureTest cft) {
				var lower = cft.LowerBound;
				var upper = cft.UpperBound;

				var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex: testIndex);

				if (float.IsNegativeInfinity(lower))
					lower = featureValues[0];

				if (float.IsPositiveInfinity(upper))
					upper = featureValues[^1];

				return upper - lower;
			}

			if (test is NullFeatureTest) {
				var (min, max) = Dataset.GetMinimumAndMaximumFeatureValues(featureIndex: testIndex);
				return max - min;
			}

			throw CommonExceptions.UnknownFeatureTestImplementation;
		}
	}
}