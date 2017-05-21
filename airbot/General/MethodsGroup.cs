using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using airbot.General.Types;

namespace airbot.General
{
    public abstract class MethodsGroup
    {
        public Api Api { get; private set; }

        public abstract string Name { get; }

        public MethodsGroup(Api api)
        {
            Api = api;
        }

        protected virtual string GetFullName(string methodName)
        {
            return Name + "/" + methodName;
        }


        public async Task Send(string method, Dictionary<string, object> parameters = null)
        {
            await Send<object>(method, null, parameters);
        }

        public async Task Send<TArg>(string method, TArg arg, Dictionary<string, object> parameters = null)
        {
            await Api.Send<TArg>(GetFullName(method), arg, parameters);
        }
    }
}
