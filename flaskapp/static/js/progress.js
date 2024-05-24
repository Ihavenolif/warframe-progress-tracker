const tableBody = document.getElementById("tableBody")

/**
 * 
 * @param {Array[Object]} list 
 */
function updateTable(list){
    tableBody.innerHTML = ""

    list.forEach(item => {
        console.log(item)
        tableBody.innerHTML += "<tr>"+
                            "<td>" + item.name + "</td>"+
                            "<td>" + item.class + "</td>"+
                            "<td>" + item.owned + "</td>"+
                            "<td>" + item.mastered + "</td>"+
                            "</tr>"
    })
}