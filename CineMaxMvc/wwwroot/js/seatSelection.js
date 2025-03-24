$(document).ready(function () {
    let selectedSeats = [];
    let totalAmount = 0;
    let selectedConcessions = [];

    // -------------  SELECT SEAT --------------
    $(".seat.available").on("click", function () {
        const $seat = $(this);
        const seatId = $seat.data("seat-id");
        const row = $seat.data("row");
        const seatNumber = $seat.data("seat-number");
        const price = parseInt($seat.data("price"));

        const index = selectedSeats.findIndex(s => s.id === seatId);

        if (index > -1) {
            // Bỏ chọn
            selectedSeats.splice(index, 1);
            $seat.removeClass("selected");
        } else {
            // Chọn ghế
            selectedSeats.push({ id: seatId, row, number: seatNumber, price });
            $seat.addClass("selected");
        }

        updateUI();
    });

    // ------------- UPDATE SELECTED SEATS LIST AND UI --------------
    function updateSelectedSeatsList() {
        const $listContainer = $("#selectedSeatsList").empty();
        const $noSeatsMessage = $("#noSeatsMessage");

        if (selectedSeats.length === 0) {
            $noSeatsMessage.show();
            return;
        }

        $noSeatsMessage.hide();

        // sort the seat by row and number
        selectedSeats.sort((a, b) => a.row.localeCompare(b.row) || a.number - b.number);

        // add selected seats to the list of selected seats in ui
        selectedSeats.forEach(seat => {
            const seatItem = `
                            <div class="selected-seat-item">
                                <span>${seat.row}${seat.number}</span>
                                <span>${Math.floor(seat.price / 1000) + "K"}</span>
                                <i class="fas fa-times remove-seat" data-seat-id="${seat.id}"></i>
                            </div>`;
            $listContainer.append(seatItem);
        });
    }

    // update total price
    function updateTotalPrice() {
        totalAmount = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);
        $("#totalPrice").text(formatCurrency(totalAmount));
        $("#totalAmountInput").val(totalAmount);
    }

    // update state of continue button
    function updateContinueButton() {
        $("#continueButton").prop("disabled", selectedSeats.length === 0);
    }

    // update ui when unselect a seat
    function updateUI() {
        updateSelectedSeatsList();
        updateTotalPrice();
        updateContinueButton();
    }

    // handle remove selected seat
    $(document).on("click", ".remove-seat", function () {
        const seatId = $(this).data("seat-id");
        selectedSeats = selectedSeats.filter(s => s.id !== seatId);

        // Bỏ chọn ghế trên giao diện
        $(`[data-seat-id="${seatId}"]`).removeClass("selected");

        updateUI();
    });

    // handle choose concession
    window.changeQuantity = function (itemId, delta) {
        const inputElement = $(`#quantity-${itemId}`);
        let currentValue = parseInt(inputElement.val()) || 0;
        let newValue = Math.max(0, currentValue + delta);
        inputElement.val(newValue);

        updateSelectedConcessions(itemId, newValue);
    };

    // update selected concessions
    function updateSelectedConcessions(itemId, quantity) {
        // get the detail of the concession by Id
        const concessionDetails = $(`#concession-${itemId}`).data();
        selectedConcessions = selectedConcessions.filter(item => item.id !== itemId);

        if (quantity > 0) {
            selectedConcessions.push({
                id: itemId,
                name: concessionDetails.name,
                price: concessionDetails.price,
                quantity: quantity,
                subtotal: concessionDetails.price * quantity
            });
        }

        updateConcessionsUI();
    }

    function updateConcessionsUI() {
        $("#selectedConcessionsInput").val(JSON.stringify(selectedConcessions));
        updateBookingSummary();
    }

    function updateBookingSummary() {
        const concessionsTotal = selectedConcessions.reduce((sum, item) => sum + item.subtotal, 0);
        const seatsTotal = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);
        totalAmount = seatsTotal + concessionsTotal;

        $("#totalPrice").text(formatCurrency(totalAmount));
        $("#totalAmountInput").val(totalAmount);

        if (selectedConcessions.length > 0) {
            if ($("#concessionsSummary").length === 0) {
                $(`<div id="concessionsSummary" class="mt-3">
                                <h6>Thức ăn & Đồ uống</h6>
                                <div id="concessionsList"></div>
                                <div class="d-flex justify-content-between mt-2">
                                    <span>Tổng phụ:</span>
                                    <span id="concessionsSubtotal">${formatCurrency(concessionsTotal)}</span>
                                </div>
                            </div>`).insertBefore(".order-summary");
            } else {
                $("#concessionsSubtotal").text(formatCurrency(concessionsTotal));
            }

            const $concessionsList = $("#concessionsList").empty();
            selectedConcessions.forEach(item => {
                $concessionsList.append(`
                                <div class="d-flex justify-content-between concession-summary-item">
                                    <span>${item.name} x${item.quantity}</span>
                                    <span>${formatCurrency(item.subtotal)}</span>
                                </div>
                            `);
            });
        } else {
            $("#concessionsSummary").remove();
        }
    }


    // Chuyển đổi số thành tiền VND
    function formatCurrency(amount) {
        return amount.toLocaleString("vi-VN") + " đ";
    }

    // Xử lý khi nhấn "Tiếp tục"
    $("#continueButton").on("click", function () {
        if (selectedSeats.length === 0) return;

        $("#selectedSeatsInput").val(JSON.stringify(selectedSeats));
        $("#selectedConcessionsInput").val(JSON.stringify(selectedConcessions));
        $("#bookingForm").submit();
    });
});
