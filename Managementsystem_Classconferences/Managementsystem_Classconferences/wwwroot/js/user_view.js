import * as General from './functions.js';

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

    General.WriteInElement("c1_room", order_parsed[0]);
    General.WriteInElement("c2_room", order_parsed[1]);

    for (var i = 0; i < order_parsed.length; i++) {
        connection.invoke("LoadUserPage", order_parsed[i]).catch(function (err) {
            return console.error(err.toString());
        });
    }
});


connection.on("ReceiveGeneralContent", function (myobject) {

    var obj_parsed = JSON.parse(myobject);

    if (document.getElementById("c1_room").innerHTML == obj_parsed.room) {
        WriteUserViewInformation("c1_", obj_parsed);
    }
    else {
        WriteUserViewInformation("c2_", obj_parsed);
    }

});

function WriteUserViewInformation(element, obj_parsed) {

    General.WriteDataInTable(element + "classesCompleted", obj_parsed.classesCompleted);
    General.WriteDataInTable(element + "classesNotEdited", obj_parsed.classesNotEdited);

    General.WriteInElement(element + "room", obj_parsed.room)
    General.WriteInElement(element + "classname", obj_parsed.classname);
    General.WriteInElement(element + "formTeacher", obj_parsed.formTeacher);
    General.WriteInElement(element + "headOfDepartment", obj_parsed.headOfDepartment);
    General.WriteInElement(element + "time", obj_parsed.time);
}