function loadTheaters(provinceId) {
    $.get("/Customer/Home/GetTheaters", { provinceId: provinceId }, function (data) {
        $("#theaterList").html(data);
    });

    // set active for the province
    $(".theater-list-item").removeClass("active");
    $(`.theater-list-item[date-province-id=${provinceId}]`).addClass("active");
}

function loadShowTimes(theaterId, element) {
    // get the selected date
    var selectedDate = $(".date-item.active").data("date");
    // load the id in to the hidden input
    $("#activeTheater").val(theaterId);

    // change active status
    $(".cinema-list-item").removeClass("active");
    $(element).addClass("active");

    // change theater info display
    var theaterName = $(element).data("theater-name");
    var theaterAddress = $(element).data("theater-address");
    $("#theaterName").text(theaterName);
    $("#theaterAddress").text(theaterAddress);

    // send the request
    $.get("/Customer/Home/GetShowTimes", { theaterId: theaterId, date: selectedDate }, function (data) {
        $("#showtimeContainer").html(data);
    });
}

function selectDate(element) {
    // get the selected theater id
    var theaterId = $("#activeTheater").val();
    // Get the full date from the data attribute
    var fullDateStr = $(element).data("date");

    // send the request
    $.get("/Customer/Home/GetShowTimes", { theaterId: theaterId, date: fullDateStr }, function (data) {
        $("#showtimeContainer").html(data);
    });

    $(".date-item").removeClass("active");
    $(element).addClass("active");



    // Convert to JavaScript Date object
    var fullDate = new Date(fullDateStr);

    // Get the weekday name (Vietnamese format)
    var weekdays = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];
    var dayOfWeek = weekdays[fullDate.getDay()]; // Get the correct day name

    // Format the date as "dd/MM/yyyy"
    var formattedDate = fullDate.toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
    });

    // Construct the final string
    var finalDate = `${dayOfWeek}, ${formattedDate}`;

    // Update the theater info section
    $("#selectedDate").text(finalDate);
}