namespace Statikk_Data.Features.RiotApiClient;

public ref struct RiotUrlBuilder
{
    private Span<char> _span;
    private int _pos;
    private bool _hasQuery;

    public RiotUrlBuilder(Span<char> buffer, string baseUrl)
    {
        _span = buffer;
        _pos = 0;
        _hasQuery = false;
        AppendRaw(baseUrl);
    }

    public void AppendPath(string segment)
    {
        _span[_pos++] = '/';
        segment.AsSpan().CopyTo(_span[_pos..]);
        _pos += segment.Length;
    }

    public void AppendQuery(string key, long value)
    {
        _span[_pos++] = _hasQuery ? '&' : '?';
        _hasQuery = true;
        
        key.AsSpan().CopyTo(_span[_pos..]);
        _pos += key.Length;
        _span[_pos++] = '=';
        
        value.TryFormat(_span[_pos..], out var written);
        _pos += written;
    }

    private void AppendRaw(string value)
    {
        value.AsSpan().CopyTo(_span[_pos..]);
        _pos += value.Length;
    }

    public override string ToString() => new string(_span[.._pos]);
}