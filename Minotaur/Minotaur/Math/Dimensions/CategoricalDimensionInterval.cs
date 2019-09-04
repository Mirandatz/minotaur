namespace Minotaur.Math.Dimensions {
	using System;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Float;

	// @Assumption: intervals can not  be empty.
	public sealed class CategoricalDimensionInterval: IDimensionInterval {
		public int DimensionIndex { get; }

		private readonly float[] _values;

		private CategoricalDimensionInterval(int dimensionIndex, float[] values) {
			DimensionIndex = dimensionIndex;
			_values = values;
		}

		public Array<float> SortedValues => Array<float>.Wrap(_values);

		public static CategoricalDimensionInterval FromSortedUniqueValues(
			int dimensionIndex,
			Array<float> sortedUniqueValues
			) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (sortedUniqueValues is null)
				throw new ArgumentNullException(nameof(sortedUniqueValues));
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
				values: copyOfValues);
		}

		public static CategoricalDimensionInterval FromSingleValue(int dimensionIndex, float value) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			return new CategoricalDimensionInterval(
				dimensionIndex: dimensionIndex,
				values: new float[] { value });
		}

		public bool Contains(float value) {
			if (value.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(value) + " must be finite.");

			var index = Array.BinarySearch(_values, value);
			return index >= 0;
		}
	}
}
