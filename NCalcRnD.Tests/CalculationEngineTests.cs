using System;
using System.Collections;
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
    public class CalculationEngineTests
    {
        //TODO
        //need to write test to handle writing calculation result to extended data
        
        [Test]
        public void ShouldTakeFormulaAndUseCorrectPropertyValues()
        {

            CalculationContext context = new CalculationContext()
            {
                ChargeElement = new ChargeElement() { Name = "Standing Charge", NetAmount = new decimal(12.71000) },
                Bill = new Bill() { BillStartDate = new DateTime(2012, 2, 10), BillEndDate = new DateTime(2012, 3, 12), TaxPointDate = new DateTime(2012, 03, 12) },
                Contract = new Contract() {}
            };

            var calculation = new Calculation()
                                 {
                                     Formula = "[ChargeElement.NetAmount] / [Bill.BillPeriod]",
                                     Target = "ChargeElement.Quantity"
                                 };

            var calculationEngine = new CalculationEngine();
            calculationEngine.Calculate(context, calculation);
        
            Assert.AreEqual(0.41d, context.ChargeElement.Quantity);
            
        }

        [Test]
        public void Should_Take_Value_From_ExtendedProperties_Using_Name()
        {
            CalculationContext context = new CalculationContext()
            {
                ChargeElement = new ChargeElement() { Name = "Standing Charge", NetAmount = new decimal(12.71000) },
                Bill = new Bill() { BillStartDate = new DateTime(2012, 2, 10), BillEndDate = new DateTime(2012, 3, 12), TaxPointDate = new DateTime(2012, 03, 12),ExtendedData = new List<ExtendedDataElement>(){new ExtendedDataElement(){ElementName = "PowerFactor", ElementValue = "123"}}},
                Contract = new Contract() { }
            };

            var calculation = new Calculation()
            {
                Formula = "[ChargeElement.NetAmount] / [Bill.BillPeriod] * [Bill.ExtendedData.PowerFactor]",
                Target = "ChargeElement.Quantity"
            };

            var calculationEngine = new CalculationEngine();
            calculationEngine.Calculate(context, calculation);

            Assert.AreEqual(50.43d, context.ChargeElement.Quantity);
        }

        
    }

    public class CalculationEngine
    {
        public object Calculate(CalculationContext context, Calculation calculation)
        {
            Expression expression = new Expression(calculation.Formula);

            var regex = new Regex(@"\[(.*?)\]");
            var parameters = regex.Matches(calculation.Formula);


            GetValue(context, expression, parameters);

            var result = expression.Evaluate();

            SetValue(result, calculation, context);

            return result;
        }

        private static void GetValue(CalculationContext context, Expression expression, MatchCollection parameters)
        {
            foreach (var parameter in parameters)
            {
                var name = parameter.ToString().Replace("[", "").Replace("]", "");


                object propValue = context;
                foreach (string propName in name.Split('.'))
                {
                    PropertyInfo propInfo = propValue.GetType().GetProperty(propName);

                    if (propValue.GetType().Equals(typeof(List<ExtendedDataElement>)))
                    {
                        List<ExtendedDataElement> extended = (List<ExtendedDataElement>) propValue;
                        propValue = extended.Where(x => x.ElementName == propName).First().ElementValue;
                        break;
                    }

                    propValue = propInfo.GetValue(propValue, null);
                }

                expression.Parameters[name] = propValue;
            }
        }

        private static void SetValue(object result, Calculation calculation, CalculationContext context)
        {
            object proploc = context;

            foreach (var propName in calculation.Target.Split('.'))
            {
                PropertyInfo propInfo = proploc.GetType().GetProperty(propName);

               
                if (propInfo.Name == calculation.Target.Split('.').Last())
                {
                    propInfo.SetValue(proploc, result, null);
                    break;
                }
                proploc = propInfo.GetValue(proploc, null);
            }
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

    public class Calculation
    {
        public string Formula { get; set; }
        public string Target { get; set; }
    }
}
