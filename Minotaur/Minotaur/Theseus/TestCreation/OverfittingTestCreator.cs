namespace Minotaur.Theseus.TestCreation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class OverfittingTestCreator: ITestCreator {

		public Dataset Dataset { get; }

		public OverfittingTestCreator(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		// @Remarks: any IFeatureTests created by this method
		// will use feature values _from the dataset_, instead of random values.
		// That means that values that appear more often in the dataset
		// have a higher chance of being used.
		public IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval) {
			throw new NotImplementedException();
			//if (dimensionInterval is null)
			//	throw new ArgumentNullException(nameof(dimensionInterval));
			//if (!Dataset.ContainsDimensionInterval(dimensionInterval))
			//	throw new ArgumentOutOfRangeException(nameof(dimensionInterval));

			//switch (dimensionInterval) {

			//case ContinuousDimensionInterval cont:
			//return FromContinuousNasty(cont);

			//default:
			//throw CommonExceptions.UnknownDimensionIntervalImplementation;
			//}
		}

		private ContinuousFeatureTest FromContinuousNasty(ContinuousDimensionInterval cont) {
			var dimensionIndex = cont.DimensionIndex;
			var startValue = cont.Start.Value;
			var endValue = cont.End.Value;

			return ContinuousFeatureTest.FromUnsortedBounds(
				featureIndex: dimensionIndex,
				firstBound: startValue,
				secondBound: endValue);
		}
	}
}
