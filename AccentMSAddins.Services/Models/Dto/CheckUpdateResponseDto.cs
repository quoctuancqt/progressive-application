namespace AccentMSAddins.Services.Models
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class CheckUpdateResponseDto
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusText { get; set; }
        public IDictionary<string, object> Errors { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }

        public CheckUpdateResponseDto() { }

        public CheckUpdateResponseDto(HttpStatusCode statusCode, string statusText)
        {
            StatusCode = statusCode;
            StatusText = statusText;
        }

        public CheckUpdateResponseDto(HttpStatusCode statusCode, Exception exception)
        {
            StatusCode = statusCode;
            ProcessException(exception);
        }


        private void ProcessException(Exception exception)
        {
            if (exception != null)
            {
                Errors = new Dictionary<string, object>()
                {
                    { "message", exception.Message },
                    { "stackTrace", exception.StackTrace }
                };
            }
        }
    }

    public class CheckUpdateException : Exception
    {
        public CheckUpdateException()
        {
        }

        public CheckUpdateException(string message) : base(message)
        {
        }
    }
}
