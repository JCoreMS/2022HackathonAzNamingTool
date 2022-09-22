using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Collections.Generic;
using System.Text.Json;

namespace AzureNamingTool.Helpers
{
    public class LogHelper
    {
        /// <summary>
        /// This function logs the Admin message.
        /// </summary>
        /// <param name="title">string - Message title</param>
        /// <param name="message">string - MEssage body</param>
        public static async void LogAdminMessage(string title, string message)
        {
            try
            {
                //create admin message
                AdminLogMessage adminmessage = new()
                {
                    Id = 1,
                    CreatedOn = DateTime.Now,
                    Title = title,
                    Message = message
                };

                // Log the created name
                var lstAdminLogMessages = new List<AdminLogMessage>();

                ServiceResponse serviceResponse = new();
                serviceResponse = await AdminLogService.GetItems();
                lstAdminLogMessages = (List<AdminLogMessage>)serviceResponse.ResponseObject;

                if (lstAdminLogMessages.Count > 0)
                {
                    adminmessage.Id = lstAdminLogMessages.Max(x => x.Id) + 1;
                }

                lstAdminLogMessages.Add(adminmessage);
                var jsonAdminLogMessages = JsonSerializer.Serialize(lstAdminLogMessages);
                await FileSystemHelper.WriteFile("adminlog.json", jsonAdminLogMessages);
                GeneralHelper.InvalidateCacheObject("AdminLogMessage");
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action. 
            }
        }

        
    }
}
