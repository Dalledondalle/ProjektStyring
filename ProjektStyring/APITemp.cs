using Newtonsoft.Json;
using ProjektStyring.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektStyring
{
    //Static class that was used to replace interfaces to communicate with databases etc. But ended up being a class to hold the info of the different users.
    public static class APITemp
    {
        //The list with the different kind of users and dictonaries holding Departments and Roles with <ID, Value>
        public static List<PersonModel> Students { get; set; } = new List<PersonModel>();
        public static List<PersonModel> Instructors { get; set; }
        public static List<PersonModel> Customers { get; set; } = new List<PersonModel>();

        public static List<PersonModel> NoneCustomers {
            get 
            {
                List<PersonModel> t = new List<PersonModel>();
                foreach (PersonModel p in Users)
                    if (!Customers.Any(x => p.Id == x.Id))
                        t.Add(p);
                return t;
            }  
        }
        public static List<PersonModel> Users { get; set; } = new List<PersonModel>();
        public static Dictionary<string, string> Departments { get; set; }

        public static Dictionary<string, string> Roles { get; set; }

        //All the "Refresh X" methods call the Auth server to refresh the given list
        public static void RefreshStudentList(string bearerToken)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest request = new RestRequest("students", Method.GET);
            request.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(request);
            if (response.Content != null && response.Content.Length > 1)
                Students = JsonConvert.DeserializeObject<List<PersonModel>>(response.Content);
        }

        public static void RefreshInstructorList(string bearerToken)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest request = new RestRequest("Users?Roles=Instructor", Method.GET);
            request.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(request);
            if (response.Content != null && response.Content.Length > 1)
                Instructors = JsonConvert.DeserializeObject<List<PersonModel>>(response.Content);
        }

        public static void RefreshCustomerList(string bearerToken)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest request = new RestRequest("users?roles=stockmanager", Method.GET);
            request.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(request);
            if (response.Content != null && response.Content.Length > 1)
                Customers = JsonConvert.DeserializeObject<List<PersonModel>>(response.Content);
        }

        public static void RefreshUserList(string bearerToken)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest request = new RestRequest("Users", Method.GET);//?Departments=Data
            request.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(request);
            if (response.Content != null && response.Content.Length > 1)
                Users = JsonConvert.DeserializeObject<List<PersonModel>>(response.Content);
        }

        //Method to give a user by ID the StockManagerRole
        public static void AddStockManagerRole(string bearerToken, string userId)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest postRequest = new RestRequest($"Users/{userId}/Roles/3", Method.PUT);
            postRequest.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(postRequest);
            if (response.Content != null && response.Content.Length > 1)
                Debug.WriteLine("Success");
        }

        //Method to remove a StockMangerRole from a user by ID
        public static void RemoveStockManagerRole(string bearerToken, string userId)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest postRequest = new RestRequest($"Users/{userId}/Roles/3", Method.DELETE);
            postRequest.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(postRequest);
            if (response.Content != null && response.Content.Length > 1)
                Debug.WriteLine("Success");
        }

        //Method to refresh or fill the list of departments with departments
        public static void RefreshDepartments(string bearerToken)
        {
            RestClient client = new RestClient("https://skpauth.azurewebsites.net/");
            RestRequest postRequest = new RestRequest($"Departments", Method.GET);
            postRequest.AddHeader("Authorization", $"Bearer {bearerToken}");
            IRestResponse response = client.Execute(postRequest);
            if (response.Content != null && response.Content.Length > 1)
            {
                List<Deparment> t = JsonConvert.DeserializeObject<List<Deparment>>(response.Content);
                Departments = new Dictionary<string, string>();
                foreach (Deparment dep in t)
                {
                    Departments.Add(dep.Id, dep.Name);
                }
            }
        }
    }
    class Deparment
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}