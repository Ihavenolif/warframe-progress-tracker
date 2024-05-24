const tableBody = document.getElementById("tableBody")

class SortType {
    /**
     * @type {string}
     */
    type
    /**
     *  @type {boolean}
     */
    ascending

    /**
     * 
     * @param {string} type 
     * @param {boolean} ascending 
     */
    constructor(type, ascending) {
        this.type = type
        this.ascending = ascending
    }
}

/**
 * @type {SortType[]}
 */
const sortOrder = [
    new SortType("mastered", true),
    new SortType("class", true),
    new SortType("name", true)
]

/**
 * 
 * @param {Array[Object]} list 
 */
function updateTable() {
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            // Typical action to be performed when the document is ready:
            tableBody.innerHTML = xhttp.responseText;
        }
    };
    xhttp.open("POST", "/progress", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify(sortOrder));
}

/**
 * Brings an element of the array to the front.
 * 
 * @param {Array} array - The array to modify.
 * @param {*} element - The element to bring to the front.
 * @returns {Array} - The modified array with the element at the front.
 */
function bringToFront(array, element) {
    const index = array.indexOf(element);

    if (index === -1) {
        return array; // Element not found, return the original array
    }

    // Remove the element from its current position
    array.splice(index, 1);

    // Add the element to the front of the array
    array.unshift(element);

    return array;
}

/**
 * 
 * @param {string} type 
 */
function sortBy(type) {
    if (sortOrder[0].type == type) {
        sortOrder[0].ascending = !sortOrder[0].ascending
    }
    else {
        /**
         * @type {SortType}
         */
        var sortElem;
        sortOrder.forEach(x => {
            if (x.type == type) {
                sortElem = x;
            }
        })
        bringToFront(sortOrder, sortElem)
    }
    console.table(sortOrder)
    updateTable()
}