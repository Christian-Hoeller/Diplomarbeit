var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

//gets called when the connection is established
connection.start().then(function () {
    var currentroom = GetCurrentRoom();

    WriteInElement("room", currentroom);
    document.getElementById("sendButton").disabled = false;

    connection.invoke("LoadModeratorPage", currentroom).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

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
    return $.urlParam('handler');
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

        $("#classname").html(obj_parsed.classname);
        $("#formTeacher").html(obj_parsed.formTeacher);
        $("#headOfDepartment").html(obj_parsed.headOfDepartment);
        $("#time").html(obj_parsed.time);

        WriteDataInTable("classesCompleted", obj_parsed.classesCompleted);
        WriteDataInTable("classesNotEdited", obj_parsed.classesNotEdited);
    }

});

connection.on("ReceiveModeratorContent", function (obj) {

    var obj_parsed = JSON.parse(obj);

    document.getElementById("sendButton").value = obj_parsed.buttonText;
    WriteTeachersWithButtonsInTable(obj_parsed.teachers);
});

connection.on("ReveiveIntersections", function (obj) {

    WriteDataInTable("intersections", obj);
});








function callTeacher(indexOfCalledTeacher) {

    var moderatorID = $("#moderatorID").val();

    connection.invoke("SendTeacherCall", indexOfCalledTeacher, moderatorID, GetCurrentRoom()).catch(function (err) {
        return console.error(err.toString());
    });
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
            var nameShort = parsedArray[i].Name_Short; 
            var fullName = parsedArray[i].Name;

            teacherData = "<td><p title='" + fullName + "'>" + nameShort + "</p></td>";
            buttonData = "<td><button onclick='callTeacher(" + i + ")' class='btn btn-primary' style='margin: 6px;'>ausrufen</button></td>";

            $("#teachers").append("<tr>" + teacherData + buttonData + "</tr>");
        }
    }
}

function WriteDataInTable(tablename, jsonArray) {
    $("#" + tablename).empty();
    var parsedArray = JSON.parse(jsonArray);

    for (var i = 0; i < parsedArray.length; i++) {
        $("#" + tablename).append("<tr><td>" + parsedArray[i] + "</td></tr>")
    }
}

function WriteInElement(elementname, value) {
    var element = document.getElementById(elementname).innerHTML = value;
}
