namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class Consequent: IEquatable<Consequent> {

		public readonly Array<bool> Labels;

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => Label.GetHashCode();

		public override bool Equals(object? obj) => Equals((Consequent) obj!);

		public bool Equals([AllowNull] Consequent other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return Label.Equals(other.Label);
		}
	}
}
