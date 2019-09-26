using System.IO;
using System.Text;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Admin
{
    public class EditModel : PageModel
    {
        private string path_json;
        private string jsonstring;
        private string id;
        private Teacher teacher;

        #region Properties

        public Teacher Teacher
        {
            get
            {
                teacher = new Teacher();
                JObject obj = JObject.Parse(JsonString);
                JArray teachers = (JArray)obj["teachers"];

                foreach (JObject o in teachers)
                {
                    if ((string)o["ID"] == ID)
                    {
                        teacher.ID = ID;
                        teacher.Name = (string)o["Name"];
                        teacher.Lastname = teacher.Name.Split(' ')[0];
                        teacher.Firstname = teacher.Name.Split(' ')[1];

                    }

                }

                return teacher;
            }
        }

        public string ID
        {
            get
            {
                string id = Request.Query["handler"];
                return id;
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
            string new_firstname = Request.Form["firstname"];
            string new_lastname = Request.Form["lastname"];
            string new_fullname = $"{new_lastname} {new_firstname}";
            string new_id = Request.Form["id"];



            JObject obj = JObject.Parse(JsonString);    // https://www.newtonsoft.com/json/help/html/ModifyJson.htmhttps://www.newtonsoft.com/json/help/html/ModifyJson.htm
            JArray teachers = (JArray)obj["teachers"];

            //Check if the input is not null or too long
            if (string.IsNullOrWhiteSpace(new_firstname) == true || string.IsNullOrWhiteSpace(new_firstname) || string.IsNullOrWhiteSpace(new_id) == true)
            {
                return new RedirectToPageResult("Index", "Bearbeiten_fehlgeschlagen:_Ungültige_Eingabe");
            }

            //check if the id is already taken
            foreach (JObject o in teachers)
            {
                if ((string)o["ID"] == new_id && (string)o["ID"] != ID) //check if the new ID isnt already in the system and if the id is the same it's ignored
                    return new RedirectToPageResult("Index", "Bearbeiten_fehlgeschlagen:_ID_bereits_vorhanden");
            }

            //manipulate the json string
            bool found = false;
            foreach (JObject o in teachers)
            {
                if ((string)o["ID"] == ID) //get the object with the id
                {
                    o["ID"] = new_id;
                    o["name"] = new_fullname;
                    found = true;
                }
                if (found)
                    break;
            }

            //Write the new jsonstring in the file
            using (StreamWriter writer = new StreamWriter(Path_Json))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return new RedirectToPageResult("Index", "Bearbeiten_erfolgreich!");
        }


        #endregion
    }
}