namespace Utility
{
    public static class HtmlContent
    {
        public static string GetTicketEmailHtml(string ticketId, byte[] barcodeBytes)
        {
            return $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9;">
                        <h1 style="color: #d32f2f; text-align: center;">🎬 Your CineMax Ticket</h1>
                        <p style="font-size: 16px; color: #333; text-align: center;">
                            Thank you for booking with <strong>CineMax</strong>! Here is your ticket:
                        </p>

                        <div style="text-align: center; margin: 20px 0;">
                            <h2 style="color: #444;">🎟 Ticket #{ticketId}</h2>
                            <p style="font-size: 14px; color: #666;">Show this barcode at the theater entrance:</p>
                            <img src="data:image/png;base64,{Convert.ToBase64String(barcodeBytes)}" 
                                 style="border: 1px solid #ddd; padding: 10px; border-radius: 5px;" 
                                 alt="Ticket Barcode" />
                        </div>

                        <hr style="border: none; border-top: 1px solid #ddd; margin: 20px 0;" />

                        <p style="text-align: center; font-size: 14px; color: #666;">
                            📍 Visit our website for more details: 
                            <a href="https://www.cinemax.com" style="color: #d32f2f; text-decoration: none;">CineMax.com</a>
                        </p>

                        <p style="text-align: center; font-size: 14px; color: #666;">
                            🎥 Enjoy your movie! – <strong>CineMax Team</strong>
                        </p>
                    </div>
                    """;
        }
    }
}
