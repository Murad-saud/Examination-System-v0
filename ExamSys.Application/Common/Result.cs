namespace ExamSys.Application.Common
{
    public class Result
    {
        public bool Succeeded { get; }
        public string Error { get; }
        public bool Failed => !Succeeded;

        protected Result(bool success, string error)
        {
            Succeeded = success;
            Error = error;
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error);
    }

    // Result with data
    public class Result<T> : Result
    {
        public T Data { get; }

        private Result(T data, bool success, string error) : base(success, error)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new Result<T>(data, true, null);
        public static new Result<T> Failure(string error) => new Result<T>(default, false, error);
    }
}
