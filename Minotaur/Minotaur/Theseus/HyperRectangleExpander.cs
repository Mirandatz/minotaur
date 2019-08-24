namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	// @Improve exception messages
	public sealed class HyperRectangleExpander {

		public readonly Dataset Dataset;

		public HyperRectangleExpander(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public HyperRectangle Enlarge(
			HyperRectangle target,
			Array<HyperRectangle> others,
			Array<int> dimensionExpansionOrder
			) {
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (others is null)
				throw new ArgumentNullException(nameof(others));
			if (dimensionExpansionOrder is null)
				throw new ArgumentNullException(nameof(dimensionExpansionOrder));
			if (dimensionExpansionOrder.IsEmpty)
				throw new ArgumentException(nameof(dimensionExpansionOrder) + " can't be empty.");

			// @Add buttloads of checks

			var mutable = MutableHyperRectangle.FromHyperRectangle(target);

			for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
				var dimensionIndex = dimensionExpansionOrder[i];

				var enlargedDimension = Enlarge(
					target: mutable,
					others: others,
					dimensionIndex: dimensionIndex);

				mutable.SetDimensionInterval(enlargedDimension);
			}

			return mutable.ToHyperRectangle();
		}

		private IDimensionInterval Enlarge(
			MutableHyperRectangle target,
			Array<HyperRectangle> others,
			int dimensionIndex
			) {

			switch (Dataset.GetFeatureType(dimensionIndex)) {

			case FeatureType.Categorical:
			throw new NotImplementedException();

			case FeatureType.Continuous:
			return EnlargeContinuousDimension(
				target: target,
				others: others,
				dimensionToEnlarge: dimensionIndex);


			default:
			throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}.");
			}
		}

		private IDimensionInterval EnlargeContinuousDimension(
			MutableHyperRectangle target,
			Array<HyperRectangle> others,
			int dimensionToEnlarge
			) {
			// @Assumption that continous dimensions may have values
			// from negative infinity all the way to positive infinity
			var min = float.NegativeInfinity;
			var max = float.PositiveInfinity;

			for (int i = 0; i < others.Length; i++) {
				var other = others[i];
				var intersects = HyperRectangleIntersector.Intersects(
					target: target,
					other: other,
					dimensionToSkip: dimensionToEnlarge);

				if (intersects) {
					var otherDimension = (ContinuousDimensionInterval) (other.GetDimensionInterval(dimensionToEnlarge));
					min = Math.Max(min, otherDimension.Start.Value);
					max = Math.Min(max, otherDimension.End.Value);
				}
			}

			// @Assumption that all lower bounds are inclusive,
			// because the upper bounds are exclusive
			var lowerBound = new DimensionBound(
				value: min,
				isInclusive: true);

			// @Assumption that all upper bounds are not inclusive
			var upperBound = new DimensionBound(
				value: max,
				isInclusive: false);

			return new ContinuousDimensionInterval(
				dimensionIndex: dimensionToEnlarge,
				start: lowerBound,
				end: upperBound);
		}
	}
}
