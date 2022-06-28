namespace Downla.Services.Interfaces
{
    public interface IMimeMapperService
    {
        string GetExtension(string mimeType);
        string GetMimeType(string extension);
    }
}