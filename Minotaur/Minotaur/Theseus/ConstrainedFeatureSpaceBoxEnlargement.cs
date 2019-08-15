namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections.Dataset;

	public sealed class ConstrainedFeatureSpaceBoxEnlargement {
		private readonly Dataset _dataset;

		public ConstrainedFeatureSpaceBoxEnlargement(Dataset dataset) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}
	}
}
