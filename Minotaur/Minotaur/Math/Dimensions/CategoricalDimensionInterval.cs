namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Float;
	using Minotaur.ExtensionMethods.SystemArray;

	// @Assumption: intervals can not  be empty.
	public sealed class CategoricalDimensionInterval: IDimensionInterval, IEquatable<CategoricalDimensionInterval> {

		private readonly float[] _values;

		public double Volume { get; }
		public int DimensionIndex { get; }

		private CategoricalDimensionInterval(float[] values, double volume, int dimensionIndex) {
			_values = values;
			Volume = volume;
			DimensionIndex = dimensionIndex;
		}

		public Array<float> SortedValues => Array<float>.Wrap(_values);

		public static CategoricalDimensionInterval FromSortedUniqueValues(int dimensionIndex, Array<float> sortedUniqueValues) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (sortedUniqueValues.Length == 0)
				throw new ArgumentException(nameof(sortedUniqueValues) + " can't be empty.");

			// We still copy the values because we prefer
			// to make sure that the data can not be altered from outside
			var copyOfValues = new float[sortedUniqueValues.Length];

			// Manually check the first value
			var firstValue = sortedUniqueValues[0];
			if (float.IsNaN(firstValue))
				throw new ArgumentException(nameof(sortedUniqueValues) + " can't contain NaNs.");

			// Manually copy the initial value
			copyOfValues[0] = firstValue;

			for (int i = 1; i < sortedUniqueValues.Length; i++) {
				var currentValue = sortedUniqueValues[i];
				var previousValue = sortedUniqueValues[i - 1];

				if (float.IsNaN(currentValue))
					throw new ArgumentException(nameof(sortedUniqueValues) + " can't contain NaNs.");

				if (currentValue == previousValue)
					throw new ArgumentException(nameof(sortedUniqueValues) + " can't contain duplicated elements.");

				if (currentValue < previousValue)
					throw new ArgumentException(nameof(sortedUniqueValues) + " must be sorted.");

				copyOfValues[i] = sortedUniqueValues[i];
			}

			return new CategoricalDimensionInterval(
				dimensionIndex: dimensionIndex,
				values: copyOfValues,
				volume: copyOfValues.Length);
		}

		public static CategoricalDimensionInterval FromSingleValue(int dimensionIndex, float value) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			var values = new float[] { value };

			return new CategoricalDimensionInterval(
				dimensionIndex: dimensionIndex,
				values: values,
				volume: values.Length);
		}

		public bool Contains(float value) {
			if (value.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(value));

			var index = Array.BinarySearch(_values, value);
			return index >= 0;
		}

		// Silly overrides 
		public override string ToString() => $"[{string.Join(", ", _values)}]";

		public override int GetHashCode() => HashCode.Combine(DimensionIndex, Volume);

		public override bool Equals(object? obj) {
			if (obj is CategoricalDimensionInterval other)
				return Equals(other);
			else
				return false;
		}

		// Implementation of IEquatable
		public bool Equals(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is CategoricalDimensionInterval other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(CategoricalDimensionInterval other) {
			if (ReferenceEquals(this, other))
				return true;

			if (DimensionIndex != other.DimensionIndex)
				return false;
			if (Volume != other.Volume)
				return false;

			return _values.FloatSequenceEquals(other._values);
		}
	}
}
