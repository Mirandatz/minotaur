namespace Minotaur.Math.Dimensions {

	public interface IDimensionInterval {
		int DimensionIndex { get; }
		bool Contains(float value);
		bool IsEmpty { get; }
	}
}
