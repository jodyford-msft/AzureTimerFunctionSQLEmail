using System;
using System.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Text;


namespace FunctionApp1
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
                await ExecuteSqlQuery(_logger);
                await SendEmailAsync("Test Email", "This is a test email from Azure Functions", _logger);
            }
        }

        public static async Task ExecuteSqlQuery(ILogger log)
        {
            string connectionString = "connection string";
            string query = "SELECT top 10 * FROM dbo.projects;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await reader.ReadAsync())
                        {
                            // Process the query results, e.g., log the data  
                            log.LogInformation($"Row: {reader[0]}, {reader["ProjectName"]}, {reader["ProjectDescription"]}"); // Replace with actual column names or indices  


                        }
                    }
                }
            }
        }

        public static async Task SendEmailAsync(string subject, string body, ILogger log)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("smtp.gmail.com"); // Replace with your SMTP server address  

                mail.From = new MailAddress("jody.r.ford@gmail.com"); // Replace with your email address  
                mail.To.Add("jody.r.ford@gmail.com"); // Replace with the recipient's email address  
                mail.Subject = subject;
                mail.Body = body;

                smtpServer.Port = 587; // Replace with your SMTP server port (commonly 25, 587, or 465)  
                smtpServer.Credentials = new System.Net.NetworkCredential("jody.r.ford@gmail.com", "password"); // Replace with your email address and password  
                smtpServer.EnableSsl = true;

                await smtpServer.SendMailAsync(mail);
                log.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error sending email: {ex.Message}");
            }
        }

    }
}
