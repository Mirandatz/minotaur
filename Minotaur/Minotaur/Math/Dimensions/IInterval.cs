namespace Minotaur.Math.Dimensions {
	using System;

	// @Assumption: intervals can not  be empty.
	public interface IInterval: IEquatable<IInterval> {
		int DimensionIndex { get; }
		double Volume { get; }
		bool Contains(float value);
	}
}
