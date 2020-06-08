namespace Minotaur.Theseus {
	using Minotaur.Classification.Rules;
	using Minotaur.Math.Dimensions;

	public sealed class FeatureTestDimensionIntervalConverter {

		public IInterval FromFeatureTest(IFeatureTest test) {
			return test switch
			{
				ContinuousFeatureTest cft => FromContinuousFeatureTest(cft),

				_ => throw CommonExceptions.UnknownFeatureTestImplementation
			};
		}

		public ContinuousInterval FromContinuousFeatureTest(ContinuousFeatureTest continuousFeatureTest) {
			return new ContinuousInterval(
				dimensionIndex: continuousFeatureTest.FeatureIndex,
				start: continuousFeatureTest.LowerBound,
				end: continuousFeatureTest.UpperBound);
		}

		public IFeatureTest FromDimensionInterval(IInterval interval) {
			return interval switch
			{
				ContinuousInterval cdi => FromContinousDimensionInterval(cdi),

				_ => throw CommonExceptions.UnknownDimensionIntervalImplementation
			};
		}

		public ContinuousFeatureTest FromContinousDimensionInterval(ContinuousInterval continuousDimensionInterval) {
			return new ContinuousFeatureTest(
				featureIndex: continuousDimensionInterval.DimensionIndex,
				lowerBound: continuousDimensionInterval.Start,
				upperBound: continuousDimensionInterval.End);
		}
	}
}
