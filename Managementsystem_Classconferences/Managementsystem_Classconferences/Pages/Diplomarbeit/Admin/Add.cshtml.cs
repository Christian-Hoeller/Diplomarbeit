using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Admin
{
    public class AddModel : PageModel
    {
        private string path_json;
        private string jsonstring;
        private string message;

        #region Properties

        public string Message
        {
            get
            {
                string message = Request.Query["handler"];
                return message == null ? "" : message.Replace('_', ' ');
            }
            set
            {
                message = value;
            }
        }
      

        private string Path_Json
        {
            get
            {
                if (path_json == null)
                {
                    string root = "wwwroot";
                    string location = "json";
                    string fileName = "conference-info.json";

                    path_json = Path.Combine(        //locate the path where the file is in 
                    Directory.GetCurrentDirectory(),
                    root,
                    location,
                    fileName);
                }
                return path_json;
            }
        }

        private string JsonString
        {
            get
            {
                if (jsonstring == null)
                {
                    //the filetext has to be converted into a string to bind it to the jsonstring variable

                    FileStream fileStream = new FileStream(Path_Json, FileMode.Open);
                    StreamReader reader = new StreamReader(fileStream, Encoding.UTF8);
                    //https://stackoverflow.com/questions/33649756/read-json-file-containing-umlaut-in-c-sharp
                    using (reader)
                    {
                        jsonstring += JsonConvert.DeserializeObject(reader.ReadToEnd());    //Reads every line and appends it to the jsonstring, in order to get the whole
                    }
                }
                return jsonstring;
            }
        }

 
        public IActionResult OnPost()
        {
            string name = $"{Request.Form["lastname"]} {Request.Form["firstname"]}";
            string id = Request.Form["id"];


            //Teacher newteacher = Teacher;
            //newteacher.Name = name;

            JObject obj = JObject.Parse(JsonString);    // https://www.newtonsoft.com/json/help/html/ModifyJson.htmhttps://www.newtonsoft.com/json/help/html/ModifyJson.htm
            JArray teachers = (JArray)obj["teachers"];

            //Check if the input is not null or too long
            if(string.IsNullOrWhiteSpace(name) == true || string.IsNullOrWhiteSpace(id) == true)
            {
                return new RedirectToPageResult("Add", "Ungültige_Eingabe");
            }

            //check if the id is already taken
            foreach (JObject o in teachers)
            {
                if ((string)o["ID"] == id)
                    return new RedirectToPageResult("Add", "User_bereits_vorhanden");
            }

            //generate a new json string
            string parsestring = "{\"ID\":\"" + id + "\",\"Name\":\"" + name+ "\"}";
            teachers.Add(JObject.Parse(parsestring));   //add the string to the teachers JArray


            //Write the new jsonstring in the file
            using (StreamWriter writer = new StreamWriter(Path_Json))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return new RedirectToPageResult("Add", "Erfolgreich_gespeichert");

        }


        #endregion



    }
}