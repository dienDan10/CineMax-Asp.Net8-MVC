$(document).ready(function () {

    /* --------- ADD NEW SHOW TIME SCRIPT ----------- */
    let showtimes = {}; // Object to store selected dates and times

    // When user selects a movie
    $("#movieSelect").on("change", function () {
        let selectedOption = $(this).find("option:selected");

        let poster = selectedOption.data("poster");
        let title = selectedOption.text();
        let genre = selectedOption.data("genre");
        let duration = selectedOption.data("duration");
        let description = selectedOption.data("description"); // HTML content

        $("#moviePoster").attr("src", poster);
        $("#movieTitle").text(title);
        $("#movieGenre").text(genre);
        $("#movieDuration").html(`<i class="fas fa-clock me-1"></i> ${duration} mins`);
        $("#movieDescription").html(description); 

        $("#movieDetails").fadeIn();
    });

    // When user selects a date
    $("#showDate").on("change", function () {
        let selectedDate = $(this).val();
        if (!selectedDate) return;

        // Enable time selection
        $("#showTime").prop("disabled", false);
        $("#addTimeBtn").prop("disabled", false);
    });

    // When user adds a time
    $("#addTimeBtn").on("click", function () {
        const selectedDate = $("#showDate").val();
        const selectedTime = $("#showTime").val();
        const movieDuration = $("#movieSelect option:selected").data("duration");

        if (!selectedDate || !selectedTime) return;

        if (isNaN(movieDuration)) {
            toastr.warning("Please select a movie first!");
            return;
        }

        let newTimeInMinutes = convertTimeToMinutes(selectedTime);
        // Check for conflicts
        if (showtimes[selectedDate]) {
            for (let existingTime of showtimes[selectedDate]) {
                let existingTimeInMinutes = convertTimeToMinutes(existingTime);

                // Show time start before a movie ends
                if (newTimeInMinutes >= existingTimeInMinutes && newTimeInMinutes < existingTimeInMinutes + movieDuration) {
                    toastr.error(`The time ${selectedTime} conflicts with an existing showtime.`);
                    return;
                }

                // Showtime ends after a movie starts
                if (newTimeInMinutes <= existingTimeInMinutes && newTimeInMinutes + movieDuration > existingTimeInMinutes) {
                    toastr.error(`The time ${selectedTime} conflicts with an existing showtime.`);
                    return;
                }
            }
        }

        // Initialize the array if the date is not in the object
        if (!showtimes[selectedDate]) {
            showtimes[selectedDate] = [];
        }

        // Prevent duplicate times
        if (!showtimes[selectedDate].includes(selectedTime)) {
            showtimes[selectedDate].push(selectedTime);

            // Add to the scheduled table
            $("#scheduledShowtimes").append(`
                <tr data-date="${selectedDate}" data-time="${selectedTime}">
                    <td>${selectedDate}</td>
                    <td>${selectedTime}</td>
                    <td><span class="badge bg-success">Scheduled</span></td>
                    <td><button class="btn btn-sm btn-danger remove-time"><i class="fas fa-trash-alt"></i></button></td>
                </tr>
            `);

        }

        // Once at least one time is added, disable date selection
        $("#showDate").prop("disabled", true);
    });

    function convertTimeToMinutes(timeStr) {
        let [hours, minutes] = timeStr.split(":").map(Number);
        return hours * 60 + minutes;
    }

    // Remove time from the list
    $(document).on("click", ".remove-time", function () {
        let row = $(this).closest("tr");
        let selectedDate = row.data("date");
        let selectedTime = row.data("time");

        // Remove from showtimes object
        showtimes[selectedDate] = showtimes[selectedDate].filter(time => time !== selectedTime);

        // If no more times exist for this date, allow date selection again
        if (showtimes[selectedDate].length === 0) {
            delete showtimes[selectedDate];
            $("#showDate").prop("disabled", false);
        }

        // Remove the row from the table
        row.remove();
    });

    // Form submission
    $("#submitShowtimes").on("click", function (e) {
        e.preventDefault(); // Prevent default form submission

        let movieId = $("#movieSelect").val(); // Get selected movie ID
        let screenId = $("#screenId").val(); // Get screen ID from hidden input
        let ticketPrice = parseFloat($("#ticketPrice").val());

        if (!movieId) {
            toastr.warning("Please select a movie before submitting.");
            return;
        }

        if (!ticketPrice) {
            toastr.warning("Please input a valid ticket price");
        }

        if (Object.keys(showtimes).length === 0) {
            toastr.warning("Please add at least one showtime before submitting.");
            return;
        }

        let formattedData = Object.keys(showtimes).map(date => ({
            date: date,
            times: showtimes[date]
        }));

        

        let requestData = {
            screenId: screenId,
            ticketPrice: ticketPrice,
            movieId: movieId,
            showtimes: formattedData
        };

        console.log(requestData); // Check data before sending

        $.ajax({
            url: "/Admin/Screen/AddShowtimes",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    setTimeout(() => location.reload(), 1000); // Delay reload for smooth UX

                } else {
                    toastr.error(response.message);
                }
            }
        });
    });


    /* --------- ADD NEW TIME TO SHOW TIME SCRIPT ----------- */
    // Add data to model when open
    $("#addTimeModal").on("show.bs.modal", function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal

        // Extract values from data attributes
        let movieId = button.data("movie-id");
        let movieTitle = button.data("movie-title");
        let screenId = button.data("screen-id");
        let showDate = button.data("date");

        // Populate the modal fields
        $("#movieId").val(movieId);
        $("#movieTitleDisplay").val(movieTitle);
        $("#addTimeScreenId").val(screenId);
        $("#addTimeDate").val(showDate);

        // Disable the date field
        $("#addTimeDate").prop("disabled", true);
    });

    // action on submit form
    $("#addTimeModal form").on("submit", function (e) {
        e.preventDefault(); // Prevent default form submission

        let movieId = $("#movieId").val();
        let screenId = $("#addTimeScreenId").val();
        let showDate = $("#addTimeDate").val();
        let showTime = $("#addTimeTime").val();
        let ticketPrice = parseFloat($("#addTimePrice").val());

        if (!showTime) {
            toastr.warning("Show time is required.");
            return;
        }

        if (!ticketPrice) {
            toastr.warning("Please input a valid ticket price.");
            return;
        }

        // Format data to match your existing showtime structure
        let requestData = {
            screenId: screenId,
            ticketPrice: ticketPrice,
            movieId: movieId,
            showtimes: [
                {
                    date: showDate,
                    times: [showTime] // Single time wrapped in an array to match format
                }
            ]
        };

        console.log(requestData); // Debugging: Check data before sending

        $.ajax({
            url: "/Admin/Screen/AddShowtimes",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $("#addTimeModal").modal("hide"); // Close modal on success
                    setTimeout(() => location.reload(), 1000); // Delay reload for smooth UX
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("An error occurred while adding the showtime.");
            }
        });
    });

    /* --------- CREATE UPDATE AND DELETE SHOW TIME BUTTON ----------- */
    // Initialize time buttons
    initializeTimeButtons();

    // Function to initialize time buttons (both existing and newly added)
    function initializeTimeButtons() {
        // Add control buttons to each time button
        $('.time-button').each(function () {
            const $button = $(this);

            // Skip if controls already added
            if ($button.find('.time-controls').length > 0) {
                return;
            }

            // Add edit and delete controls
            const $controls = $('<div class="time-controls"></div>');
            const $deleteBtn = $('<div class="delete-time" title="Delete Time">🗑️</div>');

            $controls.append($deleteBtn);
            $button.append($controls);

            // Handle delete button click
            $deleteBtn.on('click', function (e) {
                e.stopPropagation();
                showDeleteConfirmation($button);
            });
        });
    }

    // Function to start editing a time button
    function startEditing($button) {
        // If already editing, return
        if ($button.hasClass('editing')) {
            return;
        }

        // Add editing class
        $button.addClass('editing');

        // Get the current time from the button
        const currentTimeText = $button.contents().filter(function () {
            return this.nodeType === 3; // Text node
        }).text().trim();

        // Create an input field
        const $inputField = $('<input>', {
            type: 'time',
            class: 'time-input'
        });

        // Convert display time to input format
        const timeParts = currentTimeText.split(':');
        const hours = timeParts[0].padStart(2, '0');
        const minutes = timeParts[1] ? timeParts[1].padStart(2, '0') : '00';
        $inputField.val(`${hours}:${minutes}`);

        // Save the price tag and controls
        let $priceTag = $button.find('.price-tag');
        let $controls = $button.find('.time-controls');

        // Store original controls HTML for later restoration
        const controlsHtml = $controls.prop('outerHTML');

        // Clear the button content and add the input
        $button.empty().append($inputField);

        // Re-add the price tag and controls (but keep them visible)
        if ($priceTag && $priceTag.length) {
            $button.append($priceTag);
        }
        $button.append($controls);

        // Re-attach the delete event handler
        $button.find('.delete-time').on('click', function (e) {
            e.stopPropagation();
            // Set a flag on the button to indicate delete was clicked
            $button.data('delete-clicked', true);
            console.log("delete clicked");
            showDeleteConfirmation($button);
        });

        // Focus the input
        $inputField.focus();

        // Handle input blur with a delay to check for the delete action
        $inputField.on('blur', function () {
            setTimeout(function () {
                // Check if delete was clicked
                if ($button.data('delete-clicked') === true) {
                    // Reset the flag but don't proceed with time updating
                    $button.removeData('delete-clicked');
                    return;
                }

                // Get the new time value
                const newTime = $inputField.val();

                // Format time for display
                let timeDisplay = currentTimeText;
                if (newTime) {
                    const timeObj = new Date(`2000-01-01T${newTime}`);
                    timeDisplay = timeObj.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                        hour12: false
                    });
                }

                // Remove editing class
                $button.removeClass('editing');

                // Clear the button content and restore the original text
                $button.empty().text(timeDisplay);

                // Add back the price tag and controls
                if ($priceTag && $priceTag.length) {
                    $button.append($priceTag);
                }

                // Append controls and re-initialize the handlers
                $button.append($(controlsHtml));

                // Re-initialize the delete button handler
                $button.find('.delete-time').on('click', function (e) {
                    e.stopPropagation();
                    console.log("click");
                    showDeleteConfirmation($button);
                });

                // Check if the time has actually changed
                if (timeDisplay !== currentTimeText) {
                    updateShowtimeInDatabase($button.data('showtime-id'), timeDisplay);
                }
            }, 100); // Short delay to allow other event handlers to fire
        });

        // Handle Enter and Escape keys
        $inputField.on('keydown', function (e) {
            if (e.key === 'Enter') {
                $(this).blur();
            } else if (e.key === 'Escape') {
                $button.removeClass('editing');
                $button.empty().text(currentTimeText);
                if ($priceTag && $priceTag.length) {
                    $button.append($priceTag);
                }
                $button.append($(controlsHtml));
                // Re-initialize the delete button handler
                $button.find('.delete-time').on('click', function (e) {
                    e.stopPropagation();
                    console.log("click");
                    showDeleteConfirmation($button);
                });
            }
        });

        // Handle clicks outside the button
        $(document).on('click.timeButton', function (e) {
            if (!$(e.target).closest($button).length &&
                !$(e.target).closest('.delete-confirm').length) {
                $inputField.blur();
                $(document).off('click.timeButton');
            }
        });
    }

    // Function to show delete confirmation
    function showDeleteConfirmation($button) {
        const showtimeId = $button.data('showtime-id');

        Swal.fire({
            title: "Are you sure?",
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!"
        }).then((result) => {
            if (result.isConfirmed) {
                deleteShowtime(showtimeId, $button);
            }
        });
    }

    // Function to delete a showtime
    function deleteShowtime(showtimeId, $button) {
        if (!showtimeId) {
            console.error("Missing showtime ID.");
            return;
        }

        $.ajax({
            url: "/Admin/Screen/DeleteShowtime",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ showtimeId: showtimeId }),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $button.fadeOut(300, function () {
                        $(this).remove(); // Remove the button from UI after fade-out
                    });
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error deleting showtime:", error);
                toastr.error("Failed to delete showtime. Please try again.");
            }
        });
    }

    // Function to update showtime in database via AJAX

    function updateShowtimeInDatabase(showtimeId, newTime) {
        if (!showtimeId || !newTime) {
            console.error("Missing showtime ID or new time.");
            return;
        }

        let requestData = {
            showtimeId: showtimeId,
            newTime: newTime
        };

        console.log("Updating showtime:", requestData); // Debugging

        $.ajax({
            url: "/Admin/Screen/UpdateShowtime",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("Error updating showtime:", error);
                toastr.error("Failed to update showtime. Please try again.");
            }
        });
    }

    // Click on time button now directly opens edit mode
    $('.time-button').on('click', function (e) {
        // Only proceed if we didn't click on a control
        if (!$(e.target).closest('.time-controls').length &&
            !$(e.target).closest('.delete-confirm').length) {
            startEditing($(this));
        }
    });
});