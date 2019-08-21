namespace Minotaur.Collections {
	using System;
	using Newtonsoft.Json;

	/// <remarks>This matrix is row major.</remarks>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Matrix<T> {

		[JsonProperty] public readonly int ColumnCount;

		[JsonProperty] public readonly int RowCount;

		[JsonProperty] public readonly Array<T> FlattenedValues;

		// We store the values in two formats,
		// rows of values and flattened values,
		// for performance reasons
		public readonly Array<Array<T>> Rows;

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

			var rows = new Array<T>[rowCount];

			for (int i = 0; i < rows.Length; i++) {
				var newRow = new T[columnCount];

				System.Array.Copy(
					sourceArray: values,
					sourceIndex: i * columnCount,
					destinationArray: newRow,
					destinationIndex: 0,
					length: columnCount);

				rows[i] = Array<T>.Wrap(newRow);
			}

			Rows = Array<Array<T>>.Wrap(rows);

			var clonedValues = new T[values.Length];
			System.Array.Copy(
				sourceArray: values,
				destinationArray: clonedValues,
				length: values.Length);

			FlattenedValues = Array<T>.Wrap(clonedValues);
		}

		public T Get(int rowIndex, int columnIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");
			if (columnIndex < 0 || columnIndex >= ColumnCount)
				throw new ArgumentOutOfRangeException(nameof(columnIndex) + $" must be between [0,{ColumnCount - 1}]");

			return FlattenedValues[(rowIndex * ColumnCount) + columnIndex];
		}

		public Array<T> GetRow(int rowIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");

			return Rows[rowIndex];
		}
	}
}
