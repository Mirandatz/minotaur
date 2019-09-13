namespace Minotaur.Theseus.IndividualCreation {
	using System;
	using Minotaur.Collections.Dataset;

	public static class IndividualCreatorSelector {

		public static IIndividualCreator Select(string creatorName, Dataset trainDataset) {
			if (creatorName is null)
				throw new ArgumentNullException(nameof(creatorName));
			if (trainDataset is null)
				throw new ArgumentNullException(nameof(trainDataset));

			switch (creatorName) {

			default:
			throw new InvalidOperationException($"Unknown / unsupported individual creator mechanism name: {creatorName}.");
			}

			throw new NotImplementedException();
		}
	}
}
