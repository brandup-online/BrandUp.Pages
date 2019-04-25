using System;
using System.Collections.Generic;

namespace BrandUp.Pages
{
    public class Result
    {
        private readonly List<string> errors = new List<string>();

        public bool Succeeded { get; protected set; }
        public IEnumerable<string> Errors => errors;

        private Result() { }

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
}