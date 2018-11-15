namespace fugu.graphql.samples.Host.Logic
{
    public class Result<TError>
    {
        public Result(TError error)
        {
            Error = error;
        }

        public Result()
        {
        }
      
        public TError Error { get; set; }

        public virtual bool HasError => Error != null;

        public static implicit operator TError(Result<TError> result)
        {
            return result.Error;
        }
    }

    public class Result<TValue, TError> : Result<TError>
    {
        public Result(TValue content)
        {
            Content = content;
        }

        public Result(TError error): base(error)
        {
            Error = error;
        }

        public TValue Content { get; }

        public static implicit operator TValue(Result<TValue, TError> result)
        {
            return result.Content;
        }
    }
}