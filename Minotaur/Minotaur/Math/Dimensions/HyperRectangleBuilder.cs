namespace Minotaur.Math.Dimensions {
	using System;
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

		public HyperRectangle Build() {
			var dimensionCount = Dataset.FeatureCount;
			var intervals = new IDimensionInterval[dimensionCount];
			for (int i = 0; i < intervals.Length; i++)
				intervals[i] = BuildInterval(i);

			return new HyperRectangle(intervals);
		}

		private IDimensionInterval BuildInterval(int dimensionIndex) {
			return (Dataset.GetFeatureType(dimensionIndex)) switch
			{
				FeatureType.Continuous => BuildContinuousInterval(dimensionIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private ContinuousDimensionInterval BuildContinuousInterval(int dimensionIndex) {
			var start = _starts[dimensionIndex];
			if (float.IsNaN(start))
				throw new InvalidOperationException();

			var end = _ends[dimensionIndex];
			if (float.IsNaN(end))
				throw new InvalidOperationException();

			if (start >= end)
				throw new InvalidOperationException();

			return new ContinuousDimensionInterval(
				dimensionIndex: dimensionIndex,
				start: start,
				end: end);
		}
	}
}