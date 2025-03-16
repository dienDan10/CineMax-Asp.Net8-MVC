function toggleScreenStatus(screenId) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to enable/disable this screen!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, do it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "/Admin/Screen/ToggleStatus",
                data: JSON.stringify(screenId),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);

                        // Update Status in UI
                        let statusBadge = $('#screenStatus-' + screenId);
                        let button = $('button[onclick="toggleScreenStatus(' + screenId + ')"]');
                        let buttonText = $('#screenBtnText-' + screenId);

                        if (response.isActive) {
                            statusBadge.removeClass('bg-danger').addClass('bg-success').text('Active');
                            button.removeClass('btn-success').addClass('btn-danger');
                            buttonText.text('Disable');
                        } else {
                            statusBadge.removeClass('bg-success').addClass('bg-danger').text('Disabled');
                            button.removeClass('btn-danger').addClass('btn-success');
                            buttonText.text('Enable');
                        }
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function () {
                    toastr.error("Something went wrong. Please try again.");
                }
            });
        }
    });
}