"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();


connection.start().then(function () {
    connection.invoke("LoadRooms").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveRooms", function (orderstring) {

    var orderlist = orderstring.split(";");

    WriteInElement("c1_room", orderlist[0]);
    WriteInElement("c2_room", orderlist[1]);

    for (var i = 0; i < orderlist.length; i++) {
        connection.invoke("LoadUserViewInfo", orderlist[i]).catch(function (err) {
            return console.error(err.toString());
        });
    }
    

});


connection.on("ReceiveUserViewInfo", function (myobject) {

    var obj_parsed = JSON.parse(myobject);

    if (document.getElementById("c1_room").innerHTML == obj_parsed.room) {
        WriteUserViewInformation("c1_", obj_parsed);
    }
    else {
        WriteUserViewInformation("c2_", obj_parsed);
    }

});


function WriteUserViewInformation(element, obj_parsed) {

    WriteInElement(element + "room", obj_parsed.room)
    WriteInElement(element + "classname", obj_parsed.classname);
    WriteInElement(element + "formteacher", obj_parsed.formteacher);
    WriteInElement(element + "head_of_department", obj_parsed.head_of_department);
    WriteInElement(element + "time", obj_parsed.time);

    WriteDataInTable(element + "classes_not_edited", obj_parsed.classes_not_edited);
    WriteDataInTable(element + "classes_completed", obj_parsed.classes_completed);
}

//connection.on("ReceiveUserViewInfo", function (currentclassname, formteacher, headofdepartment, time, room, classes_completed, classes_notedited) {

//    WriteData(currentclassname, formteacher, headofdepartment, time, room, classes_completed, classes_notedited);

//});







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



