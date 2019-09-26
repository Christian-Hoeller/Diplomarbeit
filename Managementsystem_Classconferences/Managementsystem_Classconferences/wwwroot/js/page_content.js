"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    FirstStart();
}).catch(function (err) {
    return console.error(err.toString());
});


////////////////////////////////////////////////////////////
//Gets called when the page button 'senbutton' is clicked//
//////////////////////////////////////////////////////////
document.getElementById("sendButton").addEventListener("click", function (event) {

    var currentroom = GetCurrentRoom()

    connection.invoke("ConferenceAction", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

    //Call_Hub_Methods();
});

////////////////////////////////////////
//Gets called when the page is loaded//
//////////////////////////////////////
function FirstStart() {

    var currentroom = GetCurrentRoom()
    connection.invoke("LoadInformation", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

}

connection.on("ReveiveLoadInformation", function (currentClassName, buttontext, classes_completed, classes_notedited) {
    WriteClassName(currentClassName);
    WriteButtontext(buttontext);
    WriteDataInTable("classes_completed", classes_completed); 
    WriteDataInTable("classes_notedited", classes_notedited);
});

//after Hub-Mehtods have been called
connection.on("ReceiveIntersections", function (intersections) {

    WriteDataInTable("intersections", intersections)


});

connection.on("ReceiveTeachers", function (teachers) {

    WriteDataInTable("teachers", teachers);

});


function GetCurrentRoom() {
    var url = new URLSearchParams(window.location.search);
    var currentroom = url.get("handler");

    return currentroom;
}

//Write the state in the button and the classname in the classname field
function WriteClassName(classname){

    var text_classname = document.getElementById("classname");  //find the h1 with the id 'classname'
    text_classname.innerHTML = classname;
}

function WriteButtontext(buttontext) {

    var button = document.getElementById("sendButton"); //find the button with id 'sendButton'
    button.value = buttontext;
}


function WriteDataInTable(tablename, data) {

    $("#" + tablename).empty();    //clear the table content
    var result = data.split(";");    //split the message by ';'


    var table = document.getElementById(tablename);   //find the table with the id

    for (var i = 0; i < result.length; i++) {
        var row = table.insertRow(i);   //insert the row
        var cell = row.insertCell(0);   //insert the cell
        cell.innerHTML = result[i];     //write the date for the specific teacher
    }

}

