namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class TestCreator {

		public Dataset Dataset;

		public IFeatureTest TryCreate(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is null)
				throw new ArgumentNullException(nameof(dimensionInterval));
			if (!Dataset.IsDimesionIntervalValid(dimensionInterval))
				throw new ArgumentOutOfRangeException(nameof(dimensionInterval));

			switch (dimensionInterval) {

			case CategoricalDimensionInterval cat:
			return FromCategorical(cat);

			case ContinuousDimensionInterval cont:
			return FromContinuous(cont);

			default:
			throw new InvalidOperationException(
				"Unknown / unsupported implementation of " +
				$"{nameof(IDimensionInterval)} .");
			}
		}

		private CategoricalFeatureTest FromCategorical(CategoricalDimensionInterval cat) {
			throw new NotImplementedException();
		}

		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			throw new NotImplementedException();
		}
	}
}