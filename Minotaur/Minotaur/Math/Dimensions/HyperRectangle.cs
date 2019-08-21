namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class HyperRectangle {
		public readonly Array<IDimensionInterval> Dimensions;

		public HyperRectangle(Array<IDimensionInterval> dimensions) {
			Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
		}
	}
}
