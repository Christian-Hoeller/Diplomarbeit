using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Classes;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Admin.Class
{
    public class Classes_EditModel : PageModel
    {

        private General general = new General();
        private List<MyClasses_Selectlist> selectlist = new List<MyClasses_Selectlist>();
        MyClasses myclass = null;
        private string id = null;

        List<SelectListItem> selectlist_FormTeacher = null;
        List<SelectListItem> selectlist_HeadOfDepartment = null;


        #region Properties

        private string ID
        {
            get
            {
                if (id == null)
                {
                    id = Request.Query["handler"];
                }
                return id;

            }
        }


        public MyClasses Class
        {
            get
            {
                if (myclass == null)
                {

                    myclass = new MyClasses();
                    List<SelectListItem> teachers_selectlist = new List<SelectListItem>();
                    List<Teacher> teachers_list = new List<Teacher>();
                    JObject obj = JObject.Parse(general.JsonString);
                    JArray classes = (JArray)obj["classes"];

                    foreach (JObject o in classes)
                    {
                        if ((string)o["Classname"] == ID)
                        {
                            myclass.ClassName = ID;
                            myclass.FormTeacher = (string)o["Formteacher"];
                            myclass.HeadOfDepartment = (string)o["Headofdepartment"];

                            JArray teachers = (JArray)o["Teachers"];
                            myclass.Teachers = teachers.ToObject<List<Teacher>>();
                        }
                    }
                }
                return myclass;
            }
        }

        public List<SelectListItem> Selectlist_FormTeacher
        {
            get
            {

                if (selectlist_FormTeacher == null)
                {
                    selectlist_FormTeacher = new List<SelectListItem>();
                    foreach (Teacher teacher in Class.Teachers)
                    {
                        SelectListItem item;
                        if (teacher.ID == Class.FormTeacher)
                        {
                            item = new SelectListItem(teacher.ID, teacher.ID, true);
                        }
                        else
                            item = new SelectListItem(teacher.ID, teacher.ID, false);

                        selectlist_FormTeacher.Add(item);
                    }
                }
                return selectlist_FormTeacher;

            }
        }

        public List<SelectListItem> Selectlist_HeadOfDepartment
        {
            get
            {

                if (selectlist_HeadOfDepartment == null)
                {
                    selectlist_HeadOfDepartment = new List<SelectListItem>();
                    foreach (Teacher teacher in general.Teacherslist)
                    {
                        SelectListItem item;
                        if (teacher.ID == Class.HeadOfDepartment)
                        {
                            item = new SelectListItem(teacher.ID, teacher.ID, true);
                        }
                        else
                            item = new SelectListItem(teacher.ID, teacher.ID, false);

                        selectlist_HeadOfDepartment.Add(item);
                    }
                }
                return selectlist_HeadOfDepartment;

            }
        }

        public List<MyClasses_Selectlist> List_Selectlist_Teachers
        {
            get
            {
                foreach (Teacher teacher in Class.Teachers)
                {
                    MyClasses_Selectlist selectlist_item = new MyClasses_Selectlist();
                    selectlist_item.teacher = teacher;

                    List<SelectListItem> itemlist = new List<SelectListItem>();

                    foreach (Teacher t in Class.Teachers)
                    {
                        SelectListItem item;
                        if (selectlist_item.teacher.ID == t.ID)
                        {
                            item = new SelectListItem(t.ID, t.ID, true);
                        }
                        else
                            item = new SelectListItem(t.ID, t.ID, false);


                        itemlist.Add(item);
                    }
                    selectlist_item.Selectllist = itemlist;
                    selectlist.Add(selectlist_item);

                }


                return selectlist;
            }
        }

        public IActionResult OnPost()
        {
            string formTeacher = Request.Form["FormTeacher"];
            string headOfDepartment = Request.Form["HeadOfDepartment"];
            string classname = Request.Form["Classname"];

            //write Teachers in new List
            string[] teachers_requested = Request.Form["teacher"];



            JObject obj = JObject.Parse(general.JsonString);    // https://www.newtonsoft.com/json/help/html/ModifyJson.htmhttps://www.newtonsoft.com/json/help/html/ModifyJson.htm
            JArray jClasses = (JArray)obj["classes"];
            JArray jteachers = new JArray();


            //check for duplicates
            if (teachers_requested.Distinct().Count() != teachers_requested.Count())
            {
                return new RedirectToPageResult("Index", "Bearbeiten fehlgeschlagen! Lehrer dürfen nicht doppelt angelegt werden");
            }


            for (int i = 0; i < teachers_requested.Length; i++)
            {
                JObject teacherobject = new JObject();
                teacherobject.Add("id", teachers_requested[i]);
                jteachers.Add(teacherobject);
            }

            //find the specific object
            bool found = false;
            foreach (JObject o in jClasses)
            {
                if ((string)o["Classname"] == ID)
                {
                    o["Formteacher"] = formTeacher;
                    o["Headofdepartment"] = headOfDepartment;
                    o["Teachers"] = jteachers;  //write the teachers in the array: "Teachers"
                }
                if (found)
                    break;
            }

            //Write the new jsonstring in the file
            using (StreamWriter writer = new StreamWriter(general.Path_Json))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            //return new RedirectToPageResult("Index", $"User:_{id}_wurde_hinzugefügt");

            return new RedirectToPageResult("Index", "Bearbeiten erfolgreich");
        }



        #endregion

        #region Methods

        public IActionResult OnGetTeacher()
        {
            List<string> teacherslist_idonly = new List<string>();

            foreach (Teacher t in general.Teacherslist)
            {
                teacherslist_idonly.Add(t.ID);

            }
            return new JsonResult(teacherslist_idonly);
        }

        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("Index", "Bearbeiten_abgebrochen");
        }

        //public IActionResult OnGetClassTeachers()
        //{
        //    string[] teachers_requested = Request.Form["teacher"];

        //    List<string> teacherslist_idonly = new List<string>();
        //    for(int i = 0; i< teachers_requested.Length; i++)
        //    {
        //        teacherslist_idonly.Add(teachers_requested[i]);
        //    }

        //    return new JsonResult(teacherslist_idonly);
        // }

        #endregion
    }
}