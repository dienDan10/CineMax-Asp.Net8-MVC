function toggleSeatStatus(seatId, element) {
    // Send AJAX request to update seat status
    $.ajax({
        url: '/Admin/Screen/ToggleSeatStatus', // Adjust the URL to match your controller action
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(seatId),
        success: function (response) {
            if (response.success) {
                if (!response.isActive) {
                    $(element).removeClass('active').addClass('inactive');
                } else {
                    $(element).removeClass('inactive').addClass('active');
                }
                toastr.success(response.message);
            } else {
                toastr.error('Failed to update seat status.');
            }
        },
        error: function () {
            toastr.error('Error occurred while updating seat.');
        }
    });
}