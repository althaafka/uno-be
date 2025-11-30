namespace Uno.API.Models.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        private ServiceResult(bool success, T? data, string message)
        {
            Success = success;
            Data = data;
            Message = message;
        }

        public static ServiceResult<T> SuccessResult(T data, string message = "Success")
        {
            return new ServiceResult<T>(true, data, message);
        }

        public static ServiceResult<T> FailureResult(string message)
        {
            return new ServiceResult<T>(false, default, message);
        }
    }
}
