using Core.Enums;

namespace Core.Services
{
    public interface IMessageSourceService
    {
        string GetMessageSource(MessageSource messageSource);
    }
}
