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
    connection.invoke("LoadInformation", currentroom).catch(function (err) {
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

    WriteTeachersInTable("classes_completed", obj_parsed.classes_completed); 
    WriteTeachersInTable("classes_notedited", obj_parsed.classes_not_edited);
});

function WriteDataInTable(tablename, parsedArray) {

    $("#" + tablename).empty();

    var table = document.getElementById(tablename);

    for (var i = 0; i < parsedArray.length; i++) {
        var row = table.insertRow(i);
        var cell = row.insertCell(0);
        cell.innerHTML = parsedArray[i];
    }
}

//after Hub-Mehtods have been called
connection.on("ReceiveIntersections", function (intersections) {

    WriteTeachersInTable("intersections", intersections)
});

connection.on("ReceiveTeachers", function (teachers) {

    WriteTeachersInTable("teachers", teachers);
});

//connection.on("ReceiveTeachers", function (teachers) {

//    WriteDataInTable("teachers", teachers);
//});






//https://www.redips.net/javascript/adding-table-rows-and-columns/

function createCell(cell, buttonText, style, cellid) {
    //create button
    var btn = document.createElement("BUTTON");   // Create a <button> element
    btn.onclick = function () {
        alert('teacher' + cellid); return false;
    };
    btn.innerHTML = buttonText;                   // Insert text

    var div = document.createElement('div'); // create DIV element
    div.appendChild(btn);                    // append text node to the DIV
    div.setAttribute('class', style);        // set DIV class attribute
    div.setAttribute('className', style);    // set DIV class attribute for IE (?!)
    cell.appendChild(div);                   // append DIV to the table cell
}

function appendColumn() {
    var tbl = document.getElementById('teachers'), // table reference
        i;
    // open loop for each row and append cell
    for (i = 0; i < tbl.rows.length; i++) {
        createCell(tbl.rows[i].insertCell(tbl.rows[i].cells.length), "ausrufen", 'col', i);
    }
}