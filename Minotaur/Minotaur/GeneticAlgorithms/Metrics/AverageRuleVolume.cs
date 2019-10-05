namespace Minotaur.GeneticAlgorithms.Metrics {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class AverageRuleVolume: IMetric {

		public readonly Dataset Dataset;

		public AverageRuleVolume(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public string Name => nameof(AverageRuleVolume);

		public float EvaluateAsMaximizationTask(Individual individual) {
			if (individual is null)
				throw new ArgumentNullException(nameof(individual));

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

		//private double ComputeTestVolume(Rule rule, int testIndex) {

		//	switch (Dataset.GetFeatureType(testIndex)) {

		//	case FeatureType.Categorical:
		//	return 1;

		//	case FeatureType.CategoricalButTriviallyValued:
		//	return 1;

		//	case FeatureType.Continuous: {
		//		var featureValues = Dataset.GetSortedUniqueFeatureValues(
		//			featureIndex: testIndex);

		//		var test = (ContinuousFeatureTest) rule.Tests[testIndex];

		//		var testMin = test.LowerBound;
		//		var datasetMin = featureValues[0];

		//		var testMax = test.UpperBound;
		//		var datasetMax = featureValues[featureValues.Length - 1];

		//		var delta = Math.Min(testMax, datasetMax) - Math.Max(testMin, datasetMin);
		//		if (delta == 0) {
		//			return 1;
		//			//	throw new InvalidOperationException();
		//		}

		//		return delta;
		//	}

		//	case FeatureType.ContinuousButTriviallyValued:
		//	return 1;

		//	default:
		//	throw new InvalidOperationException($"Unknown or unsupported value for {nameof(FeatureType)}.");
		//	}
		//}

		private double ComputeTestVolume(Rule rule, int testIndex) {

			switch (rule.Antecedent[testIndex]) {

			case NullFeatureTest _:
			throw new NotImplementedException();

			case ContinuousFeatureTest cont: {
				var lower = cont.LowerBound;
				var upper = cont.UpperBound;

				var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex: testIndex);

				if (float.IsNegativeInfinity(lower))
					lower = featureValues[0];

				if (float.IsPositiveInfinity(upper))
					upper = featureValues[^1];

				return upper - lower;
			}

			default:
			throw CommonExceptions.UnknownFeatureType;
			}
		}
	}
}