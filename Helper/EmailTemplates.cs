namespace online_event_booking_system.Helper
{
    public static class EmailTemplates
    {
        public static string GetOrganizerAccountCreationTemplate(string fullName, string username, string password)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #1a237e;
            color: #ffffff;
            padding: 24px;
            text-align: center;
        }}
        .header img {{
            max-width: 150px;
            height: auto;
            margin-bottom: 10px;
        }}
        .content {{
            padding: 32px;
            line-height: 1.6;
            text-align: center;
        }}
        .credentials-box {{
            background-color: #e8f5e9;
            border-left: 4px solid #4caf50;
            margin: 24px 0;
            padding: 16px;
            border-radius: 4px;
        }}
        .credentials-box p {{
            margin: 0;
            color: #1b5e20;
        }}
        .credentials-box strong {{
            color: #1b5e20;
            display: block;
            font-size: 1.2em;
            margin-top: 8px;
        }}
        .button {{
            display: inline-block;
            background-color: #4caf50;
            color: #ffffff;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 4px;
            margin-top: 24px;
        }}
        .footer {{
            text-align: center;
            padding: 24px;
            color: #999;
            font-size: 12px;
            border-top: 1px solid #eee;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Welcome to STAR EVENTS Event Booking System!</h2>
        </div>
        <div class=""content"">
            <p>Hello {fullName},</p>
            <p>Your new organizer account has been successfully created. You can now log in using the credentials below. For security, please change your password upon your first login.</p>
            
            <div class=""credentials-box"">
                <h4>Your Credentials:</h4>
                <p><strong>Username:</strong> {username}</p>
                <p><strong>Password:</strong> {password}</p>
            </div>
            
            <p>Thank you,<br>Star Events Team</p>
        </div>
        <div class=""footer"">
            &copy; {DateTime.Now.Year} Star Events Booking System. All rights reserved.
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPasswordResetTemplate(string userName, string callbackUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password - Star Events</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f8f9fa;
        }}
        .container {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 20px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            color: white;
            margin-bottom: 10px;
        }}
        .tagline {{
            color: rgba(255,255,255,0.8);
            font-size: 14px;
        }}
        .content {{
            background: white;
            border-radius: 15px;
            padding: 30px;
            margin-bottom: 20px;
        }}
        .greeting {{
            font-size: 24px;
            font-weight: 600;
            color: #2d3748;
            margin-bottom: 20px;
        }}
        .message {{
            font-size: 16px;
            color: #4a5568;
            margin-bottom: 30px;
            line-height: 1.7;
        }}
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 10px;
            font-weight: 600;
            font-size: 16px;
            text-align: center;
            margin: 20px 0;
            transition: transform 0.2s ease;
        }}
        .button:hover {{
            transform: translateY(-2px);
        }}
        .security-note {{
            background: #f7fafc;
            border-left: 4px solid #4299e1;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .security-note h4 {{
            color: #2b6cb0;
            margin: 0 0 10px 0;
            font-size: 14px;
        }}
        .security-note p {{
            margin: 0;
            font-size: 13px;
            color: #4a5568;
        }}
        .footer {{
            text-align: center;
            color: rgba(255,255,255,0.8);
            font-size: 12px;
            margin-top: 20px;
        }}
        .link {{
            color: #4299e1;
            word-break: break-all;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>⭐ Star Events</div>
            <div class='tagline'>Where Every Event Becomes a Memory</div>
        </div>
        
        <div class='content'>
            <div class='greeting'>Hello {userName}!</div>
            
            <div class='message'>
                We received a request to reset your password for your Star Events account. 
                If you made this request, click the button below to reset your password:
            </div>
            
            <div style='text-align: center;'>
                <a href='{callbackUrl}' class='button'>Reset My Password</a>
            </div>
            
            <div class='security-note'>
                <h4>🔒 Security Information</h4>
                <p>
                    This password reset link will expire in 24 hours for security reasons. 
                    If you didn't request this password reset, please ignore this email and your password will remain unchanged.
                </p>
            </div>
            
            <div class='message'>
                If the button above doesn't work, you can copy and paste this link into your browser:
                <br><a href='{callbackUrl}' class='link'>{callbackUrl}</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>This email was sent from Star Events. If you have any questions, please contact our support team.</p>
            <p>&copy; {DateTime.Now.Year} Star Events. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
