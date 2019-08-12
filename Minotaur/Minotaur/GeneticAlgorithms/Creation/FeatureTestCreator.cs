namespace Minotaur.GeneticAlgorithms.Creation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class FeatureTestCreator {
		private readonly Dataset _dataset;

		public FeatureTestCreator(Dataset dataset) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public IFeatureTest CreateTest(int featureIndex) {
			if (featureIndex < 0)
				throw new ArgumentException(nameof(featureIndex) + " must be >= 0.");
			if (featureIndex >= _dataset.FeatureCount)
				throw new ArgumentException(nameof(featureIndex) + $" must be < Dataset.FeatureCount ({_dataset.FeatureCount}).");

			var featureType = _dataset.GetFeatureType(featureIndex);

			if (featureType == FeatureType.Categorical)
				return CreateCategoricalTest(featureIndex);

			if (featureType == FeatureType.Continuous)
				return CreateContinuousTest(featureIndex);

			throw new InvalidOperationException($"Unknown {nameof(FeatureType)}.");
		}

		private IFeatureTest CreateCategoricalTest(int featureIndex) {
			var possibleValues = _dataset.GetFeatureValues(featureIndex);
			var value = Random.Choice(possibleValues);

			return new CategoricalFeatureTest(
				featureIndex: featureIndex,
				value: value);
		}

		private IFeatureTest CreateContinuousTest(int featureIndex) {
			var possibleValues = _dataset.GetFeatureValues(featureIndex);

			var lowerBound = Random.Choice(possibleValues);
			var upperBound = Random.Choice(possibleValues);

			if (lowerBound > upperBound) {
				var temp = lowerBound;
				lowerBound = upperBound;
				upperBound = temp;
			}

			return new ContinuousFeatureTest(
				featureIndex: featureIndex,
				lowerBound: lowerBound,
				upperBound: upperBound);
		}
	}
}
