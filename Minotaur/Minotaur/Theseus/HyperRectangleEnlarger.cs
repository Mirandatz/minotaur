namespace Minotaur.Theseus {
	using System;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	// @Improve exception messages
	public sealed class HyperRectangleEnlarger {

		public readonly Dataset Dataset;

		public HyperRectangleEnlarger(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public HyperRectangle Enlarge(
			Array<float> seed,
			Array<HyperRectangle> others,
			NaturalRange dimensionExpansionOrder
			) {
			if (seed is null)
				throw new ArgumentNullException(nameof(seed));
			if (others is null)
				throw new ArgumentNullException(nameof(others));
			if (dimensionExpansionOrder is null)
				throw new ArgumentNullException(nameof(dimensionExpansionOrder));

			// @Add buttloads of checks

			throw new NotImplementedException();
		}
	}
}
