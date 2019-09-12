namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public static class VolumeComputer {

		public static double ComputeDatasetVolume(Dataset dataset) {
			if (dataset is null)
				throw new ArgumentNullException(nameof(dataset));

			var featureCount = dataset.FeatureCount;
			double volume = 1;

			for (int i = 0; i < featureCount; i++) {
				var featureValues = dataset.GetSortedUniqueFeatureValues(i);

				switch (dataset.GetFeatureType(i)) {
				case FeatureType.Categorical:
				volume *= featureValues.Length;
				break;

				case FeatureType.CategoricalButTriviallyValued:
				break;

				case FeatureType.Continuous:
				var min = featureValues[0];
				var max = featureValues[featureValues.Length - 1];
				volume *= (max - min);
				break;

				case FeatureType.ContinuousButTriviallyValued:
				break;

				default:
				throw new InvalidOperationException($"Unknown / unsupported value of {nameof(FeatureType)}.");
				}
			}

			return volume;
		}

		public static double ComputeRuleVolume(Dataset dataset, Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var featureCount = dataset.FeatureCount;
			double volume = 1;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {
				case FeatureType.Categorical:
				break;

				case FeatureType.CategoricalButTriviallyValued:
				break;

				case FeatureType.Continuous:
				var test = (ContinuousFeatureTest) rule.Tests[i];
				var min = test.LowerBound;
				var max = test.UpperBound;
				volume *= (max - min);
				break;

				case FeatureType.ContinuousButTriviallyValued:
				break;

				default:
				throw new InvalidOperationException($"Unknown / unsupported value of {nameof(FeatureType)}.");
				}
			}

			return volume;
		}
	}
}
