namespace Core.Models
{
    public class AvionicsInfo : AvionicsBase
    {
        public string Message = string.Empty;

        public override string ToString()
        {
            return Message;
        }
    }
}
