namespace Minotaur {
	using System;

	public static class MultiLabelCreator {

		public static MultiLabel FromSpanOfBinaryValues(ReadOnlySpan<float> values) {
			var labels = new bool[values.Length];

			for (int i = 0; i < values.Length; i++) {
				labels[i] = (values[i]) switch
				{
					0f => false,
					1f => true,
					_ => throw new InvalidOperationException(nameof(values) + " contains non binary values."),
				};
			}

			return new MultiLabel(labels);
		}
	}
}
