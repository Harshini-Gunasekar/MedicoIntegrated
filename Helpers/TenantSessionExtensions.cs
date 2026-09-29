using System;
using System.Linq;
using SharedComponents.Rcl.Models;
using SharedComponents.Rcl.Services;

namespace SharedComponents.Rcl.Services
{
    public static class TenantSessionExtensions
    {
        public static bool HasActionRight(this TenantSessionState session, string department, string moduleName, string action)
        {
            if (session == null) return false;

            // If user rights list is empty (e.g. admin or uninitialized session), allow access
            if (session.UserRightsList == null || session.UserRightsList.Count == 0)
            {
                return true;
            }

            string normDept = (department ?? "").Trim();
            string rawMod = (moduleName ?? "").Trim();
            string cleanMod = rawMod.Replace("_", " ").Replace("-", " ").Trim();
            string act = (action ?? "").Trim().ToLowerInvariant();

            var matched = session.UserRightsList.Where(r =>
            {
                string rDept = (r.Department ?? "").Trim();
                string rMod = (r.ModuleName ?? "").Trim();
                string cleanRMod = rMod.Replace("_", " ").Replace("-", " ").Trim();

                bool deptMatch = string.IsNullOrEmpty(normDept) ||
                                 string.IsNullOrEmpty(rDept) ||
                                 string.Equals(rDept, normDept, StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(rDept, "General", StringComparison.OrdinalIgnoreCase) ||
                                 (normDept.Equals("Billing", StringComparison.OrdinalIgnoreCase) && rDept.Equals("Lab", StringComparison.OrdinalIgnoreCase)) ||
                                 (normDept.Equals("Lab", StringComparison.OrdinalIgnoreCase) && rDept.Equals("Billing", StringComparison.OrdinalIgnoreCase)) ||
                                 (normDept.Equals("Reception", StringComparison.OrdinalIgnoreCase) && (rDept.Equals("OP", StringComparison.OrdinalIgnoreCase) || rDept.Equals("Billing", StringComparison.OrdinalIgnoreCase))) ||
                                 (normDept.Equals("OP", StringComparison.OrdinalIgnoreCase) && rDept.Equals("Reception", StringComparison.OrdinalIgnoreCase));

                bool moduleMatch = string.Equals(rMod, rawMod, StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(cleanRMod, cleanMod, StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(rMod.Replace(" ", "").Replace("_", "").Replace("-", ""), rawMod.Replace(" ", "").Replace("_", "").Replace("-", ""), StringComparison.OrdinalIgnoreCase);

                return deptMatch && moduleMatch;
            }).ToList();

            if (!matched.Any())
            {
                return true;
            }

            return act switch
            {
                "create" or "add" => matched.Any(r => r.ToAdd),
                "edit" or "update" => matched.Any(r => r.ToEdit),
                "delete" => matched.Any(r => r.ToDelete),
                "view" or "read" => matched.Any(r => r.ToView),
                _ => matched.Any(r => r.ToView)
            };
        }
    }
}
