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
    public class IndexModel : PageModel
    {
        private List<Teacher> teacherslist;


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

        public List<Teacher> Teacherslist
        {
            get
            {
                if (teacherslist == null)
                {
                    JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
                    JArray jTeachers = (JArray)jobject["teachers"];     //puts everey teachers object of the json file in a new JasonArray

                    teacherslist = jTeachers.ToObject<List<Teacher>>();     //put the JasonArray in to the teacherslist
                    foreach (Teacher teacher in teacherslist)
                    {
                        teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();    //get the short name for every teacher by splitting the email
                        teacher.Lastname = teacher.Name.Split(' ')[0];
                        teacher.Firstname = teacher.Name.Split(' ')[1];
                    }
                }
                return teacherslist;
            }
        }

        #endregion


        #region Methods

        

        public IActionResult OnPostDelete(string id)
        {

            JObject obj = JObject.Parse(JsonString);    // https://www.newtonsoft.com/json/help/html/ModifyJson.htmhttps://www.newtonsoft.com/json/help/html/ModifyJson.htm
            JArray teachers = (JArray)obj["teachers"];


            foreach (JObject o in teachers)
            {
                if ((string)o["ID"] == id)
                {
                    o.Remove(); //delte the object
                    //Write the new jsonstring in the file
                    using (StreamWriter writer = new StreamWriter(Path_Json))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, obj);
                    }
                    return new RedirectToPageResult("Index", $"User_mit_der_id:_{id}_wurde_gelöscht");

                }
            }
            return new RedirectToPageResult("Index", "Es_wurde_kein_User_gelöscht");



        }

        public IActionResult OnPostEdit(string id)
        {
            return new RedirectToPageResult("Edit", id);    //redirect to edit page
        }

        #endregion
    }
}