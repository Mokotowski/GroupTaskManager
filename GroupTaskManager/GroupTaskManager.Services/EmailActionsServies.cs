using GroupTaskManager.GroupTaskManager.Database;
using GroupTaskManager.GroupTaskManager.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Net;
using System.Text.Encodings.Web;
using System.Text;

namespace GroupTaskManager.GroupTaskManager.Services
{
    public class EmailActionsServies : ISendEmail, IFunctionsFromEmail
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly ILogger<EmailActionsServies> _logger;
        private readonly IConfiguration _configuration;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EmailActionsServies(UserManager<UserModel> userManager, ILogger<EmailActionsServies> logger, IConfiguration configuration, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendConfirmedEmail(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                _logger.LogError("The email address is null or empty.");
                return;
            }

            UserModel User = await _userManager.FindByEmailAsync(Email);
            if (User == null)
            {
                _logger.LogWarning($"The email address '{Email}' does not exist.");
                return;
            }

            try
            {
                var mailAddress = new MailAddress(Email);

                _logger.LogInformation("Starting to send email to {Email}", Email);

                var smtpSettings = _configuration.GetSection("SmtpSettings");

                var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                };

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(User);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var httpContext = _httpContextAccessor.HttpContext;

                string callbackUrl = _linkGenerator.GetUriByAction(
                    httpContext: httpContext,
                    action: "ConfirmEmail",
                    controller: "Account",
                    values: new { code = code, Email = Email },
                    scheme: httpContext.Request.Scheme);

                string message = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            color: #333;
                            line-height: 1.6;
                            padding: 20px;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background: #fff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                        }}
                        h1 {{
                            color: #333;
                        }}
                        p {{
                            margin-bottom: 20px;
                        }}
                        a.button {{
                            display: inline-block;
                            background-color: #007BFF;
                            color: #fff;
                            padding: 10px 20px;
                            text-decoration: none;
                            border-radius: 5px;
                            font-weight: bold;
                        }}
                        .footer {{
                            margin-top: 20px;
                            text-align: center;
                            font-size: 0.9em;
                            color: #777;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Welcome to Group Task Manager!</h1>
                        <p>Thank you for creating an account. To complete the registration process, please confirm your email address by clicking the button below.</p>
                        <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Confirm Your Email</a></p>
                        <p>If you did not create an account on our site, you may ignore this message.</p>
                        <div class='footer'>
                            <p>&copy; 2024 Group Task Manager. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";


                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"], smtpSettings["SenderName"]),
                    Subject = "Confirm your email",
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(mailAddress);
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", Email);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid email address format: {Email}", Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to {Email}", Email);
            }
        }

        public async Task ConfirmedEmail(string code, string Email)
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            UserModel user = await _userManager.FindByEmailAsync(Email);
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email: {Email} was successfully confirmed", Email);
            }
            else
            {
                _logger.LogError("Failed to confirm email: {Email}", Email);
            }
        }




        public async Task SendResetPasswordEmail(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                _logger.LogError("The email address is null or empty.");
                return;
            }

            UserModel user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                _logger.LogWarning($"The email address '{Email}' does not exist.");
                return;
            }

            try
            {
                var mailAddress = new MailAddress(Email);

                _logger.LogInformation("Starting to send reset password email to {Email}", Email);

                var smtpSettings = _configuration.GetSection("SmtpSettings");

                var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                };

                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var httpContext = _httpContextAccessor.HttpContext;

                string callbackUrl = _linkGenerator.GetUriByAction(
                    httpContext: httpContext,
                    action: "ResetPassword",
                    controller: "Account",
                    values: new { code = code, email = Email },
                    scheme: httpContext.Request.Scheme);

                string message = $@"
                    <!DOCTYPE html>
                    <html lang='pl'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                color: #333;
                                line-height: 1.6;
                                padding: 20px;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 0 auto;
                                background: #fff;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            }}
                            h1 {{
                                color: #333;
                            }}
                            p {{
                                margin-bottom: 20px;
                            }}
                            a.button {{
                                display: inline-block;
                                background-color: #007BFF;
                                color: #fff;
                                padding: 10px 20px;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                            }}
                            .footer {{
                                margin-top: 20px;
                                text-align: center;
                                font-size: 0.9em;
                                color: #777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Password Reset</h1>
                            <p>We received a request to reset the password for your account. To reset your password, please click the button below.</p>
                            <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Reset Password</a></p>
                            <p>If you did not initiate this request, please ignore this message or contact our support team.</p>
                            <div class='footer'>
                                <p>&copy; 2024 Group Task Manager. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                    ";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"], smtpSettings["SenderName"]),
                    Subject = "Resetowanie hasła",
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(mailAddress);
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Reset password email sent successfully to {Email}", Email);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid email address format: {Email}", Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending reset password email to {Email}", Email);
            }
        }


        public async Task ResetPassword(string code, string Email, string password)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("All fields (code, email, password) must be filled in.");
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                _logger.LogWarning($"Attempting to reset a password for a non-existent user: {Email}");
            }

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ResetPasswordAsync(user, decodedCode, password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"The password for user {Email} has been successfully reset.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Password reset error: {error.Description}");
                }
            }
        }
    }
}
