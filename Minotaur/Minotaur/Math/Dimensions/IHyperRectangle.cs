namespace Minotaur.Math.Dimensions {

	public interface IHyperRectangle {
		int DimensionCount { get; }
		IDimensionInterval GetDimensionInterval(int dimensionIndex);
		bool IsCompatibleWith(IHyperRectangle other);
	}
}
