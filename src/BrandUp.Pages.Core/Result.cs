using System;
using System.Collections.Generic;

namespace BrandUp.Pages
{
    public class Result : IResult
    {
        private readonly List<string> errors = new List<string>();

        public bool Succeeded { get; protected set; }
        public IEnumerable<string> Errors => errors;

        protected Result() { }

        public static Result Failed(params string[] message)
        {
            var result = new Result { Succeeded = false };

            if (message != null)
                result.errors.AddRange(message);

            return result;
        }
        public static Result Failed(Exception ex)
        {
            return Failed(ex.Message);
        }
        public static Result Success { get; } = new Result { Succeeded = true };
    }

    public class Result<TData> : IResult
    {
        private readonly List<string> errors = new List<string>();

        public bool Succeeded { get; protected set; }
        public IEnumerable<string> Errors => errors;
        public TData Data { get; private set; }

        protected Result() { }

        public static Result<TData> Failed(params string[] message)
        {
            var result = new Result<TData> { Succeeded = false };

            if (message != null)
                result.errors.AddRange(message);

            return result;
        }
        public static Result<TData> Failed(Exception ex)
        {
            return Failed(ex.Message);
        }
        public static Result<TData> Success(TData data)
        {
            return new Result<TData> { Succeeded = true, Data = data };
        }
    }

    public interface IResult
    {
        bool Succeeded { get; }
        IEnumerable<string> Errors { get; }
    }
}