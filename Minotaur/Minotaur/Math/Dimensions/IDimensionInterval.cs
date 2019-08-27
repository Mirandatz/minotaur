namespace Minotaur.Math.Dimensions {

	// @Assumption: intervals can not  be empty.
	public interface IDimensionInterval {
		int DimensionIndex { get; }
		bool Contains(float value);
	}
}
