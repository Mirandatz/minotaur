namespace Minotaur.Math.Dimensions {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections.Dataset;

	public sealed class HyperRectangleBuilder {

		public readonly Dataset Dataset;
		private readonly float[] _starts;
		private readonly float[] _ends;

		private HyperRectangleBuilder(Dataset dataset) {
			Dataset = dataset;

			var featureCount = Dataset.FeatureCount;
			_starts = new float[featureCount];
			_ends = new float[featureCount];

			for (int i = 0; i < featureCount; i++) {
				_starts[i] = float.NaN;
				_ends[i] = float.NaN;
			}
		}

		public static HyperRectangleBuilder InitializeWithSeed(Dataset dataset, int seedIndex) {
			var builder = new HyperRectangleBuilder(dataset);
			var seed = dataset.GetInstanceData(seedIndex);
			var featureCount = dataset.FeatureCount;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Continuous: {
					builder.UpdateContinuousDimensionIntervalStart(
						dimensionIndex: i,
						value: seed[i]);

					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: i,
						value: seed[i]);

					break;
				}

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return builder;
		}

		public static HyperRectangleBuilder InitializeWithLargestRectangle(Dataset dataset) {
			var builder = new HyperRectangleBuilder(dataset);
			var featureCount = dataset.FeatureCount;

			for (int i = 0; i < featureCount; i++) {
				switch (dataset.GetFeatureType(i)) {

				case FeatureType.Continuous:
				builder._starts[i] = float.NegativeInfinity;
				builder._ends[i] = float.PositiveInfinity;
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}

			return builder;
		}

		public (float Start, float End) GetContinuousDimensionPreview(int dimensionIndex) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new InvalidOperationException();

			return (Start: _starts[dimensionIndex], End: _ends[dimensionIndex]);
		}

		public void UpdateContinuousDimensionIntervalStart(int dimensionIndex, float value) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new InvalidOperationException();
			if (float.IsNaN(value))
				throw new InvalidOperationException();

			if (_ends[dimensionIndex] < value)
				throw new InvalidOperationException();

			_starts[dimensionIndex] = value;
		}

		public void UpdateContinuousDimensionIntervalEnd(int dimensionIndex, float value) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new InvalidOperationException();
			if (float.IsNaN(value))
				throw new InvalidOperationException();

			if (_starts[dimensionIndex] > value)
				throw new InvalidOperationException();

			_ends[dimensionIndex] = value;
		}

		public HyperRectangle? TryBuild() {
			var dimensionCount = Dataset.FeatureCount;
			var intervals = new IInterval[dimensionCount];

			for (int i = 0; i < intervals.Length; i++) {
				if (!TryBuildInterval(i, out var interval)) {
					return null;
				} else {
					intervals[i] = interval;
				}
			}

			return new HyperRectangle(intervals);
		}

		private bool TryBuildInterval(int dimensionIndex, [MaybeNullWhen(false)] out IInterval interval) {
			if (Dataset.GetFeatureType(dimensionIndex) != FeatureType.Continuous)
				throw new NotImplementedException();


			if (TryBuildContinuousInterval(dimensionIndex: dimensionIndex, interval: out var temp)) {
				interval = temp;
				return true;
			} else {
				interval = null!;
				return false;
			}
		}

		private bool TryBuildContinuousInterval(int dimensionIndex, [MaybeNullWhen(false)] out IInterval interval) {
			var start = _starts[dimensionIndex];
			if (float.IsNaN(start))
				throw new InvalidOperationException();

			var end = _ends[dimensionIndex];
			if (float.IsNaN(end))
				throw new InvalidOperationException();

			if (start >= end) {
				interval = null!;
				return false;
			}
			interval = new ContinuousInterval(
				dimensionIndex: dimensionIndex,
				start: start,
				end: end);

			return true;
		}
	}
}