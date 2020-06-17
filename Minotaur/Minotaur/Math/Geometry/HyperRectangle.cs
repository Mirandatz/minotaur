namespace Minotaur.Math.Geometry {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class HyperRectangle: IEquatable<HyperRectangle>, IReadOnlyList<Interval> {

		private readonly Interval[] _intervals;
		private readonly int _precomputedHashCode;

		public HyperRectangle(ReadOnlySpan<Interval> intervals) {

			var storage = new Interval[intervals.Length];
			var hash = new HashCode();

			for (int i = 0; i < intervals.Length; i++) {
				var intv = intervals[i];

				if (intv is null)
					throw new ArgumentException(nameof(intervals) + " can't contain nulls.");

				storage[i] = intv;
				hash.Add(intv);
			}

			_intervals = storage;
			_precomputedHashCode = hash.ToHashCode();
		}

		public ReadOnlySpan<Interval> AsSpan() => _intervals;

		// Silly overrides
		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((HyperRectangle) obj!);

		public bool Equals([AllowNull] HyperRectangle other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			var lhs = _intervals.AsSpan();
			var rhs = _intervals.AsSpan();

			if (lhs.Length != rhs.Length) {
				throw new InvalidOperationException($"" +
					$"All {nameof(HyperRectangle)} created in a MINOTAUR run " +
					$"should have the same number of {nameof(Interval)}.");
			}

			for (int i = 0; i < lhs.Length; i++) {
				if (!lhs[i].Equals(rhs[i]))
					return false;
			}

			return true;
		}

		// IReadOnlyList implementation

		public int Count => _intervals.Length;

		public Interval this[int index] => _intervals[index];

		public IEnumerator<Interval> GetEnumerator() => _intervals.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _intervals.GetEnumerator();
	}
}