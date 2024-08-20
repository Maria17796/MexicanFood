using System.Text.Json.Serialization;

namespace LasMarias.Dataservice
{
    public class StandardResult
    {

        public StandardResult()
        {

        }
        public StandardResult(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }

        public StandardResult(bool success, string message, object options) : this(success, message)
        {
            this.Options = options;
        }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("options")]
        public object Options { get; set; }



    }
}
