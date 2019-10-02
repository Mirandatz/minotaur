namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections.Dataset;

	public sealed class HyperRectangleBuilder {

		private enum BinaryDimensionIntervalStatus {
			Undefined,
			ContainsOnlyTrue,
			ContainsOnlyFalse,
			ContainsTrueAndFalse
		}

		public readonly Dataset Dataset;

		private readonly BinaryDimensionIntervalStatus[] _binaryIntervalStatus;
		private readonly float[] _starts;
		private readonly float[] _ends;

		public HyperRectangleBuilder(Dataset dataset) {
			Dataset = dataset;

			var featureCount = Dataset.FeatureCount;
			_binaryIntervalStatus = new BinaryDimensionIntervalStatus[featureCount];
			_starts = new float[featureCount];
			_ends = new float[featureCount];

			for (int i = 0; i < featureCount; i++) {
				_binaryIntervalStatus[i] = BinaryDimensionIntervalStatus.Undefined;
				_starts[i] = float.NaN;
				_ends[i] = float.NaN;
			}
		}

		public void UpdateIntervalValue(int dimensionIndex, float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value));

			switch (Dataset.GetFeatureType(dimensionIndex)) {

			case FeatureType.Binary:
			UpdateBinaryIntervalValue(dimensionIndex, value);
			break;

			case FeatureType.Continuous:
			UpdateContinousIntervalValue(dimensionIndex, value);
			break;

			default:
			throw CommonExceptions.UnknownFeatureType;
			}
		}

		private void UpdateBinaryIntervalValue(int dimensionIndex, float value) {
			if (value != 0f && value != 1f)
				throw new ArgumentOutOfRangeException(nameof(value));

			switch (_binaryIntervalStatus[dimensionIndex]) {

			case BinaryDimensionIntervalStatus.Undefined:
			if (value == 0f)
				_binaryIntervalStatus[dimensionIndex] = BinaryDimensionIntervalStatus.ContainsOnlyFalse;
			else
				_binaryIntervalStatus[dimensionIndex] = BinaryDimensionIntervalStatus.ContainsOnlyTrue;
			break;

			case BinaryDimensionIntervalStatus.ContainsOnlyTrue:
			if (value == 0f)
				_binaryIntervalStatus[dimensionIndex] = BinaryDimensionIntervalStatus.ContainsTrueAndFalse;
			break;

			case BinaryDimensionIntervalStatus.ContainsOnlyFalse:
			if (value == 1f)
				_binaryIntervalStatus[dimensionIndex] = BinaryDimensionIntervalStatus.ContainsTrueAndFalse;
			break;

			case BinaryDimensionIntervalStatus.ContainsTrueAndFalse:
			break;

			default:
			throw new InvalidOperationException();
			}
		}

		private void UpdateContinousIntervalValue(int dimensionIndex, float value) {
			var start = _starts[dimensionIndex];
			if (float.IsNaN(start) || value < start) {
				_starts[dimensionIndex] = value;
				return;
			}

			var end = _ends[dimensionIndex];
			if (float.IsNaN(end) || value > end)
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
				FeatureType.Binary => BuildBinaryInterval(dimensionIndex),
				FeatureType.Continuous => BuildContinuousInterval(dimensionIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private BinaryDimensionInterval BuildBinaryInterval(int dimensionIndex) {

			return (_binaryIntervalStatus[dimensionIndex]) switch
			{
				BinaryDimensionIntervalStatus.ContainsOnlyTrue => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: false,
					containsTrue: true),

				BinaryDimensionIntervalStatus.ContainsOnlyFalse => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: true,
					containsTrue: false),

				BinaryDimensionIntervalStatus.ContainsTrueAndFalse => new BinaryDimensionInterval(
					dimensionIndex: dimensionIndex,
					containsFalse: true,
					containsTrue: true),

				BinaryDimensionIntervalStatus.Undefined => throw new InvalidOperationException(),
				_ => throw new InvalidOperationException()
			};
		}

		private ContinuousDimensionInterval BuildContinuousInterval(int dimensionIndex) {
			var start = _starts[dimensionIndex];
			if (float.IsNaN(start))
				throw new InvalidOperationException();

			var end = _ends[dimensionIndex];
			if (float.IsNaN(end))
				throw new InvalidOperationException();

			return new ContinuousDimensionInterval(
				dimensionIndex: dimensionIndex,
				start: start,
				end: end);
		}
	}
}