using HRMS.Data;
using HRMS.Models;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

public class AadhaarController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;

    public AadhaarController(IHttpClientFactory httpClientFactory,
                             ApplicationDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
    }

    public IActionResult Verify(int employeeId)
    {
        ViewBag.EmployeeId = employeeId;
        return View();
    }

    // GENERATE OTP
    [HttpPost]
    public async Task<IActionResult> SendOtp([FromBody] EmployeeEditVm model)
    {
        string aadhaarNumber = model.AadhaarNumber;
        //int employeeId = model.Id;   // or EmployeeId depending on your VM
        int employeeId = HttpContext.Session.GetInt32("EmployeeId").Value;
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
        client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

        var authResponse = await client.PostAsync("https://api.sandbox.co.in/authenticate", null);
        var authResult = await authResponse.Content.ReadAsStringAsync();

        dynamic authData = JsonConvert.DeserializeObject(authResult);

        string token = authData.data.access_token;

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", token);
        client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
        client.DefaultRequestHeaders.Add("x-api-version", "1.0");

        var body = new Dictionary<string, object>
        {
            { "@entity", "in.co.sandbox.kyc.aadhaar.okyc.otp.request" },
            { "aadhaar_number", aadhaarNumber },
            { "consent", "Y" },
            { "reason", "Aadhaar verify" }
        };

        var json = JsonConvert.SerializeObject(body);

        var response = await client.PostAsync(
            "https://api.sandbox.co.in/kyc/aadhaar/okyc/otp",
            new StringContent(json, Encoding.UTF8, "application/json"));

        var result = await response.Content.ReadAsStringAsync();

        dynamic data = JsonConvert.DeserializeObject(result);

        //ViewBag.ReferenceId = data.data.reference_id;
        //ViewBag.Aadhaar = aadhaarNumber;
        //ViewBag.EmployeeId = employeeId;

        //return View("Verify");

        string referenceId = data.data.reference_id;

        return Json(new
        {
            success = true,
            referenceId = referenceId
        });
    }

    //// VERIFY OTP
    [HttpPost]
    public async Task<IActionResult> VerifyOtp([FromBody] EmployeeEditVm model)
    {
        try
        {
            string referenceId = model.ReferenceId;
            string otp = model.Otp;
            string aadhaar = model.AadhaarNumber;
            int employeeId = model.Id;

            var client = _httpClientFactory.CreateClient();

            // ---------------- AUTHENTICATE ----------------
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

            var authResponse = await client.PostAsync("https://api.sandbox.co.in/authenticate", null);
            var authResult = await authResponse.Content.ReadAsStringAsync();

            dynamic authData = JsonConvert.DeserializeObject(authResult);
            string token = authData?.data?.access_token;

            // ---------------- VERIFY OTP ----------------
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-version", "1.0");

            var body = new Dictionary<string, object>
        {
            { "@entity", "in.co.sandbox.kyc.aadhaar.okyc.request" },
            { "reference_id", referenceId },
            { "otp", otp }
        };

            var json = JsonConvert.SerializeObject(body);

            var response = await client.PostAsync(
                "https://api.sandbox.co.in/kyc/aadhaar/okyc/otp/verify",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            dynamic data = JsonConvert.DeserializeObject(result);

            if (data?.data?.status == "VALID")
            {
                string name = data.data.name;
                string dob = data.data.date_of_birth;
                string email = data.data.email_hash;
                string address = data.data.full_address;

                string district = data.data.address.district;
                string state = data.data.address.state;
                string pincode = data.data.address.pincode;

                // -------- SAVE KYC TABLE --------
                var kyc = new AadhaarKyc
                {
                    AadhaarNumber = aadhaar,
                    Name = name,
                    DateOfBirth = dob,
                    Address = address,
                    District = district,
                    State = state,
                    Pincode = pincode,
                    ReferenceId = referenceId
                };

                _context.AadhaarKycs.Add(kyc);

                // -------- UPDATE EMPLOYEE --------
                var employee = await _context.Employees.FindAsync(employeeId);

                if (employee != null)
                {
                    employee.AadhaarNumber = aadhaar;
                    employee.Name = name;

                    DateTime parsedDob;
                    if (DateTime.TryParse(dob, out parsedDob))
                    {
                        employee.DOB_Date = parsedDob;
                    }

                    employee.Email = email;
                    employee.PermanentAddress = address;
                    employee.District = district;
                    employee.State = state;
                    employee.Pincode = pincode;

                    employee.AadhaarVerified = true;
                    employee.AadhaarVerifiedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Aadhaar Verified Successfully"
                });
            }

            return Json(new
            {
                success = false,
                message = "OTP Verification Failed"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
    //[HttpPost]
    //public async Task<IActionResult> VerifyOtp(string referenceId,
    //                                           string otp,
    //                                           string aadhaar,
    //                                           int employeeId)
    //{
    //    
    //[HttpPost]
    //public async Task<IActionResult> VerifyOtp([FromBody] EmployeeEditVm model)
    //{
    //    string referenceId = model.ReferenceId;
    //    string otp = model.Otp;
    //    string aadhaar = model.AadhaarNumber;
    //    int employeeId = model.Id;
    //    var client = _httpClientFactory.CreateClient();
    //    client.DefaultRequestHeaders.Clear();
    //    client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
    //    client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

    //    var authResponse = await client.PostAsync("https://api.sandbox.co.in/authenticate", null);
    //    var authResult = await authResponse.Content.ReadAsStringAsync();

    //    dynamic authData = JsonConvert.DeserializeObject(authResult);

    //    string token = authData.data.access_token;

    //    client.DefaultRequestHeaders.Clear();
    //    client.DefaultRequestHeaders.Add("Authorization", token);
    //    client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
    //    client.DefaultRequestHeaders.Add("x-api-version", "1.0");

    //    var body = new Dictionary<string, object>
    //    {
    //        { "@entity", "in.co.sandbox.kyc.aadhaar.okyc.request" },
    //        { "reference_id", referenceId },
    //        { "otp", otp }
    //    };

    //    var json = JsonConvert.SerializeObject(body);

    //    var response = await client.PostAsync(
    //        "https://api.sandbox.co.in/kyc/aadhaar/okyc/otp/verify",
    //        new StringContent(json, Encoding.UTF8, "application/json"));

    //    var result = await response.Content.ReadAsStringAsync();

    //    dynamic data = JsonConvert.DeserializeObject(result);

    //    if (data?.data?.status == "VALID")
    //    {
    //        string name = data.data.name;
    //        string dob = data.data.date_of_birth;
    //        string email = data.data.email_hash;
    //        string address = data.data.full_address;

    //        string district = data.data.address.district;
    //        string state = data.data.address.state;
    //        string pincode = data.data.address.pincode;

    //        // SAVE KYC TABLE
    //        var kyc = new AadhaarKyc
    //        {
    //            AadhaarNumber = aadhaar,
    //            Name = name,
    //            DateOfBirth = dob,
    //            Address = address,
    //            District = district,
    //            State = state,
    //            Pincode = pincode,
    //            ReferenceId = referenceId
    //        };

    //        _context.AadhaarKycs.Add(kyc);

    //        // UPDATE EMPLOYEE TABLE
    //        var employee = await _context.Employees.FindAsync(employeeId);

    //        if (employee != null)
    //        {
    //            employee.AadhaarNumber = aadhaar;
    //            employee.Name = name;
    //            //employee.DOB_Date = dob;
    //            DateTime parsedDob;

    //            if (DateTime.TryParse(dob, out parsedDob))
    //            {
    //                employee.DOB_Date = parsedDob;
    //            }
    //            employee.Email = email;
    //            employee.PermanentAddress = address;
    //            employee.District = district;
    //            employee.State = state;
    //            employee.Pincode = pincode;
    //            employee.AadhaarVerified = true;
    //            employee.AadhaarVerifiedDate = DateTime.Now;
    //        }



    //        await _context.SaveChangesAsync();

    //        ViewBag.Kyc = kyc;
    //    }

    //    //return Json(new { success = true });


    //    return Json(new { success = false });

    //}

    [HttpPost]
    public async Task<IActionResult> VerifyPan([FromBody] EmployeeEditVm model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            string panNumber = model.PanNumber;
            string name = model.Name;
            DateTime dob = model.DOB_Date.Value;

            // Authenticate
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

            var authResponse = await client.PostAsync("https://api.sandbox.co.in/authenticate", null);
            var authResult = await authResponse.Content.ReadAsStringAsync();

            dynamic authData = JsonConvert.DeserializeObject(authResult);
            string token = authData.data.access_token;

            // Format DOB
            // string dobFormatted = dob.ToString("dd/MM/yyyy");
           // DateTime dob = model.DOB_Date.Value;

            string dobFormatted = dob.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            // PAN Verify
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-version", "1.0");

            var body = new Dictionary<string, object>
        {
            { "@entity", "in.co.sandbox.kyc.pan_verification.request" },
            { "pan", panNumber },
            { "name_as_per_pan", name },
            { "date_of_birth", dobFormatted },
            { "consent", "Y" },
            { "reason", "Pan verification" }
        };

            var json = JsonConvert.SerializeObject(body);

            var response = await client.PostAsync(
                "https://api.sandbox.co.in/kyc/pan/verify",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            dynamic data = JsonConvert.DeserializeObject(result);

            if (data?.data?.status == "valid")
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(x => x.PanNumber == panNumber);

                if (employee != null)
                {
                    employee.PanVerified = true;
                    employee.PanVerifiedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    [HttpPost]
    public async Task<IActionResult> VerifyBank([FromBody] EmployeeEditVm model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // STEP 1 — Authenticate
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

            var authResponse = await client.PostAsync(
                "https://api.sandbox.co.in/authenticate", null);

            var authResult = await authResponse.Content.ReadAsStringAsync();

            dynamic authData = JsonConvert.DeserializeObject(authResult);
            string token = authData.data.access_token;

            // STEP 2 — Verify Bank
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
            client.DefaultRequestHeaders.Add("x-api-version", "1.0");
            client.DefaultRequestHeaders.Add("x-accept-cache", "true");

            string url =
                $"https://api.sandbox.co.in/bank/{model.IFSC}/accounts/{model.AccountNumber}/verify";

            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = result });
            }

            dynamic data = JsonConvert.DeserializeObject(result);

            if (data.data.account_exists == true)
            {
                var employee = await _context.Employees.FindAsync(model.Id);

                if (employee != null)
                {
                    employee.AccountNumber = model.AccountNumber;
                    employee.IFSC = model.IFSC;
                    employee.AccountHolderName = data.data.name_at_bank;
                    employee.BankVerified = true;
                    employee.BankVerifiedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    name = data.data.name_at_bank
                });
            }

            return Json(new { success = false, message = "Account not valid" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    //    [HttpPost]
    //    public async Task<IActionResult> VerifyPan(string panNumber, string name, string dob)
    //    {
    //        try
    //        {
    //            var client = _httpClientFactory.CreateClient();

    //            // Step 1: Authenticate
    //            client.DefaultRequestHeaders.Clear();
    //            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
    //            client.DefaultRequestHeaders.Add("x-api-secret", "secret_live_bea7b5d94c1b487d803926a63195b4f3");

    //            var authResponse = await client.PostAsync("https://api.sandbox.co.in/authenticate", null);
    //            var authResult = await authResponse.Content.ReadAsStringAsync();

    //            dynamic authData = JsonConvert.DeserializeObject(authResult);

    //            string token = authData.data.access_token;

    //            // Step 2: PAN Verify
    //            client.DefaultRequestHeaders.Clear();
    //            client.DefaultRequestHeaders.Add("Authorization", token);
    //            client.DefaultRequestHeaders.Add("x-api-key", "key_live_e8436729af3a4de8ac4fc4f2cdeba0fb");
    //            client.DefaultRequestHeaders.Add("x-api-version", "1.0");

    //            DateTime dobDate = DateTime.Parse(dob, CultureInfo.InvariantCulture);

    //            string dobFormatted = dobDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    //            var body = new Dictionary<string, object>
    //{
    //    { "@entity", "in.co.sandbox.kyc.pan_verification.request" },
    //    { "pan", panNumber },
    //    { "name_as_per_pan", name },
    //    { "date_of_birth", dobFormatted },
    //    { "consent", "Y" },
    //    { "reason", "Pan verification" }
    //};

    //            var json = JsonConvert.SerializeObject(body);

    //            var response = await client.PostAsync(
    //                "https://api.sandbox.co.in/kyc/pan/verify",
    //                new StringContent(json, Encoding.UTF8, "application/json"));

    //            var result = await response.Content.ReadAsStringAsync();

    //            dynamic data = JsonConvert.DeserializeObject(result);

    //            if (data?.data?.status == "valid")
    //            {
    //                ViewBag.PanNumber = panNumber;
    //                ViewBag.PanStatus = "Verified";
    //            }
    //            else
    //            {
    //                ViewBag.PanStatus = "Verification Failed";
    //            }

    //            return View("Verify");
    //        }
    //        catch (Exception ex)
    //        {
    //            ViewBag.PanStatus = ex.Message;
    //            return View("Verify");
    //        }
    //    }
}