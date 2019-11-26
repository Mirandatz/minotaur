namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections;

	public sealed class CsvBuilder {

		private readonly string _fieldsSeparator;
		private readonly string _recordSeparator;
		private readonly Array<string> _fieldNames;
		private readonly int _fieldsCount;

		private readonly StringBuilder _recordBuilder;
		private readonly List<string> _csvBuilder;

		private int _fieldsWritten = 0;

		public CsvBuilder(string fieldsSeparator, string recordSeparator, string[] fieldNames) {
			if (fieldsSeparator != Environment.NewLine && string.IsNullOrEmpty(fieldsSeparator))
				throw new ArgumentOutOfRangeException(nameof(fieldsSeparator));
			if (recordSeparator != Environment.NewLine && string.IsNullOrWhiteSpace(recordSeparator))
				throw new ArgumentOutOfRangeException(nameof(recordSeparator));
			if (fieldsSeparator.Contains(recordSeparator) || recordSeparator.Contains(fieldsSeparator))
				throw new ArgumentException();
			if (fieldNames.Length == 0)
				throw new ArgumentException();
			if (fieldNames.Any(fn => fn.Contains(fieldsSeparator)))
				throw new ArgumentException();
			if (fieldNames.Any(fn => fn.Contains(recordSeparator)))
				throw new ArgumentException();

			_recordBuilder = new StringBuilder();
			_csvBuilder = new List<string>();

			_fieldsSeparator = fieldsSeparator;
			_recordSeparator = recordSeparator;
			_fieldNames = fieldNames.ToArray();
			_fieldsCount = _fieldNames.Length;

			for (int i = 0; i < _fieldNames.Length; i++)
				AddField(_fieldNames[i]);

			FinishRecord();
		}

		public void AddField(string value) {
			if (_fieldsWritten >= _fieldsCount)
				throw new InvalidOperationException();
			if (value.Contains(_fieldsSeparator))
				throw new InvalidOperationException();
			if (value.Contains(_recordSeparator))
				throw new InvalidOperationException();

			if (_fieldsWritten > 0)
				_recordBuilder.Append(_fieldsSeparator);

			_recordBuilder.Append(value);
			_fieldsWritten += 1;
		}

		public void FinishRecord() {
			if (_fieldsWritten != _fieldsCount)
				throw new InvalidOperationException();

			_recordBuilder.Append(_recordSeparator);
			_csvBuilder.Add(_recordBuilder.ToString());

			_recordBuilder.Clear();
			_fieldsWritten = 0;
		}

		public override string ToString() => throw new NotImplementedException($"You probably want to call {nameof(CsvBuilder.CopyTo)}.");

		public void CopyTo(FileStream file) {
			if (_fieldsWritten != 0)
				throw new InvalidOperationException();

			using var textWritter = new StreamWriter(file);
			foreach (var line in _csvBuilder)
				textWritter.Write(line);
		}
	}
}
