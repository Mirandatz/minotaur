namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using static Minotaur.Math.Dimensions.HyperRectangleBuilder;

	public sealed class NonIntersectingRectangleCreator {

		public readonly Dataset Dataset;
		private readonly HyperRectangleIntersector _intersector;

		public NonIntersectingRectangleCreator(HyperRectangleIntersector hyperRectangleIntersector) {
			_intersector = hyperRectangleIntersector;
			Dataset = _intersector.Dataset;
		}

		public HyperRectangle CreateLargestNonIntersectingRectangle(int seedIndex, Array<HyperRectangle> existingHyperRectangles, NaturalRange dimensionExpansionOrder) {
			if (dimensionExpansionOrder.Length != Dataset.FeatureCount)
				throw new InvalidOperationException();

			var builder = HyperRectangleBuilder.InitializeWithLargestRectangle(Dataset);
			if (existingHyperRectangles.IsEmpty)
				return builder.Build();

			var seed = Dataset.GetInstanceData(seedIndex);

			for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
				var dimensionIndex = dimensionExpansionOrder[i];

				switch (Dataset.GetFeatureType(dimensionIndex)) {

				case FeatureType.Binary:
				UpdateBinaryDimension(
					builder: builder,
					existingHyperRectangles: existingHyperRectangles,
					dimensionIndex: dimensionIndex);
				break;

				case FeatureType.Continuous:
				UpdateContinuousDimension(
					builder: builder,
					existingHyperRectangles: existingHyperRectangles,
					dimensionIndex: dimensionIndex,
					seed: seed);
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return builder.Build();
		}

		private void UpdateBinaryDimension(HyperRectangleBuilder builder, Array<HyperRectangle> existingHyperRectangles, int dimensionIndex) {
			var status = BinaryDimensionIntervalStatus.ContainsTrueAndFalse;

			for (int i = 0; i < existingHyperRectangles.Length; i++) {
				var currentRectangle = existingHyperRectangles[i];

				var intersects = _intersector.IntersectsInAllButOneDimension(
					builder: builder,
					rect: currentRectangle,
					dimensionToSkip: dimensionIndex);

				if (!intersects)
					continue;

				var dimension = (BinaryDimensionInterval) currentRectangle.GetDimensionInterval(dimensionIndex);

				switch (status) {
				case BinaryDimensionIntervalStatus.ContainsOnlyTrue:
				if (dimension.ContainsTrue)
					throw new InvalidOperationException();
				break;


				case BinaryDimensionIntervalStatus.ContainsOnlyFalse:
				if (dimension.ContainsFalse)
					throw new InvalidOperationException();
				break;

				case BinaryDimensionIntervalStatus.ContainsTrueAndFalse:
				if (dimension.ContainsTrue && dimension.ContainsFalse)
					throw new InvalidOperationException();
				if (dimension.ContainsFalse)
					status = BinaryDimensionIntervalStatus.ContainsOnlyTrue;
				if (dimension.ContainsTrue)
					status = BinaryDimensionIntervalStatus.ContainsOnlyFalse;
				break;

				case BinaryDimensionIntervalStatus.Undefined:
				throw new InvalidOperationException();

				default:
				throw new InvalidOperationException();
				}

				builder.UpdateBinaryDimensionIntervalValue(dimensionIndex: dimensionIndex, status: status);
			}
		}

		private void UpdateContinuousDimension(HyperRectangleBuilder builder, Array<HyperRectangle> existingHyperRectangles, int dimensionIndex, Array<float> seed) {
			var min = float.NegativeInfinity;
			var max = float.PositiveInfinity;

			for (int i = 0; i < existingHyperRectangles.Length; i++) {
				var currentRectangle = existingHyperRectangles[i];

				var intersects = _intersector.IntersectsInAllButOneDimension(
					builder: builder,
					rect: currentRectangle,
					dimensionToSkip: dimensionIndex);

				if (!intersects)
					continue;

				var dimension = (ContinuousDimensionInterval) currentRectangle.GetDimensionInterval(dimensionIndex);
				var otherEnd = dimension.End;
				var otherStart = dimension.Start;

				var seedValue = seed[dimensionIndex];
				if (seedValue >= otherEnd)
					min = Math.Max(min, otherEnd);
				else
					max = Math.Min(max, otherStart);
			}

			builder.UpdateContinuousDimensionIntervalStart(dimensionIndex: dimensionIndex, value: min);
			builder.UpdateContinuousDimensionIntervalEnd(dimensionIndex: dimensionIndex, value: max);
		}
	}
}
