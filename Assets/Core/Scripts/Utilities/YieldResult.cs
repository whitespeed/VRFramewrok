using UnityEngine;

namespace Framework
{
    public class YieldResult : CustomYieldInstruction
    {
        public string error = string.Empty;
        public bool isDone = false;

        public override bool keepWaiting
        {
            get { return !isDone; }
        }


    }

    public class YieldResult<T> : YieldResult
    {
        public T result;
    }

    public class TexureResult : YieldResult<Texture>
    {
    }
}