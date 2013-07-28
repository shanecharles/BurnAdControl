using System;

namespace BurnAdControls.Exceptions
{
    public class MissingRequirementException: Exception
    {
        public MissingRequirementException(string message):base(message){}
    }
}
