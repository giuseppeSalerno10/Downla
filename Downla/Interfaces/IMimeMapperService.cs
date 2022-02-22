namespace Downla
{
    public interface IMimeMapperService
    {
        string GetExtension(string mimeType);
        string GetMimeType(string extension);
    }
}