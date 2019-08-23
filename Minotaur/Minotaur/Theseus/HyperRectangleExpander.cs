namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	// @Improve exception messages
	public sealed class HyperRectangleExpander {

		public readonly Dataset Dataset;

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
			return EnlargeContinuousDimension(others, dimensionIndex);


			default:
			throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}.");
			}
		}

		private static IDimensionInterval EnlargeContinuousDimension(Array<HyperRectangle> others, int dimensionIndex) {
			// @Assumption that continuous feature may have
			// any value from -infinity to +infinity

			throw new NotImplementedException();

			//var min = float.NegativeInfinity;
			//var minInclusive = true;

			//var max = float.PositiveInfinity;
			//var maxInclusive = true;

			//for (int i = 0; i < others.Length; i++) {
			//	var other = others[i];

			//	var intersects = Intersects(
			//		target: mutable,
			//		others: others,
			//		dimensionIndex: dimensionIndex);
			//}

			//var lowerBound = new DimensionBound(
			//	value: min,
			//	isInclusive: minInclusive);

			//var upperBound = new DimensionBound(
			//	value: max,
			//	isInclusive: maxInclusive);

			//return new ContinuousDimensionInterval(
			//	dimensionIndex: dimensionIndex,
			//	start: lowerBound,
			//	end: upperBound);
		}
	}
}
