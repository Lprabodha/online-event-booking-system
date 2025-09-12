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
    }
}
