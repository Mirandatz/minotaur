namespace Minotaur.Theseus {
	using System;
	using Minotaur.Math.Dimensions;

	public static class HyperRectangleIntersector {

		public static bool IntersectsInAllDimensions(HyperRectangle lhs, HyperRectangle rhs) {
			if (lhs.DimensionCount != rhs.DimensionCount)
				throw new ArgumentException();

			var dimensionCount = lhs.DimensionCount;
			for (int i = 0; i < dimensionCount; i++) {
				var intersects = DimensionIntersects(
					lhs: lhs.GetDimensionInterval(i),
					rhs: rhs.GetDimensionInterval(i));

				if (!intersects)
					return false;
			}

			return true;
		}

		/// <summary>
		/// This one really needs documentation
		/// </summary>
		//		public static bool IntersectsInAllButOneDimension(
		//			MutableHyperRectangle target,
		//			HyperRectangle other,
		//			int dimensionToSkip
		//			) {
		//#if DEBUG
		//			if (!HyperRectangleCompatibilityChecker.AreCompatible(lhs: target, rhs: other)) {
		//				throw new ArgumentException(
		//					$"{nameof(target)} " +
		//					$"must be compatible (i.e. same number and types of dimensions) " +
		//					$"with {nameof(other)}.");
		//			}
		//#endif

		//			var dimensionCount = target.DimensionCount;
		//			for (int i = 0; i < dimensionCount; i++) {
		//				if (i == dimensionToSkip)
		//					continue;

		//				var lhs = target.GetDimensionInterval(i);
		//				var rhs = other.GetDimensionInterval(i);
		//				if (!DimensionIntersects(lhs, rhs))
		//					return false;
		//			}

		//			return true;
		//		}

		private static bool DimensionIntersects(IDimensionInterval lhs, IDimensionInterval rhs) {
			// Creating them scopes just to make just I'm not using
			// wrong variables... They have such similar names, nom saying?
			{
				var lhsCat = lhs as BinaryDimensionInterval;
				var rhsCat = rhs as BinaryDimensionInterval;
				if (!(lhsCat is null) && !(rhsCat is null))
					throw new NotImplementedException();
			}

			{
				var lhsCont = lhs as ContinuousDimensionInterval;
				var rhsCont = rhs as ContinuousDimensionInterval;
				if (!(lhsCont is null) && !(rhsCont is null))
					return IntersectsContinuous(lhsCont, rhsCont);
			}

			throw new InvalidOperationException("This line should never be reached.");
		}

		private static bool IntersectsContinuous(ContinuousDimensionInterval lhsCont, ContinuousDimensionInterval rhsCont) {
			throw new NotImplementedException();
		}
	}
}
