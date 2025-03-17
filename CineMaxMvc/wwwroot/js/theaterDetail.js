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

                        // Find the status element in the current row
                        let row = $('button[onclick="toggleScreenStatus(' + screenId + ')"]').closest('tr');
                        let statusBadge = row.find('td:nth-child(4) .badge');
                        let button = $('button[onclick="toggleScreenStatus(' + screenId + ')"]');
                        let buttonText = $('#screenBtnText-' + screenId);
                        let buttonIcon = button.find('i');

                        if (response.isActive) {
                            // Update to Active
                            statusBadge.removeClass('bg-danger').addClass('bg-success').text('Active');
                            button.removeClass('btn-success').addClass('btn-danger');
                            buttonText.text('Disable');
                            buttonIcon.removeClass('fa-check-circle').addClass('fa-times-circle');
                        } else {
                            // Update to Inactive
                            statusBadge.removeClass('bg-success').addClass('bg-danger').text('Inactive');
                            button.removeClass('btn-danger').addClass('btn-success');
                            buttonText.text('Enable');
                            buttonIcon.removeClass('fa-times-circle').addClass('fa-check-circle');
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