namespace Minotaur.Math.Dimensions {
	using System;

	// @Assumption: intervals can not  be empty.
	public interface IDimensionInterval: IEquatable<IDimensionInterval> {
		int DimensionIndex { get; }
		double Volume { get; }
		bool Contains(float value);
	}
}
