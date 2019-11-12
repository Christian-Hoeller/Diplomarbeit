"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();


connection.start().then(function () {
    connection.invoke("LoadRooms").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});






connection.on("ReceiveUserViewInfo", function (currentclassname, formteacher, headofdepartment, time, room, classes_completed, classes_notedited) {

    WriteData(currentclassname, formteacher, headofdepartment, time, room, classes_completed, classes_notedited);

});


connection.on("ReceiveRooms", function (orderstring) {


    WriteRooms(orderstring);

    var orderlist = orderstring.split(";");    

    for (var i = 0; i < orderlist.length - 1; i++) {
        connection.invoke("LoadUserViewInfo", orderlist[i]).catch(function (err) {
            return console.error(err.toString());
        });
    }
    

});

function WriteData(currentclassname, formteacher, headofdepartment, time, room, classes_completed, classes_notedited) {


    //decide whether the data is put into the first or the second column
    //maybe the class before the currentclass is needed to decide in which column the data has to be inserted

    var room_element = document.getElementById("c1_room");

    var value_room = room_element.innerHTML;

    if (value_room == room) {
        WriteUserViewInformation("c1_", currentclassname, formteacher, headofdepartment, time, classes_completed, classes_notedited);
    }
    else {
        WriteUserViewInformation("c2_", currentclassname, formteacher, headofdepartment, time, classes_completed, classes_notedited);
    }
}

function WriteUserViewInformation(element, currentclassname, formteacher, headofdepartment, time, classes_completed, classes_notedited) {

    var id_currentclassname = element + "classname";
    var id_formteacher = element + "formteacher";
    var id_headofdepartment = element + "headofdepartment";
    var id_time = element + "time";
    var id_classes_completed = element + "classes_completed";
    var id_classes_notedited = element + "classes_notedited";

    WriteInElement(id_currentclassname, currentclassname);
    WriteInElement(id_formteacher, formteacher);
    WriteInElement(id_headofdepartment, headofdepartment);
    WriteInElement(id_time, time);

    WriteDataInTable(id_classes_completed, classes_completed);
    WriteDataInTable(id_classes_notedited, classes_notedited);



}


function WriteRooms(orderstring) {

    var orderlist = orderstring.split(";");    

    WriteInElement("c1_room", orderlist[0]);
    WriteInElement("c2_room", orderlist[1]);

}

function WriteInElement(elementname, value) {
    var element = document.getElementById(elementname);
    element.innerHTML = value;
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



