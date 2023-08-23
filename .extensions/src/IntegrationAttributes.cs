using K4os.Compression.LZ4.Encoders;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Carbon.Integrations;

[AttributeUsage ( AttributeTargets.Method )]
public class Test : Attribute
{
    public bool CancelOnError = false;

    internal MethodInfo Origin;
    internal bool HasSuccess;
    internal bool HasErrored;
    internal Task<object> Task;

    public void Fail ( object message = null, Exception ex = null )
    {
        HasErrored = true;

        if (message != null)
        {
            IntegrationManager.Error ( message, ex );
        }
    }
    public void Succeed ()
    {
        HasSuccess = true;
    }

    [AttributeUsage ( AttributeTargets.Method )]
    public class Assert : Test
    {
        public bool CancelOnInvalid = false;

        public static bool IsTrue ( bool condition, string validMessage = null, string invalidMessage = null )
        {
            if (condition && !string.IsNullOrEmpty ( validMessage ))
            {
                IntegrationManager.Log ( validMessage );
            }
            else if (!condition && !string.IsNullOrEmpty ( invalidMessage ))
            {
                IntegrationManager.Warn ( invalidMessage );
            }

            return condition;
        }
        public static bool IsFalse ( bool condition, string validMessage = null, string invalidMessage = null )
        {
            return IsTrue ( condition, invalidMessage, validMessage );
        }
        public static bool IsNull ( object value, string validMessage = null, string invalidMessage = null )
        {
            var condition = value == null;

            if (condition && !string.IsNullOrEmpty ( validMessage ))
            {
                IntegrationManager.Log ( validMessage );
            }
            else if (!condition && !string.IsNullOrEmpty ( invalidMessage ))
            {
                IntegrationManager.Warn ( invalidMessage );
            }

            return condition;
        }
        public static bool IsNotNull ( object value, string validMessage = null, string invalidMessage = null )
        {
            return IsNull ( value, invalidMessage, validMessage );
        }
    }

    [AttributeUsage ( AttributeTargets.Method )]
    public class WaitUntil : Test
    {
        public float WaitTimeout = 2f;

        internal bool Finalized;
        internal bool TimedOut;

        public void Done ()
        {
            Finalized = true;
        }
        public void Timeout ()
        {
            TimedOut = true;
        }
    }
}