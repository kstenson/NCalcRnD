using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CSScriptLibrary;
using MC.Bureau.Core.Entities;

using NCalc;
using NUnit.Framework;

namespace NCalcRnD.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void ShouldTakeFormulaAndUseCorrectPropertyValues()
        {
          
            

            CalculationContext context = new CalculationContext()
            {
                ChargeElement = new ChargeElement() { Name = "Standing Charge", NetAmount = new decimal(12.71000) },
                Bill = new Bill() { BillStartDate = new DateTime(2012, 2, 10), BillEndDate = new DateTime(2012, 3, 12), TaxPointDate = new DateTime(2012, 03, 12) },
                Contract = new Contract() { formula = "[ChargeElement.NetAmount] / [Bill.BillPeriod]" }
            };
                Expression expression = new Expression(context.Contract.formula);
           
                var regex = new Regex(@"\[(.*?)\]");
                var parameters = regex.Matches(context.Contract.formula);

            
            foreach (var parameter in parameters)
            {
                var name = parameter.ToString().Replace("[", "").Replace("]", "");
               

                object propValue = context;
                foreach (string propName in name.Split('.'))
                {
                    PropertyInfo propInfo = propValue.GetType().GetProperty(propName);
                    propValue = propInfo.GetValue(propValue, null);
                }

                expression.Parameters[parameter.ToString()] = propValue;


            }

            var result = expression.Evaluate();





        }
    }

    public class CalculationContext
    {
        public ChargeElement ChargeElement { get; set; }
        public Bill Bill { get; set; }
        public Contract Contract { get; set; }
    }

    public class Contract
    {
        public string formula { get; set; }
    }

}
