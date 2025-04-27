namespace Core.Models
{
    public class AvionicsError : AvionicsBase
    {
        public string Message = string.Empty;

        public override string ToString()
        {
            return Message;
        }
    }
}
