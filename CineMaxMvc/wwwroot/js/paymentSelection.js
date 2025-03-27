$(document).ready(function () {
    $("#proceedPayment").on("click", function (e) {
        e.preventDefault();

        // Get form values
        const paymentData = {
            fullName: $("#fullName").val().trim(),
            phone: $("#phoneNumber").val().trim(),
            email: $("#email").val().trim(),
            paymentMethod: $("input[name='paymentMethod']:checked").val(),
            bookingId: $("#bookingId").val().trim(),
            concessionOrderId: $("#concessionOrderId").val().trim(),
            showtimeId: $("#showtimeId").val().trim(),
        };

        // Validate inputs
        if (!paymentData.fullName) {
            toastr.error("Vui lòng nhập họ và tên");
            $("#fullName").focus();
            return;
        }

        if (!paymentData.phone) {
            toastr.error("Vui lòng nhập số điện thoại");
            $("#phoneNumber").focus();
            return;
        } else if (!/^\d{10,11}$/.test(paymentData.phone)) {
            toastr.error("Số điện thoại phải có 10-11 chữ số");
            $("#phoneNumber").focus();
            return;
        }

        if (paymentData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(paymentData.email)) {
            toastr.error("Email không hợp lệ");
            $("#email").focus();
            return;
        }

        // Disable button and show loading state
        const $btn = $(this);
        $btn.prop("disabled", true);
        $btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...');

        // Send data to server
        $.ajax({
            url: "/Customer/Bookings/ProcessPayment",
            type: "POST",
            data: paymentData,
            success: function (response) {
                if (response.success && response.redirectUrl) {
                    toastr.success("Xác nhận thành công! Đang chuyển hướng...");
                    setTimeout(() => {
                        window.location.href = response.redirectUrl;
                    }, 1500);
                } else {
                    toastr.error(response.message || "Có lỗi xảy ra khi xử lý thanh toán");
                    $btn.prop("disabled", false);
                    $btn.html('<i class="fas fa-lock me-2"></i> Thanh toán');
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Lỗi kết nối đến server. Vui lòng thử lại sau.");
                $btn.prop("disabled", false);
                $btn.html('<i class="fas fa-lock me-2"></i> Thanh toán');
                console.error("Payment error:", error);
            }
        });
    });

    // Add input masking for phone number
    $("#phoneNumber").on("input", function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    // Get booking creation time from hidden input
    const bookingCreatedDate = new Date($("#bookingCreatedDate").val());
    const expirationTime = new Date(bookingCreatedDate.getTime() + 5 * 60 * 1000); // 5 minutes after booking

    function updateCountdown() {
        const now = new Date();
        const timeLeft = expirationTime - now;

        if (timeLeft <= 0) {
            $("#countdownTimer").text("00:00");
            $("#proceedPayment").prop("disabled", true);
            toastr.error("Your seat reservation time is expired, please select the seats again.");
            clearInterval(countDownInterval);

            setTimeout(() => {
                const showtimeId = $("#showtimeId").val().trim();
                window.location.href = `/Customer/Bookings/SelectSeat?showtimeId=${showtimeId}`; // Redirect to seat selection
            }, 3000);
        } else {
            const minutes = Math.floor(timeLeft / 60000);
            const seconds = Math.floor((timeLeft % 60000) / 1000);
            $("#countdownTimer").text(`${minutes}:${seconds.toString().padStart(2, '0')}`);
        }
    }

    // Start countdown timer
    updateCountdown();
    const countDownInterval = setInterval(updateCountdown, 1000);

    // Initialize Toastr with custom settings (optional)
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
});