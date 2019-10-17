namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Collections.Dataset;

	public interface IConsequentCreator {
		Dataset Dataset { get; }
		ILabel CreateConsequent(ReadOnlySpan<int> indicesOfDatasetInstances);
	}
}