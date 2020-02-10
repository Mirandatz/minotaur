namespace Minotaur.Theseus {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public static class VolumeComputer {

		public static double ComputeDatasetVolume(Dataset dataset) {
			var featureCount = dataset.FeatureCount;
			double volume = 1;

			for (int i = 0; i < featureCount; i++) {

				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Continuous:
				var featureValues = dataset.GetSortedUniqueFeatureValues(i);
				var min = featureValues[0];
				var max = featureValues[featureValues.Length - 1];
				volume *= (max - min);
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return volume;
		}

		public static double ComputeRuleVolume(Dataset dataset, Rule rule) {
			var featureCount = dataset.FeatureCount;
			double volume = 1;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Continuous:
				var test = (ContinuousFeatureTest) rule.Antecedent[i];
				var min = test.LowerBound;
				var max = test.UpperBound;
				volume *= (max - min);
				break;

				default:
				throw new InvalidOperationException($"Unknown / unsupported value of {nameof(FeatureType)}.");
				}
			}

			return volume;
		}
	}
}
