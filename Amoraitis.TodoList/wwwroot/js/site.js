$(document).ready(function () {

    $('.done-checkbox').on('click', function (e) {
        markCompleted(e.target);
    });
    
});

function markCompleted(checkbox) {
    checkbox.disabled = true;

    var row = checkbox.closest('tr');
    $(row).addClass('done');

    var form = checkbox.closest('form');
    form.submit();
}

(function () {
    var reformatTimeStamps = function () {
        var timeStamps = document.getElementsByClassName("timeStampValue");
        for (var ts of timeStamps) {
            var thisTimeStamp = ts.getAttribute("data-value");
            var date = new Date(thisTimeStamp);
            ts.textContent = moment(date).format('LLL');
        }
    }
    reformatTimeStamps();
})(); 

(function () {
    var modernizeTimeStamps = function () {
        var timeStamps = document.getElementsByClassName("timeStampValueModernized");
        for (var ts of timeStamps) {
            var thisTimeStamp = ts.getAttribute("data-value");
            var date = new Date(thisTimeStamp);
            ts.textContent = moment(date).fromNow();
        }
    }
    modernizeTimeStamps();
})();