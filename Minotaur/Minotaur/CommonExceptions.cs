namespace Minotaur {
	using System;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public static class CommonExceptions {

		public static InvalidOperationException UnknownFeatureType => new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}.");
		public static InvalidOperationException UnknownDimensionIntervalImplementation => new InvalidOperationException($"Unknown / unsupported implementation of {nameof(IInterval)}.");
		public static InvalidOperationException UnknownFeatureTestImplementation => new InvalidOperationException($"Unknown / unsupported implementation of {nameof(IFeatureTest)}.");
	}
}