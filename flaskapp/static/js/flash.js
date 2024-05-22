const flashDiv = document.getElementById("flash-div")

function hideFlash(){
    flashDiv.classList.add("flash-faded")
    setTimeout(() => {
        flashDiv.style.display = "none"
    }, 300);
}

if(flashDiv){
    document.getElementById("flash-cross").addEventListener("click", (evt) => {
        hideFlash()
    })

    setTimeout(() => {
        hideFlash()
    }, 6000);
}   
