var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

//gets called when the connection is established
connection.start().then(function () {
    var currentroom = GetCurrentRoom();

    $("#room").html(currentroom);
    document.getElementById("sendButton").disabled = false;

    connection.invoke("LoadModeratorPage", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

function GetCurrentRoom() {
    return new URLSearchParams(window.location.search).get("handler");
}

document.getElementById("sendButton").addEventListener("click", function (event) {

    connection.invoke("ConferenceAction", GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

connection.on("ReceiveGeneralContent", function (obj) {

    var obj_parsed = JSON.parse(obj);

    if (obj_parsed.room == GetCurrentRoom()) {

        var formTeacher_parsed = JSON.parse(obj_parsed.formTeacher);
        var headOfDepartment_parsed = JSON.parse(obj_parsed.headOfDepartment);

        $("#classname").html(obj_parsed.classname);
        $("#formTeacher").html(formTeacher_parsed.Name);
        $("#headOfDepartment").html(headOfDepartment_parsed.Name);
        $("#time").html(obj_parsed.time);

        console.log(obj_parsed.classesNotEdited);


        WriteDataInTable("classesCompleted", JSON.parse(obj_parsed.classesCompleted));
        WriteDataInTable("classesNotEdited", JSON.parse(obj_parsed.classesNotEdited));
    }
});

connection.on("ReceiveModeratorContent", function (obj) {

    var obj_parsed = JSON.parse(obj);

    document.getElementById("sendButton").value = obj_parsed.buttonText;
    WriteTeachersWithButtonsInTable(obj_parsed.teachers);
});

connection.on("ReveiveIntersections", function (obj) {

    var obj_parsed = JSON.parse(obj);
    var intersections = new Array();


    if (obj_parsed[0] == "") {
        intersections.push("Keine Überschneidungen");
    }
    else {
        for (var i = 0; i < obj_parsed.length; i++) {
            console.log(obj_parsed[i]);
            console.log(getShorthandForTeacher(obj_parsed[i]));
            intersections.push(getShorthandForTeacher(obj_parsed[i]));
        }
        console.log(intersections);
    }
    WriteDataInTable("intersections", intersections);


});

function getShorthandForTeacher(teacherID) {
    return teacherID.split("@")[0].toUpperCase();
}


function WriteTeachersWithButtonsInTable(teacherArray) {

    $("#teachers").empty(); //clear the table

    if (teacherArray == "") {
        $("#teachers").append("<tr><td>Keine Lehrer</td></tr>");
    }
    else {
        var teacherData, buttonData;
        var parsedArray = JSON.parse(teacherArray);

        for (var i = 0; i < parsedArray.length; i++) {
            var teacherID = parsedArray[i].ID;
            var fullName = parsedArray[i].Name; 

            teacherData = "<td><p title='" + fullName + "'>" + getShorthandForTeacher(teacherID) + "</p></td>";
            buttonData = "<td><button onclick='callTeacher(" + i + ")' class='btn btn-secundary' style='margin: 6px;'>ausrufen</button></td>";

            $("#teachers").append("<tr>" + teacherData + buttonData + "</tr>");
        }
    }
}

function callTeacher(indexOfCalledTeacher) {

    var moderatorID = $("#moderatorID").val();

    connection.invoke("SendTeacherCall", indexOfCalledTeacher, moderatorID, GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
}

function WriteDataInTable(tablename, jsonArray) {
    $("#" + tablename).empty();

    for (var i = 0; i < jsonArray.length; i++) {
        $("#" + tablename).append("<tr><td>" + jsonArray[i] + "</td></tr>")
    }
}