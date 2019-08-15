"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

//connection.on("ReceiveMessage", function (user, message) {
//    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
//    var encodedMsg = user + " says " + msg;
//    var li = document.createElement("li");
//    li.textContent = encodedMsg;
//    document.getElementById("messagesList").appendChild(li);
//});

//connection.start().then(function () {
//    document.getElementById("sendButton").disabled = false;
//}).catch(function (err) {
//    return console.error(err.toString());
//});

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});




connection.on("ReceiveIntersections", function (user, message) {
    $("#table").empty();    //clear the table content
    var result = message.split(";");    //split the message by ';' (ABLD;SOEK;GRUG, usw.)


    var table = document.getElementById("table");   //find the table with the id

    for (var i = 0; i < result.length; i++) {
        var row = table.insertRow(i);   //insert the row
        var cell = row.insertCell(0);   //insert the cell
        cell.innerHTML = result[i];     //write the date for the specific teacher
    }
    //var message;

    //$.ajax({    //new AJAX request to get the Teachers for the next class from the code behind
    //    type: "POST",
    //    url: '?handler=Refresh',
    //    cache: false,
    //    async: false,   //async: false, to continue with the other code
    //    contentType: "application/json; charset=utf-8",
    //    dataType: "json",
    //    success: function (response) {
    //        message = response;
    //    }
    //});

});

connection.on("ReceiveTeachers", function (user, message) {
    $("#teachers").empty();    //clear the table content
    var result = message.split(";");    //split the message by ';' (ABLD;SOEK;GRUG, usw.)


    var table = document.getElementById("teachers");   //find the table with the id

    for (var i = 0; i < result.length; i++) {
        var row = table.insertRow(i);   //insert the row
        var cell = row.insertCell(0);   //insert the cell
        cell.innerHTML = result[i];     //write the date for the specific teacher
    }
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var messageintersections;
    var messageteachers;
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
    var user = "sample";
    connection.invoke("SendMessage", user, messageintersections, messageteachers).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
