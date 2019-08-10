namespace Minotaur.Collections {
	using System;
	using System.Linq;
	using Newtonsoft.Json;

	/// <remarks>This matrix is row major.</remarks>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Matrix<T> {

		[JsonProperty] public readonly int ColumnCount;

		[JsonProperty] public readonly int RowCount;

		[JsonProperty] private readonly T[] _values;

		[JsonConstructor]
		public Matrix(int rowCount, int columnCount, T[] values) {
			if (columnCount < 0)
				throw new ArgumentOutOfRangeException(nameof(columnCount) + " must be equal to or greater than zero.");
			if (rowCount < 0)
				throw new ArgumentOutOfRangeException(nameof(rowCount) + " must be equal to or greater than zero.");
			if (values.Length != columnCount * rowCount)
				throw new ArgumentException(nameof(values) + $"'s Length must be equal to {nameof(columnCount)} * {nameof(rowCount)}.");

			ColumnCount = columnCount;
			RowCount = rowCount;
			_values = values.ToArray();
		}

		public ReadOnlySpan<T> FlattenedValues => new ReadOnlySpan<T>(_values);

		public T Get(int rowIndex, int columnIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");
			if (columnIndex < 0 || columnIndex >= ColumnCount)
				throw new ArgumentOutOfRangeException(nameof(columnIndex) + $" must be between [0,{ColumnCount - 1}]");

			return _values[(rowIndex * ColumnCount) + columnIndex];
		}

		public ReadOnlySpan<T> GetRow(int rowIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");

			return new ReadOnlySpan<T>(
				array: _values,
				start: rowIndex * ColumnCount,
				length: ColumnCount);
		}
	}
}
