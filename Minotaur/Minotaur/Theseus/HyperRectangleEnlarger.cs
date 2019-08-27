namespace Minotaur.Theseus {
	using System;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	// @Improve exception messages
	public sealed class HyperRectangleEnlarger {

		public readonly Dataset Dataset;

		public HyperRectangleEnlarger(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public HyperRectangle Enlarge(
			HyperRectangle target,
			Array<HyperRectangle> others,
			NaturalRange dimensionExpansionOrder
			) {
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (others is null)
				throw new ArgumentNullException(nameof(others));
			if (dimensionExpansionOrder is null)
				throw new ArgumentNullException(nameof(dimensionExpansionOrder));

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
			return EnlargeCategoricalDimension(
			target: target,
			others: others,
			dimensionToEnlarge: dimensionIndex);

			case FeatureType.Continuous:
			return EnlargeContinuousDimension(
				target: target,
				others: others,
				dimensionToEnlarge: dimensionIndex);

			default:
			throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}.");
			}
		}

		private IDimensionInterval EnlargeCategoricalDimension(
			MutableHyperRectangle target,
			Array<HyperRectangle> others,
			int dimensionToEnlarge
			) {

			// @Improve performance
			var possibleValues = Dataset
				.GetSortedUniqueFeatureValues(featureIndex: dimensionToEnlarge)
				.ToHashSet();

			for (int i = 0; i < others.Length; i++) {
				var other = others[i];
				var intersects = HyperRectangleIntersector.IntersectsInAllButOneDimension(
					target: target,
					other: other,
					dimensionToSkip: dimensionToEnlarge);

				if (intersects) {
					var otherDimension = (CategoricalDimensionInterval) (other.GetDimensionInterval(dimensionToEnlarge));
					possibleValues.ExceptWith(otherDimension.SortedValues);
				}
			}

			return new CategoricalDimensionInterval(
				dimensionIndex: dimensionToEnlarge,
				values: possibleValues.ToArray());
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
				var intersects = HyperRectangleIntersector.IntersectsInAllButOneDimension(
					target: target,
					other: other,
					dimensionToSkip: dimensionToEnlarge);

				if (intersects) {
					var otherDimension = (ContinuousDimensionInterval) (other.GetDimensionInterval(dimensionToEnlarge));
					min = Math.Max(min, otherDimension.Start.Value);
					max = Math.Min(max, otherDimension.End.Value);
				}
			}

			// @Assumption: all lower bounds are inclusive
			var lowerBound = new DimensionBound(
				value: min,
				isInclusive: true);

			// @Assumption: all upper bounds are exclusive
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
