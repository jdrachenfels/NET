using System;
using System.Collections.Generic;
using System.Linq;

namespace ClsLib
{
    public class ClsAdminUser
    {
        // Local variables
        private string _username = "";
        private string _password = "";
        private bool _authenticated = false;

        // Properties
        public string Username
        {
            get => _username;
            set
            {
                _username = (value ?? "").Trim().ToLower();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value ?? "";
            }
        }

        // Functions
        public bool Auth()
        {
            // Sample Auth
            if (_username == "admin" && _password == "drache123")
            {
                _authenticated = true;
            }
            else
            {
                _authenticated = false;
            }

            return _authenticated;
        }

        public List<string> GetPermissions()
        {
            var permissions = new List<string>();

            if (!_authenticated)
            {
                return permissions;
            }

            // Prototyp-Rechte
            permissions.Add("home.view");
            permissions.Add("home.edit");
            permissions.Add("home.off");

            return permissions;
        }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                return false;
            }

            return GetPermissions().Contains(permission.Trim().ToLower());
        }
    }
}