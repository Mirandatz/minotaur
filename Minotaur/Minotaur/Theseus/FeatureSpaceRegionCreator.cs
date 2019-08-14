namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections.Dataset;

	public sealed class FeatureSpaceRegionCreator {
		private readonly Dataset _dataset;

		public FeatureSpaceRegionCreator(Dataset dataset) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}
	}
}
