using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    public class AdminLogService
    {
        private static ServiceResponse serviceResponse = new();

        
        /// <summary>
        /// This function returns the Admin log. 
        /// </summary>
        /// <returns>List of AdminLogMessages - List of Adming Log messages.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            try
            {
                // Get list of items
                var items = await GeneralHelper.GetList<AdminLogMessage>();
                serviceResponse.ResponseObject = items;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;

        }

        /// <summary>
        /// This function clears the Admin log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task DeleteAllItems()
        {
            try
            {
                await FileSystemHelper.WriteFile("adminlog.json", "[]");
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
            }

            serviceResponse.Success = true;
        }

        /// <summary>
        /// This function logs the Admin message.
        /// </summary>
        public static async Task<ServiceResponse> PostItem(AdminLogMessage log)
        {
            ServiceResponse serviceReponse = new();
            try
            {
                AdminLogMessage adminlogMessage = new();
                {
                    adminlogMessage.Id = 1;
                    adminlogMessage.CreatedOn = DateTime.Now;
                    adminlogMessage.Title = log.Title;
                    adminlogMessage.Message = log.Message;
                };

                // Log the created name
                var lstAdminLogMessages = new List<AdminLogMessage>();

                serviceResponse = await GeneratedNamesService.GetItems();
                lstAdminLogMessages = (List<AdminLogMessage>)serviceReponse.ResponseObject;

                if (lstAdminLogMessages.Count > 0)
                {
                    adminlogMessage.Id = lstAdminLogMessages.Max(x => x.Id) + 1;
                }

                lstAdminLogMessages.Add(adminlogMessage);
                // Write items to file
                await GeneralHelper.WriteList<AdminLogMessage>(lstAdminLogMessages);
                serviceReponse.Success = true;
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action.
                serviceReponse.Success = false;
            }
            return serviceReponse;
        }



    }
}
