namespace Catalog.API.Extensions;

public static class LinqSelectExtensions
{
    
    // 
    public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(
        this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
    {
        foreach (TSource element in enumerable)
        {
            // Start with a null result and no exception
            SelectTryResult<TSource, TResult> returnedValue;
            try
            {
                // Try to execute the selector on the element
                returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
            }
            catch (Exception ex)
            {
                // If it fails, the exception will be caught and stored in the result
                returnedValue = new SelectTryResult<TSource, TResult>(element, default, ex);
            }

            yield return returnedValue;
        }
    }


    // handle the exception and work on it
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(
        this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
    {
        return enumerable.Select(x => x.Exception is null ? x.Result : exceptionHandler(x.Exception));
    }

    
    // handle the exception and work on it
    public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(
        this IEnumerable<SelectTryResult<TSource, TResult>> enumerable,
        Func<TSource, Exception, TResult> exceptionHandler)
    {
        return enumerable.Select(x =>
            x.Exception == null ? x.Result : exceptionHandler(x.Source, x.Exception));
    }

    
    // class for the result
    public class SelectTryResult<TSource, TResult>
    {
        internal SelectTryResult(TSource? source, TResult? result, Exception? exception)
        {
            Source = source;
            Result = result;
            Exception = exception;
        }

        public TSource? Source { get; set; }
        public TResult? Result { get; set; }
        public Exception? Exception { get; set; }
    }
}