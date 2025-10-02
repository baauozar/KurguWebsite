namespace KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin
{
    public class ResultViewModel<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ResultViewModel<T> SuccessResult(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message };

        public static ResultViewModel<T> FailureResult(string error)
            => new() { Success = false, Errors = new List<string> { error } };

        public static ResultViewModel<T> FailureResult(List<string> errors)
            => new() { Success = false, Errors = errors };
    }

    public class OperationResultViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
