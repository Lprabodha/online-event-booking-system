namespace online_event_booking_system.Helper
{
    public static class EmailTemplates
    {
        public static string GetCustomerWelcomeTemplate(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to Star Events</title>
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
            background: white;
            border-radius: 16px;
            padding: 30px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.08);
        }}
        .header {{
            text-align: center;
            margin-bottom: 20px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            color: #4f46e5;
        }}
        .greeting {{
            font-size: 22px;
            font-weight: 600;
            color: #1f2937;
            margin: 10px 0 15px;
        }}
        .content {{
            background: #f9fafb;
            border: 1px solid #e5e7eb;
            border-radius: 12px;
            padding: 18px;
        }}
        .cta {{
            text-align: center;
            margin-top: 24px;
        }}
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 12px 22px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
        }}
        .footer {{
            text-align: center;
            color: #6b7280;
            font-size: 12px;
            margin-top: 20px;
        }}
    </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <div class='logo'>⭐ Star Events</div>
            </div>
            <div class='greeting'>Welcome, {fullName}!</div>
            <div class='content'>
                <p>Thanks for creating your account at Star Events. You're all set to discover and book great events.</p>
                <p>Here are a few tips to get started:</p>
                <ul>
                    <li>Browse events and filter by category or venue</li>
                    <li>Use promo codes at checkout when available</li>
                    <li>Access your tickets and invoices from your profile</li>
                </ul>
            </div>
            <div class='cta'>
                <a href='{"/"}' class='button'>Explore Events</a>
            </div>
            <div class='footer'>
                &copy; {DateTime.Now.Year} Star Events. All rights reserved.
            </div>
        </div>
    </body>
    </html>";
        }
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

        public static string GetTicketConfirmationTemplate(
            string customerName, 
            string eventName, 
            DateTime eventDate, 
            string venueName, 
            string ticketNumber, 
            string qrCodeUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Event Ticket - Star Events</title>
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
        .ticket-info {{
            background: #f7fafc;
            border-left: 4px solid #4299e1;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
        }}
        .ticket-info h3 {{
            color: #2b6cb0;
            margin: 0 0 15px 0;
            font-size: 18px;
        }}
        .ticket-detail {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .ticket-detail:last-child {{
            border-bottom: none;
        }}
        .ticket-detail strong {{
            color: #2d3748;
        }}
        .ticket-detail span {{
            color: #4a5568;
        }}
        .qr-section {{
            text-align: center;
            margin: 30px 0;
            padding: 20px;
            background: #f7fafc;
            border-radius: 10px;
        }}
        .qr-section h3 {{
            color: #2b6cb0;
            margin-bottom: 15px;
        }}
        .qr-code {{
            max-width: 200px;
            height: auto;
            border: 3px solid #4299e1;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }}
        .instructions {{
            background: #e8f5e9;
            border-left: 4px solid #4caf50;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .instructions h4 {{
            color: #1b5e20;
            margin: 0 0 10px 0;
            font-size: 14px;
        }}
        .instructions p {{
            margin: 0;
            font-size: 13px;
            color: #2e7d32;
        }}
        .footer {{
            text-align: center;
            color: rgba(255,255,255,0.8);
            font-size: 12px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🎫 Star Events</div>
            <div class='tagline'>Where Every Event Becomes a Memory</div>
        </div>
        
        <div class='content'>
            <div class='greeting'>Hello {customerName}!</div>
            
            <p>Your ticket has been successfully generated! We're excited to see you at the event.</p>
            
            <div class='ticket-info'>
                <h3>🎟️ Ticket Information</h3>
                <div class='ticket-detail'>
                    <strong>Event:</strong>
                    <span>{eventName}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Date & Time:</strong>
                    <span>{eventDate:dddd, MMMM dd, yyyy 'at' h:mm tt}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Venue:</strong>
                    <span>{venueName}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Ticket Number:</strong>
                    <span>{ticketNumber}</span>
                </div>
            </div>
            
            <div class='qr-section'>
                <h3>📱 Your QR Code</h3>
                <p>Present this QR code at the event entrance for quick check-in:</p>
                <img src='{qrCodeUrl}' alt='Ticket QR Code' class='qr-code'>
            </div>
            
            <div class='instructions'>
                <h4>📋 Important Instructions</h4>
                <p>
                    • Please arrive 15-30 minutes before the event starts<br>
                    • Bring a valid ID for verification<br>
                    • Keep this email and QR code accessible on your phone<br>
                    • The QR code is unique to your ticket and cannot be transferred
                </p>
            </div>
            
            <p style='text-align: center; margin-top: 30px;'>
                <strong>Thank you for choosing Star Events!</strong><br>
                We look forward to providing you with an amazing experience.
            </p>
        </div>
        
        <div class='footer'>
            <p>This is your official ticket confirmation. Please keep this email safe.</p>
            <p>&copy; {DateTime.Now.Year} Star Events. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetMultiTicketBookingTemplate(
            string customerName,
            string eventName,
            DateTime eventDate,
            string venueName,
            string bookingReference,
            List<TicketInfo> tickets,
            decimal subtotal,
            decimal discountAmount,
            decimal total)
        {
            var ticketsHtml = string.Join("", tickets.Select(ticket => $@"
                <div class='ticket-card'>
                    <div class='ticket-header'>
                        <h3>🎫 {ticket.Category}</h3>
                        <span class='ticket-number'>#{ticket.TicketNumber}</span>
                    </div>
                    <div class='ticket-details'>
                        <div class='ticket-info'>
                            <div class='info-row'>
                                <span class='label'>Price:</span>
                                <span class='value'>LKR {ticket.Price:F2}</span>
                            </div>
                            <div class='info-row'>
                                <span class='label'>Description:</span>
                                <span class='value'>{ticket.Description}</span>
                            </div>
                        </div>
                        <div class='qr-section'>
                            <img src='{ticket.QRCodeUrl}' alt='QR Code for {ticket.TicketNumber}' class='qr-code'>
                            <p class='qr-note'>Present this QR code at the entrance</p>
                        </div>
                    </div>
                </div>"));

            var discountRow = discountAmount > 0 ? $@"<div class='summary-row'><span>Discount</span><span>- LKR {discountAmount:F2}</span></div>" : string.Empty;

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Event Tickets - Star Events</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 800px;
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
        .event-info {{
            background: #f7fafc;
            border-left: 4px solid #4299e1;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
        }}
        .event-info h3 {{
            color: #2b6cb0;
            margin: 0 0 15px 0;
            font-size: 18px;
        }}
        .event-detail {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .event-detail:last-child {{
            border-bottom: none;
        }}
        .event-detail strong {{
            color: #2d3748;
        }}
        .event-detail span {{
            color: #4a5568;
        }}
        .tickets-section {{
            margin: 30px 0;
        }}
        .tickets-section h3 {{
            color: #2b6cb0;
            margin-bottom: 20px;
            font-size: 20px;
        }}
        .ticket-card {{
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            margin: 20px 0;
            overflow: hidden;
            background: white;
        }}
        .ticket-header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .ticket-header h3 {{
            margin: 0;
            font-size: 16px;
        }}
        .ticket-number {{
            background: rgba(255,255,255,0.2);
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
        }}
        .ticket-details {{
            padding: 20px;
            display: flex;
            gap: 20px;
            align-items: center;
        }}
        .ticket-info {{
            flex: 1;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            margin: 8px 0;
            padding: 4px 0;
        }}
        .label {{
            font-weight: 600;
            color: #2d3748;
        }}
        .value {{
            color: #4a5568;
        }}
        .qr-section {{
            text-align: center;
            flex-shrink: 0;
        }}
        .qr-code {{
            width: 120px;
            height: 120px;
            border: 3px solid #4299e1;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }}
        .qr-note {{
            font-size: 11px;
            color: #666;
            margin-top: 8px;
        }}
        .instructions {{
            background: #e8f5e9;
            border-left: 4px solid #4caf50;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .instructions h4 {{
            color: #1b5e20;
            margin: 0 0 10px 0;
            font-size: 14px;
        }}
        .instructions p {{
            margin: 0;
            font-size: 13px;
            color: #2e7d32;
        }}
        .footer {{
            text-align: center;
            color: rgba(255,255,255,0.8);
            font-size: 12px;
            margin-top: 20px;
        }}
        .download-section {{
            text-align: center;
            margin: 20px 0;
            padding: 20px;
            background: #f7fafc;
            border-radius: 10px;
        }}
        .download-btn {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            margin: 5px;
        }}
        .order-summary {{
            width: 100%;
            max-width: 360px;
            margin-left: auto;
            background: #f7fafc;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            padding: 16px 20px;
        }}
        .summary-row {{
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-bottom: 1px solid #e2e8f0;
            font-size: 14px;
        }}
        .summary-row:last-child {{
            border-bottom: none;
        }}
        .summary-total {{
            font-weight: 700;
            color: #1f2937;
        }}
        @media (max-width: 600px) {{
            .ticket-details {{
                flex-direction: column;
                text-align: center;
            }}
            .qr-code {{
                width: 100px;
                height: 100px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🎫 Star Events</div>
            <div class='tagline'>Where Every Event Becomes a Memory</div>
        </div>
        
        <div class='content'>
            <div class='greeting'>Hello {customerName}!</div>
            
            <p>Your booking has been confirmed! Here are all your tickets for the event.</p>
            
            <div class='event-info'>
                <h3>🎪 Event Information</h3>
                <div class='event-detail'>
                    <strong>Event:</strong>
                    <span>{eventName}</span>
                </div>
                <div class='event-detail'>
                    <strong>Date & Time:</strong>
                    <span>{eventDate:dddd, MMMM dd, yyyy 'at' h:mm tt}</span>
                </div>
                <div class='event-detail'>
                    <strong>Venue:</strong>
                    <span>{venueName}</span>
                </div>
                <div class='event-detail'>
                    <strong>Booking Reference:</strong>
                    <span>{bookingReference}</span>
                </div>
            </div>
            
            <div class='tickets-section'>
                <h3>🎟️ Your Tickets ({tickets.Count})</h3>
                {ticketsHtml}
            </div>

            <div class='order-summary'>
                <div class='summary-row'><span>Subtotal</span><span>LKR {subtotal:F2}</span></div>
                {discountRow}
                <div class='summary-row summary-total'><span>Total</span><span>LKR {total:F2}</span></div>
            </div>
            
            <div class='download-section'>
                <h4>📱 Mobile-Friendly Options</h4>
                <p>Save this email to your phone or take screenshots of individual QR codes for easy access at the event.</p>
            </div>
            
            <div class='instructions'>
                <h4>📋 Important Instructions</h4>
                <p>
                    • Please arrive 15-30 minutes before the event starts<br>
                    • Bring a valid ID for verification<br>
                    • Each QR code is unique and cannot be transferred<br>
                    • Keep this email accessible on your phone<br>
                    • Individual ticket emails have also been sent for your convenience
                </p>
            </div>
            
            <p style='text-align: center; margin-top: 30px;'>
                <strong>Thank you for choosing Star Events!</strong><br>
                We look forward to providing you with an amazing experience.
            </p>
        </div>
        
        <div class='footer'>
            <p>This is your official booking confirmation. Please keep this email safe.</p>
            <p>&copy; {DateTime.Now.Year} Star Events. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    public class TicketInfo
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string QRCodeUrl { get; set; } = string.Empty;
    }
}
