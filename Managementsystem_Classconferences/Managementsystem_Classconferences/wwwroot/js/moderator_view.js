"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;

    document.getElementById("room").innerHTML = GetCurrentRoom();

    FirstStart();
    
}).catch(function (err) {
    return console.error(err.toString());
});

function FirstStart() {

    var currentroom = GetCurrentRoom();
    connection.invoke("LoadModeratorViewInfo", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

}

function GetCurrentRoom() {

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
        if (results == null) {
            return null;
        }
        else {
            return decodeURI(results[1]) || 0;
        }
    }

    return  $.urlParam('handler');  
}

////////////////////////////////////////////////////////////
//Gets called when the page button 'senbutton' is clicked//
//////////////////////////////////////////////////////////
document.getElementById("sendButton").addEventListener("click", function (event) {

    connection.invoke("ConferenceAction", GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

});

////////////////////////////////////////
//Gets called when the page is loaded//
//////////////////////////////////////

connection.on("ReveiveLoadInformation", function (obj) {
    var obj_parsed = JSON.parse(obj);

    document.getElementById("classname").innerHTML = obj_parsed.classname;
    document.getElementById("sendButton").value = obj_parsed.buttontext;

    WriteDataInTable("classes_completed", obj_parsed.classes_completed); 
    WriteDataInTable("classes_notedited", obj_parsed.classes_not_edited);
});

function WriteDataInTable(tablename, jsonArray) {

    $("#" + tablename).empty();
    var parsedArray = JSON.parse(jsonArray);

    var table = document.getElementById(tablename);

    for (var i = 0; i < parsedArray.length; i++) {
        var row = table.insertRow(i);
        var cell = row.insertCell(0);
        cell.innerHTML = parsedArray[i];
    }
}

function WriteTeachersWithButtonsInTable(jsonArray) {

    $("#teachers").empty();
    var parsedArray = JSON.parse(jsonArray);

    var teacherData, buttonData;

    if (parsedArray[0] == "Keine Lehrer") {
        $("#teachers").append("<tr><td>" + parsedArray[0]  + "</td></tr>");
    }
    else {
        for (var i = 0; i < parsedArray.length; i++) {
            teacherData = "<td>" + parsedArray[i] + "</td>";
            buttonData = "<td><button onclick= callTeacher(" + i + ") class='btn btn-info' > ausrufen</button></td>";
            $("#teachers").append("<tr>" + teacherData + buttonData + "</tr>");
        }
    }
}

function callTeacher(indexOfTeacher) {
    connection.invoke("SendTeacherCall", indexOfTeacher, GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
}

function appendColumn() {
    $("#teachers").append("<tr><td>" + "<button class=" + "btn btn-info" + ">ausrufen</button>" + "</td></tr>");
}


//after Hub-Mehtods have been called
connection.on("ReceiveIntersections", function (intersections) {

    WriteDataInTable("intersections", intersections)
});

connection.on("ReceiveTeachers", function (teachers) {

    WriteTeachersWithButtonsInTable(teachers);
});


//https://www.redips.net/javascript/adding-table-rows-and-columns/

