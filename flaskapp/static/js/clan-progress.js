const tableBody = document.getElementById("tableBody")
const searchField = document.getElementById("search-field")
const classFilter = document.getElementById("class-filter")
/**
 * @type {HTMLFormElement}
 */
const filterForm = document.getElementById("filter-form")

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

const sortOrder = [
    new SortType("class", true),
    new SortType("name", true)
]
const filters = {
    "class": "",
    "name": ""
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

function sortBy(type) {
    const carets = document.getElementsByClassName("table-head-caret")
    for (let i = 0; i < carets.length; i++) {
        const element = carets.item(i);

        element.style.display = "none"
        element.classList.remove("fa-caret-down")
        element.classList.remove("fa-caret-up")
    }

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
        if (!sortElem) {
            sortElem = new SortType(type, true)
            sortOrder.push(sortElem)
        }
        bringToFront(sortOrder, sortElem)
        sortOrder[0].ascending = true;
    }
    console.table(sortOrder)
    const caretElem = document.getElementById(type + "-caret")
    caretElem.style.display = "inline-block"
    caretElem.classList.add(sortOrder[0].ascending ? "fa-caret-down" : "fa-caret-up")
    updateTable()
}

function updateTable() {
    const xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            // Typical action to be performed when the document is ready:
            tableBody.innerHTML = xhttp.responseText;
        }
    };
    xhttp.open("POST", window.location, true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify({
        "sorting": sortOrder,
        "filters": filters
    }));
}

function updateFilters() {
    filters["class"] = classFilter.value
    filters["name"] = searchField.value;
    updateTable()
}

filterForm.onsubmit = (e) => {
    e.preventDefault()

    updateFilters()
}