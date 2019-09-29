namespace Minotaur {
	using Minotaur.Math.Dimensions;

	public static class ExceptionMessages {
		public static readonly string UnknownFeatureType = $"Unknown / unsupported value for {nameof(FeatureType)}.";
		public static readonly string UnknownDimensionIntervalType = $"Unknown / unsupported implementation of {nameof(IDimensionInterval)}.";
	}
}
