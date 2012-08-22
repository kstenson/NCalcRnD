using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NCalc;

namespace NCalcRnD.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof (IEntity).IsAssignableFrom(p) && !p.IsInterface)
                .Select(type => new EntityDef()
                {
                    TypeName = type.Name,
                    Properties =type.GetProperties().Select(x=> new Properties(){Name = type.Name+"." + x.Name, Value = ""}).ToList()
                }).ToList();
   
                                         
                                 
            return View(types);
        }

        public ActionResult Calculate(string calculation)
        {
            dynamic response = new CalculationResult();
            Expression expression = new Expression(calculation);
            if (expression.HasErrors())
            {
                response.success = false;
                response.errors = expression.Error;
            }
           
            try
            {
               response.result = expression.Evaluate();
               response.success = true;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.errors = ex.Message;
            }
            return Json(response);
        }

        public class CalculationResult
        {
            public object result { get; set; }
            public bool success { get; set; }
            public string errors { get; set; }
        }

        public class Bill : IEntity
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal NetAmount { get; set; }
            public int Quantity { get; set; }
        }

        public class Contract :IEntity
        {
            public int MIC { get; set; }
        }
    }

    public interface IEntity
    {
    }

    public class Properties
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class EntityDef
    {
        public string TypeName { get; set; }
        public List<Properties> Properties { get; set; }

        public EntityDef()
        {
            Properties = new List<Properties>();
        }
    }
}
