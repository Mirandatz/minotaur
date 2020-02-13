namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections;

	/// <remarks>
	/// This class does not support escape characters.
	/// </remarks>
	public sealed class CsvBuilder {

		private readonly CsvSeparators _separators;
		private readonly Array<string> _fieldNames;
		private readonly int _fieldsCount;

		private readonly StringBuilder _recordBuilder;
		private readonly List<string> _records;

		private int _fieldsWritten = 0;

		public CsvBuilder(CsvSeparators separators, string[] fieldNames) {
			if (fieldNames.Length == 0)
				throw new ArgumentException();
			if (fieldNames.Any(fn => separators.IntersectsSeparator(fn)))
				throw new ArgumentException();
			if (fieldNames.Any(fn => separators.IntersectsSeparator(fn)))
				throw new ArgumentException();

			_separators = separators;
			_recordBuilder = new StringBuilder();
			_records = new List<string>();
			_fieldNames = fieldNames.ToArray();
			_fieldsCount = _fieldNames.Length;

			for (int i = 0; i < _fieldNames.Length; i++)
				AddField(_fieldNames[i]);

			FinishRecord();
		}

		public void AddField(string value) {
			if (_fieldsWritten >= _fieldsCount)
				throw new InvalidOperationException();
			if (_separators.IntersectsSeparator(value))
				throw new ArgumentException();

			if (_fieldsWritten > 0)
				_recordBuilder.Append(_separators.FieldSeparator);

			_recordBuilder.Append(value);
			_fieldsWritten += 1;
		}

		public void FinishRecord() {
			if (_fieldsWritten != _fieldsCount)
				throw new InvalidOperationException();

			_recordBuilder.Append(_separators.RecordSeparator);
			_records.Add(_recordBuilder.ToString());

			_recordBuilder.Clear();
			_fieldsWritten = 0;
		}

		public override string ToString() => throw new NotImplementedException($"You probably want to call {nameof(CsvBuilder.CopyTo)}.");

		public void CopyTo(FileStream file) {
			if (_fieldsWritten != 0)
				throw new InvalidOperationException();

			using var textWritter = new StreamWriter(file);
			foreach (var line in _records)
				textWritter.Write(line);
		}
	}
}
