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
        let selectedDate = $("#showDate").val();
        let selectedTime = $("#showTime").val();

        if (!selectedDate || !selectedTime) return;

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
                </tr>
            `);

        }

        // Once at least one time is added, disable date selection
        $("#showDate").prop("disabled", true);
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

        console.log(requestData); // Debugging: Check data before sending

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
    $('#addTimeModal').on('show.bs.modal', function (event) {
        // Button that triggered the modal
        const button = $(event.relatedTarget);

        // Extract info from data-* attributes
        const movieId = button.data('movie-id');
        const movieTitle = button.data('movie-title');

        // Update the modal's content
        $('#movieId').val(movieId);
        $('#movieTitleDisplay').val(movieTitle);
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
            const $editBtn = $('<div class="edit-time" title="Edit Time">✏️</div>');
            const $deleteBtn = $('<div class="delete-time" title="Delete Time">🗑️</div>');

            $controls.append($editBtn).append($deleteBtn);
            $button.append($controls);

            // Handle edit button click
            $editBtn.on('click', function (e) {
                e.stopPropagation();
                startEditing($button);
            });

            // Handle delete button click
            $deleteBtn.on('click', function (e) {
                e.stopPropagation();
                showDeleteConfirmation($button);
            });
        });
    }

    // Function to start editing a time button
    function startEditing($button) {

        console.log("Click");
        // If already editing, return
        if ($button.hasClass('editing')) {
            return;
        }

        // Add editing class
        $button.addClass('editing');

        // Get the current time from the button (the first text node)
        const currentTimeText = $button.contents().filter(function () {
            return this.nodeType === 3; // Text node
        }).text().trim();

        // Create an input field
        const $inputField = $('<input>', {
            type: 'time',
            class: 'time-input'
        });

        // Convert display time (HH:MM) to input time format
        const timeParts = currentTimeText.split(':');
        const hours = timeParts[0].padStart(2, '0');
        const minutes = timeParts[1] ? timeParts[1].padStart(2, '0') : '00';
        $inputField.val(`${hours}:${minutes}`);

        // Save the price tag and controls if they exist
        let $priceTag = $button.find('.price-tag').detach();
        let $controls = $button.find('.time-controls').detach();

        // Clear the button content and add the input
        $button.empty().append($inputField);

        // Add back the price tag and controls
        if ($priceTag && $priceTag.length) {
            $button.append($priceTag);
        }
        $button.append($controls);

        // Focus the input
        $inputField.focus();

        // Handle input blur (when user clicks away)
        $inputField.on('blur', function () {
            // Get the new time value
            const newTime = $(this).val();

            // Format time for display (HH:MM)
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
            $button.append($controls);

            // Check if the time has actually changed
            if (timeDisplay !== currentTimeText) {
                // Send an AJAX request to update the time in the database
                updateShowtimeInDatabase($button.data('showtime-id'), timeDisplay);
            }
        });

        // Handle Enter key
        $inputField.on('keydown', function (e) {
            if (e.key === 'Enter') {
                $(this).blur(); // Trigger the blur event
            }
            // Handle Escape key to cancel editing
            else if (e.key === 'Escape') {
                $button.removeClass('editing');
                $button.empty().text(currentTimeText);
                if ($priceTag && $priceTag.length) {
                    $button.append($priceTag);
                }
                $button.append($controls);
            }
        });

        // Handle clicks outside the button
        $(document).on('click.timeButton', function (e) {
            if (!$(e.target).closest($button).length) {
                $inputField.blur();
                $(document).off('click.timeButton');
            }
        });
    }

    // Function to show delete confirmation
    function showDeleteConfirmation($button) {
        // Create confirmation overlay
        const $confirm = $(`
            <div class="delete-confirm">
                <div>Delete this time?</div>
                <div>
                    <button class="confirm-yes">Yes</button>
                    <button class="confirm-no">No</button>
                </div>
            </div>
        `);

        // Add to button
        $button.append($confirm);

        // Handle Yes click
        $confirm.find('.confirm-yes').on('click', function () {
            const showtimeId = $button.data('showtime-id');
            deleteShowtime(showtimeId, $button);
        });

        // Handle No click
        $confirm.find('.confirm-no').on('click', function () {
            $confirm.remove();
        });

        // Handle clicks outside the confirmation
        $(document).on('click.deleteConfirm', function (e) {
            if (!$(e.target).closest($confirm).length && !$(e.target).closest('.delete-time').length) {
                $confirm.remove();
                $(document).off('click.deleteConfirm');
            }
        });
    }

    // Function to delete a showtime
    function deleteShowtime(showtimeId, $button) {
        // Add a loading indicator
        $.notify({
            message: 'Deleting showtime...'
        }, {
            type: 'info',
            placement: { from: "bottom", align: "right" }
        });

        // Replace with your actual API endpoint
        const url = '/api/Screen/DeleteShowtime';

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                showtimeId: showtimeId
            }),
            success: function (response) {
                console.log('Showtime deleted successfully:', response);

                // Remove the button with animation
                $button.fadeOut(300, function () {
                    $(this).remove();
                });

                // Show success notification
                $.notify({
                    message: 'Showtime deleted successfully'
                }, {
                    type: 'success',
                    placement: { from: "bottom", align: "right" }
                });
            },
            error: function (xhr, status, error) {
                console.error('Error deleting showtime:', error);

                // Remove the confirmation dialog
                $button.find('.delete-confirm').remove();

                // Show error notification
                $.notify({
                    message: 'Failed to delete showtime: ' + (xhr.responseJSON?.message || error)
                }, {
                    type: 'danger',
                    placement: { from: "bottom", align: "right" }
                });
            }
        });
    }

    // Function to update showtime in database via AJAX
    function updateShowtimeInDatabase(showtimeId, newTime) {
        // Add a loading indicator
        $.notify({
            message: 'Updating showtime...'
        }, {
            type: 'info',
            placement: { from: "bottom", align: "right" }
        });

        // Replace with your actual API endpoint
        const url = '/api/Screen/UpdateShowtime';

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                showtimeId: showtimeId,
                time: newTime
            }),
            success: function (response) {
                console.log('Showtime updated successfully:', response);
                // Show success notification
                $.notify({
                    message: 'Showtime updated successfully'
                }, {
                    type: 'success',
                    placement: { from: "bottom", align: "right" }
                });
            },
            error: function (xhr, status, error) {
                console.error('Error updating showtime:', error);
                // Show error notification
                $.notify({
                    message: 'Failed to update showtime: ' + (xhr.responseJSON?.message || error)
                }, {
                    type: 'danger',
                    placement: { from: "bottom", align: "right" }
                });
            }
        });
    }

    // Click on time button now directly opens edit mode
    //$('.time-button').on('click', function (e) {
    //    // Only proceed if we didn't click on a control
    //    if (!$(e.target).closest('.time-controls').length &&
    //        !$(e.target).closest('.delete-confirm').length) {
    //        startEditing($(this));
    //    }
    //});
});