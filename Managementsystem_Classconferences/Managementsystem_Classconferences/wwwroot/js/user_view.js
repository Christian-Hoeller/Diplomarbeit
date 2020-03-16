
var connection = new signalR.HubConnectionBuilder().withUrl("/mainHub").build();
var allowNotifications = false;

connection.start().then(function () {
    connection.invoke("LoadRooms").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveRooms", function (order) {

    var order_parsed = JSON.parse(order);

    $("#c1_room").html(order_parsed[0]);
    $("#c2_room").html(order_parsed[1]);

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

function WriteUserViewInformation(element, obj) {

    $("#" + element + "room").html(obj.room);
    $("#" + element + "classname").html(obj.classname);
    $("#" + element + "headOfDepartment").html(obj.headOfDepartment);
    $("#" + element + "formTeacher").html(obj.formTeacher);
    $("#" + element + "time").html(obj.time);
    
    WriteDataInTable(element + "classesCompleted", obj.classesCompleted);
    WriteDataInTable(element + "classesNotEdited", obj.classesNotEdited);
}

connection.on("ReceiveTeacherCall", function (teacherID, message) {

    if (allowNotifications == true) {
        var userID = $("#userID").val();
        if (teacherID == userID.toLowerCase()) {
        
            toastr.options = {
                "closeButton": false,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-bottom-right",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "8000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
            toastr.info(message, "Ausruf");

            const msg = new SpeechSynthesisUtterance(message);
            speechSynthesis.speak(msg);
        }
    }
});

function WriteDataInTable(tablename, jsonArray) {
    $("#" + tablename).empty();
    var parsedArray = JSON.parse(jsonArray);

    for (var i = 0; i < parsedArray.length; i++) {
        $("#" + tablename).append("<tr><td>" + parsedArray[i] + "</td></tr>")
    }
}


function yesToNotifications() {

    document.getElementById('notiContent').style.visibility = "hidden";

    allowNotifications = true;
}

function noToNotifications() {

    document.getElementById('notiContent').style.visibility = "hidden";

    allowNotifications = false;
}



