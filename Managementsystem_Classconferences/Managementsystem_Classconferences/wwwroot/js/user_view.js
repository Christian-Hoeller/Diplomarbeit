"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();


connection.start().then(function () {
    connection.invoke("LoadRooms").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveRooms", function (order) {

    var order_parsed = JSON.parse(order);

    WriteInElement("c1_room", order_parsed[0]);
    WriteInElement("c2_room", order_parsed[1]);

    for (var i = 0; i < order_parsed.length; i++) {
        connection.invoke("LoadUserViewInfo", order_parsed[i]).catch(function (err) {
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


function WriteInElement(elementname, value) {
    var element = document.getElementById(elementname);
    element.innerHTML = value;
}

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



