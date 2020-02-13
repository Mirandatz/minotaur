namespace Minotaur.Output {
	using System;

	public sealed class CsvSeparators {

		public readonly string FieldSeparator;
		public readonly string RecordSeparator;

		public CsvSeparators(string fieldSeparator, string recordSeparator) {
			if (fieldSeparator.Contains(recordSeparator))
				throw new ArgumentException();
			if (recordSeparator.Contains(fieldSeparator))
				throw new ArgumentException();

			if (fieldSeparator != Environment.NewLine && string.IsNullOrEmpty(fieldSeparator))
				throw new ArgumentException();
			if (recordSeparator != Environment.NewLine && string.IsNullOrEmpty(recordSeparator))
				throw new ArgumentException();

			FieldSeparator = fieldSeparator;
			RecordSeparator = recordSeparator;
		}

		public bool IntersectsSeparator(string other) {
			return
				other.Contains(FieldSeparator) ||
				other.Contains(RecordSeparator) ||
				FieldSeparator.Contains(other) ||
				RecordSeparator.Contains(other);
		}
	}
}
