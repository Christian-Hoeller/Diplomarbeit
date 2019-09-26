function AddTeacher() {

    var table = document.getElementById("teachers");   //find the table with the id

    var select = document.createElement('select');  //create a dropdownlist
    select.type = "select";
    select.name = "teacher";   //name = teachers to get the value with Request.Form
    select.id = "teacher";

    var teachers = GetAllTeachers();  //get the teachers in a string 

    for (var i = 1; i < teachers.length; i++) {     //loop the string 
        var option = document.createElement("option");  //create option
        option.text = teachers[i];
        option.value = teachers[i];
        if (i == 0) {
            option.selected = true;
        }
        select.add(option);     //add the option to the select
    }

    var tablerow = document.createElement("tr");
    var tabledata = document.createElement("td");

    tabledata.appendChild(select);
    tablerow.appendChild(tabledata)

    table.appendChild(tablerow);

}

function DeleteTeacher(teacher_to_delete) {

    var items = document.getElementById("teacher"); //get all the select elements in the table
    var table = document.getElementById("teachers");   //find the table with the id
   

    var index = 0;

    var found = false;
    for (var i = 0; i < table.rows.length; i++) {
        var row = table.rows[i]
        var teacher_id = row.cells[0].childNodes[0].value;
        if (teacher_id == teacher_to_delete) {
            index = i;
            found = true;
        }
        if (found)
            break;
    }

    

    table.deleteRow(index); //+1 because the thead is counting too
   
}


function GetAllTeachers() {

    var teachers;

    //request querystring
    //querystring mitgeben
    $.ajax({
        type: "GET",
        url: '?handler=Teacher',
        async: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            teachers = response;
        }
    });

    return teachers;
}

function GetClassTeachers() {

    var teachers;

    //request querystring
    //querystring mitgeben
    $.ajax({
        type: "GET",
        url: '?handler=ClassTeachers',
        async: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            teachers = response;
        }
    });

    return teachers;
}