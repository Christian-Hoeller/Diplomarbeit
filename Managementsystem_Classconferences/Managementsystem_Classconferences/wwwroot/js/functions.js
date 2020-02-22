

export function WriteDataInTable(tablename, jsonArray) {

    $("#" + tablename).empty();
    var parsedArray = JSON.parse(jsonArray);

    var table = document.getElementById(tablename);

    for (var i = 0; i < parsedArray.length; i++) {
        var row = table.insertRow(i);
        var cell = row.insertCell(0);
        cell.innerHTML = parsedArray[i];
    }
}

export function WriteInElement(elementname, value) {
    var element = document.getElementById(elementname);
    element.innerHTML = value;
}

