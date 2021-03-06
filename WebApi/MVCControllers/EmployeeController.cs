﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebApi.Models;

namespace WebApi.MVCControllers
{
    [MyAuthorization("Admin")]
    public class EmployeeController : Controller
    {
        IEnumerable<EmployeeViewModel> Employees;
        EmployeeViewModel Employee = new EmployeeViewModel();
      
        public ActionResult EmployeeIndex()
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));
                var responseTask = client.GetAsync("api/Employee");
                responseTask.Wait();



                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<EmployeeViewModel>>();
                    readTask.Wait();
                    Employees = readTask.Result;


                }
                else
                {
                    Employees = Enumerable.Empty<EmployeeViewModel>();
                    ModelState.AddModelError(string.Empty, "No employees in db");
                }
                
                return View(Employees);
            }
        }
        
        public ActionResult CreateEmployee()
        {
            Employee.EmployeeId = WebApi.Controllers.EmployeeController.NextEmployeeIndex;
            return View(Employee);

        }
        [HttpPost]       
        public ActionResult CreateEmployee(EmployeeViewModel employee)
        {
            employee.EmployeeId =  WebApi.Controllers.EmployeeController.NextEmployeeIndex;
            try
            {
                if (ModelState.IsValid)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));
                        var postTask = client.PostAsJsonAsync<EmployeeViewModel>("api/employee", employee);
                        postTask.Wait();

                        var result = postTask.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            return RedirectToAction("EmployeeIndex");
                        }
                    }
                }
            }catch(DataException)
            {


            }

            ModelState.AddModelError(string.Empty, "Cannot create employee");


            return View(employee);
        }
       
       
        
        public ActionResult EditEmployee(int id)
        {
            IEnumerable<EmployeeViewModel> employee = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));

                var responseTask = client.GetAsync("api/employee?id=" + id.ToString());
                responseTask.Wait();

                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<EmployeeViewModel>>();
                    readTask.Wait();

                    employee = readTask.Result;
                }
            }

            return View(employee.SingleOrDefault());
        }

        [HttpPost]
        public ActionResult EditEmployee(EmployeeViewModel employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));


                        var putTask = client.PutAsJsonAsync<EmployeeViewModel>("api/employee", employee);
                        putTask.Wait();

                        var result = putTask.Result;
                        if (result.IsSuccessStatusCode)
                        {

                            return RedirectToAction("EmployeeIndex");
                        }
                    }
                }
            }catch(DataException)
            {

            }
            return View(employee);
        }
        
        public ActionResult DeleteEmployee(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));


                var deleteTask = client.DeleteAsync("api/employee/" + id.ToString());
                deleteTask.Wait();

                var result = deleteTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    return RedirectToAction("EmployeeIndex");
                }
            }
            TempData["error"] = "Cannot delete. Employee used in invoice.";
            return RedirectToAction("EmployeeIndex");
        }
    }
}