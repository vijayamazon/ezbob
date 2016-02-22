namespace Ezbob.Utils.Security {
	using System;
	using System.Collections.Generic;

	public class RefNumber {
		public static implicit operator int(RefNumber rn) {
			return rn == null ? 0 : rn.Raw;
		} // operator int

		public static implicit operator string(RefNumber rn) {
			return rn == null ? string.Empty : rn.Encoded;
		} // operator string

		/// <summary>
		/// Creates instance of the object.
		/// </summary>
		/// <param name="raw">Number to encode.</param>
		public RefNumber(int raw) {
			Raw = raw;

			char prefix = (Raw >= 0) ? 'P' : 'N';

			int encrypted = Math.Abs(Raw) ^ MagicNumber;

			int running = encrypted;

			var target = new Stack<char>();
			do {
				int reminder = running % TargetBase;
				running /= TargetBase;

				char c = (char)('A' + reminder);

				char m = c;
				if (!map.TryGetValue(c, out m))
					m = c;

				target.Push(m);
			} while (running > 0);

			Encoded = prefix + string.Join("", target).PadLeft(MaxTargetLength, 'A');
		} // constructor

		/// <summary>
		/// Original number (that was encoded).
		/// </summary>
		public int Raw { get; private set; }

		/// <summary>
		/// Encoded number, always 8 characters long.
		/// </summary>
		public string Encoded { get; private set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("{0}: {1}", Raw, Encoded);
		} // ToString

		/// <summary>
		/// Just a random number.
		/// </summary>
		private const int MagicNumber = 42363758;

		/// <summary>
		/// Number of letters in the alphabet.
		/// </summary>
		private const int TargetBase = 26;

		private const int MaxTargetLength = 7;

		/// <summary>
		/// The aim of this map: to exclude letters that appear in swear words.
		/// </summary>
		private static readonly SortedDictionary<char, char> map = new SortedDictionary<char, char> {
			{ 'A', '9' },
			{ 'E', '8' },
			{ 'I', '7' },
			{ 'O', '6' },
			{ 'U', '5' },
			{ 'S', '4' },
			{ 'Y', '3' },
			{ 'F', '2' },
			{ 'H', '1' },
			{ 'T', '0' },
		};
	} // class RefNumber
} // namespace
