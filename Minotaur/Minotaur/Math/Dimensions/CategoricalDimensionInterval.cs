namespace Minotaur.Math.Dimensions {
	using System;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Float;

	public sealed class CategoricalDimensionInterval: IDimensionInterval {
		public int DimensionIndex { get; }

		private readonly float[] _values;
		public Array<float> SortedValues => Array<float>.Wrap(_values);

		public CategoricalDimensionInterval(int dimensionIndex, float[] values) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			DimensionIndex = dimensionIndex;

			// @Improve performance
			_values = values
				.Distinct()
				.OrderBy(v => v)
				.ToArray();

			if (_values.Any(v => v.IsNanOrInfinity()))
				throw new ArgumentException(nameof(values) + " must contain only finite values.");
		}

		public bool Contains(float value) {
			if (value.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(value) + " must be finite.");

			var index = Array.BinarySearch(_values, value);
			return index != -1;
		}
	}
}
