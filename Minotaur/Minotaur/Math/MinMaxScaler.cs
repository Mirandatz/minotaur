namespace Minotaur.Math {
	using System;

	public sealed class MinMaxScaler {
		private readonly float _min;
		private readonly float _max;
		private readonly float _scalingFactor;

		public MinMaxScaler(float min, float max) {
			if (float.IsNaN(min))
				throw new ArgumentOutOfRangeException(nameof(min) + " can't be NaN");
			if (float.IsNaN(max))
				throw new ArgumentOutOfRangeException(nameof(max) + " can't be NaN");
			if (max < min)
				throw new ArgumentOutOfRangeException(nameof(max) + " must be >= minimum");

			_min = min;
			_max = max;
			_scalingFactor = max - min;

			// We could compute this factor in
			// the Rescale calls...
			// But its arguably simpler, and possibly more performant,
			// to do this (only once) here 
			if (_min == _max)
				_scalingFactor = 1;
		}

		public static MinMaxScaler Create(ReadOnlySpan<float> values) {
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			var min = float.PositiveInfinity;
			var max = float.NegativeInfinity;

			for (int i = 0; i < values.Length; i++) {
				var current = values[i];
				min = Math.Min(min, current);
				max = Math.Max(max, current);
			}

			return new MinMaxScaler(min: min, max: max);
		}

		public float Rescale(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " cant be NaN");
			if (value < _min)
				throw new ArgumentOutOfRangeException(nameof(value) + $" must be >= {_min}");
			if (value > _max)
				throw new ArgumentOutOfRangeException(nameof(value) + $" must be <= {_max}");

			return (value - _min) / _scalingFactor;
		}

		public void Rescale(Span<float> values) {
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			for (int i = 0; i < values.Length; i++)
				values[i] = Rescale(values[i]);
		}
	}
}
