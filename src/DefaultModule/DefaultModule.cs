using IProc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVal;

namespace DefaultModule
{
    [ProcAttr(IsSingleInstance = false, Category = "Default")]
    public class DefaultModule : ProcModule, IDisposable
    {
        public DefaultModule(ProcCtx procCtx) : base(procCtx)
        {
        }

        public void Dispose()
        {
        }

        public override IValue ValidateInitialize(IValue param)
        {
            return new DictionaryValue(new Dictionary<string, object>()
            {
                { "elrha111", null},
                { "elrha222", null}
            }, null);
        }

        public override IValue ValidateExecute(IValue param)
        {
            return new NullValue(null);
        }

        public override void Initialize(IValue param)
        {
        }

        public override void Execute(IValue param)
        {
        }
    }
}
