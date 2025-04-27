using Core.Enums;
using Core.Services;

namespace Service.Services
{
    public class MessageSourceService : IMessageSourceService
    {
        public string GetMessageSource(MessageSource messageSource)
        {
            return messageSource switch
            {
                MessageSource.GroundStation => "Yİ",
                MessageSource.Avionics => "UKB",
                _ => "Unknown",
            };
        }
    }
}
