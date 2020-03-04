
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
        connection.invoke("LoadUserPageContent", order_parsed[i]).catch(function (err) {
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

    WriteDataInTable(element + "classesCompleted", obj_parsed.classesCompleted);
    WriteDataInTable(element + "classesNotEdited", obj_parsed.classesNotEdited);

    WriteInElement(element + "room", obj_parsed.room)
    WriteInElement(element + "classname", obj_parsed.classname);
    WriteInElement(element + "formTeacher", obj_parsed.formTeacher);
    WriteInElement(element + "headOfDepartment", obj_parsed.headOfDepartment);
    WriteInElement(element + "time", obj_parsed.time);
}

connection.on("ReceiveTeacherCall", function (teacherID, message) {

    console.log(teacherID);
    console.log(message);

    var userID = $("#userID").val();
    console.log(userID.toLowerCase());
    if (teacherID == userID.toLowerCase()) {
        alert(message);
        const msg = new SpeechSynthesisUtterance(message);
        speechSynthesis.speak(msg);
    }
});

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

function WriteInElement(elementname, value) {
    var element = document.getElementById(elementname).innerHTML = value;
}


