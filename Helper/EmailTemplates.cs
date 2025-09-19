namespace online_event_booking_system.Helper
{
    public static class EmailTemplates
    {
        public static string GetAdminWeeklyReportTemplate(DateTime weekStart, DateTime weekEnd, int newUsers, int eventsCreated, int ticketsSold, decimal revenue, List<(string Title, int Tickets, decimal Sales)> topEvents, List<(string Organizer, decimal Sales)> topOrganizers)
        {
            var eventRows = string.Join("", topEvents.Select(e => $"<tr><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb'>{e.Title}</td><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:center'>{e.Tickets}</td><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:right'>LKR {e.Sales:N2}</td></tr>"));
            if (string.IsNullOrEmpty(eventRows)) eventRows = "<tr><td colspan='3' style='padding:12px;text-align:center;color:#6b7280'>No event sales this week</td></tr>";
            var orgRows = string.Join("", topOrganizers.Select(o => $"<tr><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb'>{o.Organizer}</td><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:right'>LKR {o.Sales:N2}</td></tr>"));
            if (string.IsNullOrEmpty(orgRows)) orgRows = "<tr><td colspan='2' style='padding:12px;text-align:center;color:#6b7280'>No organizer sales this week</td></tr>";
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Weekly Report {weekStart:MMM dd} - {weekEnd:MMM dd}</title>
    <style>
        body {{ margin:0; padding:0; background:#f8f9fa; color:#1f2937; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; }}
        .wrap {{ padding:24px 12px; }}
        .card {{ max-width:800px; margin:0 auto; background:#ffffff; border:1px solid #e5e7eb; border-radius:16px; overflow:hidden; box-shadow:0 10px 30px rgba(0,0,0,.06); }}
        .hdr {{ background:linear-gradient(135deg,#4f46e5,#06b6d4); padding:24px; text-align:center; font-weight:800; font-size:22px; color:#fff; }}
        .content {{ padding:22px 20px; line-height:1.6; }}
        .kpis {{ display:flex; gap:10px; flex-wrap:wrap; }}
        .kpi {{ flex:1 1 180px; background:#f8fafc; border:1px solid #e5e7eb; border-radius:12px; padding:12px; }}
        .kpi h4 {{ margin:0 0 6px; color:#6b7280; font-size:12px; letter-spacing:.3px; }}
        .kpi p {{ margin:0; font-weight:800; font-size:18px; color:#111827; }}
        .table {{ width:100%; background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; margin-top:14px; overflow:hidden; }}
        .footer {{ text-align:center; color:#6b7280; font-size:12px; padding:0 0 22px; }}
    </style>
    </head>
    <body>
        <div class='wrap'>
            <div class='card'>
                <div class='hdr'>⭐ Star Events — Weekly Report ({weekStart:MMM dd} - {weekEnd:MMM dd})</div>
                <div class='content'>
                    <div class='kpis'>
                        <div class='kpi'><h4>New Users</h4><p>{newUsers}</p></div>
                        <div class='kpi'><h4>Events Created</h4><p>{eventsCreated}</p></div>
                        <div class='kpi'><h4>Tickets Sold</h4><p>{ticketsSold}</p></div>
                        <div class='kpi'><h4>Gross Revenue</h4><p>LKR {revenue:N2}</p></div>
                    </div>

                    <h3 style='margin:18px 0 8px'>Top Events</h3>
                    <div class='table'>
                        <table style='width:100%; border-collapse:collapse;'>
                            <thead><tr><th style='text-align:left;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Event</th><th style='text-align:center;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Tickets</th><th style='text-align:right;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Sales</th></tr></thead>
                            <tbody>{eventRows}</tbody>
                        </table>
                    </div>

                    <h3 style='margin:18px 0 8px'>Top Organizers</h3>
                    <div class='table'>
                        <table style='width:100%; border-collapse:collapse;'>
                            <thead><tr><th style='text-align:left;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Organizer</th><th style='text-align:right;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Sales</th></tr></thead>
                            <tbody>{orgRows}</tbody>
                        </table>
                    </div>
                </div>
                <div class='footer'>© {DateTime.Now.Year} Star Events</div>
            </div>
        </div>
    </body>
    </html>";
        }
        public static string GetOrganizerDailySummaryTemplate(string organizerName, DateTime date, int eventsCount, int ticketsSold, decimal revenue, List<(string Title, int Tickets, decimal Sales)> topEvents)
        {
            var rows = string.Join("", topEvents.Select(e => $"<tr><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb'>{e.Title}</td><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:center'>{e.Tickets}</td><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:right'>LKR {e.Sales:N2}</td></tr>"));
            if (string.IsNullOrEmpty(rows))
            {
                rows = "<tr><td colspan='3' style='padding:12px;text-align:center;color:#6b7280'>No sales today</td></tr>";
            }
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Daily Summary - {date:MMM dd, yyyy}</title>
    <style>
        body {{ margin:0; padding:0; background:#f8f9fa; color:#1f2937; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; }}
        .wrap {{ padding:24px 12px; }}
        .card {{ max-width:720px; margin:0 auto; background:#ffffff; border:1px solid #e5e7eb; border-radius:16px; overflow:hidden; box-shadow:0 10px 30px rgba(0,0,0,.06); }}
        .hdr {{ background:linear-gradient(135deg,#6366f1,#06b6d4); padding:24px; text-align:center; font-weight:800; font-size:22px; color:#fff; }}
        .content {{ padding:22px 20px; line-height:1.6; }}
        .kpis {{ display:flex; gap:10px; flex-wrap:wrap; }}
        .kpi {{ flex:1 1 180px; background:#f8fafc; border:1px solid #e5e7eb; border-radius:12px; padding:12px; }}
        .kpi h4 {{ margin:0 0 6px; color:#6b7280; font-size:12px; letter-spacing:.3px; }}
        .kpi p {{ margin:0; font-weight:800; font-size:18px; color:#111827; }}
        .table {{ width:100%; background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; margin-top:14px; overflow:hidden; }}
        .footer {{ text-align:center; color:#6b7280; font-size:12px; padding:0 0 22px; }}
    </style>
    </head>
    <body>
        <div class='wrap'>
            <div class='card'>
                <div class='hdr'>⭐ Star Events — Daily Summary</div>
                <div class='content'>
                    <p style='margin:0 0 10px;color:#374151'>Hi {organizerName}, here is your summary for {date:MMMM dd, yyyy}.</p>
                    <div class='kpis'>
                        <div class='kpi'><h4>Active Events</h4><p>{eventsCount}</p></div>
                        <div class='kpi'><h4>Tickets Sold</h4><p>{ticketsSold}</p></div>
                        <div class='kpi'><h4>Gross Revenue</h4><p>LKR {revenue:N2}</p></div>
                    </div>
                    <div class='table'>
                        <table style='width:100%; border-collapse:collapse;'>
                            <thead>
                                <tr>
                                    <th style='text-align:left;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Event</th>
                                    <th style='text-align:center;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Tickets</th>
                                    <th style='text-align:right;padding:10px 12px;border-bottom:1px solid #e5e7eb;color:#6b7280;font-weight:700'>Sales</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class='footer'>© {DateTime.Now.Year} Star Events</div>
            </div>
        </div>
    </body>
    </html>";
        }
        public static string GetEventPromoTemplate(string organizerName, string title, string message, string? ctaText = null, string? ctaUrl = null)
        {
            var button = string.IsNullOrWhiteSpace(ctaUrl) ? string.Empty : $"<div style='text-align:center;margin-top:18px;'><a href='{ctaUrl}' style='display:inline-block;background:linear-gradient(135deg,#7c3aed,#06b6d4);color:#fff;padding:12px 22px;border-radius:10px;text-decoration:none;font-weight:700'>{(string.IsNullOrWhiteSpace(ctaText) ? "View Event" : ctaText)}</a></div>";
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{ margin:0; padding:0; background:#0b1220; color:#e5e7eb; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; }}
        .wrap {{ padding:24px 12px; }}
        .card {{ max-width:640px; margin:0 auto; background:#0f172a; border:1px solid rgba(255,255,255,.08); border-radius:16px; overflow:hidden; box-shadow:0 10px 30px rgba(0,0,0,.35); }}
        .hdr {{ background:linear-gradient(135deg,#6d28d9,#0ea5e9); padding:24px; text-align:center; font-weight:800; font-size:22px; color:#fff; }}
        .content {{ padding:22px 20px; line-height:1.6; }}
        .footer {{ text-align:center; color:#64748b; font-size:12px; padding:0 0 22px; }}
    </style>
    </head>
    <body>
        <div class='wrap'>
            <div class='card'>
                <div class='hdr'>⭐ Star Events</div>
                <div class='content'>
                    <h2 style='margin:0 0 8px;font-size:20px;color:#f8fafc'>{title}</h2>
                    <p style='margin:0 0 14px;color:#cbd5e1'>{message}</p>
                    {button}
                    <p style='margin-top:18px;color:#94a3b8;font-size:13px'>— {organizerName}</p>
                </div>
                <div class='footer'>© {DateTime.Now.Year} Star Events</div>
            </div>
        </div>
    </body>
    </html>";
        }
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
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Organizer Account - Star Events</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background: #0b1220;
            color: #e5e7eb;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
        }}
        .wrapper {{
            width: 100%;
            padding: 24px 12px;
        }}
        .container {{
            max-width: 640px;
            margin: 0 auto;
            background: #0f172a;
            border: 1px solid rgba(255,255,255,0.08);
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 10px 30px rgba(0,0,0,0.35);
        }}
        .header {{
            background: linear-gradient(135deg, #6d28d9, #0ea5e9);
            padding: 28px 24px;
            text-align: center;
        }}
        .brand {{
            font-size: 26px;
            font-weight: 800;
            color: #fff;
            letter-spacing: 0.5px;
        }}
        .subbrand {{
            color: rgba(255,255,255,0.85);
            margin-top: 6px;
            font-size: 13px;
        }}
        .content {{
            padding: 28px 24px 8px;
        }}
        .greeting {{
            font-size: 20px;
            font-weight: 700;
            color: #f8fafc;
            margin: 0 0 10px;
        }}
        .lead {{
            color: #cbd5e1;
            line-height: 1.6;
            margin: 0 0 18px;
        }}
        .panel {{
            background: #0b1220;
            border: 1px solid rgba(255,255,255,0.08);
            border-radius: 12px;
            padding: 16px 18px;
            margin: 18px 0;
        }}
        .panel h4 {{
            margin: 0 0 10px;
            font-size: 14px;
            color: #93c5fd;
            font-weight: 700;
            letter-spacing: .3px;
        }}
        .row {{
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-top: 1px dashed rgba(255,255,255,0.08);
        }}
        .row:first-of-type {{
            border-top: 0;
        }}
        .label {{ color: #94a3b8; font-size: 14px; }}
        .value {{ color: #e5e7eb; font-weight: 700; font-size: 14px; }}
        .cta {{ text-align: center; padding: 8px 24px 28px; }}
        .button {{
            display: inline-block;
            background: linear-gradient(135deg, #7c3aed, #06b6d4);
            color: #fff;
            padding: 12px 22px;
            text-decoration: none;
            border-radius: 10px;
            font-weight: 700;
            letter-spacing: .3px;
        }}
        .hint {{
            margin-top: 10px;
            color: #94a3b8;
            font-size: 12px;
        }}
        .footer {{
            text-align: center;
            color: #64748b;
            font-size: 12px;
            padding: 18px 16px 26px;
        }}
    </style>
    </head>
    <body>
        <div class='wrapper'>
            <div class='container'>
                <div class='header'>
                    <div class='brand'>⭐ Star Events</div>
                    <div class='subbrand'>Organizer Account Invitation</div>
                </div>
                <div class='content'>
                    <div class='greeting'>Hello {fullName},</div>
                    <p class='lead'>An administrator has created an organizer account for you on Star Events. Use the credentials below to sign in and start creating events.</p>
                    <div class='panel'>
                        <h4>Your credentials</h4>
                        <div class='row'>
                            <span class='label'>Username</span>
                            <span class='value'>{username}</span>
                        </div>
                        <div class='row'>
                            <span class='label'>Temporary Password</span>
                            <span class='value'>{password}</span>
                        </div>
                    </div>
                </div>
                <div class='cta'>
                    <a href='{"/Identity/Account/Login"}' class='button'>Sign in to your organizer dashboard</a>
                    <div class='hint'>For security, please change your password after your first login.</div>
                </div>
                <div class='footer'>
                    © {DateTime.Now.Year} Star Events • This is an automated message, please do not reply.
                </div>
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
