namespace EFCore.BulkExtensions;

using System;

static class SpanSplitExtensions
{
    public static TokenSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> delimiters)
        where T : IEquatable<T>
    {
        return new TokenSplitEnumerator<T>(span, delimiters);
    }

    public ref struct TokenSplitEnumerator<T> where T : IEquatable<T>
    {
        private readonly ReadOnlySpan<T> _delimiters;
        private ReadOnlySpan<T> _span;

        public TokenSplitEntry<T> Current { get; private set; }

        public TokenSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> delimiters)
        {
            _span = span;
            _delimiters = delimiters;
            Current = default;
        }

        public TokenSplitEnumerator<T> GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _span;

            if (span.Length == 0)
            {
                return false;
            }

            var index = span.IndexOfAny(_delimiters);
            if (index == -1)
            {
                _span = ReadOnlySpan<T>.Empty;
                Current = new TokenSplitEntry<T>(span, ReadOnlySpan<T>.Empty);
                return true;
            }

            Current = new TokenSplitEntry<T>(span[..index], span.Slice(index, 1));
            _span = span[(index + 1)..];

            return true;
        }
    }

    public readonly ref struct TokenSplitEntry<T>
    {
        public ReadOnlySpan<T> Token { get; }
        public ReadOnlySpan<T> Delimiters { get; }
        
        public TokenSplitEntry(ReadOnlySpan<T> token, ReadOnlySpan<T> delimiters)
        {
            Token = token;
            Delimiters = delimiters;
        }

        public void Deconstruct(out ReadOnlySpan<T> token, out ReadOnlySpan<T> delimiters)
        {
            token = Token;
            delimiters = Delimiters;
        }

        public static implicit operator ReadOnlySpan<T>(TokenSplitEntry<T> entry) => entry.Token;
    }
}
