using Microsoft.AspNetCore.Components.Forms;

public class BrowserFileFormFile : IFormFile
{
    private readonly IBrowserFile _browserFile;
    private readonly MemoryStream _stream;

    public BrowserFileFormFile(IBrowserFile browserFile, MemoryStream stream)
    {
        _browserFile = browserFile;
        _stream = stream;
    }

    public string ContentType => _browserFile.ContentType;
    public string ContentDisposition => $"form-data; name=\"Image\"; filename=\"{_browserFile.Name}\"";
    public IHeaderDictionary Headers => new HeaderDictionary
    {
        { "Content-Type", ContentType },
        { "Content-Disposition", ContentDisposition }
    };
    public long Length => _stream.Length;
    public string Name => "Image";
    public string FileName => _browserFile.Name;

    public void CopyTo(Stream target)
    {
        _stream.Position = 0;
        _stream.CopyTo(target);
    }

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        _stream.Position = 0;
        await _stream.CopyToAsync(target, cancellationToken);
    }

    public Stream OpenReadStream()
    {
        _stream.Position = 0;
        return _stream;
    }
}