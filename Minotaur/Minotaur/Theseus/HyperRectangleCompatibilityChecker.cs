namespace Minotaur.Theseus {
	using Minotaur.Math.Dimensions;

	public static class HyperRectangleCompatibilityChecker {

		public static bool AreCompatible(MutableHyperRectangle lhs, HyperRectangle rhs) {
			if (lhs.DimensionCount != rhs.DimensionCount)
				return false;

			var dimensionCount = lhs.DimensionCount;

			for (int i = 0; i < dimensionCount; i++) {
				var lhsDimension = lhs.GetDimensionInterval(dimensionIndex: i);
				var rhsDimension = rhs.GetDimensionInterval(dimensionIndex: i);

				if (lhsDimension.GetType() != rhsDimension.GetType())
					return false;
			}

			return true;
		}
	}
}
