namespace Minotaur.Collections {
	using System;

	public sealed class MutableMatrix<T> {
		public readonly int ColumnCount;
		public readonly int RowCount;
		public readonly T[] FlattenedValues;

		public MutableMatrix(int rowCount, int columnCount, T[] values) {
			if (columnCount < 0)
				throw new ArgumentOutOfRangeException(nameof(columnCount) + " must be equal to or greater than zero.");
			if (rowCount < 0)
				throw new ArgumentOutOfRangeException(nameof(rowCount) + " must be equal to or greater than zero.");
			if (values.Length != columnCount * rowCount)
				throw new ArgumentException(nameof(values) + $"'s Length must be equal to {nameof(columnCount)} * {nameof(rowCount)}.");

			ColumnCount = columnCount;
			RowCount = rowCount;
			FlattenedValues = values;
		}

		public MutableMatrix(int rowCount, int columnCount) {
			if (columnCount < 0)
				throw new ArgumentOutOfRangeException(nameof(columnCount) + " must be equal to or greater than zero.");
			if (rowCount < 0)
				throw new ArgumentOutOfRangeException(nameof(rowCount) + " must be equal to or greater than zero.");

			ColumnCount = columnCount;
			RowCount = rowCount;
			FlattenedValues = new T[columnCount * rowCount];
		}

		public T Get(int rowIndex, int columnIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");
			if (columnIndex < 0 || columnIndex >= ColumnCount)
				throw new ArgumentOutOfRangeException(nameof(columnIndex) + $" must be between [0,{ColumnCount - 1}]");

			return FlattenedValues[(rowIndex * ColumnCount) + columnIndex];
		}

		public Span<T> GetRow(int rowIndex) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");

			return new Span<T>(
				array: FlattenedValues,
				start: rowIndex * ColumnCount,
				length: ColumnCount);
		}

		public void Set(int rowIndex, int columnIndex, T value) {
			if (rowIndex < 0 || rowIndex >= RowCount)
				throw new ArgumentOutOfRangeException(nameof(rowIndex) + $" must be between [0,{RowCount - 1}]");
			if (columnIndex < 0 || columnIndex >= ColumnCount)
				throw new ArgumentOutOfRangeException(nameof(columnIndex) + $" must be between [0,{ColumnCount - 1}]");

			FlattenedValues[(rowIndex * ColumnCount) + columnIndex] = value;
		}

		public MutableMatrix<U> CastValues<U>(Func<T, U> castingFunction) {
			if (castingFunction == null)
				throw new ArgumentNullException(nameof(castingFunction));

			var casted = new MutableMatrix<U>(rowCount: RowCount, columnCount: ColumnCount);
			var castedValues = casted.FlattenedValues;
			for (int i = 0; i < FlattenedValues.Length; i++)
				castedValues[i] = castingFunction(FlattenedValues[i]);

			return casted;
		}

		public MutableMatrix<T> Transpose() {
			var translated = new MutableMatrix<T>(
				rowCount: ColumnCount,
				columnCount: RowCount);

			for (int y = 0; y < RowCount; y++) {
				for (int x = 0; x < ColumnCount; x++) {
					translated.Set(
						rowIndex: x,
						columnIndex: y,
						value: Get(rowIndex: y, columnIndex: x));
				}
			}

			return translated;
		}

		public Matrix<T> ToMatrix() {
			return new Matrix<T>(
				rowCount: RowCount,
				columnCount: ColumnCount,
				values: FlattenedValues);
		}
	}
}
