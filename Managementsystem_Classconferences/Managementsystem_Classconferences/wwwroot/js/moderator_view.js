import * as General from './functions.js';

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

//gets called when the connection is established
connection.start().then(function () {
    var currentroom = GetCurrentRoom();

    General.WriteInElement("room", currentroom);
    document.getElementById("sendButton").disabled = false;

    connection.invoke("LoadModeratorPage", currentroom).catch(function (err) {
        return console.error(err.toString());
    });

}).catch(function (err) {
    return console.error(err.toString());
});

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

    return $.urlParam('handler');
}

document.getElementById("sendButton").addEventListener("click", function (event) {

    connection.invoke("ConferenceAction", GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

});

connection.on("ReceiveModeratorContent", function (obj) {

    var obj_parsed = JSON.parse(obj);

    document.getElementById("sendButton").value = obj_parsed.buttontext;

    General.WriteDataInTable("intersections", obj_parsed.intersections)
    WriteTeachersWithButtonsInTable(obj_parsed.teachers);


});

connection.on("ReceiveGeneralContent", function (obj) {

    var obj_parsed = JSON.parse(obj);

    General.WriteInElement("classname", obj_parsed.classname);

    General.WriteInElement("formTeacher", obj_parsed.formTeacher);
    General.WriteInElement("headOfDepartment", obj_parsed.headOfDepartment);
    General.WriteInElement("time", obj_parsed.time);


    General.WriteDataInTable("classesCompleted", obj_parsed.classesCompleted);
    General.WriteDataInTable("classesNotEdited", obj_parsed.classesNotEdited);
});



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

function appendColumn() {
    $("#teachers").append("<tr><td>" + "<button class=" + "btn btn-info" + ">ausrufen</button>" + "</td></tr>");
}

function callTeacher(indexOfTeacher) {
    connection.invoke("SendTeacherCall", indexOfTeacher, GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
}






