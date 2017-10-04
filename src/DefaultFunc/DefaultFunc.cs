using IFunc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVal;

namespace DefaultFunc
{
    public class AppendElrha : ICalculator
    {
        private ICalculator firstCalculator;
        private ICalculator secondCalculator;

        public AppendElrha(ICalculator firstCalculator, ICalculator secondCalculator)
        {
            this.firstCalculator = firstCalculator;
            this.secondCalculator = secondCalculator;
        }

        public IValue Calculate(Env env)
        {
            var firstResult = this.firstCalculator.Calculate(env);
            var secondResult = this.secondCalculator.Calculate(env);
            
            var stringResult = firstResult.ToStringValue().GetVal();
            var stringResult2 = secondResult.ToStringValue().GetVal();
            
            var result = stringResult + "_elrha_" + stringResult2;

            return new StringValue(result, null);
        }
    }
}
