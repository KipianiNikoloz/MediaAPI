namespace API.Errors
{
    public class ApiException
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }

        public ApiException(int status, string message = null, string details = null)
        {
            StatusCode = status;
            Message = message;
            Details = details;
        }
    }
}