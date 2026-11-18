using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TrackAdmin.Models;
using SharedComponents.Rcl.Models;

namespace TrackAdmin.Data
{
    public class TenantService
    {
        private readonly TenantHttpService _http;
        private readonly string _baseUrl;

        public TenantService(TenantHttpService http, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _http = http;
            _baseUrl = config["ApiSettings:BaseApiUrl"];
        }

        public string GetLogoUrl(string tenantId, string type = null) => $"{_baseUrl}Tenant/GetLogo?tenantId={tenantId}{(string.IsNullOrEmpty(type) ? "" : $"&type={type}")}";

        private class RiderNameResponse
        {
            public string Name { get; set; }
        }

        public async Task<string> GetRiderNameAsync(int riderId)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}User/GetRiderName?riderId={riderId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RiderNameResponse>();
                    return result?.Name ?? "Rider";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rider name: {ex.Message}");
            }
            return "Rider";
        }

        public async Task<TenantProfile> GetBasicTenantInfoAsync(string tenantIdentifier)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}Tenant/GetBasicTenantInfo?tenantIdentifier={tenantIdentifier}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TenantProfile>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching basic tenant info: {ex.Message}");
            }
            return null;
        }

        public async Task<TenantProfile> GetTenantProfileAsync(string tenantCode)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}Tenant/GetTenantProfile?tenantCode={tenantCode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TenantProfile>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tenant profile: {ex.Message}");
            }
            return null;
        }

        public async Task<List<BranchMaster>> GetBranchesAsync(string tenantCode)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}Branch/GetBranchesByTenant?tenantCode={tenantCode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<BranchMaster>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching branches: {ex.Message}");
            }
            return new List<BranchMaster>();
        }

        public async Task<List<ProductList>> GetTenantProductsAsync(string tenantCode)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}Tenant/GetTenantProducts?tenantCode={tenantCode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductList>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching products: {ex.Message}");
            }
            return new List<ProductList>();
        }

        public async Task<List<User_Master>> GetUsersAsync(string productId = null)
        {
            try
            {
                var url = $"{_baseUrl}User/allstaff";
                if (!string.IsNullOrEmpty(productId)) url += $"?productId={productId}";
                
                var response = await _http.GetAsync(url);
                Console.WriteLine($"[DEBUG] GetUsersAsync Response: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<User_Master>>();
                    Console.WriteLine($"[DEBUG] Received {data?.Count ?? 0} users");
                    return data ?? new List<User_Master>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all users: {ex.Message}");
            }
            return new List<User_Master>();
        }

        public async Task<List<User_Master>> GetBranchUsersAsync(int bhCode)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}User/branchusers?bhCode={bhCode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<User_Master>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching branch users: {ex.Message}");
            }
            return new List<User_Master>();
        }

        public async Task<ProductPermission> GetUserPermissionsAsync(int userId, string tenantId, string productId, string token = null)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}User/GetPermissions?userId={userId}&tenantId={tenantId}&productId={productId}", token);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ProductPermission>();
                }
                else 
                {
                     var error = await response.Content.ReadAsStringAsync();
                     Console.WriteLine($"API Error fetching permissions: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching permissions: {ex.Message}");
            }
            return null; // Return null instead of fake fallback
        }

        public async Task<bool> SaveUserPermissionsAsync(ProductPermission pp)
        {
            try
            {
                var response = await _http.PostAsync($"{_baseUrl}User/SavePermissions", pp);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving permissions: {ex.Message}");
                return false;
            }
        }
        public async Task<List<RoleTemplate>> GetRoleTemplatesAsync(string tenantId, string productId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Fetching templates for Tenant: {tenantId}, Product: {productId}");
                var response = await _http.GetAsync($"{_baseUrl}User/GetRoleTemplates?tenantId={tenantId}&productId={productId}");
                Console.WriteLine($"[DEBUG] GetRoleTemplates Response: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<RoleTemplate>>();
                    Console.WriteLine($"[DEBUG] Received {data?.Count ?? 0} templates");
                    return data ?? new List<RoleTemplate>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching templates: {ex.Message}");
            }
            return new List<RoleTemplate>();
        }

        public async Task<List<ProductFeature>> GetProductFeaturesAsync(string productId)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}User/GetProductFeatures?productId={productId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductFeature>>() ?? new List<ProductFeature>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product features: {ex.Message}");
            }
            return new List<ProductFeature>();
        }

        public async Task<bool> SaveProductFeatureAsync(ProductFeature feature)
        {
            try
            {
                var response = await _http.PostAsync($"{_baseUrl}User/SaveProductFeature", feature);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product feature: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProductFeatureAsync(int featureId)
        {
            try
            {
                var response = await _http.PostAsync<object?>($"{_baseUrl}User/DeleteProductFeature?featureId={featureId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product feature: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProductList>> GetAllProductsAsync()
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}User/GetAllProducts");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProductList>>() ?? new List<ProductList>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all products: {ex.Message}");
            }
            return new List<ProductList>();
        }

        public async Task<bool> SaveProductAsync(ProductList product)
        {
            try
            {
                var response = await _http.PostAsync($"{_baseUrl}User/SaveProduct", product);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
                return false;
            }
        }

        public async Task<WelcomeStats> GetWelcomeStatsAsync()
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}Tenant/GetWelcomeStats");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<WelcomeStats>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching welcome stats: {ex.Message}");
            }
            return null;
        }
    }
}
