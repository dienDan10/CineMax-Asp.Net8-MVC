$(document).ready(function () {
    $("#proceedPayment").on("click", function (e) {
        e.preventDefault();

        // Get form values
        const paymentData = {
            fullName: $("#fullName").val().trim(),
            phone: $("#phoneNumber").val().trim(),
            email: $("#email").val().trim(),
            paymentMethod: $("input[name='paymentMethod']:checked").val()
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