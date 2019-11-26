namespace Minotaur.Theseus {
	using System;
	using System.Diagnostics;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Minotaur.Profiling;

	public sealed class NonIntersectingRectangleCreator {

		public readonly Dataset Dataset;
		private readonly HyperRectangleIntersector _intersector;

		public NonIntersectingRectangleCreator(HyperRectangleIntersector hyperRectangleIntersector) {
			_intersector = hyperRectangleIntersector;
			Dataset = _intersector.Dataset;
		}

		public HyperRectangle? TryCreateLargestNonIntersectingRectangle(int seedIndex, Array<HyperRectangle> existingHyperRectangles, NaturalRange dimensionExpansionOrder) {
			var sw = Stopwatch.StartNew();
			try {
				if (dimensionExpansionOrder.Length != Dataset.FeatureCount)
					throw new InvalidOperationException();

				if (existingHyperRectangles.IsEmpty) {
					var tempBuilder = HyperRectangleBuilder.InitializeWithLargestRectangle(Dataset);
					var hyperRectangle = tempBuilder.TryBuild();

					if (hyperRectangle is null)
						throw new InvalidOperationException();
					else
						return hyperRectangle;
				}

				var builder = HyperRectangleBuilder.InitializeWithSeed(
					dataset: Dataset,
					seedIndex: seedIndex);

				var seed = Dataset.GetInstanceData(seedIndex);

				for (int i = 0; i < dimensionExpansionOrder.Length; i++) {
					var dimensionIndex = dimensionExpansionOrder[i];

					switch (Dataset.GetFeatureType(dimensionIndex)) {

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

				var largestHyperRectangle = builder.TryBuild();
				return largestHyperRectangle;
			} finally {
				Timers.IncrementCfsbeTicks(sw.ElapsedTicks);
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

				var dimension = (ContinuousInterval) currentRectangle.GetDimensionInterval(dimensionIndex);
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
