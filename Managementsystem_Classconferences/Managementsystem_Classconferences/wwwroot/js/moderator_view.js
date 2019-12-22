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

    var currentroom = GetCurrentRoom();
    connection.invoke("LoadInformation", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

}

connection.on("ReveiveLoadInformation", function (obj) {
    var obj_parsed = JSON.parse(obj);

    WriteClassName(obj_parsed.classname);
    WriteButtontext(obj_parsed.buttontext);
    WriteDataInTable("classes_completed", obj_parsed.classes_completed); 
    WriteDataInTable("classes_notedited", obj_parsed.classes_not_edited);
});

//after Hub-Mehtods have been called
connection.on("ReceiveIntersections", function (intersections) {

    WriteDataInTable("intersections", intersections)


});

connection.on("ReceiveTeachers", function (teachers) {

    WriteDataInTable("teachers", teachers);

});


function GetCurrentRoom() {
    https://stackoverflow.com/questions/45758837/script5009-urlsearchparams-is-undefined-in-ie-11

    $.urlParam = function (name) {
        var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
        if (results == null) {
            return null;
        }
        else {
            return decodeURI(results[1]) || 0;
        }
    }

    var currentroom = $.urlParam('handler');  

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

    appendColumn();
}

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