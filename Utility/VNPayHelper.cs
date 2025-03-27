using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Utility
{
    public static class VNPayHelper
    {
        public static string CreatePaymentUrl(double amount, string orderInfo, string ipAddress, string returnUrl, string tmnCode, string hashSecret, string baseUrl)
        {
            var vnpParams = new SortedList<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((int)(amount * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", DateTime.Now.Ticks.ToString()},
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(5).ToString("yyyyMMddHHmmss") }
            };

            // Generate Secure Hash
            var queryString = string.Join("&", vnpParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            var secureHash = HmacSha512(hashSecret, queryString);

            // Append hash to the URL
            var paymentUrl = $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
            return paymentUrl;
        }

        public static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }


        public static string ComputeHmacSha512(string key, string data)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
            }
        }

        public static string ComputeSha512Hash(string input)
        {
            using (var sha512 = SHA512.Create())
            {
                byte[] bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public static bool ValidateResponse(SortedList<string, string> responseData, string hashSecret)
        {
            string secureHash = responseData["vnp_SecureHash"];
            responseData.Remove("vnp_SecureHash");

            var queryString = string.Join("&", responseData.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            var signData = $"{hashSecret}{queryString}";
            var checkSum = ComputeSha512Hash(signData);

            return checkSum.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
