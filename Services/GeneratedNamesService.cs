﻿using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    public class GeneratedNamesService
    {
        private static ServiceResponse serviceResponse = new();

        /// <summary>
        /// This function gets the generated names log. 
        /// </summary>
        /// <returns>List of GeneratedNames - List of generated names</returns>
        public static async Task<ServiceResponse> GetItems() 
        {
            List<GeneratedName> lstGeneratedNames = new();
            try
            {
                // Get list of items
                var items = await GeneralHelper.GetList<GeneratedName>();
                serviceResponse.ResponseObject = items;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage{ Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            serviceResponse.ResponseObject = lstGeneratedNames;
            serviceResponse.Success = true;
            return serviceResponse;

        }

        /// <summary>
        ///  This function logs the generated name. 
        /// </summary>
        /// <param name="lstGeneratedName">GeneratedName - Generated name and components.</param>
        public static async Task<ServiceResponse> PostItem(GeneratedName lstGeneratedName)
        {
            ServiceResponse serviceReponse = new();
            try
            {
                /// Get the previously generated names
                var items = await GeneralHelper.GetList<GeneratedName>();

                if (items.Count > 0)
                {
                    lstGeneratedName.Id = items.Max(x => x.Id) + 1;
                }
                else
                {
                    lstGeneratedName.Id = 1;
                }

                items.Add(lstGeneratedName);

                // Write items to file
                await GeneralHelper.WriteList<GeneratedName>(items);

                GeneralHelper.InvalidateCacheObject("generatednames.json");

                serviceReponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceReponse.Success = false;
            }
            return serviceReponse;
        }

        /// <summary>
        /// This function clears the generated names log. 
        /// </summary>
        /// <returns>void</returns>
        public static async Task<ServiceResponse> DeleteAllItems()
        {
            ServiceResponse serviceReponse = new();
            try
            {
                await FileSystemHelper.WriteFile("generatednames.json", "[]");
                // Write the log to cache
                GeneralHelper.InvalidateCacheObject("generatednames.json");
                serviceReponse.Success = true;
            }
            catch (Exception ex)
            {
                await AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceReponse.Success = false;
            }
            return serviceReponse;
        }

        public static async Task<ServiceResponse> PostConfig(List<GeneratedName> items)
        {
            try
            {
                // Get list of items
                var newitems = new List<GeneratedName>();
                int i = 1;

                // Determine new item id
                foreach (GeneratedName item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await GeneralHelper.WriteList<GeneratedName>(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }
    }
}