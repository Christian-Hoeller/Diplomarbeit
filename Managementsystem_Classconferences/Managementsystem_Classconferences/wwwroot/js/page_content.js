"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;


function ConferenceStart() {

    var user = "sample"
    var buttontext;
    var teachers;
    var intersections;
    var classname;
    var conferencestate;

    conferencestate = GetConferenceState();

    if (conferencestate != "completed") {
        WriteClassName();



        teachers = GetTeachers();
        intersections = GetIntersections();

        WriteTeachersInTable(teachers);
        WriteIntersectionsInTable(intersections);

    }

    WriteButtonText();  //Write button text (besprechung starten, stoppen, abgeschlossen)

}

function WriteButtonText() {

    var buttontext;

    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=ButtonText',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            buttontext = response;
        }
    }); 

    var button = document.getElementById("sendButton");
    button.value = buttontext;
}

function WriteClassName() {

    var classname;
    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=ClassName',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            classname = response;
        }
    });

    var text_classname = document.getElementById("classname");
    text_classname.innerHTML = classname;
}

//When Hub method for intersections is called
connection.on("ReceiveIntersections", function (user, message) {

    WriteIntersectionsInTable(message);
   
});

function WriteIntersectionsInTable(intersections) {

    $("#table").empty();    //clear the table content
    var result = intersections.split(";");    //split the message by ';' (ABLD;SOEK;GRUG, usw.)


    var table = document.getElementById("table");   //find the table with the id

    for (var i = 0; i < result.length; i++) {
        var row = table.insertRow(i);   //insert the row
        var cell = row.insertCell(0);   //insert the cell
        cell.innerHTML = result[i];     //write the date for the specific teacher
    }
}

//When Hub method for teachers is called
connection.on("ReceiveTeachers", function (user, message) {

    WriteTeachersInTable(message);
});

function WriteTeachersInTable(teachers) {
    $("#teachers").empty();    //clear the table content
    var result = teachers.split(";");    //split the message by ';' (ABLD;SOEK;GRUG, usw.)


    var table = document.getElementById("teachers");   //find the table with the id

    for (var i = 0; i < result.length; i++) {
        var row = table.insertRow(i);   //insert the row
        var cell = row.insertCell(0);   //insert the cell
        cell.innerHTML = result[i];     //write the date for the specific teacher
    }
}



connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
   
    var user = "sample";
    var conferencestate;
 

    conferencestate = GetConferenceState();

    if (conferencestate != "completed") {

        ConferenceAction();
        var messageintersections = GetIntersections();
        var messageteachers = GetTeachers();


        connection.invoke("Message", user, messageintersections, messageteachers).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    }
    WriteButtonText();  //Write button text (besprechung starten, stoppen, abgeschlossen)
    WriteClassName();

   
});



function GetIntersections() {
    var messageintersections;

    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=Intersections',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            messageintersections = response;
        }
    });
    return messageintersections;
}



function ConferenceAction() {

    var conferencestate;
    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=ConferenceAction',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
    });

    return conferencestate;
}

function GetConferenceState() {

    var conferencestate;
    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=ConferenceState',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            conferencestate = response;
        }
    });

    return conferencestate;
}



function GetTeachers() {

    var messageteachers;
    $.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
        type: "GET",
        url: '?handler=Teachers',
        cache: false,
        async: false,   //async: false, to continue with the other code
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            messageteachers = response;
        }
    });

    return messageteachers;
}



